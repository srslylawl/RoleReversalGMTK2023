
using System;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Random = System.Random;

public class FrogController : MonoBehaviour, IController
{
    [SerializeField] private GameObject splatFrog;
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject killArrow;

    private GameObject corpseRev;

    [SerializeField] private float jumpStrength;
    [SerializeField] private float jumpAngle;
    [SerializeField] private float maxChargeTime;

    private Vector3 jumpDirection;

    private Vector3 rotatedVector;

    private Vector3 chargeEnd;
    public bool charging;
    private float chargeTime;

    public bool rememberMe = false;

    private Rigidbody rb;
    private Animator fAnim;

    private bool inputHeld;
    private bool inputUp;
    private bool inputDown;

    private bool isHighPitched;
    
    private bool IsBeingReplayed;
    public FrogTimeData ownDataRef;

    public Action OnGettingRoadKilledCallBack;


    public void SetKillArrowActive(bool active) {
        killArrow.SetActive(active);
    }

    public void OnEnable()
    {
        if(corpseRev != null)
        {
            Destroy(corpseRev);
            corpseRev = null;
        }
    }

    public void Start()
    {
        rb = GetComponent<Rigidbody>();

        fAnim = GetComponentInChildren<Animator>();

        chargeEnd = transform.position - transform.forward;

        if (UnityEngine.Random.Range(0, 2) == 0) {
            isHighPitched = true;
        }
    }

    public ObjectTimeData GetTimeData() {
        return new ObjectTimeData() {
            AngularVelocity = rb.angularVelocity,
            Position = rb.position,
            Rotation = rb.rotation,
            Velocity = rb.velocity,
            HasInput = inputHeld
        };
    }
    
    public void SetMode(bool beingReplayed, FrogTimeData _ownDataRef) {
        // rb.isKinematic = beingReplayed;
        IsBeingReplayed = beingReplayed;
        ownDataRef = _ownDataRef;
        ownDataRef.TimeDataMode = beingReplayed ? TimeDataMode.Replay : TimeDataMode.Record;
    }


    public void ApplyTimeData(ObjectTimeData timeData) {
        rb.angularVelocity = timeData.AngularVelocity;
        rb.position = timeData.Position;
        rb.rotation = timeData.Rotation;
        rb.velocity = timeData.Velocity;
        inputHeld = timeData.HasInput;
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
            jumpDirection = (transform.position - chargeEnd).normalized;
        }
    }

    private void Update() {
        if (IsBeingReplayed) return;
        
        if(inputDown && IsGrounded())
        {
            charging = true;
        }

        if (inputUp && charging && IsGrounded())
        {
            rememberMe = true;
        }

        inputUp = false;
        inputDown = false;
    }

    private bool hadInputLastFrame;

    private void FixedUpdate() {
        if (IsBeingReplayed) {
            arrow.transform.localScale = new Vector3(1f, 1f, 0f);

            if (!inputHeld && hadInputLastFrame) {
                var sound = isHighPitched ? "frogHopHigh" : "frogHopLow";
                AudioManager.PlaySoundAtPosition(transform.position, sound);
            }
            
            
            hadInputLastFrame = inputHeld;
            return;
        }
        

        if (charging)
        {
            var targetRotation = new Vector3(0f, Quaternion.LookRotation(jumpDirection, Vector3.up).eulerAngles.y, 0f);

            chargeTime = Mathf.Clamp(chargeTime + Time.fixedDeltaTime, 0f, maxChargeTime);

            arrow.transform.localScale = new Vector3(1f, 1f, (chargeTime / maxChargeTime) * 2f);

            transform.eulerAngles = targetRotation;
        }

        if (rememberMe && charging)
        {
            rotatedVector = Vector3.RotateTowards(jumpDirection, Vector3.up, jumpAngle * Mathf.Deg2Rad, 0);

            rb.velocity = rotatedVector.normalized * jumpStrength * (chargeTime / maxChargeTime);

            if (rotatedVector != Vector3.zero)
            {
                fAnim.speed = ((maxChargeTime/chargeTime)*jumpStrength*3)/rotatedVector.magnitude;
                fAnim.SetTrigger("Airborne");
            }

            chargeEnd = transform.position - transform.forward;
            arrow.transform.localScale = new Vector3(1f, 1f, 0f);
            jumpDirection = Vector3.zero;
            chargeTime = 0f;

            charging = false;
            rememberMe = false;
        }
        
        if (!inputHeld && hadInputLastFrame) {
            var sound = isHighPitched ? "frogHopHigh" : "frogHopLow";
            AudioManager.PlaySoundAtPosition(transform.position, sound);
        }
        hadInputLastFrame = inputHeld;
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
        if(Physics.Raycast(transform.position, Vector3.down, out var hit, 0.1f))
        {
            return true;
        }

        return false;
    }

    public void Die() {
        var pos = transform.position;
        pos.y = 0.05f;
        corpseRev = Instantiate(splatFrog, pos, splatFrog.transform.rotation);
        AudioManager.PlaySoundAtPosition(pos, "frogHit");
        gameObject.SetActive(false);
        
        OnGettingRoadKilledCallBack?.Invoke();
    }
}
