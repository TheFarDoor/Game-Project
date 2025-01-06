using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endMenu;

    // Start is called before the first frame update
    void Start()
    {
        endMenu.SetActive(false);
    }

    public void backToMenu()
    {
        endMenu.SetActive(false);
        Destroy(gameObject);
        SceneManager.LoadScene("MainMenu");
    }
}
