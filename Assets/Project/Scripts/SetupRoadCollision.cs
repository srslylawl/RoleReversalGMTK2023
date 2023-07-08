using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SetupRoadCollision : MonoBehaviour
{

    [SerializeField] GameObject roadColChild;

    private Quaternion startRot;

    // Start is called before the first frame update
    void Awake()
    {
        startRot = transform.rotation;
        transform.rotation = Quaternion.identity;

        EdgeCollider2D edgeCol2D = GetComponent<EdgeCollider2D>();

        Vector2[] points = edgeCol2D.points;

        foreach(Vector2 pos in points)
        {
            var col = Instantiate(roadColChild, transform.position, Quaternion.identity);
            col.transform.parent = this.transform;

            col.transform.position += (Vector3)pos;
            col.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        transform.rotation = startRot;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
