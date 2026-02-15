using UnityEngine;
using UnityEngine.UI;

public class VolumeFillSync : MonoBehaviour
{
    [Header("References")]
    public Transform sliderObject;   // 被推动的那个 slider GameObject
    public Transform leftWaypoint;   // 最左 waypoint
    public Transform rightWaypoint;  // 最右 waypoint
    public Image fillImage;          // UI Fill Image（Fill Method=Horizontal）

    [Header("Smooth")]
    public float smoothSpeed = 10f;  // 0 = instant

    void Reset()
    {
        fillImage = GetComponent<Image>();
    }

    void Update()
    {
        if (sliderObject == null || leftWaypoint == null || rightWaypoint == null || fillImage == null)
            return;

        float leftX = leftWaypoint.position.x;
        float rightX = rightWaypoint.position.x;

        float t = 0f;

        if (Mathf.Abs(rightX - leftX) > 0.0001f)
        {
            t = Mathf.InverseLerp(leftX, rightX, sliderObject.position.x);
        }

        if (smoothSpeed <= 0f)
        {
            fillImage.fillAmount = t;
        }
        else
        {
            fillImage.fillAmount = Mathf.Lerp(
                fillImage.fillAmount,
                t,
                smoothSpeed * Time.deltaTime
            );
        }
    }
}
