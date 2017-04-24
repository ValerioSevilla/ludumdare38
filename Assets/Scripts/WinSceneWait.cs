using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinSceneWait : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
		yield return new WaitForSeconds (3.0f);

		GameObject.Find ("Canvas/Fade").GetComponent<FadeScript> ().fadeOut ("Main");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
