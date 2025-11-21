using UnityEngine;
using UnityEngine.SceneManagement;

public class main_menu : MonoBehaviour
{
   public void Graj()
    {
        SceneManager.LoadScene("scena1");
    }

    public void Ustawienia()
    {
        SceneManager.LoadScene("menu_ustawien");
    }

    public void menu()
    {
        SceneManager.LoadScene("main_menu");
    }

    public void Wyjdz()
    {
        Application.Quit();
    }
}
