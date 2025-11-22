using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    public float scrollSpeed = 3f;
    public float distance = 1344f; // szerokość twojej sceny
    private bool scrolling = false;

    private float moved = 0f;

    public void StartScrolling()
    {
        scrolling = true;
        moved = 0f;
    }

    void Update()
    {
        if (!scrolling) return;

        float moveStep = scrollSpeed * Time.deltaTime;
        transform.position += new Vector3(-moveStep, 0, 0);
        moved += moveStep;

        // kiedy przesunie o 1344 px → koniec animacji
        if (moved >= distance)
        {
            scrolling = false;
        }
    }
}
