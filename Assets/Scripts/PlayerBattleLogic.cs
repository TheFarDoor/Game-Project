using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleLogic : MonoBehaviour
{
    private bool isMyTurn;

    void Update(){
        if (isMyTurn){
            
        }
    }

    public void Start_P_Turn(){
        isMyTurn = true;
    }

    void End_P_Turn(){
        isMyTurn = false;
    }
}
