using UnityEngine;

public class OxygenGauge : MonoBehaviour {

	private UnityEngine.UI.Text text;

	public void setRemainingOxygen (float _o2) {
		float _t = (100.0f - _o2) / 100.0f;

		text.text = "" + (int)_o2 + "%";
		text.color = Color.green * (1.0f - _t) + Color.red * _t;
	}

	void Awake () {
		text = GetComponent<UnityEngine.UI.Text> ();
	}
}
