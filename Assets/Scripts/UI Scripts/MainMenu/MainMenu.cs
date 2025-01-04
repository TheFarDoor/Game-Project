using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void playMenu() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Changes from Main Menu Scene to next scene (game scene)
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
