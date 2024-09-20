using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.Events;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using LobbyEvents = InfimaGames.LowPolyShooterPack.Assets_ăn_trộm._External_Assets.Infima_Games.Low_Poly_Shooter_Pack.Code.GameFramework.Events.LobbyEvents;

namespace InfimaGames.LowPolyShooterPack.Assets_ăn_trộm._External_Assets.Infima_Games.Low_Poly_Shooter_Pack.Code.GameFramework.Manager
{
    public class LobbyManager : Core.Singleton<LobbyManager>
    {
        private static Lobby _lobby;
        private Coroutine _heartbeatCoroutine;
        private static CancellationTokenSource _updateLobbySource;
        private const int LobbyRefreshRate = 2;
        //private Coroutine _refreshLobbyCoroutine;

        private List<string> _joinedLobbiesId;
        
       public async Task<bool> HasActiveLobbies()
       {
           _joinedLobbiesId = await LobbyService.Instance.GetJoinedLobbiesAsync();
           if (_joinedLobbiesId.Count > 0)
           {
               return true;
           }

           return false;
       }
        
        public string GetLobbyCode()
        {
            return _lobby?.LobbyCode;
        }
        public async Task<bool> CreateLobby(int maxPlayers, bool isPrivate, Dictionary<string, string> data, Dictionary<string, string> lobbyData)
        {
            Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
            Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);
            
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                Data = SerializeLobbyData(lobbyData),
                IsPrivate = isPrivate,
                Player = player
            };
            try
            {
                _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayers, options);
            }
            catch (System.Exception)
            {
                return false;
            }
            
            //Debug.Log($"Lobby created with lobby id {_lobby.Id}");

            _heartbeatCoroutine = StartCoroutine(HeartbeatLobbyCoroutine(_lobby.Id, 6f));
            //_refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
            PeriodicallyRefreshLobby();
            
            return true;
        }


        private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
                //Debug.Log("Heartbeat");
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return new WaitForSecondsRealtime(waitTimeSeconds);
            }
        }

       // private IEnumerator RefreshLobbyCoroutine(string lobbyId, float waitTimeSeconds)
       // {
       //     while (true)
       //     {
       //         Task<Lobby> task = LobbyService.Instance.GetLobbyAsync(lobbyId);
       //         yield return new WaitUntil(() => task.IsCompleted);
       //         Lobby newLobby = task.Result;
       //         if (newLobby.LastUpdated > _lobby.LastUpdated)
       //         {
       //             _lobby = newLobby;
       //             LobbyEvents.OnLobbyUpdated?.Invoke(_lobby);
       //         }
       //         yield return new WaitForSecondsRealtime(waitTimeSeconds);
       //     }
       // }
       
       private static async void PeriodicallyRefreshLobby()
       {
           _updateLobbySource = new CancellationTokenSource();
           await Task.Delay(LobbyRefreshRate * 1000); // Initial delay
           while (!_updateLobbySource.IsCancellationRequested && _lobby != null)
           {
               _lobby = await Lobbies.Instance.GetLobbyAsync(_lobby.Id);
               LobbyEvents.OnLobbyUpdated?.Invoke(_lobby);
               await Task.Delay(LobbyRefreshRate * 1000); // Delay between updates
           }
       }

        private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
        {
            Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();
            foreach (var (key, value) in data)
            {
                playerData.Add(key, new PlayerDataObject(
                    visibility: PlayerDataObject.VisibilityOptions.Member, // Visible only to members of the lobby
                    value: value));
            }
            return playerData;
        }
        
        private Dictionary<string, DataObject> SerializeLobbyData(Dictionary<string, string> data)
        {
            Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>();
            foreach (var (key, value) in data)
            {
                lobbyData.Add(key, new DataObject(
                    visibility: DataObject.VisibilityOptions.Member,
                    value: value));
            }
            return lobbyData;
        }
        public void OnApplicationQuit()
        {
            if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
            }
        }

        public async Task<bool> JoinLobby(string code, Dictionary<string, string> playerData)
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
            Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));
            
            options.Player = player;

            try
            {
                _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);
            }
            catch (System.Exception)
            {
                return false;
            }
            
            //_refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
            PeriodicallyRefreshLobby();

            return true;
        }

        public List<Dictionary<string, PlayerDataObject>> GetPlayersData()
        {
            List<Dictionary<string, PlayerDataObject>> data = new List<Dictionary<string, PlayerDataObject>>();

            foreach (Player player in _lobby.Players)
            {
                data.Add(player.Data);
            }

            return data;
        }

        public async Task<bool> UpdatePlayerData(string playerId, Dictionary<string, string> data, string allocationId = default, string connectionData = default)
        {
            Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
            UpdatePlayerOptions options = new UpdatePlayerOptions()
            {
                Data = playerData,
                AllocationId = allocationId,
                ConnectionInfo = connectionData
            };
            try
            {
               _lobby = await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, playerId, options);
            }
            catch (System.Exception)
            {
                return false;
            }

            LobbyEvents.OnLobbyUpdated(_lobby);
            return true;
        }

        public async Task<bool> UpdateLobbyData(Dictionary<string, string> data)
        {
           Dictionary<string, DataObject> lobbyData = SerializeLobbyData(data); 
           
           UpdateLobbyOptions options = new UpdateLobbyOptions()
           {
               Data = lobbyData
           };
           try
           {
                _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, options);
           }
           catch (System.Exception)
           {
               return false;
           }
           LobbyEvents.OnLobbyUpdated(_lobby);
           return true;
        }

        public string GetHostId()
        {
            return _lobby.HostId;
        }

        public async Task<bool> RejoinLobby()
        {
            try
            {
                _lobby = await LobbyService.Instance.ReconnectToLobbyAsync(_joinedLobbiesId[0]);
                LobbyEvents.OnLobbyUpdated(_lobby);
            }
            catch (System.Exception)
            {
                return false;
            }
            //_refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_joinedLobbiesId[0], 1f));
            PeriodicallyRefreshLobby();
            return true;
        }

        public async Task<bool> LeaveAllLobby()
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            foreach (string lobbyId in _joinedLobbiesId)
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
                } catch (System.Exception)
                {
                    return false;
                }
            }

            return true;
        }
    }
}