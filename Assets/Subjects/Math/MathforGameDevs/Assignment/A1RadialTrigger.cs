using System;
using UnityEditor;
using UnityEngine;

public class A1RadialTrigger : MonoBehaviour
{
    public Transform target;
    public float radius = 1f;

    void OnDrawGizmos()
    {
        if (target == null) return;

        var center = transform.position;
        var targetPos = target.position;
        Handles.DrawDottedLine(center, targetPos, 5f);
        var dt = targetPos - center;
        // var distance = Mathf.Sqrt(dt.x * dt.x + dt.y * dt.y + dt.z * dt.z);
        // Gizmos.color = distance > radius ? Color.white : Color.red;
        // 不开根号判断
        var sqrtDist = dt.x * dt.x + dt.y * dt.y + dt.z * dt.z;
        Gizmos.color = sqrtDist > radius * radius ? Color.white : Color.red;
        Gizmos.DrawWireSphere(center, radius);
    }
}