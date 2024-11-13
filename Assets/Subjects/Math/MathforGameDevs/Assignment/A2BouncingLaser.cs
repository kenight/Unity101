using UnityEngine;

public class A2BouncingLaser : MonoBehaviour
{
    public int maxLaser = 10;

    void OnDrawGizmos()
    {
        var origin = transform.position;
        var laserDir = transform.right;

        // 初始射线
        var ray = new Ray(origin, laserDir);

        for (int i = 0; i < maxLaser; i++)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(ray.origin, hitInfo.point);
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(hitInfo.point, hitInfo.normal);
                var reflectVector = Reflect(ray.direction, hitInfo.normal);
                // 更新射线
                ray = new Ray(hitInfo.point, reflectVector);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawRay(ray.origin, ray.direction * 100f);
                break;
            }
        }
    }

    // 获取反射向量(图：反射向量计算)
    Vector3 Reflect(Vector3 dir, Vector3 normal)
    {
        // 入射方向投影到法线的投影长度
        // 注意 len 肯定为负值(入射向量与法线方向相反)
        var len = Vector3.Dot(dir, normal);
        // 得到在法线上的投影向量
        var p = len * normal;
        // 获得反射向量
        return dir + 2 * -p;
    }
}