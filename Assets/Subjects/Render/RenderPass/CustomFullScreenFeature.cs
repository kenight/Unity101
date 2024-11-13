using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

/// <summary>
/// https://docs.unity3d.com/6000.0/Documentation/Manual/urp/renderer-features/how-to-fullscreen-blit.html
/// </summary>
public class CustomFullScreenFeature : ScriptableRendererFeature
{
    public Material passMaterial;

    CustomPass _mainCameraPass;

    public override void Create()
    {
        _mainCameraPass = new CustomPass(passMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        if (passMaterial == null)
            return;

        renderer.EnqueuePass(_mainCameraPass);
    }

    class CustomPass : ScriptableRenderPass
    {
        Material _passMaterial;

        public CustomPass(Material passMaterial)
        {
            _passMaterial = passMaterial;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.clearBuffer = false;
            destinationDesc.depthBufferBits = 0;
            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            RenderGraphUtils.BlitMaterialParameters param = new(source, destination, _passMaterial, 0);
            renderGraph.AddBlitPass(param);

            resourceData.cameraColor = destination;
        }
    }
}