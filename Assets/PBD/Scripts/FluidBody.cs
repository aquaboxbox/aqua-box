using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PBDFluid {

    public class FluidBody : IDisposable {

        // SIMULATION
        public int NumParticles { get; private set; }
        public Bounds Bounds;

        // PARTICLE
        public float Density { get; set; }
        public float Viscosity { get; set; }
        public float Dampning { get; set; }
        public float ParticleRadius { get; private set; }
        public float ParticleDiameter { get { return ParticleRadius * 2.0f; } }
        public float ParticleMass { get; set; }
        public float ParticleVolume { get; private set; }

        // GENERAL
        public ComputeBuffer Pressures { get; private set; }
        public ComputeBuffer Densities { get; private set; }
        public ComputeBuffer Positions { get; private set; }

        // PBD
        public ComputeBuffer[] Predicted { get; private set; }
        public ComputeBuffer[] Velocities { get; private set; }

        // RENDERING
        private ComputeBuffer m_argsBuffer;

        public FluidBody(IList<Vector3> Positions, float radius, float density, Matrix4x4 RTS) {
            NumParticles = Positions.Count;
            Density = density;
            Viscosity = 0.002f;
            Dampning = 0.0f;

            ParticleRadius = radius;
            ParticleVolume = (4.0f / 3.0f) * Mathf.PI * Mathf.Pow(radius, 3);
            ParticleMass = ParticleVolume * Density;

            CreateParticles(Positions, RTS);
        }

        public void Draw(Camera cam, Mesh mesh, Material material, int layer) {
            if (m_argsBuffer == null)
                CreateArgBuffer(mesh.GetIndexCount(0));

            material.SetBuffer("_Particles", Positions);
            material.SetFloat("_ParticleSize", ParticleDiameter);

            Graphics.DrawMeshInstancedIndirect(
                mesh, 
                0, 
                material, 
                Bounds, 
                m_argsBuffer, 
                0, 
                null, 
                ShadowCastingMode.Off, 
                true, 
                layer, 
                null
            );
        }

        public void Dispose() {
            Positions?.Release();
            Densities?.Release();
            Pressures?.Release();

            Predicted[0]?.Release();
            Predicted[1]?.Release();
            Velocities[0]?.Release();
            Velocities[1]?.Release();

            m_argsBuffer?.Release();
        }

        private void CreateParticles(IList<Vector3> Positions, Matrix4x4 RTS) {
            Vector4[] positions = new Vector4[NumParticles];
            Vector4[] velocities = new Vector4[NumParticles];

            float inf = float.PositiveInfinity;
            Vector3 min = new Vector3(inf, inf, inf);
            Vector3 max = new Vector3(-inf, -inf, -inf);

            for (int i = 0; i < NumParticles; i++) {
                Vector4 pos = RTS * Positions[i];
                positions[i] = pos;

                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }

            min -= Vector3.one * ParticleRadius;
            max += Vector3.one * ParticleRadius;

            Bounds = new Bounds();
            Bounds.SetMinMax(min, max);

            this.Positions = new ComputeBuffer(NumParticles, 4 * sizeof(float));
            this.Positions.SetData(positions);

            Densities = new ComputeBuffer(NumParticles, sizeof(float));
            Pressures = new ComputeBuffer(NumParticles, sizeof(float));

            //Predicted and velocities use a double buffer as solver step
            //needs to read from many locations of buffer and write the result
            //in same pass. Could be removed if needed as long as buffer writes 
            //are atomic. Not sure if they are.

            Predicted = new ComputeBuffer[2];
            Predicted[0] = new ComputeBuffer(NumParticles, 4 * sizeof(float));
            Predicted[0].SetData(positions);
            Predicted[1] = new ComputeBuffer(NumParticles, 4 * sizeof(float));
            Predicted[1].SetData(positions);

            Velocities = new ComputeBuffer[2];
            Velocities[0] = new ComputeBuffer(NumParticles, 4 * sizeof(float));
            Velocities[0].SetData(velocities);
            Velocities[1] = new ComputeBuffer(NumParticles, 4 * sizeof(float));
            Velocities[1].SetData(velocities);
        }

        private void CreateArgBuffer(uint indexCount) {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = indexCount;
            args[1] = (uint)NumParticles;

            m_argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            m_argsBuffer.SetData(args);
        }
    }
}