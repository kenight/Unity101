using UnityEngine;

public class A3Transformation : MonoBehaviour
{
    public Vector3 localA;
    public Vector3 worldToLocal;

    void OnDrawGizmos()
    {
        var world = LocalToWorld(localA);
        Gizmos.DrawSphere(world, 0.1f);
        worldToLocal = WorldToLocal(world);
    }

    Vector3 LocalToWorld(Vector3 local)
    {
        // 思路：
        // 将 local 带来的相对偏移量缩放到父级坐标系各轴
        // 再叠加到父级坐标系原点所在位置上即可
        var origin = transform;
        var selfWs = origin.position;
        selfWs += origin.right * local.x;
        selfWs += origin.up * local.y;
        selfWs += origin.forward * local.z;
        return selfWs;
    }

    Vector3 WorldToLocal(Vector3 world)
    {
        // 思路：
        // 和 local to world 相反, ltw 是知道这个 local 而 wtl 是要求这个 local
        // 首先要知道父级原点与 world 之间的偏移量, 通过减法获得这个向量 delta
        // 再将这个偏移量投影到对应的轴上即可(必须要投影, 因为要考虑到旋转问题)
        var origin = transform;
        var delta = world - origin.position;
        var x = Vector3.Dot(origin.right, delta);
        var y = Vector3.Dot(origin.up, delta);
        var z = Vector3.Dot(origin.forward, delta);
        return new Vector3(x, y, z);
    }

    // 使用 unity 提供的内置函数或矩阵
    Vector3 LocalToWorldUnity(Vector3 local)
    {
        // 最简单的方法
        // var world = transform.TransformPoint(local);

        // 使用矩阵辅助函数
        // Transforms a position by this matrix
        // 普通3D变换使用 MultiplyPoint3x4 更快, MultiplyPoint 可处理投影变换
        // 另外 MultiplyVector transforms a direction by this matrix
        // var world = transform.localToWorldMatrix.MultiplyPoint3x4(local);

        // 直接使用矩阵的方式, 转换 point 或 vector 时, 需要自己构造 vector4 最后一位元素
        // var world = transform.localToWorldMatrix * new Vector4(local.x, local.y, local.z, 1);

        // 手动构建矩阵的方式
        // 按列构建新的矩阵, 第一列为 x 轴的方向与缩放信息, 以此类推, 第四列为坐标原点在世界坐标系下的位置信息
        // var mtx = new Matrix4x4(column0, column1...);

        // 使用 TRS Creates a translation, rotation and scaling matrix
        var matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
        var world = matrix * new Vector4(local.x, local.y, local.z, 1);

        return world;
    }
}