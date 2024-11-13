using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// Create a texture in the render graph system in URP
/// https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph-create-a-texture.html
/// </summary>
public class CreateYellowTextureFeature : ScriptableRendererFeature
{
    CreateYellowTexturePass _customPass;

    /// <inheritdoc/>
    public override void Create()
    {
        _customPass = new CreateYellowTexturePass();

        // Configures where the render pass should be injected.
        _customPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_customPass);
    }

    class CreateYellowTexturePass : ScriptableRenderPass
    {
        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class PassData
        {
            TextureHandle _cameraColorTexture;
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            context.cmd.ClearRenderTarget(true, true, Color.yellow);
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CreateYellowTexture", out var passData))
            {
                // 创建贴图描述信息
                RenderTextureDescriptor texDesc = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
                // 创建贴图
                TextureHandle texture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, texDesc, "My Texture", false);

                // 将新创建的贴图作为 RT
                builder.SetRenderAttachment(texture, 0);

                // 由于新创建的贴图未使用,阻止被移除
                builder.AllowPassCulling(false);

                // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }
}