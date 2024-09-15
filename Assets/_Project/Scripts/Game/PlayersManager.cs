using System;
using InfimaGames.LowPolyShooterPack.Assets_ăn_trộm._External_Assets.Infima_Games.Low_Poly_Shooter_Pack.Code.GameFramework.Core;
using PlayFab.MultiplayerModels;
using Unity.Netcode;

namespace InfimaGames.LowPolyShooterPack._Project.Scripts.Game
{
    public class PlayersManager : NetworkSingleton<PlayersManager>
    {
        private NetworkVariable<int> playersInGame = new NetworkVariable<int>();
        
        public int PlayersInGame
        {
            get
            {
                return playersInGame.Value;
            }
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    playersInGame.Value++;
                }
            };
            
            NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    playersInGame.Value--;
                }
            };
        }
    }
}