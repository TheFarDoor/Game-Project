using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaBattleCam : MonoBehaviour
{
    private enum CamKeys{
        None,
        Q,
        E,
    }

    private CamKeys currentCamKey;

    // Update is called once per frame
    void Update()
    {
        // check if b is pressed
        if (Input.GetKeyDown(KeyCode.Q) && currentCamKey == CamKeys.None)
        {
            currentCamKey = CamKeys.Q;  // set q to pressed
        }

        // check if e is pressed
        if (Input.GetKeyDown(KeyCode.E) && currentCamKey == CamKeys.None)
        {
            currentCamKey = CamKeys.E;  // set e to pressed
        }

        // rotate camera
        switch(currentCamKey){
            case CamKeys.Q:
                this.transform.RotateAround(this.transform.root.position, Vector3.up, 0.1f * BattleManager.Instance.arenaCamSpeed);
                break;

            case CamKeys.E:
                this.transform.RotateAround(this.transform.root.position, Vector3.up, -0.1f * BattleManager.Instance.arenaCamSpeed);
                break;
        }


        // Reset state when the key is released
        if (Input.GetKeyUp(KeyCode.Q) && currentCamKey == CamKeys.Q)
        {
            currentCamKey = CamKeys.None;  // set none pressed when q released
        }

        if (Input.GetKeyUp(KeyCode.E) && currentCamKey == CamKeys.E)
        {
            currentCamKey = CamKeys.None;  // set none pressed when e released
        }
    }
}
