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

    public int GetCardId(){
        return this.id;
    }

    public void GetCardType(){
        Debug.Log(this.type);
    }

    public void SetCardType(string type){
        for (int i=0; i<(int)Card_Type.Max; i++){
            if (((Card_Type)i).ToString().ToLower() == type.ToLower()){
                this.type = (Card_Type)i;
            }
        }
        Debug.Log("NO SUCH TYPE!");
    }

    public string GetCardName(){
        return this.card_name;
    }

    public string GetCardDescription(){
        return this.description;
    }

    public int GetCardDamage(){
        return this.damage;
    }

    public void SetCardDamage(int val){
        int newDamage = this.damage + val;
        if (newDamage < 0){
            this.SetCardDamage(0);
        }
        else if (newDamage > 100){
            this.SetCardDamage(100);
        }
    }

    public int GetCardDefence(){
        return this.defence;
    }

    public void SetCardDefence(int val){
        int newDefence = this.defence + val;
        if (newDefence < 0){
            this.SetCardDefence(0);
        }
        else if (val > 100){
            this.SetCardDefence(100);
        }
    }

    public int GetCardCost(){
        return this.cost;
    }

    public bool GetCardUsedStatus(){
        return this.used;
    }

    public GameObject GetCardModel(){
        return this.model;
    }
}
