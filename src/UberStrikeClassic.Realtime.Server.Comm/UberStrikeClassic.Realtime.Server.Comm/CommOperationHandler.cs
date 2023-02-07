using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Comm.Core;
using Cmune.DataCenter.Common.Entities;
using UberStrikeClassic.Realtime.Server.Comm.Helper;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.Synchronization;

namespace UberStrikeClassic.Realtime.Server.Comm
{
    public class CommOperationHandler : BaseCommOperationHandler
    {
        public override void OnJoinCommPeer(CommPeer peer, CommActorInfo actor)
        {
            if(peer != null && actor != null) 
            {
                peer.AddActorInfo(actor);

                CommServerApplication.Instance.Room.Join(peer);
            }
            else { }
        }

        public override void OnPlayerBanned(CommPeer peer, int cmid)
        {
            if(peer.Actor.AccessLevel > MemberAccessLevel.Default) 
            {
                var Player = CommServerApplication.Instance.Room.Find(cmid);

                if(Player != null) 
                {
                   // Player.Events.SendPlayerBannedMessage("Your account has been banned.");
                }
            }
        }

        public override void OnLobbyChatMessage(int actorId, string message)
        {
            if(actorId != -1 && !string.IsNullOrEmpty(message)) 
            {
                CommServerApplication.Instance.Room.SendChatMessageInLobby(actorId, message);
            }
        }

        public override void OnUpdateActorInfo(int actorId, SyncObject syncActor)
        {
            var CommPeer = CommServerApplication.Instance.Room.Find(actorId);

            if(CommPeer != null) 
            {
                CommPeer.Actor.View.ReadSyncData(syncActor);

                foreach(CommPeer peer in CommServerApplication.Instance.Room.CurrentPeers) 
                {
                    peer.Events.SendUpdateActorInfo(syncActor);
                }
            }
        }

        public override void OnUpdatePlayerRoom(int actorId, CmuneRoomID room)
        {
            var CommPeer = CommServerApplication.Instance.Room.Find(actorId);

            if(CommPeer != null && !room.IsEmpty) 
            {
                CommPeer.UpdateActorRoom(room);

                SyncObject updatedActor = SyncObjectBuilder.GetSyncData(CommPeer.Actor.View, false);

                foreach (CommPeer peer in CommServerApplication.Instance.Room.CurrentPeers)
                {
                  if(peer.PeerID != CommPeer.PeerID) peer.Events.SendUpdateActorInfo(updatedActor);
                }
            }
        }

        public override void OnResetPlayerRoom(int actorId)
        {
            var CommPeer = CommServerApplication.Instance.Room.Find(actorId);

            if(CommPeer != null) 
            {
                CommPeer.ResetActorRoom();

                SyncObject updatedActor = SyncObjectBuilder.GetSyncData(CommPeer.Actor.View, false);

                foreach (CommPeer peer in CommServerApplication.Instance.Room.CurrentPeers)
                {
                    if (peer.PeerID != CommPeer.PeerID) peer.Events.SendUpdateActorInfo(updatedActor);
                }
            }
        }

        public override void OnGameIngameChatMessage(GameChatEvent args)
        {
            CmuneRoomID CurrentRoom = args.Peer.Actor.View.CurrentRoom;

            if(CurrentRoom != null) 
            {
                foreach(CommPeer peer in CommServerApplication.Instance.Room.CurrentPeers) 
                {
                    if(peer.PeerID != args.Peer.PeerID && peer.Actor.View.CurrentRoom.ID == CurrentRoom.ID) 
                    {
                        peer.Events.SendIngameChatMessage(args.SenderCmid, args.SenderActorId, args.SenderName, 
                            args.Message, args.SenderAccessLvl, args.ChatContext);
                    }
                }
            }
        }

        public override void OnSetContactList(int cmid, HashSet<int> contacts)
        {
            var peer = CommServerApplication.Instance.Room.Find(cmid);

            if(peer != null)
            {
                if(peer.Actor.ContactList.Count > 0)
                {
                    foreach(int i in contacts)
                    {
                        if (peer.Actor.ContactList.Contains(i)) continue;

                        peer.Actor.ContactList.Add(i);
                    }
                }
                else
                {
                    peer.Actor.ContactList = contacts;
                }
            }
        }

        public override void OnUpdateContacts(int cmid)
        {
            var peer = CommServerApplication.Instance.Room.Find(cmid);

            if (peer != null)
            {
                if(peer.Actor.ContactList.Count > 0)
                {
                    List<SyncObject> updated = new List<SyncObject>();
                    List<int> removed = new List<int>();

                    foreach(int contact in peer.Actor.ContactList)
                    {
                        var contactPeer = CommServerApplication.Instance.Room.Find(contact);

                        if(contactPeer != null && contactPeer.Actor.View != null)
                        {
                            var syncObj = SyncObjectBuilder.GetSyncData(contactPeer.Actor.View, false);

                            if (!syncObj.IsEmpty)
                                updated.Add(syncObj);
                        }
                        else
                        {
                            removed.Add(contactPeer.Actor.Cmid);
                        }
                    }

                    peer.Events.SendContactsUpdate(updated, removed);
                }
            }
        }

        public override void OnModerationCustomMessage(int actorId, string message)
        {
            var peer = CommServerApplication.Instance.Room.Find(actorId);

            if (peer != null)
            {
                if(peer.Actor.View.CurrentRoom.Number == StaticRoomID.CommCenter || peer.Actor.View.CurrentRoom.Number == StaticRoomID.LobbyCenter)
                {
                    peer.Events.SendModerationMessage(message);
                }
                else
                {
                    // Send mod msg ingame
                }
            }
        }
    }
}
