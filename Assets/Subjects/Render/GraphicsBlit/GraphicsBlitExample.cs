using UnityEngine;

public class GraphicsBlitExample : MonoBehaviour
{
    public Texture2D source;
    public Material material;
    public RenderTexture renderTexture;

    void Start()
    {
        // Blit 使用 shader 将原 source 的像素信息拷贝到 RT 上
        // Uses a shader to copy the pixel data from a texture into a render target.
        // 将 source 传递到 shader 的 _MainTex 属性上, 如果 shader 中没有 _MainTex 属性则不会使用 source
        Graphics.Blit(source, renderTexture, material);
    }
}