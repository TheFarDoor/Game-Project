using UnityEngine;

public class StartBattle : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L)){
            StartCoroutine(this.GetComponent<BattleManager>().InitializeBattle(this.transform.GetChild(0), transform.GetChild(1)));
            Debug.Log("IN battle");
        }
    }
}
