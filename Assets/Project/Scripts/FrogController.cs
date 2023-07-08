using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.LightAnchor;

public class FrogController : MonoBehaviour
{
    [SerializeField] private float chargeMultiplier;
    [SerializeField] private float maxJumpCharge;
    [SerializeField] private float jumpAngle;

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

        if ((chargeStart - chargeEnd).magnitude > 0.1f)
        {
            jumpDirection = (chargeStart - chargeEnd) * chargeMultiplier;

            if(jumpDirection.sqrMagnitude > maxJumpCharge * maxJumpCharge)
            {
                jumpDirection = jumpDirection.normalized * maxJumpCharge;
            }
        }
    }

    private void Update()
    {
        if (jumpDirection != Vector3.zero)
        {
            var targetRotation = Quaternion.LookRotation(jumpDirection, Vector3.up);

            transform.rotation = targetRotation;
        }

        if (Input.GetMouseButtonUp(0) && IsGrounded())
        {
            rotatedVector = Vector3.RotateTowards(jumpDirection, Vector3.up, jumpAngle * Mathf.Deg2Rad, 0);
            
            rb.velocity = rotatedVector;

            chargeStart = Vector3.zero;
            chargeEnd = Vector3.zero;
            jumpDirection = Vector3.zero;
        }
    }

    private void OnDrawGizmos()
    {
        var pos = transform.position;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(pos, pos + rotatedVector.normalized);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pos, 0.02f);
    }

    private bool IsGrounded()
    {
        if(Physics.CheckSphere(transform.position, 0.05f))
            return true;

        return false;
    }
}
