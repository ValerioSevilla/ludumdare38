using UnityEngine;

public class NormalDebugger : MonoBehaviour {
    public GameObject normalDebugArrowPrefab;

    private ContactPoint2D[] contactPoints;
    public ContactPoint2D[] ContactPoints {
        get { return contactPoints; }
        set {
            contactPoints = value;
            updateNormalDebugArrows();
        }
    }

    private void updateNormalDebugArrows() {
        foreach (Transform _child in transform) {
            Destroy(_child.gameObject);
        }

        foreach (var _contactPoint in contactPoints) {
            GameObject _normalDebugArrow = Instantiate(normalDebugArrowPrefab);
            NormalDebugArrow _normalDebugArrowScript = _normalDebugArrow.GetComponent<NormalDebugArrow>();
            _normalDebugArrowScript.Position = _contactPoint.point;
            _normalDebugArrowScript.Orientation = Mathf.Atan2(_contactPoint.normal.y, _contactPoint.normal.x) * Mathf.Rad2Deg - 90.0f;
            _normalDebugArrow.transform.SetParent(transform);
        }
    }
}
