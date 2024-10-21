using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHealthDisplay : MonoBehaviour
{
    private int playerHealth = 10;
    private int playerMana = 5;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;

    private void Update()
    {
        healthText.text = "Health: " + playerHealth;
        manaText.text = "Mana: " + playerMana;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerHealth--;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            playerMana++;
        }
    }
}
