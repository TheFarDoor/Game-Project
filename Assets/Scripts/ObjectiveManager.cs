using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{

    public TextMeshProUGUI cardCollectText;

    // Start is called before the first frame update
    void Start()
    {
        cardCollectText = GameObject.Find("/Canvas/NormalUI/CollectCards").GetComponent<TextMeshProUGUI>();
        cardCollectText.text = "Collect Cards";
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("PickUpHolder").transform.childCount == 0)
        {
            cardCollectText.color = Color.green;
        }
    }
}
