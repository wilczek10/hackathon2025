using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnSpace : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("poziom1");
        }
    }
}
