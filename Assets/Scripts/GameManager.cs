using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{   
    [Header("References")]
    public static GameManager instance;
    private BattleManager battle_manager;
    public GameObject player;
    [Header("Battle UI control")]
    private GameObject Battle_UI;
    public bool battleUIactive;

    // Start is called before the first frame update
    void Start()
    {   
        player = GameObject.Find("/Player");
        Battle_UI = GameObject.Find("/Canvas/BattleUI");
        battle_manager = this.GetComponent<BattleManager>();

        // Check if an instance already exists
        if (instance == null)
        {
            // If not, set this as the instance
            instance = this;
            // Ensure that this instance is not destroyed when loading new scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        Battle_UI.SetActive(battle_manager.battleOngoing);
        if(Battle_UI.activeSelf != battleUIactive){
            if (Battle_UI.activeSelf){
                player.GetComponent<HealthSystem>().Initialize();
                player.GetComponent<ManaSystem>().Initialize();
            }
        }
    }
}
