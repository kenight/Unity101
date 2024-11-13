using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class A7AdvanceTrigger : MonoBehaviour
{
    public TriggerMode mode = TriggerMode.Wedge;
    public Transform target;
    public float radiusInner = 0.5f;
    public float radiusOuter = 1f;
    public float height = 1f;
    public float fov = 60f;

    void OnDrawGizmos()
    {
        // 注解参考 A5WedgeTrigger.cs
        Gizmos.matrix = Handles.matrix = transform.localToWorldMatrix;

        switch (mode)
        {
            case TriggerMode.Wedge:
                DrawWedge();
                break;
            case TriggerMode.Cone:
                DrawCone();
                break;
            case TriggerMode.Spherical:
                DrawSpherical();
                break;
        }
    }

    void DrawWedge()
    {
        // 思路：
        // 考虑一个斜边长为 1 的直角三角形, 在本地坐标中邻边即为正方向, 与斜边形成的夹角为 fov / 2, 这是已知角度求向量的问题
        // 余弦值就是领边 z 的长度
        var z = Mathf.Cos(fov * Mathf.Deg2Rad * .5f);
        // 再通过勾股定理得到对边 x 的长度
        var x = Mathf.Sqrt(1 - z * z);

        var vL = new Vector3(-x, 0, z);
        var vR = new Vector3(x, 0, z);
        var vT = Vector3.up * height;

        var vInnerL = vL * radiusInner;
        var vInnerR = vR * radiusInner;
        var vOuterL = vL * radiusOuter;
        var vOuterR = vR * radiusOuter;

        // 画线开始
        Gizmos.color = Handles.color = ContainsWedge(target) ? Color.red : Color.green;

        // 画边线
        Gizmos.DrawLine(vInnerL, vOuterL);
        Gizmos.DrawLine(vInnerR, vOuterR);
        Gizmos.DrawLine(vT + vInnerL, vT + vOuterL);
        Gizmos.DrawLine(vT + vInnerR, vT + vOuterR);
        // 画弧线
        Handles.DrawWireArc(default, Vector3.up, vInnerL, fov, radiusInner);
        Handles.DrawWireArc(vT, Vector3.up, vInnerL, fov, radiusInner);
        Handles.DrawWireArc(default, Vector3.up, vOuterL, fov, radiusOuter);
        Handles.DrawWireArc(vT, Vector3.up, vOuterL, fov, radiusOuter);
        // 画竖线
        Gizmos.DrawRay(vInnerL, vT);
        Gizmos.DrawRay(vInnerR, vT);
        Gizmos.DrawRay(vOuterL, vT);
        Gizmos.DrawRay(vOuterR, vT);
    }

    void DrawCone()
    {
        var z = Mathf.Cos(fov * Mathf.Deg2Rad * .5f);
        var x = Mathf.Sqrt(1 - z * z);

        var vL = new Vector3(-x, 0, z);
        var vR = new Vector3(x, 0, z);

        var vInnerL = vL * radiusInner;
        var vInnerR = vR * radiusInner;
        var vOuterL = vL * radiusOuter;
        var vOuterR = vR * radiusOuter;

        // 画线开始
        Gizmos.color = Handles.color = ContainsCone(target) ? Color.red : Color.green;

        // 画水平扇形
        DrawFlatWedge();
        // 画垂直扇形
        // 暂存原矩阵
        var m0 = Gizmos.matrix;
        // 做一个绕 z 轴旋转 90 度的变换矩阵
        Gizmos.matrix = Handles.matrix = m0 * Matrix4x4.TRS(default, Quaternion.Euler(0, 0, 90), Vector3.one);
        // 用变换后的矩阵画垂直的扇形
        DrawFlatWedge();
        // 画完后重置矩阵
        Gizmos.matrix = Handles.matrix = m0;

        // 画前后圆盘
        DrawDist(radiusInner);
        DrawDist(radiusOuter);

        // 画扇形
        void DrawFlatWedge()
        {
            Gizmos.DrawLine(vInnerL, vOuterL);
            Gizmos.DrawLine(vInnerR, vOuterR);

            Handles.DrawWireArc(default, Vector3.up, vInnerL, fov, radiusInner);
            Handles.DrawWireArc(default, Vector3.up, vOuterL, fov, radiusOuter);
        }

        // 画圆盘
        void DrawDist(float radius)
        {
            // 距离
            var dist = z * radius;
            // 半径
            var r = x * radius;
            Handles.DrawWireDisc(new Vector3(0, 0, dist), Vector3.forward, r);
        }
    }

    void DrawSpherical()
    {
        // 画线开始
        Gizmos.color = Handles.color = ContainsSpherical(target) ? Color.red : Color.green;
        Gizmos.DrawWireSphere(default, radiusInner);
        Gizmos.DrawWireSphere(default, radiusOuter);
    }

    bool ContainsWedge(Transform other)
    {
        var dt = other.position - transform.position;
        dt = transform.InverseTransformVector(dt);

        // 高度 check
        if (dt.y < 0 || dt.y > height)
        {
            return false;
        }

        dt.y = 0;
        // 距离 check
        var dist = dt.magnitude;
        if (dist < radiusInner || dist > radiusOuter)
        {
            return false;
        }

        // 角度 check
        // 预设角度值的投影长度
        var fovProj = Mathf.Cos(fov * Mathf.Deg2Rad * .5f);
        dt.Normalize();
        if (dt.z < fovProj)
        {
            return false;
        }

        return true;
    }

    bool ContainsSpherical(Transform other)
    {
        var dt = other.position - transform.position;
        var sqrtDist = dt.x * dt.x + dt.y * dt.y + dt.z * dt.z;
        return sqrtDist < radiusOuter * radiusOuter && sqrtDist > radiusInner * radiusInner;
    }

    bool ContainsCone(Transform other)
    {
        var dt = other.position - transform.position;
        // 检测目标与 trigger 的 forward 方向形成的夹角是否在指定角度范围内
        var proj = Vector3.Dot(transform.forward, dt.normalized);
        // 预设角度值的投影长度
        var fovProj = Mathf.Cos(fov * Mathf.Deg2Rad * .5f);
        if (proj < fovProj)
        {
            return false;
        }

        // 检测距离
        var sqrtDist = dt.x * dt.x + dt.y * dt.y + dt.z * dt.z;
        if (sqrtDist < radiusInner * radiusInner || sqrtDist > radiusOuter * radiusOuter)
        {
            return false;
        }

        return true;
    }

    public enum TriggerMode
    {
        Wedge,
        Cone,
        Spherical
    }
}