
using UnityEngine;

public class FrogController : MonoBehaviour
{
    [SerializeField] private GameObject splatFrog;

    [SerializeField] private GameObject arrow;

    [SerializeField] private float chargeMultiplier;
    [SerializeField] private float maxJumpCharge;
    [SerializeField] private float jumpAngle;

    private Vector3 jumpDirection;

    private Vector3 rotatedVector;

    private Vector3 chargeEnd;

    private bool rememberMe = false;

    private Rigidbody rb;
    private Animator fAnim;

    private bool inputHeld;
    private bool inputUp;
    private bool inputDown;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();

        fAnim = GetComponentInChildren<Animator>();

        chargeEnd = transform.position - transform.forward;
    }

    public ObjectTimeData GetTimeData() {
        return new ObjectTimeData() {
            AngularVelocity = rb.angularVelocity,
            Position = rb.position,
            Rotation = rb.rotation,
            Velocity = rb.velocity
        };
    }

    public void ApplyTimeData(ObjectTimeData timeData) {
        rb.angularVelocity = timeData.AngularVelocity;
        rb.position = timeData.Position;
        rb.rotation = timeData.Rotation;
        rb.velocity = timeData.Velocity;
    }

    public void ReceiveInputHeld(bool held) {
        inputHeld = held;
    }
    
    public void ReceiveInputDown(bool b) {
        inputDown = b;
    }

    public void ReceiveInputUp(bool up) {
        inputUp = up;
    }

    public void ReceiveTargetInput(Vector3 mouseWorldPosition)
    {
        if (inputHeld && IsGrounded())
        {
            chargeEnd = mouseWorldPosition;
        }

        if ((transform.position - chargeEnd).magnitude > 0.1f)
        {
            jumpDirection = (transform.position - chargeEnd) * chargeMultiplier;

            if(jumpDirection.sqrMagnitude > maxJumpCharge * maxJumpCharge)
            {
                jumpDirection = jumpDirection.normalized * maxJumpCharge;
            }
        }
    }

    private void Update()
    {
        if (inputUp && IsGrounded())
        {
            rememberMe = true;
        }

        inputUp = false;
        inputDown = false;
    }

    private void FixedUpdate()
    {
        if (inputHeld)
        {
            var targetRotation = new Vector3(0f, Quaternion.LookRotation(jumpDirection, Vector3.up).eulerAngles.y, 0f);

            arrow.transform.localScale = new Vector3(1f, 1f, Mathf.Lerp(arrow.transform.localScale.z, jumpDirection.magnitude*2/maxJumpCharge, 0.25f));

            transform.eulerAngles = targetRotation;
        }

        if (rememberMe)
        {
            rotatedVector = Vector3.RotateTowards(jumpDirection, Vector3.up, jumpAngle * Mathf.Deg2Rad, 0);

            rb.velocity = rotatedVector;

            if (rotatedVector != Vector3.zero)
            {
                fAnim.speed = (maxJumpCharge/4*3)/rotatedVector.magnitude;
                fAnim.SetTrigger("Airborne");
            }

            chargeEnd = transform.position - transform.forward;
            arrow.transform.localScale = new Vector3(1f, 1f, 0f);
            jumpDirection = Vector3.zero;

            rememberMe = false;
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
        if(Physics.Raycast(transform.position, Vector3.down, out var hit, 0.3f))
        {
            return true;
        }

        return false;
    }

    public void Die()
    {
        Instantiate(splatFrog, this.transform.position, splatFrog.transform.rotation);

        gameObject.SetActive(false);
        // Destroy(this.gameObject);
    }


}
