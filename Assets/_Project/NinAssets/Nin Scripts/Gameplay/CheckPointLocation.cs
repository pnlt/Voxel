using UnityEngine;

public class CheckPointLocation : MonoBehaviour
{
    [SerializeField] private float checkPointRecoverTime;
    private float _checkPointRecoverTimer;
    private bool _isInRecover;

    /// <summary>
    /// Restoring time after being interacted
    /// </summary>
    private void Restoring()
    {
        if (_isInRecover)
        {
            //Start recovering
            _checkPointRecoverTimer += Time.deltaTime;
            if (_checkPointRecoverTimer >= checkPointRecoverTime)
            {
                _checkPointRecoverTimer = 0;
                _isInRecover = false;
            }
        }
    }

    private void Update()
    {
        Restoring();
    }


    /// <summary>
    /// Check if player has the secretBox to allow player get Voxel
    /// </summary>
    /// <param name="obj"></param>
    private void OnTriggerEnter(Collider obj)
    {
        if (obj.TryGetComponent<PlayerBuff>(out var playerBuff) && obj.TryGetComponent<PlayerFunction>(out var player))
        {
            if (player.hasSecretBox)
            {
                //If Checkpoint is in restoring state
                if (_isInRecover)
                {
                    player.CanInteractVoxel = false;
                }
                else
                {
                    player.CanInteractVoxel = true;
                    if (player.Interacted)
                        _isInRecover = true;
                }
                
                //Show UI to get Voxel
                var showUIFlag = player.CanInteractVoxel;
                playerBuff.GetVoxelUI.SetActive(showUIFlag);
            }
        }
        
        //Allow player to use Pick up button to interact and get the Voxel
    }

    private void OnTriggerStay(Collider obj)
    {
        if (obj.TryGetComponent<PlayerBuff>(out var playerBuff) && obj.TryGetComponent<PlayerFunction>(out var player))
        {
            if (player.hasSecretBox)
            {
                //If Checkpoint is in restoring state
                if (_isInRecover)
                {
                    Debug.Log("can not interact");
                    player.CanInteractVoxel = false;
                }
                else
                {
                    Debug.Log("can interact");
                    player.CanInteractVoxel = true;
                    if (player.Interacted)
                        _isInRecover = true;
                }
                
                //Show UI to get Voxel
                var showUIFlag = player.CanInteractVoxel;
                playerBuff.GetVoxelUI.SetActive(showUIFlag);
            }
            else
            {
                Debug.Log("can not interact");
                player.CanInteractVoxel = false;
                playerBuff.GetVoxelUI.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Turn off UI for player to get Voxel
    /// </summary>
    /// <param name="obj"></param>
    private void OnTriggerExit(Collider obj)
    {
        if (obj.TryGetComponent<PlayerBuff>(out var playerBuff) && obj.TryGetComponent<PlayerFunction>(out var player))
        {
            if (player.hasSecretBox)
            {
                playerBuff.GetVoxelUI.SetActive(false);
            }

            player.CanInteractVoxel = false;
        }
        
    }
}
