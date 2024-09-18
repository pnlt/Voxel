using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretBox : MonoBehaviour
{
    private bool _activated;
    private static int _voxelAvailable;

    private void SecretBoxOpen()
    {
        ActivateSecretBox();
        _voxelAvailable++;
    }

    private void ActivateSecretBox()
    {
        //If the number of Voxels reach 3
        if (_voxelAvailable == 3)
            _activated = true;
    }
}
