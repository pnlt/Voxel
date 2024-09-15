using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class NetworkUI : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI playersCountText;

        private NetworkVariable<int> playersNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);

        private void Update()
        {
            playersCountText.text ="Players:"+ playersNum.Value.ToString();
            if (!IsServer) return;
            playersNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }
}
