using System.Collections.Generic;
using UnityEngine;

public class NormalDebugger : MonoBehaviour {
    public GameObject normalDebugArrowPrefab;

    private List<ContactPoint2D> contactPoints;

    private Vector2 computedNormal;
    public Vector2 ComputedNormal {
        get { return computedNormal; }
        set {
            computedNormal = value;
            addNormalDebugArrow(transform.position, computedNormal, true);
        }
    }

    private void addNormalDebugArrow(Vector2 position, Vector2 normal, bool highlight) {
        GameObject _normalDebugArrow = Instantiate(normalDebugArrowPrefab);
        NormalDebugArrow _normalDebugArrowScript = _normalDebugArrow.GetComponent<NormalDebugArrow>();
        _normalDebugArrowScript.Position = position;
        _normalDebugArrowScript.Orientation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90.0f;
        _normalDebugArrowScript.Highlight = highlight;
        _normalDebugArrow.transform.SetParent(transform);
    }

    private void updateNormalDebugArrows() {
        foreach (Transform _child in transform) {
            Destroy(_child.gameObject);
        }

        foreach (var _contactPoint in contactPoints) {
            addNormalDebugArrow(_contactPoint.point, _contactPoint.normal, false);
        }
    }

    public void ClearContactPoints() {
        contactPoints = new List<ContactPoint2D>();
        updateNormalDebugArrows();
    }

    public void AddContactPoints(ContactPoint2D[] newContactPoints) {
        contactPoints.AddRange(newContactPoints);
        updateNormalDebugArrows();
    }

    private void Awake() {
        contactPoints = new List<ContactPoint2D>();
    }
}
