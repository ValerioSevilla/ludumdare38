using UnityEngine;

public class LifeGauge : MonoBehaviour {

	private UnityEngine.UI.Text text;

	public void setRemainingLife (float _life) {
		float _t = (100.0f - _life) / 100.0f;

		text.text = "" + (int)_life + "%";
		text.color = Color.green * (1.0f - _t) + Color.red * _t;
	}

	void Awake () {
		text = GetComponent<UnityEngine.UI.Text> ();
	}
}
