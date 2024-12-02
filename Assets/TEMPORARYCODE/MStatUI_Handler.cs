using UnityEngine;

public class MStatUI_Handler : MonoBehaviour
{
    GameObject thisArenaCam;

    void Start(){
        thisArenaCam = this.transform.root.Find("Cam").gameObject;
    }

    void Update(){
        if(thisArenaCam != null){
            Vector3 dir = thisArenaCam.transform.position - this.transform.position;
            this.transform.rotation = Quaternion.LookRotation(dir);

            this.transform.Rotate(0, 180, 0); // flip text so it isnt inverted
        }
    }
}
