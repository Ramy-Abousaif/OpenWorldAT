using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    public float swaySize;
    public float swaySmooth;

    private void Update()
    {
        Vector2 mouseDelta = -new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, swaySmooth * Time.deltaTime);
        transform.localPosition += (Vector3)mouseDelta * swaySize;
    }
}
