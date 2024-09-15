using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;

namespace InfimaGames.LowPolyShooterPack._Project.Scripts.Game
{
    public class PlayerHud : NetworkBehaviour
    {
        private NetworkVariable<NetworkString> playersName = new NetworkVariable<NetworkString>();

        private bool overlaySet = false;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                playersName.Value = $"Player {OwnerClientId}";
            }
        }

        public void SetOverlay()
        {
            var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            localPlayerOverlay.text = playersName.Value;
        }

        private void Update()
        {
            if (!overlaySet && !string.IsNullOrEmpty(playersName.Value))
            {
                SetOverlay();
                overlaySet = true;
            }
        }
    }
    
}