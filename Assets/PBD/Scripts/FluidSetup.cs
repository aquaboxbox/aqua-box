using UnityEngine;
using System;
using System.Collections.Generic;

namespace PBDFluid
{
    public class FluidSetup : MonoBehaviour
    {

        [Header("Compute Shaders")]
        [SerializeField] public ComputeShader m_gridHash;
        [SerializeField] public ComputeShader m_bitonicSort;
        [SerializeField] public ComputeShader m_fluidSolver;

        [Header("Unity References")]
        public Material m_fluidParticleMat;
        public Material m_boundaryParticleMat;
        public Mesh m_sphereMesh;

        [Header("Particle Settings")]
        [Range(0f, 1f)] public float damping = 0f;
        public float radius = 0.1f;
        private float density = 1000;
        public bool m_drawFluidParticles = false;

        [Header("Simulation Settings")]
        public float scale = 1;
        public bool m_run = true;
        public float dt = 0.013f;
        public Bounds simulationBounds = new Bounds(new Vector3(0, 5, -2), new Vector3(8, 5, 2));
        public Bounds[] fluidSpawnBounds = { new Bounds(new Vector3(-6, 2, -2), new Vector3(2, 2, 2)) };

        public FluidBody m_fluid;
        private PBDFluidSolver m_solver;

        Bounds outerBounds;

        public ComputeBuffer GetPositionBuffer() => m_fluid.Positions;
        public ComputeBuffer GetDensityBuffer() => m_fluid.Densities;


        void OnEnable()
        {
            // Set the compute shaders
            GridHash.m_shader = m_gridHash;
            BitonicSort.m_shader = m_bitonicSort;
            PBDFluidSolver.m_shader = m_fluidSolver;

            m_fluid = CreateFluid(radius, density);

            Debug.Log("Fluid Particles = " + m_fluid.NumParticles);

            m_fluid.Bounds = simulationBounds;
            m_solver = new PBDFluidSolver(m_fluid);

        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                OnDisable();
                OnEnable();
            }

            if (m_run)
            {
                Vector3 pos = transform.position;
                transform.position = transform.position * 1f / scale * (1f - damping);
                m_solver.LocalToWorld = transform.localToWorldMatrix;
                m_solver.WorldToLocal = transform.worldToLocalMatrix;
                transform.position = pos;
                m_solver.StepPhysics(dt, simulationBounds.min, simulationBounds.max);
            }

            if (m_drawFluidParticles)
            {
                m_fluidParticleMat.SetFloat("_Scale", scale);
                m_fluidParticleMat.SetFloat("_Damping", damping);
                m_fluidParticleMat.SetVector("_SimulationCenter", transform.position);
                m_fluid.Draw(Camera.main, m_sphereMesh, m_fluidParticleMat, 0);
            }
        }

        private void OnDisable()
        {
            m_fluid.Dispose();
            m_solver.Dispose();
        }

        private FluidBody CreateFluid(float radius, float density)
        {
            List<Vector3> Positions = new List<Vector3>();

            // The sources will create arrays of particles evenly spaced inside the bounds.
            Vector3 particleOffset = Vector3.one * radius;
            foreach (Bounds b in fluidSpawnBounds)
            {
                b.SetMinMax(b.min + particleOffset, b.max - particleOffset);
                Positions.AddRange(CreateParticles(radius * 1.8f, b));
            }

            return new FluidBody(Positions, radius, density, Matrix4x4.identity);
        }

        // Fills bounds with evenly spaced particles
        public static IList<Vector3> CreateParticles(float spacing, Bounds bounds)
        {
            Vector3Int particleCount = Vector3Int.FloorToInt(bounds.size / spacing);
            List<Vector3> Positions = new List<Vector3>((int)particleCount.x * particleCount.y * particleCount.z);

            for (int z = 0; z < particleCount.z; z++)
            {
                for (int y = 0; y < particleCount.y; y++)
                {
                    for (int x = 0; x < particleCount.x; x++)
                    {
                        Vector3 pos = bounds.min + new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) * spacing;
                        Positions.Add(pos);
                    }
                }
            }
            return Positions;
        }
    }
}
