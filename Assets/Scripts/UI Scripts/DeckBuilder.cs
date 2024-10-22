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

        Inventory.SetActive(false);
        d1 = GameObject.Find("/Player").GetComponent<Deck>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (Vis == true)
            {
                Vis = false;
                Inventory.SetActive(Vis);
            }
            else if (Vis == false)
            {
                Vis = true;
                Inventory.SetActive(Vis);
                ViewCards();
            }
        }


    }
    void ViewCards()
    {
        foreach (Transform slot in Inventory.transform)
        {
            Destroy(slot.gameObject);
        }
        for (int i = 0; i < d1.UserCardCollection.Count; i++)
        {
            GameObject newPre = Instantiate(pref, Inventory.transform);
            newPre.transform.localPosition = new Vector3(0, -30 * i, 0); // Adjust position if necessary
            newPre.transform.localScale = Vector3.one;

            cardText = newPre.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
            cardText.text = d1.UserCardCollection[i].ToString();
        }
    }
}

