using UnityEditor;
using UnityEngine;

// 使用屏幕分辨率作为图片的大小
public class CapturePNG : MonoBehaviour
{
    public Camera renderCamera;
    public bool captureTransparent;
    public KeyCode captureKey = KeyCode.K;

    int _width, _height;

    void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            Capture();
        }
    }

    void Capture()
    {
        if (captureTransparent)
        {
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        }

        _width = Screen.width;
        _height = Screen.height;

        var rt = RenderTexture.GetTemporary(_width, _height, 24, RenderTextureFormat.ARGB32);
        // 设置相机渲染目标
        renderCamera.targetTexture = rt;
        // 设置激活的 RT
        RenderTexture.active = rt;
        // Do a render
        renderCamera.Render();
        // 用于读取 RT
        var texture = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
        texture.Apply();

        // 保存 PNG
        byte[] bytes = texture.EncodeToPNG();
        var path = Application.dataPath + "/Capture.png";
        System.IO.File.WriteAllBytes(path, bytes);
        // 立即刷新资源 (否则不会立即出现)
        AssetDatabase.Refresh();
        Debug.Log("Save PNG to " + path);

        // 重置激活的 RT
        RenderTexture.active = null;
        Destroy(texture);
    }
}