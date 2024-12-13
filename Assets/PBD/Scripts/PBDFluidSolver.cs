using System;
using System.Collections.Generic;
using UnityEngine;

namespace PBDFluid
{

    public class PBDFluidSolver {
        public static ComputeShader m_shader;
        public Matrix4x4 LocalToWorld;
        public Matrix4x4 WorldToLocal;

        protected const int THREADS = 128;
        protected const int READ = 0;
        protected const int WRITE = 1;

        public int SolverIterations { get; set; }
        public int ConstraintIterations { get; set; }

        public int Groups { get; protected set; }

        public FluidBody Body { get; protected set; }
        public GridHash Hash { get; protected set; }

        public SmoothingKernel Kernel { get; protected set; }

        public PBDFluidSolver(FluidBody body) {
            Body = body;

            float cellSize = Body.ParticleRadius * 4.0f;
            int total = Body.NumParticles;
            Hash = new GridHash(body.Bounds, total, cellSize);
            Kernel = new SmoothingKernel(cellSize);

            int numParticles = Body.NumParticles;
            Groups = numParticles / THREADS;
            if (numParticles % THREADS != 0) Groups++;

            SolverIterations = 1;
            ConstraintIterations = 2;
        }

        public void StepPhysics(float dt, Vector3 minBounds, Vector3 maxBounds) {
            if (dt <= 0.0 || SolverIterations <= 0 || ConstraintIterations <= 0) return;

            dt = Mathf.Min(dt, Time.deltaTime * 3f);

            dt /= SolverIterations;

            m_shader.SetVector("MinBounds", minBounds);
            m_shader.SetVector("MaxBounds", maxBounds);

            m_shader.SetInt("NumParticles", Body.NumParticles);
            m_shader.SetVector("Gravity", new Vector3(0.0f, -9.81f, 0.0f)); //new Vector3(0.0f, 0.0f, 0.0f)
            m_shader.SetFloat("Dampning", Body.Dampning);
            m_shader.SetFloat("DeltaTime", dt);
            m_shader.SetFloat("Density", Body.Density);
            m_shader.SetFloat("InvDensity", 1f / Body.Density);
            m_shader.SetFloat("Viscosity", Body.Viscosity);
            m_shader.SetFloat("ParticleMass", Body.ParticleMass);

            m_shader.SetFloat("KernelRadius", Kernel.Radius);
            m_shader.SetFloat("KernelRadius2", Kernel.Radius2);
            m_shader.SetFloat("Poly6Zero", Kernel.Poly6(Vector3.zero));
            m_shader.SetFloat("Poly6", Kernel.POLY6);
            m_shader.SetFloat("SpikyGrad", Kernel.SPIKY_GRAD);
            m_shader.SetFloat("ViscLap", Kernel.VISC_LAP);

            m_shader.SetFloat("HashScale", Hash.InvCellSize);
            m_shader.SetVector("HashSize", Hash.Bounds.size);
            m_shader.SetVector("HashTranslate", Hash.Bounds.min);

            m_shader.SetMatrix("_LocalToWorld", LocalToWorld);
            m_shader.SetMatrix("_WorldToLocal", WorldToLocal);

            //Predicted and velocities use a double buffer as solver step
            //needs to read from many locations of buffer and write the result
            //in same pass. Could be removed if needed as long as buffer writes 
            //are atomic. Not sure if they are.
            for (int i = 0; i < SolverIterations; i++) {
                PredictPositions(dt);
 
                Hash.Process(Body.Predicted[READ], WorldToLocal);

                ConstrainPositions();

                UpdateVelocities(dt);

                SolveViscosity();

                UpdatePositions();
            }
        }

        private void PredictPositions(float dt) {
            int kernel = m_shader.FindKernel("PredictPositions");

            m_shader.SetBuffer(kernel, "Positions", Body.Positions);
            m_shader.SetBuffer(kernel, "PredictedWRITE", Body.Predicted[WRITE]);
            m_shader.SetBuffer(kernel, "VelocitiesREAD", Body.Velocities[READ]);
            m_shader.SetBuffer(kernel, "VelocitiesWRITE", Body.Velocities[WRITE]);

            m_shader.Dispatch(kernel, Groups, 1, 1);

            Swap(Body.Predicted);
            Swap(Body.Velocities);
        }

        public void ConstrainPositions() {
            int computeKernel = m_shader.FindKernel("ComputeDensityAndPressure");
            int solveKernel = m_shader.FindKernel("SolveConstraint");

            m_shader.SetBuffer(computeKernel, "Densities", Body.Densities);
            m_shader.SetBuffer(computeKernel, "Pressures", Body.Pressures);
            m_shader.SetBuffer(computeKernel, "IndexMap", Hash.IndexMap);
            m_shader.SetBuffer(computeKernel, "Table", Hash.Table);

            m_shader.SetBuffer(solveKernel, "Pressures", Body.Pressures);
            m_shader.SetBuffer(solveKernel, "IndexMap", Hash.IndexMap);
            m_shader.SetBuffer(solveKernel, "Table", Hash.Table);

            for (int i = 0; i < ConstraintIterations; i++) {
                m_shader.SetBuffer(computeKernel, "PredictedREAD", Body.Predicted[READ]);
                m_shader.Dispatch(computeKernel, Groups, 1, 1);

                m_shader.SetBuffer(solveKernel, "PredictedREAD", Body.Predicted[READ]);
                m_shader.SetBuffer(solveKernel, "PredictedWRITE", Body.Predicted[WRITE]);
                m_shader.Dispatch(solveKernel, Groups, 1, 1);

                Swap(Body.Predicted);
            }
        }

        private void UpdateVelocities(float dt) {
            int kernel = m_shader.FindKernel("UpdateVelocities");

            m_shader.SetBuffer(kernel, "Positions", Body.Positions);
            m_shader.SetBuffer(kernel, "PredictedREAD", Body.Predicted[READ]);
            m_shader.SetBuffer(kernel, "VelocitiesWRITE", Body.Velocities[WRITE]);

            m_shader.Dispatch(kernel, Groups, 1, 1);

            Swap(Body.Velocities);
        }

        private void SolveViscosity() {
            int kernel = m_shader.FindKernel("SolveViscosity");

            m_shader.SetBuffer(kernel, "Densities", Body.Densities);
            m_shader.SetBuffer(kernel, "IndexMap", Hash.IndexMap);
            m_shader.SetBuffer(kernel, "Table", Hash.Table);

            m_shader.SetBuffer(kernel, "PredictedREAD", Body.Predicted[READ]);
            m_shader.SetBuffer(kernel, "VelocitiesREAD", Body.Velocities[READ]);
            m_shader.SetBuffer(kernel, "VelocitiesWRITE", Body.Velocities[WRITE]);

            m_shader.Dispatch(kernel, Groups, 1, 1);

            Swap(Body.Velocities);
        }

        private void UpdatePositions() {
            int kernel = m_shader.FindKernel("UpdatePositions");

            m_shader.SetBuffer(kernel, "Positions", Body.Positions);
            m_shader.SetBuffer(kernel, "PredictedREAD", Body.Predicted[READ]);

            m_shader.Dispatch(kernel, Groups, 1, 1);
        }

        private void Swap(ComputeBuffer[] buffers) {
            ComputeBuffer tmp = buffers[0];
            buffers[0] = buffers[1];
            buffers[1] = tmp;
        }

        public void Dispose() {
            Hash.Dispose();
        }
    }
}