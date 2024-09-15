using UnityEngine;

//This class describes the key, which is necessary part to be able to open secret box 
[CreateAssetMenu (fileName = "voxel", menuName = "Data/Voxel data")]
public class Voxel : ScriptableObject
{
    [SerializeField] private Effect effect;
}
