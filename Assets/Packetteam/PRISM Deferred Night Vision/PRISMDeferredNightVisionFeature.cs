using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace Prism { 

    public class PRISMDeferredNightVisionFeature : ScriptableRendererFeature
    {
        class PRISMDeferredNightVisionRenderPass : ScriptableRenderPass
        {

            Material m_Material;
            string m_PassName = "PRISM Deferred Night Vision";
            public PRISMDeferredNightVisionRenderPass(Material material)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
                m_Material = material;
            }

            private static readonly int GbufferLightingIndex = 3;
            private PRISMDeferredNightVision m_VolumeComponent;

            private static readonly int[] s_GBufferShaderPropertyIDs = new int[]
            {
                // Contains Albedo Texture
                Shader.PropertyToID("_GBuffer0"),

                // Contains Specular Metallic Texture
                Shader.PropertyToID("_GBuffer1"),

                // Contains Normals and Smoothness, referenced as _CameraNormalsTexture in other shaders
                Shader.PropertyToID("_GBuffer2"),

                // Contains Lighting texture
                Shader.PropertyToID("_GBuffer3"),

                // Contains Depth texture, referenced as _CameraDepthTexture in other shaders (optional)
                Shader.PropertyToID("_GBuffer4"),

                // Contains Rendering Layers Texture, referenced as _CameraRenderingLayersTexture in other shaders (optional)
                Shader.PropertyToID("_GBuffer5"),

                // Contains ShadowMask texture (optional)
                Shader.PropertyToID("_GBuffer6")
            };

            private class PassData
            {
                // In this example, we want to use the gBuffer components in our pass.
                public TextureHandle SourceTexture;
                public TextureHandle[] gBuffer;
                public Material material;
            }

            public void Setup(Material material)
            {
                m_Material = material;
            }

            public void UpdateShaderValues()
            {
                if (m_Material == null)
                    return;

                if (m_VolumeComponent == null)
                {
                    Debug.LogError("No volume component found for deferred night vision! Add it to your volume.");
                    return;
                }

                m_Material.SetVector("_NVColor", m_VolumeComponent.m_NVColor.value);

                m_Material.SetVector("_TargetWhiteColor", m_VolumeComponent.m_TargetBleachColor.value);

                m_Material.SetFloat("_BaseLightingContribution", m_VolumeComponent.m_baseLightingContribution.value);

                m_Material.SetFloat("_LightSensitivityMultiplier", m_VolumeComponent.m_LightSensitivityMultiplier.value);

                // State switching		
                m_Material.shaderKeywords = null;

                if (m_VolumeComponent.useVignetting.value == true)
                {
                    Shader.EnableKeyword("USE_VIGNETTE");
                }
                else
                {
                    Shader.DisableKeyword("USE_VIGNETTE");
                }

            }

            // This method will draw the contents of the gBuffer component requested in the shader
            static void ExecutePass(PassData data, RasterGraphContext context, int passNum)
            {

                if (passNum == 0)
                {
                    // Here, we read all the gBuffer components as an example even though the shader only needs one.
                    // We still need to set it explicitly since it is not accessible globally (so the
                    // shader won't have access to it by default).
                    for (int i = 0; i < data.gBuffer.Length; i++)
                    {
                        data.material.SetTexture(s_GBufferShaderPropertyIDs[i], data.gBuffer[i]);
                    }

                    // Draw the gBuffer component requested by the shader over the geometrys
                    //context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1);

                    Blitter.BlitTexture(context.cmd, data.SourceTexture, Vector4.one, data.material, passNum);
                }
                else
                {
                    Blitter.BlitTexture(context.cmd, data.SourceTexture, Vector4.one, 0, false);
                }
            }

            // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
            // FrameData is a context container through which URP resources can be accessed and managed.
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
                // The gBuffer components are only used in deferred mode
                if (m_Material == null || universalRenderingData.renderingMode != RenderingMode.Deferred)
                    return;

                m_VolumeComponent = VolumeManager.instance.stack.GetComponent<PRISMDeferredNightVision>();

                if(m_VolumeComponent == null)
                {
                    Debug.LogError("No Deferred Night Vision component found on any volumes. Ensure you have added a PRISM Deferred Night Vision effect to a Volume component in your scene.");
                    return;
                }

                UpdateShaderValues();

                // Get the gBuffer texture handles stored in the resourceData
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                TextureHandle[] gBuffer = resourceData.gBuffer;
                TextureHandle sourceTexture = resourceData.activeColorTexture;

                // Temporary destination texture for blitting.
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                if (cameraData.isSceneViewCamera || cameraData.isPreviewCamera || m_VolumeComponent.active == false || m_VolumeComponent.AnyPropertiesIsOverridden() == false)
                {
                    return;
                }
                RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                TextureHandle temporaryDestinationTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_DeferredNVRT", true, FilterMode.Bilinear);

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(m_PassName, out var passData))
                {
                    passData.material = m_Material;

                    builder.AllowPassCulling(false);
                    // For this pass, we want to write to the activeColorTexture, which is the gBuffer Lighting component (_GBuffer3) in the deferred path.
                    //builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                    // Read the sourceTexture as pass input.
                    builder.SetInputAttachment(sourceTexture, 0, AccessFlags.Read);
                    passData.SourceTexture = sourceTexture;

                    // Bind the temporary texture as a framebuffer color attachment at index 0.
                    builder.SetRenderAttachment(temporaryDestinationTexture, 0, AccessFlags.Write);

                    // We are reading the gBuffer components in our pass, so we need call UseTexture on them.s
                    // When they are global, they can be all read with builder.UseAllGlobalTexture(true), but
                    // in this pass they are not global.
                    for (int i = 0; i < resourceData.gBuffer.Length; i++)
                    {
                        if (i == GbufferLightingIndex)
                        {
                            // We already specify we are writing to it above (SetRenderAttachment)
                            continue;
                        }

                        builder.UseTexture(resourceData.gBuffer[i]);
                    }

                    // We need to set the gBuffer in the pass' data, otherwise the pass won't have access to it when it is executed.
                    passData.gBuffer = gBuffer;

                    // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context, 0));
                }

                renderGraph.AddCopyPass(temporaryDestinationTexture, resourceData.activeColorTexture);

            }
        }

        PRISMDeferredNightVisionRenderPass m_GBufferRenderPass;
        private Material m_Material;

        /// <inheritdoc/>
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial("PRISM/DeferredNightVisionShader");
            m_GBufferRenderPass = new PRISMDeferredNightVisionRenderPass(m_Material);
        }

        protected override void Dispose(bool disposing)
  => CoreUtils.Destroy(m_Material);

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // The gBuffers are only used in the Deferred rendering path
            if (m_Material != null)
            {
                m_GBufferRenderPass.Setup(m_Material);
                renderer.EnqueuePass(m_GBufferRenderPass);
            }
        }
    }

}