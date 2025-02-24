using UnityEngine.UI;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public Button endTurnBtn;
    public void OnEnable(){
        if(endTurnBtn == null){
            endTurnBtn = this.GetComponent<Button>();
            endTurnBtn.onClick.AddListener(OnButtonClick);
        }
    }

    public void Update(){
        this.GetComponent<Button>().interactable = (BattleManager.Instance.currentTurn == BattleManager.Turn.A && BattleManager.Instance.currentBattleState == (BattleManager.BattleState.Idle));
    }

    public void OnButtonClick(){
        Debug.Log("End Turn Clicked");
        BattleManager.Instance.SwitchToEnemyTurn();
    }
}
