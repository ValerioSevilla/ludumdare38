using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject target;

	private Vector3 cameraOffset;
	private Animator anim;
	private Character character;

	public void introFinished() {
		anim.enabled = false;
		character.startGame ();
	}

	void Awake () {
		cameraOffset = transform.position - target.transform.position;
		anim = GetComponent<Animator> ();
		character = GameObject.Find ("Character").GetComponent<Character> ();
	}

	void Update () {
		Quaternion _targetRotation = target.transform.rotation;
		Quaternion _rotation = Quaternion.AngleAxis (_targetRotation.eulerAngles.z, Vector3.forward);
		transform.rotation = _rotation;

		transform.position = target.transform.position + (_rotation * cameraOffset);
	}
}
