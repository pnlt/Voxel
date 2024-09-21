using System;
using Unity.Netcode;
using UnityEngine;

public class SecretBox : NetworkBehaviour
{
    public enum BoxState
    {
        POSSESSED,
        DROPPED
    }

    public PlayerBuff _player;
    public BoxState state;
    
    private bool _activated;
    public bool Activate =>  _activated;
    private NetworkVariable<int> _voxelAvailable = new NetworkVariable<int>();
    [SerializeField] private int _maxVoxels = 3;

    public int MaxVoxels => _maxVoxels;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _voxelAvailable.OnValueChanged += OnChangingValue;
    }

    private void OnChangingValue(int previousvalue, int newvalue)
    {
        ActivateSecretBoxServerRpc();
    }

    /// <summary>
    /// Secret box takes voxel to get opened
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void AssignVoxelServerRpc()
    {
        AssignVoxelClientRpc();
    }

    [ClientRpc]
    private void AssignVoxelClientRpc()
    {
        if (IsServer)
        {
            _voxelAvailable.Value++;
        }
    }

    private void ChangeState(BoxState state, PlayerBuff player)
    {
        this.state = state;
        if (this.state == BoxState.POSSESSED)
            _player = player;
        else if (this.state == BoxState.DROPPED)
            _player = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateSecretBoxServerRpc()
    {
        ActivateSecretBoxClientRpc();
    }

    [ClientRpc]
    private void ActivateSecretBoxClientRpc()
    {
        //If the number of Voxels reach the need voxels
        if (_voxelAvailable.Value == _maxVoxels)
        {
            _activated = true;
            var playerNetworkObj = NetworkManager.LocalClient.PlayerObject;
            var playerObj = playerNetworkObj.GetComponent<PlayerFunction>();
            

            if (playerObj.HasSecretBox)
            {
                Debug.LogError("Winner");
            }
            else
            {
                Debug.LogError("Lose");
            }
            
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        //if some player collide with the secretBox
        if (collider.TryGetComponent<PlayerFunction>(out var player))
        {
            player.HasSecretBox = true;
        }
    }
}