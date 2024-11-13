using System;
using UnityEditor;
using UnityEngine;

public class VectorBase : MonoBehaviour
{
    public Transform vectorA;
    public Transform vectorB;

    void OnDrawGizmos()
    {
        if (vectorA == null || vectorB == null) return;

        var vA = vectorA.position;
        var vB = vectorB.position;

        Handles.DrawDottedLine(default, vA, 5f);
        Handles.DrawDottedLine(default, vB, 5f);


        // Normalize
        var lenA = Mathf.Sqrt(vA.x * vA.x + vA.y * vA.y + vA.z * vA.z);
        var lenB = Mathf.Sqrt(vB.x * vB.x + vB.y * vB.y + vB.z * vB.z);

        var norA = vA / lenA;
        var norB = vB / lenB;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(norA, 0.05f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(norB, 0.05f);

        // Scale projection
        // 当其中一个向量为单位向量时, 得到投影长度
        // 当都为单位向量时, 值大小可判断相似程度或夹角大小
        var scaleProj = Vector3.Dot(vA, norB);

        // Vector projection
        // 投影长度 * 单位向量
        var vectorProj = scaleProj * norB;

        // 符号可判断正反或前后
        Gizmos.color = scaleProj > 0 ? Color.yellow : Color.red;
        Gizmos.DrawLine(default, vectorProj);
    }
}