using UnityEngine;
namespace cowsins
{
    public class Scope : Attachment
    {
        [Header("Scope")]
        [Tooltip("Vector3 added to the position of th weapon when aiming with this scoped equipped.")] public Vector3 aimingPosition;
        [Tooltip("Rotation of the weapon when aiming with this scope equipped.")] public Vector3 aimingRotation;

        // Place custom code for Stocks here if needed!
    }
}