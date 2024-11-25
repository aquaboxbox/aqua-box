using System;
using UnityEngine;

namespace PBDFluid {
    public class SmoothingKernel {
        public float POLY6 { get; private set; }
        public float SPIKY_GRAD { get; private set; }
        public float VISC_LAP { get; private set; }

        public float Radius { get; private set; }
        public float InvRadius { get; private set; }
        public float Radius2 { get; private set; }

        public SmoothingKernel(float radius) {
            Radius = radius;
            Radius2 = radius * radius;
            InvRadius = 1.0f / radius;

            POLY6 = 315.0f / (65.0f * Mathf.PI * Mathf.Pow(Radius, 9.0f));
            SPIKY_GRAD = -45.0f / (Mathf.PI * Mathf.Pow(Radius, 6.0f));
            VISC_LAP = 45.0f / (Mathf.PI * Mathf.Pow(Radius, 6.0f));
        }

        float Pow3(float v) {
            return v * v * v;
        }

        float Pow2(float v) {
            return v * v;
        }

        public float Poly6(Vector3 p) {
            return Math.Max(0, POLY6 * Pow3(Radius2 - p.sqrMagnitude));
        }

        public Vector3 SpikyGrad(Vector3 p) {
            return p.normalized * SPIKY_GRAD * Pow2(Mathf.Max(0, Radius - p.magnitude));
        }

        public float ViscLap(Vector3 p) {
            return VISC_LAP * Mathf.Max(0, Radius - p.magnitude);
        }
    }
}
