using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public enum GameState{
        MainMenu,
        Exploring,
        Inventory,
        Paused,
        InBattle,
        GameOver,
    }

    public static GameManager Instance;

    public GameState currentGameState;

    [Header("UI References")]
    public GameObject inventoryUI;
    public GameObject worldUI;
    public GameObject battleUI;
    public GameObject pauseUI;

    [Header("Enemy AI"), Space(10)]
    [Range(0, 2.0f)]public float enemySearchDelay = 0.2f;

    [Header("Player Inputs"), Space(20)]
    public PlayerInput controls;


    private void Awake(){ // ensuring there is only one gameobjects with the gamemanager script at any given time
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    public void Start(){
        inventoryUI = GameObject.Find("/Canvas-Cam/").transform.GetChild(2).gameObject;
        worldUI = GameObject.Find("/Canvas-Cam/NormalUI");
        battleUI = GameObject.Find("/Canvas-Cam/BattleUI");
        pauseUI = GameObject.Find("/Canvas-Cam/PauseMenuUI");

        controls = GameObject.Find("/Player").GetComponent<PlayerInput>();

        SetState(GameState.Exploring);
    }

    // Update current state with new value
    public void SetState(GameState newState){
        currentGameState = newState;
        StateTransition(newState);
    }

    // Change different things based on state such as UI depending on which state is given
    public void StateTransition(GameState newState){
        switch(newState){
            case GameState.Exploring:
                worldUI.SetActive(true);
                battleUI.SetActive(false);
                inventoryUI.SetActive(false);
                pauseUI.SetActive(false);
                controls.SwitchCurrentActionMap("Player");
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case GameState.InBattle:
                worldUI.SetActive(false);
                battleUI.SetActive(true);
                inventoryUI.SetActive(false);
                pauseUI.SetActive(false);
                controls.SwitchCurrentActionMap("InBattle");
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case GameState.GameOver:
                worldUI.SetActive(false);
                battleUI.SetActive(false);
                inventoryUI.SetActive(false);
                pauseUI.SetActive(false);
                break;
            case GameState.Inventory:
                worldUI.SetActive(false);
                battleUI.SetActive(false);
                inventoryUI.SetActive(true);
                pauseUI.SetActive(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case GameState.Paused:
                worldUI.SetActive(false);
                battleUI.SetActive(false);
                inventoryUI.SetActive(false);
                pauseUI.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }
}
