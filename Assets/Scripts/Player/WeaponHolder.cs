using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    float time = 0.2f;
    public bool holding = false;
    public float throwForce;
    public float throwExtraForce;
    public float rotationForce;

    public int weaponGFXLayer;
    public GameObject weaponGFX;
    public Collider[] gfxColliders;

    private bool _held;
    private Rigidbody rb;

    private void Start()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    public void PickUp(Transform weaponHolder)
    {
        if (_held) return;
        Destroy(rb);
        StartCoroutine(PickingUp(weaponHolder));
        foreach (var col in gfxColliders)
        {
            col.enabled = false;
        }
        _held = true;
    }

    public void Drop(Transform playerCamera)
    {
        if(!holding)
        {
            if (!_held) return;
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            var forward = playerCamera.forward;
            forward.y = 0;
            rb.velocity = forward * throwForce;
            rb.velocity += Vector3.up * throwExtraForce;
            rb.angularVelocity = Random.onUnitSphere * rotationForce;
            foreach (var col in gfxColliders)
            {
                col.enabled = true;
            }
            //_ammoText.text = " ";
            transform.parent = null;
            _held = false;
        }
    }

    IEnumerator PickingUp(Transform weaponHolder)
    {
        float elapsedTime = 0.0f;
        transform.parent = weaponHolder;
        Vector3 startingPosition = transform.localPosition;
        Quaternion startingRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0,0,0);
        while (elapsedTime < time)
        {
            holding = true;
            elapsedTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startingPosition, Vector3.zero, (elapsedTime / time));
            // Rotations
            transform.localRotation = Quaternion.Slerp(startingRotation, targetRotation, (elapsedTime / time));
            yield return new WaitForEndOfFrame();
        }
        holding = false;
    }
}
