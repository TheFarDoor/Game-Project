using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private BattleManager battle_manager;
    private GameObject Battle_UI;

    // Start is called before the first frame update
    void Start()
    {
        Battle_UI = GameObject.Find("/Canvas/BattleUI");
        battle_manager = GameObject.Find("/Game Manager").GetComponent<BattleManager>();

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
        Battle_UI.SetActive(battle_manager.battle);
    }
}
