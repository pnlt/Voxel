using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CheckPointAchievement : NetworkBehaviour
{
    [SerializeField] private Voxel _voxel;

    private PlayerBuff _playerBuff;
    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    /// <summary>
    /// Grant the voxel for player who reaches the checkpoint
    /// </summary>
    /// <param name="voxel"></param>
    /// <param name="t">message</param>
    private void GiveVoxel(object t)
    {
        //_playerBuff.ApplyEffect(_voxel.Effect);
        Debug.Log("cac");
    }
    
    private void OnTriggerEnter(Collider obj)
    {
        if (obj.TryGetComponent<PlayerBuff>(out var playerBuff) && obj.TryGetComponent<PlayerFunction>(out var player))
        {
            if (player.HasSecretBox)
            {
                _playerBuff = playerBuff;
            }
        }
    }

    private void OnTriggerStay(Collider obj)
    {
        if (obj.TryGetComponent<PlayerBuff>(out var playerBuff) && obj.TryGetComponent<PlayerFunction>(out var player))
        {
            if (player.HasSecretBox)
            {
                _playerBuff = playerBuff;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
}