using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack.Assets_ăn_trộm._External_Assets.Infima_Games.Low_Poly_Shooter_Pack.Code.GameFramework.Manager
{
    public class RelayManager : Core.Singleton<RelayManager>
    {

        private bool _isHost = false;
        private string _joinCode;
        private string _ip;
        private int _port;
        private byte[] _key;
        private byte[] _connectionData;
        private byte[] _hostConnectionData;
        private System.Guid _allocationId;
        private byte[] _allocationIdBytes;

        public bool IsHost
        {
            get { return _isHost; }
        }

        public string GetAllocationId()
        {
            return _allocationId.ToString();
        }
        public string GetConnectionData()
        {
            return _connectionData.ToString();
        }
        
        public async Task<string> CreateRelay(int maxConnection)
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(conn => conn.ConnectionType == "dtls");
            _ip = dtlsEndpoint.Host;
            _port = dtlsEndpoint.Port;
            
            _allocationId = allocation.AllocationId;
            _allocationIdBytes = allocation.AllocationIdBytes;
            _connectionData = allocation.ConnectionData;
            _key = allocation.Key;

            _isHost = true; 
            
            return _joinCode;
        }
        
        public async Task<bool> JoinRelay(string joinCode)
        {
            
            _joinCode = joinCode;
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(conn => conn.ConnectionType == "dtls");
            _ip = dtlsEndpoint.Host;
            _port = dtlsEndpoint.Port;
            
            _allocationId = allocation.AllocationId;
            _allocationIdBytes = allocation.AllocationIdBytes;
            _connectionData = allocation.ConnectionData;
            _hostConnectionData = allocation.HostConnectionData;
            _key = allocation.Key;

            return true;
        }
        
        public (byte[] AllocationId, byte[] Key, byte[] ConnectionData, string _dtlsAddress, int _dtlsPort) GetHostConnectionInfo()
        {
            return (_allocationIdBytes, _key, _connectionData, _ip, _port);
        }
        public (byte[] AllocationId, byte[] Key, byte[] ConnectionData, byte[] HostConnectionData,  string _dtlsAddress, int _dtlsPort) GetClientConnectionInfo()
        {
            return (_allocationIdBytes, _key, _connectionData, _hostConnectionData, _ip, _port);
        }
        

    }
}