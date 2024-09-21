using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Selector : NetworkBehaviour
{
    [Header("UI")]
    public GameObject keepBoxUI;
    public GameObject robBoxUI;
    [SerializeField] private float waitTime;
    
    [Header("Secret box")]
    public SecretBox secretBox;
    
    private NetworkVariable<ulong> luckyNum = new NetworkVariable<ulong>();
    private SecretBox _secretBoxOnNetwork;
    private NetworkVariable<ulong> _networkObjId = new NetworkVariable<ulong>();
    
    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SelectionProcess;
        base.OnNetworkSpawn();
        
        if (IsServer && IsOwner)
        {
            luckyNum.Value = (ulong)Random.Range(55, 101);
        }

        if (!IsOwner) return;
        SpawnSecretBoxServerRpc();
    }
    
    private void SelectionProcess(ulong val)
    {
        if (!IsOwner) return;
        
        if (luckyNum.Value <= 50)
            ChosenOneServerRpc(1);
        else
            ChosenOneServerRpc(0);
    }
    
    [ServerRpc]
    private void SpawnSecretBoxServerRpc()
    {
        //Spawn SecretBox on network for synchronization in gameplay
        _secretBoxOnNetwork = Instantiate(secretBox, Vector3.zero, Quaternion.identity);
        _secretBoxOnNetwork.GetComponent<NetworkObject>().Spawn(true);

        ReceiveSecretBoxClientRpc(_secretBoxOnNetwork.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    private void ReceiveSecretBoxClientRpc(NetworkObjectReference reference)
    {
        _networkObjId.Value = reference.NetworkObjectId;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ChosenOneServerRpc(ulong val)
    {
        ChosenOneClientRpc(val);
    }

    [ClientRpc]
    private void ChosenOneClientRpc(ulong val)
    {
        if (val == NetworkManager.Singleton.LocalClientId)
        {
            DefenseToggleNotification();
            
            //Notify the player who is dictated to keep Box already possessed the secret box
            if (!IsHost && !_secretBoxOnNetwork)
            {
                var spawnedObjs = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
                foreach (var objs in spawnedObjs)
                {
                    if (objs.Key == _networkObjId.Value)
                        _secretBoxOnNetwork = objs.Value.GetComponent<SecretBox>();
                }
            }
            
            var playerNetworkObject = NetworkManager.LocalClient.PlayerObject;
            playerNetworkObject.GetComponent<PlayerFunction>().HasSecretBox = true;
            playerNetworkObject.GetComponent<PlayerFunction>().GetSecretBox(_secretBoxOnNetwork.GetComponent<NetworkObject>());
        }
        else
        {
            AttackToggleNotification();
            if (!IsHost && !_secretBoxOnNetwork)
            {
                var spawnedObjs = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
                foreach (var objs in spawnedObjs)
                {
                    if (objs.Key == _networkObjId.Value)
                        _secretBoxOnNetwork = objs.Value.GetComponent<SecretBox>();
                }
            }
            
            var playerNetworkObject = NetworkManager.LocalClient.PlayerObject;
            playerNetworkObject.GetComponent<PlayerFunction>().GetSecretBox(_secretBoxOnNetwork.GetComponent<NetworkObject>());
        }

    }

    private async void DefenseToggleNotification()
    {
        keepBoxUI.SetActive(true);

        await UniTask.WaitForSeconds(waitTime);
        
        keepBoxUI.SetActive(false);
    }
    
    private async void AttackToggleNotification()
    {
        robBoxUI.SetActive(true);

        await UniTask.WaitForSeconds(waitTime);
        
        robBoxUI.SetActive(false);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkManager.Singleton.OnClientConnectedCallback -= SelectionProcess;
    }
}
