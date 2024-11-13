using UnityEngine;

public class A6TurretLook : MonoBehaviour
{
    public Transform target;
    public A5WedgeTrigger wedgeTrigger;
    public Transform gun;
    public float smoothness = 1f;

    Quaternion tarRotation;

    void Update()
    {
        if (wedgeTrigger.Contains(target))
        {
            var dir = target.position - gun.position;
            tarRotation = Quaternion.LookRotation(dir, transform.up);
        }

        gun.rotation = Quaternion.Slerp(gun.rotation, tarRotation, smoothness * Time.deltaTime);
    }
}