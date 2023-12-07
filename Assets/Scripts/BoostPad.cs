using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BoostPad : NetworkBehaviour
{

    public void BoostPlayer(Player thePickerUpper)
    {
        Vector3 boostAmount = new Vector3(0, .3f, 0);
        thePickerUpper.transform.Translate(boostAmount);
        NetworkHelper.Log(this, "I am being boosted!");
    }
    
    
}
