using UnityEngine;

public class MoonPosition : MonoBehaviour
{
    public Light moonLight;
    public float distance = 3000f;

    void LateUpdate()
    {
        transform.position =
            -moonLight.transform.forward * distance;
    }
}