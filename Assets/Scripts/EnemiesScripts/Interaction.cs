using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        if(Physics.SphereCast(this.transform.position, 2, transform.forward, out hit, 3)){
            Debug.Log("Someones here!");
        }
    }

    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(this.transform.position, 2);
    }
}
