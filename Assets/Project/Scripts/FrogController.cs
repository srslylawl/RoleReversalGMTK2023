using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogController : MonoBehaviour
{
    [SerializeField] private float maxJumpCharge;

    private float jumpCharge;

    private Vector3 jumpDirection;

    private Vector3 rotatedVector;

    private Vector3 chargeStart;
    private Vector3 chargeEnd;

    private Rigidbody rb;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();

        chargeStart = Vector3.zero;
        chargeEnd = Vector3.zero + Vector3.back;
    }

    public void ReceiveTargetInput(Vector3 mouseWorldPosition)
    {
        if (Input.GetMouseButtonDown(0))
        {
            chargeStart = mouseWorldPosition;
        }

        if (Input.GetMouseButton(0))
        {
            chargeEnd = mouseWorldPosition;
        }

        if (chargeStart - chargeEnd != Vector3.zero)
        {
            jumpDirection = chargeStart - chargeEnd;

            if(jumpDirection.sqrMagnitude > maxJumpCharge * maxJumpCharge)
            {
                jumpDirection = jumpDirection.normalized * maxJumpCharge;
            }

            rotatedVector = Vector3.RotateTowards(jumpDirection, Vector3.up, 45 * Mathf.Deg2Rad, 0);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            rb.velocity = rotatedVector;
        }

        var targetRotation = Quaternion.LookRotation(jumpDirection, Vector3.up);

        transform.rotation = targetRotation;
    }

    private void OnDrawGizmos()
    {
        var pos = transform.position;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + transform.forward);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(pos, pos + rotatedVector.normalized);
    }
}
