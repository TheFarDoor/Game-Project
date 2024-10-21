using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaSystem : MonoBehaviour
{
    public Slider manaBar;  // Reference to the Slider UI
    public float maxMana = 100f;  // Max Mana value
    private float currentMana = 5;  // Variable to track the current Mana
    public TextMeshProUGUI manaText;

    void Start() // Instantiate the mana with the value in the slider
    {
        currentMana = maxMana;
        manaBar.maxValue = maxMana;
        manaBar.value = currentMana;
        UpdateManaText();
    }

    public void RemoveMana(float manaCost) // Take damage and update the mana slider
    {
        currentMana -= manaCost;
        manaBar.value = currentMana;
        UpdateManaText();

        if (currentMana <= 0)
        {
            NoMana();
        }
    }

    public void RecoverMana(float manaRecoveryAmount) // Function to calculate healing
    {
        currentMana += manaRecoveryAmount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana); // Prevent mana from going above the max and below the min
        manaBar.value = currentMana;
        UpdateManaText();

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            currentMana--;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            manaBar.value = currentMana;
            UpdateManaText();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            currentMana++;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            manaBar.value = currentMana;
            UpdateManaText();
        }
    }

    void UpdateManaText() // Updates the UI to change the current Mana count
    {
        manaText.text = "Mana: " + currentMana.ToString("F0") + " / " + maxMana.ToString("F0");
    }

    void NoMana()
    {
        Debug.Log("No Mana Left!!!");
    }
}