using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

	private static class Constants {
		public const float GRAVITY_MAGNITUDE = -9.8f;
	}

	private GameObject planet;
	private Rigidbody2D rigidBody;

	void Awake () {
		planet = GameObject.Find ("Planet");
		rigidBody = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 _direction = transform.position - planet.transform.position;
		Vector2 _gravity = new Vector2 (_direction.x, _direction.y).normalized * Constants.GRAVITY_MAGNITUDE;

		rigidBody.AddForce (_gravity);
		transform.rotation = Quaternion.LookRotation(Vector3.back, _direction);
	}
}
