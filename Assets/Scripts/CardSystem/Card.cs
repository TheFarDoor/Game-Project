using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Card", menuName = "Card")]
public class Card : ScriptableObject
{
    private enum Card_Type{
        Fire,
        Water,
        Air,
        Earth,
        Max // a value to track end of enums
    };

    [SerializeField] private int id; // card id
    [SerializeField] private Card_Type type; // card type
    [SerializeField] private string card_name; // card name
    [SerializeField] private string description; // card description
    [SerializeField] [Range(0,100)] private int damage; // card damage which can be between 0 - 100 
    [SerializeField] [Range(0,100)] private int defence; // card defence which can be between 0 - 100 
    [SerializeField] private int cost; // cost to use card
    [SerializeField] bool used; // bool to track if card is used
    [SerializeField] GameObject model; // model for summon if card summons something

    // Getter method for each private variable
    public int Id => id;
    public string CardName => card_name;
    public string Description => description;
    public int Damage => damage;
    public int Defence => defence;
    public int Cost => cost;
    public bool Used => used;
    public GameObject Model => model;

    public void SetCardType(string type)
    {
        if (Card_Type.TryParse(type, true, out Card_Type cardType))
        {
            this.type = cardType;
        }
        else
        {
            Debug.LogError("No such type!");
        }
    }

    public void SetCardDamage(int val)
    {
        int newDamage = this.damage + val;
        this.damage = Mathf.Clamp(newDamage, 0, 10); // Clamps value between 0 and 100
    }

    public void SetCardDefence(int val)
    {
        int newDefence = this.defence + val;
        this.defence = Mathf.Clamp(newDefence, 0, 10); // Clamps value between 0 and 100
    }

    public void UseCard()
    {
        this.used = true; // Mark card as used
    }

    public void ResetCardUsage()
    {
        this.used = false; // Reset usage status for the next battle
    }
}
