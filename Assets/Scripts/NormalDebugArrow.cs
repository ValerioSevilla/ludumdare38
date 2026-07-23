using UnityEngine;

public class NormalDebugArrow : MonoBehaviour {
    private Vector2 position;
    public Vector2 Position {
        get { return position; }
        set {
            position = value;
            transform.position = new Vector3(position.x, position.y, 0.0f);
        }
    }

    private float orientation;
    public float Orientation {
        get { return orientation; }
        set {
            orientation = value;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, orientation);
        }
    }
}
