using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleLogic : MonoBehaviour
{
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentMana;

    
    private bool isMyTurn;
    

    public void Start_P_Turn(){
        isMyTurn = true;
    }

    void Update(){
        if (isMyTurn){

        }
    }

    void End_P_Turn(){
        isMyTurn = false;
    }
}
