using UnityEngine;

public class A4TurretAlignment : MonoBehaviour
{
    public Transform tank;
    public bool drawGizmos = true;

    void OnDrawGizmos()
    {
        if (tank == null) return;

        var ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var upwards = hit.normal;
            // 使用叉乘得到垂直于 upwards 的 right 向量
            var right = Vector3.Cross(upwards, ray.direction);
            // 同理得到 forward 向量
            var forward = Vector3.Cross(right, upwards);
            
            tank.position = hit.point;
            tank.rotation = Quaternion.LookRotation(forward, upwards);
            
            if (drawGizmos)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(hit.point, upwards);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(hit.point, right);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hit.point, forward);
            }
        }
    }
}