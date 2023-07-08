using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowFrog : MonoBehaviour
{

    [SerializeField] private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = target.transform.position + Vector3.up * 10f + Vector3.right * 2.5f;

        transform.LookAt(target.transform.position, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.transform.position + Vector3.up * 10f + Vector3.right * 2.5f;

        transform.LookAt(target.transform.position, Vector3.up);
    }
}
