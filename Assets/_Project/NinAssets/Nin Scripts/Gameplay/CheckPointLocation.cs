using UnityEngine;

public class CheckPointLocation : MonoBehaviour
{
    private Voxel _voxel;

    private void OnEnable()
    {
        GameManager.GetVoxel += GiveVoxel;
    }

    /// <summary>
    /// Grant the voxel for player who reaches the checkpoint
    /// </summary>
    /// <param name="voxel"></param>
    private void GiveVoxel(Voxel voxel)
    {
        GameManager.voxels.Add(voxel);
    }

    private void OnDisable()
    {
        GameManager.GetVoxel -= GiveVoxel;
    }
}
