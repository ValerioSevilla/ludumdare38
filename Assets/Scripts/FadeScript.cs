using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeScript : MonoBehaviour {

	private int FADEOUT_TRIGGER_HASH = Animator.StringToHash("FadeOut");

	private Animator anim;
	private string nextSceneName;

	public void fadeOut (string _nextSceneName) {
		anim.SetTrigger (FADEOUT_TRIGGER_HASH);

		nextSceneName = _nextSceneName;
	}

	public void commitFadeOut () {
		SceneManager.LoadScene (nextSceneName);
	}

	void Awake () {
		anim = GetComponent<Animator> ();
	}
}
