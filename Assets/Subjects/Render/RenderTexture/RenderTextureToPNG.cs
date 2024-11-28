using System.Collections;
using System.IO;
using UnityEngine;

// 使用：
// 新建一个 renderTexture 赋值给 RTCamera 进行渲染
public class RenderTextureToPNG : MonoBehaviour
{
    public RenderTexture renderTexture;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(SaveRenderTextureToPNG());
        }
    }

    IEnumerator SaveRenderTextureToPNG()
    {
        yield return new WaitForEndOfFrame();

        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false, true);

        // All rendering goes into the active RenderTexture.
        // If the active RenderTexture is null everything is rendered in the main window.

        // 记住当前 RT 执行完成后恢复
        RenderTexture currentRenderTexture = RenderTexture.active;
        // 临时切换到指定的 RT 让 ReadPixels 进行复制
        RenderTexture.active = renderTexture;

        // 复制当前 RT 到 Texture2D 中 (与 Graphics.Blit 正好相反,它是复制 Texture2D 到 RT)
        // 第一个参数定义了包含 RT 的矩形框
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        // 必须调用该方法才起效
        texture.Apply();

        // 复制完成后恢复 RT 到之前
        RenderTexture.active = currentRenderTexture;

        // 编码为 PNG 格式
        byte[] bytes = texture.EncodeToPNG();

        string filePath = Application.dataPath + "/Subjects/Render/RenderTexture/RenderTextureToPNG.png";
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Saved PNG to: " + filePath);

        // 清理不用的资源
        Destroy(texture);
    }
}