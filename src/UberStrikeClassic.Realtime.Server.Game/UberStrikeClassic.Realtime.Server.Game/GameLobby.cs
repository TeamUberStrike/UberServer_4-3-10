using Cmune.DataCenter.Common.Entities;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Utils;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using Photon.SocketServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Core.UberStrok;

namespace UberStrikeClassic.Realtime.Server.Game
{
    public class GameLobby : IDisposable
    {
        private bool _disposed;

        private readonly List<GamePeer> _peers;
        private readonly List<GamePeer> _unlockedPeers;

        public ICollection<GamePeer> Peers;

        public LobbyRoomManager Rooms;

        private readonly LoopScheduler Scheduler;

        private readonly Loop Loop;

        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public GameLobby() 
        {
            _peers = new List<GamePeer>();
            _unlockedPeers = new List<GamePeer>();

            Peers = _unlockedPeers.AsReadOnly();

            Rooms = new LobbyRoomManager();

            Loop = new Loop(OnTick, OnTickError);

            Scheduler = new LoopScheduler(15f);

            Scheduler.Schedule(Loop);

            Scheduler.Start();
        }

        public void Join(GamePeer peer) 
        {
            lock(_peers)
            {
                if (!_peers.Contains(peer))
                {
                    _peers.Add(peer);
                }
            }
        }

        public GamePeer Find(int connectionId) 
        {
            foreach(var peer in Peers) 
            {
                if (peer.ConnectionId == connectionId) return peer;
            }

            return null;
        }

        public void Leave(GamePeer peer) 
        {
            lock(_peers)
            {
                if (_peers.Contains(peer))
                {
                    _peers.Remove(peer);
                }
            }

            log.Info("GamePeer Left LobbyCenter");
        }

        private void OnTick() 
        {
            _unlockedPeers.Clear();

            lock(_peers)
            {
                _unlockedPeers.AddRange(_peers);
            }

            Rooms.Tick();
        }

        private void OnTickError(Exception ex) 
        {
            log.ErrorFormat("OnTick Error: {0}", ex);
        }

        public void Dispose() 
        {
            if (_disposed)
                return;

            Scheduler.Dispose();

            _disposed = true;
        }
    }
}
