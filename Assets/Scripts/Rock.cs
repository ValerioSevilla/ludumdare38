using UnityEngine;

public class Rock : MonoBehaviour {

	private GameObject planet;
	private Rigidbody2D rigidBody;

	void Awake () {
		planet = GameObject.Find ("Planet");
		rigidBody = GetComponent<Rigidbody2D> ();
	}

	void Update () {
		Vector3 _direction = transform.position - planet.transform.position;
		Vector2 _gravity = new Vector2 (_direction.x, _direction.y).normalized * Common.Constants.GRAVITY_MAGNITUDE;

		rigidBody.AddForce (rigidBody.mass * _gravity);
	}
}
