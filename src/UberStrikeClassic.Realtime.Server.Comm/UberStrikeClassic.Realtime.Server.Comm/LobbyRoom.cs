using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Synchronization;
using Cmune.Realtime.Common.Utils;
using ExitGames.Logging;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Comm
{
    public class LobbyRoom
    {
        private static readonly ILogger Logging = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        private readonly ICollection<CommPeer> Peers;

        public ICollection<CommPeer> CurrentPeers { get { return Peers; } }

        public LobbyRoom() 
        {
            Peers = new List<CommPeer>();
        }

        public void Join(CommPeer peer) 
        {
            if (!Peers.Contains(peer)) 
            {
                Peers.Add(peer);

                if (Logging.IsDebugEnabled) 
                {
                    Logging.DebugFormat("New CommPeer joined the Lobby. ActorID: {0}", peer.Actor.View.ActorId);
                }

                // FullActorsUpdate();
                List<SyncObject> syncObjs = new List<SyncObject>();
                foreach (CommPeer p in Peers)
                {
                    if (p.Actor.View != null && p.ConnectionId != peer.ConnectionId)
                    {
                        syncObjs.Add(SyncObjectBuilder.GetSyncData(p.Actor.View, true));
                    }
                }

                peer.Events.SendFullActorsList(syncObjs);

                SyncObject joinedSyncObj = SyncObjectBuilder.GetSyncData(peer.Actor.View, true);
                foreach (CommPeer p in Peers)
                {
                    if(p.ConnectionId != peer.ConnectionId)
                    {
                        p.Actor.ContactList.Add(peer.Actor.Cmid);
                    }
                    p.Events.SendContactsUpdate(new List<SyncObject>() { joinedSyncObj }, new List<int>());
                }
            }
        }

        public void Leave(CommPeer peer) 
        {
            if (Peers.Contains(peer)) 
            {
                Peers.Remove(peer);

                if (Logging.IsDebugEnabled)
                {
                    Logging.DebugFormat("CommPeer left the Lobby. ActorID: {0}", peer.Actor.View.ActorId);
                }
            }

            foreach(CommPeer current in Peers) 
            {
                current.Events.SendPlayerLeft(peer.Actor.Cmid);
            }
        }

        public CommPeer Find(int id) 
        {
            foreach(CommPeer peer in Peers) 
            {
                if (peer.Actor.Cmid == id || peer.Actor.View.ActorId == id) return peer;
            }

            return null;
        }

        public CommPeer FindPhotonBasePeer(int id)
        {
            foreach (CommPeer peer in Peers)
            {
                if (peer.PeerID == id) return peer;
            }

            return null;
        }

        public void FullActorsUpdate() 
        {
            List<SyncObject> syncObjs = new List<SyncObject>();

            foreach (CommPeer peer in Peers) 
            {
                if(peer.Actor.View != null) 
                {
                    syncObjs.Add(SyncObjectBuilder.GetSyncData(peer.Actor.View, true));
                }
            }

            foreach(CommPeer peer in Peers) { peer.Events.SendActorsUpdate(syncObjs); }
        }

        public void SendChatMessageInLobby(int senderActorId, string message) 
        {
            var sender = Find(senderActorId);

            if(sender != null) 
            {
                foreach(CommPeer peer in Peers) 
                {
                    if (peer != sender) peer.Events.SendLobbyMessage(sender.Actor.Cmid, sender.Actor.View.ActorId, sender.Actor.Name, message);
                }
            }
        }
    }
}
