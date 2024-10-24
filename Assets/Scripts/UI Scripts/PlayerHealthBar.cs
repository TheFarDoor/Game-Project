using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public class HealthSystem : MonoBehaviour
{
    public Slider healthBar;  // Reference to the Slider UI
    public float maxHealth = 100f;  // Max health value
    private float currentHealth = 20;  // Variable to track the current health
    public TextMeshProUGUI healthText;

    public bool isInitialized = false;

    public float CurrentHealth => currentHealth; // getter for current health

    public void Initialize() // Instantiate the health with the value in the slider
    {   
        if(!isInitialized){
            healthText = GameObject.Find("/Canvas/BattleUI/Health").GetComponent<TextMeshProUGUI>();
            healthBar =  GameObject.Find("/Canvas/BattleUI/HealthBar").GetComponent<Slider>();
            currentHealth = maxHealth; 
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            UpdateHealthText();
        }
    }
    

    public void TakeDamage(float damage) // Take damage and update the health slider
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;
        UpdateHealthText();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void HealHealth(float healAmount) // Function to calculate healing
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Prevent health from going above the max and below the min
        healthBar.value = currentHealth;
        UpdateHealthText();

    }

    void UpdateHealthText() // Update the Health UI to reflect current Health
    {
        healthText.text = "Health: " + currentHealth.ToString("F0") + " / " + maxHealth.ToString("F0");
    }

    void Die()
    {
        // Code for when the player dies (e.g., game over)
        Debug.Log("Player is dead!");
    }
}