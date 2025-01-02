using UnityEngine;
using TMPro;

public class MonsterStatus : MonoBehaviour
{
    public TextMeshPro monsterStatText;
    public int attack;
    public int health;
    public string monstName;
    public bool canAttack;
    

    void Start()
    {
        monsterStatText = this.transform.GetChild(1).GetComponent<TextMeshPro>(); // Get text reference for relevant slot
    }

    public void UpdateStats(string name, int atk, int hp){
        monsterStatText.text = name + "\n" + "Attack:  " + atk.ToString("F0") + "\n" + "Health:  " + hp.ToString("F0");
        attack = atk;
        health = hp;
        monstName = name;
    }

    void Update(){
        if(monsterStatText != null){
            if(canAttack){
            monsterStatText.color = Color.green;
            }
            else{
                monsterStatText.color = Color.red;
            }
        }
        
    }

    public void ClearStats(){
        Debug.Log("Clearing " + this.name + " stats");
        monsterStatText.text = "";
        attack = health = -1;
        monstName = "";
    }

    public void UpdateAttackBool(bool val){
        canAttack = val;
    }
}
