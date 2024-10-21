using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    public GameObject Inventory;
    public bool Vis = false;
    [SerializeField] private List<Card> UserDeck; // the deck itself
    // Start is called before the first frame update
    void Start()
    {
        Inventory.SetActive(Vis);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
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
            }
        }
    }
}
