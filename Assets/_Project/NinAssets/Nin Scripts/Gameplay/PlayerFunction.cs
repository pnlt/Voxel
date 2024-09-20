using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerFunction : NetworkBehaviour
{
    public bool HasSecretBox { get; set; }

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
    
    public void GetSecretBox(NetworkObjectReference box)
    {
        if (box.TryGet(out var secretBox))
        {
            _box = secretBox.GetComponent<SecretBox>();
        }
    }
    
    private bool _isCheckPointRestoring;

    private void Update()
    {
        if (!IsOwner) return;
        
        //Check if player can interact the button to get the Voxels. Avoiding spam
        if (Input.GetKeyDown(KeyCode.F) && _isInteractVoxel)
        {
            _interacted = true;
            //Emerge the UI to congratulate for getting the Voxels successfully
            
            //Player gets Voxel and its respective effect
            
            //The num of voxels are collected for box increase 1
            _box.AssignVoxelServerRpc();
        }
        else
        {
            _interacted = false;
        }
    }
}