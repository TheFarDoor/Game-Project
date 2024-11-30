using UnityEngine;
using TMPro;

public class MonsterStatus : MonoBehaviour
{
    public TextMeshProUGUI monsterStatText;
    public float attack;
    public float health;
    public bool canAttack;
    

    void Awake()
    {
        int slotIndex = this.transform.GetSiblingIndex(); // get index of slot in container
        bool playerSlot = (this.tag == BattleManager.Instance.playerCardZoneTag); // bool to track if slot is for player
        monsterStatText = this.transform.parent.parent.parent.Find("Canvas-Arena/PlacedCardStats/"+(playerSlot? "Player": "Enemy")).GetChild(slotIndex).GetComponent<TextMeshProUGUI>(); // Get text reference for relevant slot
    }

    public void UpdateStats(float atk, float hp){
        monsterStatText.text = "Attack:  " + atk.ToString("F0") + "\n" + "Health:  " + hp.ToString("F0");
        attack = atk;
        health = hp;
    }

    public void ClearStats(){
        monsterStatText.text = "";
        attack = health = -1;
    }

    public void UpdateAttackBool(bool val){
        canAttack = val;
    }
}
