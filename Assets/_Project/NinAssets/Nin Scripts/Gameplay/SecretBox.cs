using UnityEngine;

public class SecretBox : MonoBehaviour
{
    public enum BoxState
    {
        POSSESSED,
        DROPPED
    }

    public PlayerBuff _player;
    public BoxState state;
    
    private bool _activated;
    private static int _voxelAvailable;
    private int _maxVoxels = 3;

    public int MaxVoxels => _maxVoxels;
    
    /// <summary>
    /// Secret box takes voxel to get opened
    /// </summary>
    public void AssignVoxel()
    {
        ActivateSecretBox();
        _voxelAvailable++;
    }

    private void ChangeState(BoxState state, PlayerBuff player)
    {
        this.state = state;
        if (this.state == BoxState.POSSESSED)
            _player = player;
        else if (this.state == BoxState.DROPPED)
            _player = null;
    }

    private void ActivateSecretBox()
    {
        //If the number of Voxels reach 3
        if (_voxelAvailable == _maxVoxels)
            _activated = true;
    }

    private void OnTriggerEnter(Collider collider)
    {
        //if some player collide with the secretBox
        if (collider.TryGetComponent<PlayerFunction>(out var player))
        {
            player.hasSecretBox = true;
        }
    }
}