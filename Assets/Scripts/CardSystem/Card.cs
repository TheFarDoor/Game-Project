using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Card", menuName = "Card")]
public class Card : ScriptableObject
{
    [SerializeField] private enum Card_Type{
        Fire,
        Water,
        Air,
        Earth,
    };

    [SerializeField] private int id; // card id
    [SerializeField] private Card_Type type; // card type
    [SerializeField] private string cardName; // card name
    [SerializeField] private string description; // card description
    [SerializeField] [Range(0,100)] private int damage; // card damage which can be between 0 - 100 
    [SerializeField] [Range(0,100)] private int health; // card defence which can be between 0 - 100 
    [SerializeField] private int cost; // cost to use card
    [SerializeField] private GameObject model; // model for summon if card summons something

    // Getter method for each private variable
    public int Id => id;
    public string CardName => cardName;
    public string Description => description;
    public int Damage => damage;
    public int Health => health;
    public int Cost => cost;
    public GameObject Model => model;

    public static Card CreateInstance(Card data){ // create new instance of template card

        Card newCard = CreateInstance<Card>();

        newCard.id = data.Id;
        newCard.cardName = data.CardName;
        newCard.description = data.Description;
        newCard.damage = data.Damage;
        newCard.health = data.Health;
        newCard.cost = data.Cost;
        newCard.model = data.Model;

        return newCard;
    }

    public void OnEnable(){ // Called when a new card is created in the asset menu
        cardName = name; // assign the file name as the cardName
    }

    public void SetCardDamage(int val)
    {
        int newDamage = this.damage + val;
        this.damage = Mathf.Clamp(newDamage, 0, 10); // Clamps value between 0 and 100
    }

    public void SetCardDefence(int val)
    {
        int newDefence = this.health + val;
        this.health = Mathf.Clamp(newDefence, 0, 10); // Clamps value between 0 and 100
    }

    public void UseCard()
    {
        this.used = true; // Mark card as used
    }

    public void ResetCardUsage()
    {
        this.used = false; // Reset usage status for the next battle
    }

    public void SetId(int val){
        this.id = val;
    }
}
