using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject target;

	private Vector3 cameraOffset;

	void Awake () {
		cameraOffset = transform.position - target.transform.position;
	}

	void Update () {
		Quaternion _targetRotation = target.transform.rotation;
		Quaternion _rotation = Quaternion.AngleAxis (_targetRotation.eulerAngles.z, Vector3.back);
		transform.rotation = _rotation;

		transform.position = target.transform.position + (_rotation * cameraOffset);
	}
}
