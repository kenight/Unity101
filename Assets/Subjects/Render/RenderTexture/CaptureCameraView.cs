using UnityEngine;

// 使用：
// 指定一个摄像机就行了, RT 会动态创建并释放 (区别于 RenderTextureToPNG 使用创建好的 RT)
public class CaptureCameraView : MonoBehaviour
{
    public Camera renderCamera;
    public int width = 1024;
    public int height = 1024;
    public bool transparent;

    CameraClearFlags _cameraClearFlags;
    Color _cameraBackgroundColor;

    void Awake()
    {
        _cameraClearFlags = renderCamera.clearFlags;
        _cameraBackgroundColor = renderCamera.backgroundColor;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SaveCaptured();
        }
    }

    void SaveCaptured()
    {
        if (transparent)
        {
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        }

        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        renderCamera.targetTexture = rt;
        RenderTexture.active = rt;
        renderCamera.Render();
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        RenderTexture.active = null; // Reset
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/Subjects/Render/RenderTexture/CaptureCameraView.png", bytes);

        // 恢复并清理资源
        if (transparent)
        {
            renderCamera.clearFlags = _cameraClearFlags;
            renderCamera.backgroundColor = _cameraBackgroundColor;
        }

        renderCamera.targetTexture = null;
        Destroy(rt);
        Destroy(texture);
    }
}