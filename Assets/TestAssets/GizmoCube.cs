using UnityEngine;
using System.Collections;

public class GizmoCube : MonoBehaviour {

	public Color color = Color.red;

	void OnDrawGizmos ()
	{
		Gizmos.color = color;
		Gizmos.DrawCube (transform.position, Vector3.one);
	}
}
