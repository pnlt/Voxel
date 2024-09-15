using System;
using System.Collections;
using System.Collections.Generic;
using InfimaGames.LowPolyShooterPack.Assets_ăn_trộm._External_Assets.Infima_Games.Low_Poly_Shooter_Pack.Code.GameFramework.Manager;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            if (RelayManager.Instance.IsHost)
            {
               NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApproval; 
               (byte[] allocationId, byte[] key, byte[] connectionData, string ip, int port) = RelayManager.Instance.GetHostConnectionInfo();
               NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(ip, (ushort)port, allocationId, key, connectionData, true);
               NetworkManager.Singleton.StartHost();
            }
            else
            {
               (byte[] allocationId, byte[] key, byte[] connectionData, byte[] hostConnectionData, string ip, int port) = RelayManager.Instance.GetClientConnectionInfo();
               NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(ip, (ushort)port, allocationId, key, connectionData, hostConnectionData, true);
               NetworkManager.Singleton.StartClient();
            }
        }

        private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Pending = false;
        }
    }
}
