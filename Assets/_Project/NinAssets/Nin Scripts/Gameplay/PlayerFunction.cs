using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerFunction : NetworkBehaviour
{
    public float timeToCount;

    public NetworkVariable<ulong> luckyNum = new NetworkVariable<ulong>();
    public bool hasSecretBox;
    
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

    private void CooldownCalculation()
    {
        timeToCount -= Time.deltaTime;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        luckyNum.OnValueChanged += SelectionProcess;
        if (IsServer && OwnerClientId == 0)
        {
            Debug.Log("gg");
            luckyNum.Value = (ulong)Random.Range(1, 101);
        }
        else
        {
            ChosenOneServerRpc(luckyNum.Value);
        }
    }

    private void SelectionProcess(ulong previousvalue, ulong newvalue)
    {
        luckyNum.OnValueChanged -= SelectionProcess;
        if (IsServer)
        {
            if (newvalue <= 50)
                luckyNum.Value = 1;
            else
                luckyNum.Value = 0;
        }
        
        ChosenOneServerRpc(luckyNum.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChosenOneServerRpc(ulong val)
    {
        //ChosenOneClientRpc(val);
        if (val == OwnerClientId)
            Debug.Log(OwnerClientId + "Get secretBox" + val);
        else
            Debug.Log(OwnerClientId + "Nope" + val);
    }

    [ClientRpc]
    private void ChosenOneClientRpc(ulong val)
    {
    }

    private void Start()
    {
    }

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