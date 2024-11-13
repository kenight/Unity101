using UnityEditor;
using UnityEngine;

// 扇形区域检测
public class A5WedgeTrigger : MonoBehaviour
{
    public Transform target;
    public float radius = 1f;
    public float height = 1f;
    public float fov = 60f;

    void OnDrawGizmos()
    {
        if (target == null) return;

        // Gizmos.matrix 默认值为单位矩阵(不做任何变换)
        // 默认情况下 Gizmos 在 world space 下绘制
        // 使用 localToWorldMatrix 可方便的在 local space 下绘制
        // Handles 也一样
        Gizmos.matrix = Handles.matrix = transform.localToWorldMatrix;

        // 思路：
        // 考虑一个斜边长为 1 的直角三角形, 在本地坐标中邻边即为正方向, 与斜边形成的夹角为 fov / 2, 这是已知角度求向量的问题
        // 余弦值就是领边 z 的长度, 再通过勾股定理得到对边 x 的长度
        var z = Mathf.Cos(fov * Mathf.Deg2Rad * 0.5f);
        var x = Mathf.Sqrt(1 - z * z);

        // 通过两个分量合成向量, 形成两个边的向量
        var vL = new Vector3(-x, 0, z) * radius;
        var vR = new Vector3(x, 0, z) * radius;

        // 竖直方向的向量
        var vT = Vector3.up * height;

        // 画线开始
        Gizmos.color = Handles.color = Contains(target) ? Color.red : Color.green;
        // 画边线
        Gizmos.DrawRay(default, vL);
        Gizmos.DrawRay(default, vR);
        Gizmos.DrawRay(vT, vL);
        Gizmos.DrawRay(vT, vR);
        // 画前端弧线
        Handles.DrawWireArc(default, Vector3.up, vL, fov, radius);
        Handles.DrawWireArc(vT, Vector3.up, vL, fov, radius);
        // 画竖线
        Gizmos.DrawRay(default, vT);
        Gizmos.DrawRay(vL, vT);
        Gizmos.DrawRay(vR, vT);
    }

    public bool Contains(Transform other)
    {
        // world to local
        var origin = transform;
        var deltaWS = other.position - origin.position;
        var x = Vector3.Dot(origin.right, deltaWS);
        var y = Vector3.Dot(origin.up, deltaWS);
        var z = Vector3.Dot(origin.forward, deltaWS);
        var deltaLS = new Vector3(x, y, z);

        // 高度检测
        if (deltaLS.y < 0 || deltaLS.y > height)
        {
            return false;
        }

        // 注意下面的检测都不考虑 y 值影响, 只考虑在水平面上做判断, 否则结果不对
        deltaLS.y = 0;

        // 距离检测
        var dist = Mathf.Sqrt(deltaLS.x * deltaLS.x + deltaLS.z * deltaLS.z);
        if (dist > radius)
        {
            return false;
        }

        // 检测角度
        // normalize
        deltaLS = deltaLS / dist;
        // 预设角度值的投影长度
        var fovProj = Mathf.Cos(fov * Mathf.Deg2Rad * 0.5f);
        // 与目标向量的 z 值对比, 小于说明超出预设的角度值
        if (deltaLS.z < fovProj)
        {
            return false;
        }

        return true;
    }
}