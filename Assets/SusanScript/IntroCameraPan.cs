using UnityEngine;

public class IntroCameraPan : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public float moveSecondsOneWay = 6f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    float _t;
    bool _forward = true;

    public void ResetToA()
    {
        _t = 0f;
        _forward = true;
        if (pointA)
            transform.position = new Vector3(pointA.position.x, pointA.position.y, transform.position.z);
    }

    void Update()
    {
        if (!pointA || !pointB) return;

        float dir = _forward ? 1f : -1f;
        _t += (Time.unscaledDeltaTime / Mathf.Max(0.01f, moveSecondsOneWay)) * dir;

        if (_t >= 1f) { _t = 1f; _forward = false; }
        if (_t <= 0f) { _t = 0f; _forward = true; }

        float eased = ease.Evaluate(_t);
        Vector3 p = Vector3.Lerp(pointA.position, pointB.position, eased);
        transform.position = new Vector3(p.x, p.y, transform.position.z);
    }
}
