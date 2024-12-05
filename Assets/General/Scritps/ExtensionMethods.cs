using UnityEngine;

/// <summary>
/// 扩展方法
/// </summary>
public static class ExtensionMethods
{
    // 对齐附近整数
    public static Vector3 Round(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
        return v;
    }

    // 对齐到指定整数
    public static Vector3 Round(this Vector3 v, float size)
    {
        return (v / size).Round() * size;
    }
}