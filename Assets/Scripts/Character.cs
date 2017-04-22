using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

	private static class Constants {
		public const float GRAVITY_MAGNITUDE = -9.8f;
		public const float WALK_FORCE = 15.0f;
		public const float JUMP_FORCE = 1.0f;
		public const float JUMP_FORCE_DEGRADATION_TIME = 0.5f;
		public const float JUMP_FORCE_DEGRADATION_TIME_INVERSE = 1.0f / JUMP_FORCE_DEGRADATION_TIME;

		public const float MAX_LINEAR_VELOCITY = 5.0f;
		public const float MAX_LINEAR_VELOCITY_INVERSE = 1.0f / MAX_LINEAR_VELOCITY;
	}

	private GameObject planet;
	private Rigidbody2D rigidBody;

	private HashSet<GameObject> onTheGround;
	private Coroutine jumpForceCoroutine;
	private bool tryingToJump;

	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Ground") {
			{//if (coll.relativeVelocity.y > 0.0f && coll.relativeVelocity.y > Mathf.Abs(coll.relativeVelocity.x)) {
				if (jumpForceCoroutine != null) {
					StopCoroutine (jumpForceCoroutine);
					jumpForceCoroutine = null;
				}

				onTheGround.Add (coll.gameObject);
			}
		}

	}

	void OnCollisionExit2D(Collision2D coll) {
		if (coll.gameObject.tag == "Ground") {
			if (onTheGround.Contains (coll.gameObject)) {
				onTheGround.Remove (coll.gameObject);
			}
		}
	}

	void Awake () {
		planet = GameObject.Find ("Planet");
		rigidBody = GetComponent<Rigidbody2D> ();

		onTheGround = new HashSet<GameObject> ();
		jumpForceCoroutine = null;
		tryingToJump = false;
	}

	private IEnumerator jumpForce (){
		float _jumpCommand;
		float _elapsedTime = 0.0f;

		do {
			_elapsedTime += Time.deltaTime;
			_jumpCommand = Input.GetAxis ("Jump");
			Vector3 _direction = (transform.position - planet.transform.position).normalized;

			if(_elapsedTime > Constants.JUMP_FORCE_DEGRADATION_TIME)
				break;
			
			float _jumpForceFactor = (Constants.JUMP_FORCE_DEGRADATION_TIME - _elapsedTime) * Constants.JUMP_FORCE_DEGRADATION_TIME_INVERSE;
			float _jumpForce = Constants.JUMP_FORCE * _jumpForceFactor;

			rigidBody.AddForce (_direction * (_jumpCommand * _jumpForce * rigidBody.mass), ForceMode2D.Impulse);

			yield return null;
		} while (_jumpCommand > 0.0f);

		jumpForceCoroutine = null;
	}

	private Vector2 getWalkForce(Vector2 _walkDirection) {
		float _currentVelocityMagnitude = _walkDirection.magnitude;
		Vector2 _currentWalkVector = _walkDirection
			* (Vector2.Dot (rigidBody.velocity, _walkDirection) / (_currentVelocityMagnitude * _currentVelocityMagnitude));
		
		// Project the current velocity onto the walk direction vector
		float _currentWalkMagnitude = _currentWalkVector.magnitude;
		float _walkForceFactor = (Constants.MAX_LINEAR_VELOCITY - _currentWalkMagnitude) * Constants.MAX_LINEAR_VELOCITY_INVERSE;
		_walkForceFactor = Mathf.Min (_walkForceFactor, Constants.WALK_FORCE);
		_walkForceFactor = Mathf.Max (_walkForceFactor, -Constants.WALK_FORCE);
		float _walkForce = Constants.WALK_FORCE * _walkForceFactor;

		return _walkDirection * rigidBody.mass * Input.GetAxis ("Horizontal") * _walkForce;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 _direction = transform.position - planet.transform.position;
		Vector2 _gravity = new Vector2 (_direction.x, _direction.y).normalized * Constants.GRAVITY_MAGNITUDE;

		rigidBody.AddForce (rigidBody.mass * _gravity);
		transform.rotation = Quaternion.LookRotation(Vector3.back, _direction);

		Vector3 _walkVector = Vector3.Cross (Vector3.back, _direction).normalized;
		Vector2 _walkDirection = new Vector2 (_walkVector.x, _walkVector.y);

		rigidBody.AddForce (getWalkForce (_walkDirection));

		float _jumpCommand = Input.GetAxis ("Jump");
		if (_jumpCommand > 0.0f) {
			if (!tryingToJump) {
				if (onTheGround.Count > 0) {
					if (jumpForceCoroutine == null)
						jumpForceCoroutine = StartCoroutine (jumpForce ());
				}
				tryingToJump = true;
			}
		} else
			tryingToJump = false;
	}
}
