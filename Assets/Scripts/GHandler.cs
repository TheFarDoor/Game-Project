using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GHandler : MonoBehaviour
{
    GameObject cardHolder;
    void Awake(){
        if (GameObject.Find("/CardHolder")){
            cardHolder = GameObject.Find("/CardHolder");
        }
        else {
            cardHolder = new GameObject("CardHolder");
        }

        for (int i=0; i<5; i++){
            GameObject newCard = new GameObject("Card " + i);
            newCard.AddComponent<Card>();
            newCard.transform.parent = cardHolder.transform;
        }
    }
}
