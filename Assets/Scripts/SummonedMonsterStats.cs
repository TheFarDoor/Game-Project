using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using UnityEngine;

public class SummonedMonsterStats : MonoBehaviour
{
    public int atk;
    public int health; 

    public void SetStats(int attack, int HP){
        atk = attack;
        health = HP;
    }

    public void Reset(){
        atk = 0;
        health = 0;
    }
}
