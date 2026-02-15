using System.Collections;
using UnityEngine;

public class DoorSpriteController : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer sr;
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Timing")]
    public float openDuration = 0.6f;

    Coroutine routine;

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        SetClosedInstant();
    }

    public void PlayOpenClose()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(OpenCloseRoutine());
    }

    IEnumerator OpenCloseRoutine()
    {
        // open
        if (openSprite)
            sr.sprite = openSprite;

        yield return new WaitForSeconds(openDuration);

        // close
        if (closedSprite)
            sr.sprite = closedSprite;
    }

    public void SetClosedInstant()
    {
        if (closedSprite)
            sr.sprite = closedSprite;
    }

    public void SetOpenInstant()
    {
        if (openSprite)
            sr.sprite = openSprite;
    }
}
