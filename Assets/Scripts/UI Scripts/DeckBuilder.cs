using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBuilder : MonoBehaviour
{
    public GameObject Inventory;
    public GameObject pref;
    public bool Vis;
    public Deck d1;
    public TMP_Text cardText;
    // Start is called before the first frame update
    void Start()
    {
        Inventory = this.transform.Find("IHolder/Inventory").gameObject;

        Inventory.SetActive(false); // Set the inventory canvas to false when loading up the game so its not visible
        d1 = GameObject.Find("/Player").GetComponent<Deck>(); //Get the deck

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) //Condition so when they press B the inventory loads
        {
            if (Vis == true)
            {
                Vis = false;
                Inventory.SetActive(Vis); //If it is already visible and the button is pressed make it not visible by setting it to false
            }
            else if (Vis == false)
            {
                Vis = true;
                Inventory.SetActive(Vis);//Make the inventory visible
                ViewCards(); //Call the ViewCards function below
            }
        }


    }
    void ViewCards()
    {
        foreach (Transform slot in Inventory.transform)
        {
            Destroy(slot.gameObject); //Loop through each slot in the inventory canvas and destroy it
        }
        for (int i = 0; i < d1.UserCardCollection.Count; i++) //Loop through all of user cards
        {
            GameObject newPre = Instantiate(pref, Inventory.transform);//Create the new prefab using a card sprite and inventory canvas
            cardText = newPre.transform.GetChild(0).gameObject.GetComponent<TMP_Text>(); //Get the text component of the prefab
            cardText.text = d1.UserCardCollection[i].ToString(); //Change the text on the prefab to information got from the user card collection deck
        }
    }
}

