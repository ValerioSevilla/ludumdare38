using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

	private static class Constants {
		public const float WALK_FORCE = 20.0f;
		public const float JUMP_FORCE = 0.875f;
		public const float JUMP_FORCE_DEGRADATION_TIME = 0.5f;
		public const float JUMP_FORCE_DEGRADATION_TIME_INVERSE = 1.0f / JUMP_FORCE_DEGRADATION_TIME;

		public const float MAX_LINEAR_VELOCITY = 10.0f;
		public const float MAX_LINEAR_VELOCITY_INVERSE = 1.0f / MAX_LINEAR_VELOCITY;

		public const float NOT_WALKING_THRESHOLD = 0.01f;
		public const float LEAVING_FLOOR_WAIT_TIME = 0.025f;
		public const float NOT_SLIPPING_WAIT_TIME = 0.075f;

		public const float O2_PER_SECOND = 1.5f;
		public const float LIFE_WITH_NO_O2_PER_SECOND = 7.0f;

		public const float FALL_DAMAGE_THRESHOLD = 15.0f;
		public const float FALL_DAMAGE = 25.0f;

		public const float MAX_SLOPE_VERTICAL_ANGLE_TO_WALK = 45.0f;
	}

	private static int ONTHEFLOOR_BOOL_HASH = Animator.StringToHash ("OnTheFloor");
	private static int WALKING_BOOL_HASH = Animator.StringToHash ("Walking");
	private static int JUMPING_BOOL_HASH = Animator.StringToHash ("Jumping");
	private static int SLIPPING_BOOL_HASH = Animator.StringToHash ("Slipping");
	private static int DIE_TRIGGER_HASH = Animator.StringToHash ("Die");
	private static int LOOKINGLEFT_TRIGGER_HASH = Animator.StringToHash ("LookingLeft");

	private static int SPACEMAN_SLIPPING_ANIM_HASH = Animator.StringToHash ("Base Layer.SpacemanSlipping");

	private GameObject planet;
	private OxygenGauge oxygenGauge;
	private LifeGauge lifeGauge;

	private Rigidbody2D rigidBody;
	private Animator anim;
	private GameObject sprite;

	private Vector2 slopeNormal;
	private Vector2 upDirection;
	private bool onTheGround;
	private Coroutine jumpForceCoroutine;
	private Coroutine notOnTheFloorCoroutine;
	private Coroutine notSlippingCoroutine;
	private bool tryingToJump;
	private bool gameStarted;

	private float life;
	public float Life {
		get { return life; }
		private set { life = value; }
	}

	private float oxygen;
	public float Oxygen {
		get { return oxygen; }
		private set { oxygen = value; }
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.relativeVelocity.magnitude >= Constants.FALL_DAMAGE_THRESHOLD) {
			damage (Constants.FALL_DAMAGE);
		}

		if (coll.gameObject.tag == "Deadly") {
			die ();
		} else if (coll.gameObject.tag == "Spaceship") {
			win ();
		}
	}

	void OnCollisionStay2D(Collision2D coll) {
		if (coll.gameObject.tag == "Rock")
			return;
		
		foreach (var _contactPoint in coll.contacts) {
			slopeNormal += _contactPoint.normal;
		}
	}

	void OnTriggerStay2D(Collider2D coll) {
		if (coll.gameObject.tag != "Deadly"
			&& coll.gameObject.tag != "Spaceship"
			&& coll.gameObject.tag != "Hill") {
			onTheGround = true;
		}
	}

	private void checkGroundStatus() {
		if (onTheGround) {
			if (!anim.GetBool (ONTHEFLOOR_BOOL_HASH)) {
				if (jumpForceCoroutine != null) {
					StopCoroutine (jumpForceCoroutine);
					jumpForceCoroutine = null;

					anim.SetBool (JUMPING_BOOL_HASH, false);
				}

				anim.SetBool (ONTHEFLOOR_BOOL_HASH, true);
			}

			if (notOnTheFloorCoroutine != null) {
				StopCoroutine (notOnTheFloorCoroutine);
				notOnTheFloorCoroutine = null;
			}

			onTheGround = false;
		} else {
			slopeNormal = Vector2.zero;
			if (notOnTheFloorCoroutine == null)
				notOnTheFloorCoroutine = StartCoroutine (leavingFloorWait ());
		}
	}

	private void win () {
		GameObject.Find ("Canvas/Fade").GetComponent<FadeScript> ().fadeOut ("Win");
	}

	private void die () {
		Life = 0.0f;

		lifeGauge.setRemainingLife (Life);

		anim.SetTrigger (DIE_TRIGGER_HASH);
	}

	public void commitDeath () {
		GameObject.Find ("Canvas/Fade").GetComponent<FadeScript> ().fadeOut ("Main");
	}

	public void startGame () {
		gameStarted = true;
	}

	private void damage (float _damage) {
		Life -= _damage;
		if (Life <= 0.0f) {
			Life = 0.0f;
			die ();
		}
	}

	private IEnumerator notSlippingWait () {
		yield return new WaitForSeconds (Constants.NOT_SLIPPING_WAIT_TIME);

		anim.SetBool (SLIPPING_BOOL_HASH, false);

		notSlippingCoroutine = null;
	}

	private IEnumerator leavingFloorWait (){

		yield return new WaitForSeconds (Constants.LEAVING_FLOOR_WAIT_TIME);

		anim.SetBool (ONTHEFLOOR_BOOL_HASH, false);

		notOnTheFloorCoroutine = null;
	}

	private IEnumerator jumpForce (){
		float _jumpCommand;
		float _elapsedTime = 0.0f;

		anim.SetBool (JUMPING_BOOL_HASH, true);
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

		anim.SetBool (JUMPING_BOOL_HASH, false);
		jumpForceCoroutine = null;
	}

	private static Vector2 projectVector(Vector2 _source, Vector2 _dst) {
		float _dstMagnitude = _dst.magnitude;

		return _dst * (Vector2.Dot (_source, _dst) / (_dstMagnitude * _dstMagnitude));
	}

	private Vector2 getWalkForce(Vector2 _walkDirection) {
		if (anim.GetBool (SLIPPING_BOOL_HASH))
			return Vector2.zero;

		float _walkDirectionVectorMagnitude = _walkDirection.magnitude;
		float _controllerDirection = Input.GetAxis ("Horizontal");
		float _currentDirection = Vector3.Cross (rigidBody.velocity, upDirection).z < 0.0f ? -1.0f : 1.0f;

		float _walkForce;
		if (_controllerDirection * _currentDirection < 0.0f)	// Opposite direction
			_walkForce = Constants.WALK_FORCE;
		else {
			// Project the current velocity onto the walk direction vector
			Vector2 _currentWalkVector = projectVector(rigidBody.velocity, _walkDirection);
			
			float _currentWalkMagnitude = _currentWalkVector.magnitude;
			float _walkForceFactor = (Constants.MAX_LINEAR_VELOCITY - _currentWalkMagnitude) * Constants.MAX_LINEAR_VELOCITY_INVERSE;
			_walkForceFactor = Mathf.Min (_walkForceFactor, Constants.WALK_FORCE);
			_walkForceFactor = Mathf.Max (_walkForceFactor, -Constants.WALK_FORCE);
			_walkForce = Constants.WALK_FORCE * _walkForceFactor;
		}

		return _walkDirection * rigidBody.mass * _controllerDirection * _walkForce;
	}

	void Awake () {
		planet = GameObject.Find ("Planet");
		oxygenGauge = GameObject.Find ("Canvas/OxygenGauge").GetComponent<OxygenGauge> ();
		lifeGauge = GameObject.Find ("Canvas/LifeGauge").GetComponent<LifeGauge> ();

		rigidBody = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		sprite = transform.Find ("CharacterV").gameObject;

		jumpForceCoroutine = null;
		notOnTheFloorCoroutine = null;
		notSlippingCoroutine = null;
		tryingToJump = false;

		Life = 100.0f;
		Oxygen = 100.0f;

		gameStarted = false;
	}

	private bool updateSlipping () {
		bool _condition = (slopeNormal != Vector2.zero && (Vector2.Angle (upDirection, slopeNormal) > Constants.MAX_SLOPE_VERTICAL_ANGLE_TO_WALK));
		bool _previousValue = anim.GetBool (SLIPPING_BOOL_HASH);

		if (_condition) {
			if (notSlippingCoroutine != null) {
				StopCoroutine (notSlippingCoroutine);
				notSlippingCoroutine = null;
			}

			{
				anim.SetBool (
					SLIPPING_BOOL_HASH,
					_condition
				);

				if (Vector3.Cross (upDirection, slopeNormal).z > 0.0f)
					lookLeft ();
				else
					lookRight ();
			}

			return true;
		} else if (notSlippingCoroutine == null && _previousValue) {	// Just stopped slipping
			notSlippingCoroutine = StartCoroutine (notSlippingWait ());

			return true;
		}

		return anim.GetCurrentAnimatorStateInfo(0).fullPathHash == SPACEMAN_SLIPPING_ANIM_HASH;
	}
	
	private void lookLeft () {
		anim.SetBool (LOOKINGLEFT_TRIGGER_HASH, true);
	}
	
	private void lookRight () {
		anim.SetBool (LOOKINGLEFT_TRIGGER_HASH, false);
	}

	void FixedUpdate () {
		Vector3 _direction = transform.position - planet.transform.position;
		upDirection = new Vector2 (_direction.x, _direction.y).normalized;
		Vector2 _gravity = upDirection * Common.Constants.GRAVITY_MAGNITUDE;

		transform.rotation = Quaternion.LookRotation(Vector3.forward, _direction);

		{	// Finish slope normal calculation
			slopeNormal.Normalize ();

			Vector2 _localSlopeNormal = Quaternion.Inverse (transform.localRotation) * slopeNormal;
			transform.Find ("CollisionArrow").localRotation = Quaternion.LookRotation (Vector3.forward, _localSlopeNormal);
		}

		if (!updateSlipping ()) {
			// Invert the character if needed
			float _horizontalAxis = Input.GetAxis ("Horizontal");
			if (_horizontalAxis < 0.0f)
				lookLeft ();
			else if (_horizontalAxis > 0.0f)
				lookRight ();
		}

		checkGroundStatus ();

		if (Life == 0.0f)
			return;

		rigidBody.AddForce (rigidBody.mass * _gravity);

		if (!gameStarted)
			return;

		Vector3 _walkVector = Vector3.Cross (Vector3.back, _direction).normalized;
		Vector2 _walkDirection = new Vector2 (_walkVector.x, _walkVector.y);

		Vector2 _walkForce = getWalkForce (_walkDirection);
		rigidBody.AddForce (_walkForce);

		anim.SetBool (WALKING_BOOL_HASH, _walkForce.magnitude >= Constants.NOT_WALKING_THRESHOLD);

		// Reset slope normal
		slopeNormal = Vector2.zero;
	}
	
	// Update is called once per frame
	void Update () {
		if (!gameStarted)
			return;
		
		if (Life == 0.0f)
			return;

		float _jumpCommand = Input.GetAxis ("Jump");
		if (_jumpCommand > 0.0f) {
			if (!tryingToJump) {
				if (anim.GetBool(ONTHEFLOOR_BOOL_HASH)) {
					if (jumpForceCoroutine == null)
						jumpForceCoroutine = StartCoroutine (jumpForce ());
				}
				tryingToJump = true;
			}
		} else
			tryingToJump = false;

		Oxygen -= (Time.deltaTime * Constants.O2_PER_SECOND);
		if (Oxygen <= 0.0f) {
			Oxygen = 0.0f;
			damage(Time.deltaTime * Constants.LIFE_WITH_NO_O2_PER_SECOND);
		}

		oxygenGauge.setRemainingOxygen (Oxygen);

		lifeGauge.setRemainingLife (Life);
	}
}
