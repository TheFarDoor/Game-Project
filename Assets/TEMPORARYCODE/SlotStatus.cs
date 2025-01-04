using UnityEngine;
using TMPro;
using System;

public class SlotStatus : MonoBehaviour
{
    public TextMeshPro monsterStatText;

    public Tuple<bool, string, float, float, bool> slotState = new Tuple<bool, string, float, float, bool>(false, "", -1.0f, -1.0f, false);
    
    void Update(){
        if(slotState != null && slotState.Item1 && monsterStatText.text != ""){
            monsterStatText.text = slotState.Item2 + "\n" + "Attack:  " + slotState.Item3.ToString("F0") + "\n" + "Health:  " + slotState.Item4.ToString("F0");
        }
        else{
            monsterStatText.text = "";
        }

        if(monsterStatText != null){
            if(slotState.Item5){
            monsterStatText.color = Color.green;
            }
            else{
                monsterStatText.color = Color.red;
            }
        }
    }

    public void UpdateSlot(bool occupationState, string cardName, float damage, float health, bool canAttack){
        slotState = new Tuple<bool, string, float, float, bool>(occupationState, cardName, damage, health, canAttack);
    }

    public void ClearSlot(){
        slotState = new Tuple<bool, string, float, float, bool>(false, "", -1.0f, -1.0f, false);
    }
}
