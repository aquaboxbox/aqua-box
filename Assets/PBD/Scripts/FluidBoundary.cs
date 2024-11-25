using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PBDFluid
{

    public class FluidBoundary : IDisposable
    {
        private const int THREADS = 128;

        public Bounds Bounds;

        public int NumParticles { get; private set; }
        public float ParticleRadius { get; private set; }
        public float ParticleDiameter { get { return ParticleRadius * 2.0f; } }
        public float Density { get; private set; }
        
        // Compute Buffers
        public ComputeBuffer Positions { get; private set; }
        private ComputeBuffer m_argsBuffer;

        public FluidBoundary(IList<Vector3> Positions, float radius, float density, Matrix4x4 RTS) {
            NumParticles = Positions.Count;
            ParticleRadius = radius;
            Density = density;

            CreateParticles(Positions, RTS);
        }

        public void Draw(Camera cam, Mesh mesh, Material material, int layer) {
            if (m_argsBuffer == null)
                CreateArgBuffer(mesh.GetIndexCount(0));

            material.SetBuffer("_Particles", Positions);
            material.SetFloat("_ParticleSize", ParticleDiameter);

            ShadowCastingMode castShadow = ShadowCastingMode.Off;
            bool recieveShadow = false;

            Graphics.DrawMeshInstancedIndirect(
                mesh, 
                0, 
                material, 
                Bounds, 
                m_argsBuffer, 
                0, 
                null, 
                castShadow, 
                recieveShadow, 
                layer, 
                null
            );
        }

        public void Dispose() {
            Positions?.Release();
            m_argsBuffer?.Release();
        }

        private void CreateParticles(IList<Vector3> Positions, Matrix4x4 RTS) {
            Vector4[] positions = new Vector4[NumParticles];

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