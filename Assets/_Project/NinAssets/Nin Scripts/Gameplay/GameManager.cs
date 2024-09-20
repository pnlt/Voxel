using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static List<Voxel> voxels = new();
    public static Action<Voxel> GetVoxel;
    
    // [ContextMenu("Reach checkpoint")]
    // private void DisplayResult()
    // {
    //     Debug.Log("Collect voxel successfully");
    //     GetVoxel.Invoke(null);
    // }
    //
    // private void Update()
    // {
    //     if (voxels.Count == 3)
    //         Debug.Log("Secret Box opened");
    // }
}
