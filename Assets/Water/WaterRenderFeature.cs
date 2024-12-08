using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WaterRenderFeature : ScriptableRendererFeature
{
    [Serializable]
    public class WaterRenderSettings
    {
        [Header("Depth Blur")]
        [Range(0, 20)]
        public int depthBlurRadius = 5;
        [Range(0.1f, 20f)]
        public float depthDistanceSigma = 1.0f;
        [Range(0.0001f, 0.01f)]
        public float depthIntesitySigma = 1.0f;
        [Range(0, 0.5f)]
        public float depthRounding = 0.05f;
        [Range(1, 8)]
        public int depthBlurResolution = 1;

        [Header("Thickness Blur")]
        [Range(0, 1f)]
        public float thickness = 0.1f;
        [Range(0, 30)]
        public int thicknessBlurRadius = 5;
        [Range(0.1f, 30f)]
        public float thicknessDistanceSigma = 1.0f;
        [Range(1, 8)]
        public int thicknessBlurResolution = 1;

        [Header("Sphere")]
        public Mesh mesh;
        [Range(0, 1)]
        public float radius = 0.5f;
        public int maxParticles = 10000;

        [Header("Rendering")]
        public bool blur = false;
        public bool depth = false;
        public Vector3 lightPos = new Vector3(0, 0, 0);
        public bool limitFps = false;
        [Range(0f, 200f)]
        public float specularHighlight = 100f;
    }

    public class WaterRenderPass : ScriptableRenderPass
    {
        private WaterRenderSettings settings;
        private Material material;
        private RenderTextureDescriptor depthTextureDesc;
        private RenderTextureDescriptor thicknessTextureDesc;

        private RTHandle depthHandle;
        private RTHandle depthHorizontalHandle;
        private RTHandle depthVerticalHandle;

        private RTHandle thicknessHandle;
        private RTHandle thicknessHorizontalHandle;
        private RTHandle thicknessVerticalHandle;

        private ComputeBuffer positionBuffer;
        private ComputeBuffer indirectDrawArgsBuffer;
        private uint[] indirectDrawArgs;

        public WaterRenderPass(Material material, WaterRenderSettings settings)
        {
            this.settings = settings;
            this.material = material;

            this.depthTextureDesc = new RenderTextureDescriptor(Screen.width, Screen.height);
            this.depthTextureDesc.colorFormat = RenderTextureFormat.Depth;
            this.depthTextureDesc.depthBufferBits = 32;

            this.thicknessTextureDesc = new RenderTextureDescriptor(Screen.width, Screen.height);
            this.thicknessTextureDesc.colorFormat = RenderTextureFormat.Default;

            this.positionBuffer = new(this.settings.maxParticles, sizeof(float) * 4);
            this.indirectDrawArgsBuffer = new(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
            this.indirectDrawArgs = new uint[]{
                6,
                (uint)this.settings.maxParticles,
                0,
                0,
                0,
            };
        }

        private void UpdateSettings()
        {
            if (material == null)
            {
                Debug.LogWarning("No material set for water render pass");
                return;
            }

            //TODO MISMATCH?
            material.SetInt("_BlurRadius", this.settings.depthBlurRadius);
            material.SetFloat("_DepthRounding", this.settings.depthRounding);
            material.SetFloat("_DistanceSigma", this.settings.depthDistanceSigma);
            material.SetFloat("_IntensitySigma", this.settings.depthIntesitySigma);

            material.SetFloat("_Thickness", this.settings.thickness);
            material.SetInt("_ThicknessBlurRadius", this.settings.thicknessBlurRadius);
            material.SetFloat("_ThicknessSigma", this.settings.thicknessDistanceSigma);

            material.SetVector("_LightPos", this.settings.lightPos);
            material.SetFloat("_SpecularHighlight", this.settings.specularHighlight);

        }

        // before execute
        // TODO: think this leaks
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Full res
            this.depthTextureDesc.width = cameraTextureDescriptor.width;
            this.depthTextureDesc.height = cameraTextureDescriptor.height;
            this.thicknessTextureDesc.width = cameraTextureDescriptor.width;
            this.thicknessTextureDesc.height = cameraTextureDescriptor.height;

            RenderingUtils.ReAllocateIfNeeded(ref depthHandle, depthTextureDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);
            RenderingUtils.ReAllocateIfNeeded(ref thicknessHandle, thicknessTextureDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);

            // Half res
            this.depthTextureDesc.width = cameraTextureDescriptor.width / this.settings.depthBlurResolution;
            this.depthTextureDesc.height = cameraTextureDescriptor.height / this.settings.depthBlurResolution;
            this.thicknessTextureDesc.width = cameraTextureDescriptor.width / this.settings.thicknessBlurResolution;
            this.thicknessTextureDesc.height = cameraTextureDescriptor.height / this.settings.thicknessBlurResolution;

            RenderingUtils.ReAllocateIfNeeded(ref depthHorizontalHandle, depthTextureDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);
            RenderingUtils.ReAllocateIfNeeded(ref depthVerticalHandle, depthTextureDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);
            RenderingUtils.ReAllocateIfNeeded(ref thicknessHorizontalHandle, thicknessTextureDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);
            RenderingUtils.ReAllocateIfNeeded(ref thicknessVerticalHandle, thicknessTextureDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            this.material.enableInstancing = true;
            // renderingData.cameraData.requiresDepthTexture = true;

            this.material.SetTexture("_DepthTexture", this.depthHandle);
            this.material.SetTexture("_DepthHorizontalTexture", this.depthHorizontalHandle);
            this.material.SetTexture("_DepthVerticalTexture", this.depthVerticalHandle);

            this.material.SetTexture("_ThicknessTexture", this.thicknessHandle);
            this.material.SetTexture("_ThicknessHorizontalTexture", this.thicknessHorizontalHandle);
            this.material.SetTexture("_ThicknessVerticalTexture", this.thicknessVerticalHandle);

            this.material.SetBuffer("_PositionBuffer", this.positionBuffer);

            Camera cam = renderingData.cameraData.camera;
            Matrix4x4 viewMatrix = cam.worldToCameraMatrix;
            Matrix4x4 projectionMatrix = cam.projectionMatrix;
            Matrix4x4 viewProjectionMatrix = projectionMatrix * viewMatrix;
            material.SetMatrix("_InverseView", viewMatrix.inverse);
            material.SetMatrix("_InverseProjection", projectionMatrix.inverse);
            material.SetMatrix("_InverseViewProjection", viewProjectionMatrix.inverse);

            RTHandle[] handles = {
                depthHandle,
                depthHorizontalHandle,
                depthVerticalHandle,
                thicknessHandle, thicknessHorizontalHandle, thicknessVerticalHandle
            };
            foreach (var handle in handles)
            {
                cmd.SetRenderTarget(handle);
                cmd.ClearRenderTarget(true, true, Color.clear);
            }
            // cmd.SetRenderTarget(depthHandle);
            // cmd.ClearRenderTarget(true, false, Color.clear);
            // cmd.SetRenderTarget(depthHorizontalHandle);
            // cmd.ClearRenderTarget(true, false, Color.clear);
            // cmd.SetRenderTarget(depthVerticalHandle);
            //
            // cmd.ClearRenderTarget(true, false, Color.clear);
            // cmd.SetRenderTarget(thicknessHandle);
            // cmd.ClearRenderTarget(false, true, Color.clear);
            // cmd.SetRenderTarget(thicknessHorizontalHandle);
            // cmd.ClearRenderTarget(false, true, Color.clear);
            // cmd.SetRenderTarget(thicknessVerticalHandle);
            // cmd.ClearRenderTarget(false, true, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            this.UpdateSettings();

            CommandBuffer cmd = CommandBufferPool.Get("WaterRenderPass");

            Camera camera = renderingData.cameraData.camera;
            RTHandle colorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            RTHandle depthTarget = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            // TODO: need to combine with camera depth

            int depthIndex = 0;
            int thicknessIndex = 1;
            int depthBlurHorizontalIndex = 2;
            int depthBlurVerticalIndex = 3;
            int thicknessBlurHorizontalIndex = 4;
            int thicknessBlurVerticalIndex = 5;
            int renderIndex = 6;

            // render depth
            cmd.SetRenderTarget(this.depthHandle);
            this.DrawSpheres(cmd, depthIndex, camera);

            // blur depth
            Blit(cmd, this.depthHorizontalHandle, this.depthHorizontalHandle, this.material, depthBlurHorizontalIndex);
            Blit(cmd, this.depthVerticalHandle, this.depthVerticalHandle, this.material, depthBlurVerticalIndex);

            // render thickness
            cmd.SetRenderTarget(this.thicknessHandle);
            this.DrawSpheres(cmd, thicknessIndex, camera);

            // blur thickness
            Blit(cmd, this.thicknessHorizontalHandle, this.thicknessHorizontalHandle, this.material, thicknessBlurHorizontalIndex);
            Blit(cmd, this.thicknessVerticalHandle, this.thicknessVerticalHandle, this.material, thicknessBlurVerticalIndex);

            if (this.settings.depth)
            {
                Blit(cmd, this.depthVerticalHandle, colorTarget);
            }
            else if (this.settings.blur)
                Blit(cmd, this.thicknessVerticalHandle, colorTarget);
            else
            {
                // render
                cmd.SetRenderTarget(depthTarget, depthTarget);
                Blit(cmd, colorTarget, colorTarget, this.material, renderIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void DrawSpheres(CommandBuffer cmd, int pass_index, Camera camera)
        {
            List<Vector4> centers = new();

            PBDFluid.FluidSetup fluidSimulation = FindObjectOfType<PBDFluid.FluidSetup>();
            if (fluidSimulation.m_fluid != null)
            {
                Debug.Log(fluidSimulation.GetPositionBuffer().stride);

                this.positionBuffer = fluidSimulation.GetPositionBuffer();

                this.indirectDrawArgs[1] = (uint)fluidSimulation.GetPositionBuffer().count;
                this.indirectDrawArgsBuffer.SetData(this.indirectDrawArgs);

                this.material.SetFloat("_Radius", this.settings.radius);
                //this.material.SetFloat("_Radius", fluidSimulation.radius / 8); // TODO: this does not map correctly, fluidSimulation.radius is probably in world space
                this.material.SetFloat("_Scale", fluidSimulation.scale);
                this.material.SetFloat("_Damping", fluidSimulation.damping);
                this.material.SetVector("_SimulationCenter", fluidSimulation.transform.position);
            }


            cmd.DrawMeshInstancedIndirect(this.settings.mesh, 0, this.material, pass_index, this.indirectDrawArgsBuffer);
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                UnityEngine.Object.Destroy(material);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
#else
            UnityEngine.Object.Destroy(material);
#endif

            this.positionBuffer.Release();

            RTHandle[] handles = {
                depthHandle,
                depthHorizontalHandle,
                depthVerticalHandle,
                thicknessHandle,
                thicknessHorizontalHandle,
                thicknessVerticalHandle
            };
            foreach (var handle in handles)
            {
                handle?.Release();
            }
        }
    }

    [SerializeField] private WaterRenderSettings settings;
    [SerializeField] private Shader shader;
    // [SerializeField] private PBDFluid.FluidSetup fluidBody;
    private Material material;
    private WaterRenderPass renderPass;

    // every frame
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(this.renderPass);
        }
    }

    // init/variable changes
    public override void Create()
    {
        if (this.settings.limitFps)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }
        else
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 500;
        }

        if (this.shader == null)
        {
            Debug.LogWarning("Shader not set in water render feature");
            return;
        }

        this.material = new Material(this.shader);
        this.renderPass = new WaterRenderPass(this.material, this.settings);

        this.renderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    protected override void Dispose(bool disposing)
    {
        this.renderPass.Dispose();
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            UnityEngine.Object.Destroy(material);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
#else
            UnityEngine.Object.Destroy(material);
#endif
    }
}

