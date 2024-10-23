using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{

    public TextMeshProUGUI cardCollectText;
    public TextMeshProUGUI defeatBossText;

    // Start is called before the first frame update
    void Start()
    {
        cardCollectText = GameObject.Find("/Canvas/NormalUI/CollectCards").GetComponent<TextMeshProUGUI>();
        defeatBossText = GameObject.Find("/Canvas/NormalUI/KillTheBoss").GetComponent<TextMeshProUGUI>();
        cardCollectText.text = "Find and collect Cards";
        defeatBossText.text = "Find the Red Enemy in the forestand defeat him";
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("PickUpHolder").transform.childCount == 0)
        {
            cardCollectText.color = Color.green;
        }

        if(GameObject.Find("Enemy").GetComponent<Enemy>().isDefeated){
            defeatBossText.color = Color.green;
        }

        if(GameObject.Find("Enemy").GetComponent<Enemy>().isDefeated && GameObject.Find("PickUpHolder").transform.childCount == 0){
            StartCoroutine(DelayEnd());
        }
    }

    IEnumerator DelayEnd(){
        yield return new WaitForSeconds(5);
        Application.Quit();
    }
}
