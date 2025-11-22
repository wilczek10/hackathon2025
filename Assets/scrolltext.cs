using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ScrollingText : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 50f;
    public string nextSceneName;

    [Header("Optional")]
    public bool useStartPosition = true;
    public bool showSkipMessage = true;

    private RectTransform rectTransform;
    private float screenHeight;
    private bool isScrolling = true;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        screenHeight = Screen.height;

        if (useStartPosition)
        {
            Vector2 startPosition = rectTransform.anchoredPosition;
            startPosition.y = -screenHeight - rectTransform.rect.height;
            rectTransform.anchoredPosition = startPosition;
        }

        // Opcjonalna informacja o pominiêciu
        if (showSkipMessage)
        {
            Debug.Log("Naciœnij SPACJÊ aby pomin¹æ");
        }
    }

    void Update()
    {
        // SprawdŸ naciœniêcie spacji
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkipToNextScene();
            return;
        }

        // Alternatywnie: klikniêcie mysz¹ równie¿ przenosi
        if (Input.GetMouseButtonDown(0))
        {
            SkipToNextScene();
            return;
        }

        if (!isScrolling) return;

        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.y > screenHeight + rectTransform.rect.height)
        {
            EndScrolling();
        }
    }

    void SkipToNextScene()
    {
        if (!isScrolling) return; // Zapobiega wielokrotnemu ³adowaniu

        isScrolling = false;
        Debug.Log("Przechodzenie do: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    void EndScrolling()
    {
        SkipToNextScene();
    }
}