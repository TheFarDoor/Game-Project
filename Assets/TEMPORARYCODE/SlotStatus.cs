using UnityEngine;
using TMPro;
using System;

public class SlotStatus : MonoBehaviour
{
    public TextMeshPro monsterStatText;

    public Tuple<bool, Card, float, bool> slotState = new Tuple<bool, Card, float, bool>(false, null, -1.0f, false);

    public void UpdateAttackBool(bool newAttkState){
        Tuple<bool, Card, float, bool> temp = slotState;
        slotState = new Tuple<bool, Card, float, bool>(temp.Item1, temp.Item2, temp.Item3, newAttkState);
    }

    public void UpdateHp(float newHP){
        Tuple<bool, Card, float, bool> temp = slotState;
        slotState = new Tuple<bool, Card, float, bool>(temp.Item1, temp.Item2, newHP, temp.Item4);
    }

    public void UpdateTuple(bool occupied, Card card, float hptracker, bool canAttack){
        slotState = new Tuple<bool, Card, float, bool>(occupied, card, hptracker, canAttack);
    }

    public void ClearTuple(){
        slotState = new Tuple<bool, Card, float, bool>(false, null, -1.0f, false);
    }

    void Update(){
        if(monsterStatText != null){
            if(slotState.Item4){
            monsterStatText.color = Color.green;
            }
            else{
                monsterStatText.color = Color.red;
            }
        }

        if(slotState.Item1){
            monsterStatText.text = slotState.Item2.CardName + "\n" + "Attack:  " + slotState.Item2.Damage.ToString("F0") + "\n" + "Health:  " + slotState.Item3.ToString("F0");
        }
        else{
            monsterStatText.text = "";
        }
        
        BattleManager.Instance.InformationSlots[this.name] = slotState;
    }
}
