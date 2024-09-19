using System;
using UnityEngine;

public class PlayerFunction : MonoBehaviour
{
    private bool _isInteractVoxel = false;
    private bool _interacted;
    public bool Interacted => _interacted;

    public bool CanInteractVoxel
    {
        get { return _isInteractVoxel;}
        set
        {
            _isInteractVoxel = value;
        }
    }
    private SecretBox _box;
    private bool _isCheckPointRestoring;
    

    private void Update()
    {
        //Check if player can interact the button to get the Voxels. Avoiding spam
        if (Input.GetKeyDown(KeyCode.F) && _isInteractVoxel)
        {
            _interacted = true;
            Debug.Log("interact");
            //Emerge the UI to congratulate for getting the Voxels successfully
            
            //Player gets Voxel and its respective effect
            
            //The num of voxels are collected for box increase 1
            //_box.AssignVoxel();
        }
        else
        {
            _interacted = false;
        }
    }
}
