using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	private void OnDrawGizmos() {
		var tf = transform;
		var pos = tf.position;

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(pos, pos + tf.forward * 2f);
		
		Gizmos.color = Color.black;
		Gizmos.DrawWireSphere(pos, 1f);
	}
}
