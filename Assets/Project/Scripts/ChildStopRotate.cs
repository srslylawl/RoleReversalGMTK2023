using UnityEngine;

[ExecuteAlways]
public class ChildStopRotate : MonoBehaviour
{
    [SerializeField] private Vector3 lockedRotation;

    private void LateUpdate()
    {
        // Reset the child object's rotation to the initial rotation in each frame
        transform.rotation = Quaternion.Euler(lockedRotation);
    }
}
