using Cmune.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Comm.Helper
{
    public class GameChatEvent
    {
        public GameChatEvent(CommPeer peer, string message,byte context) 
        {
            Peer = peer;
            Message = message;
            ChatContext = context;
        }

        public CommPeer Peer { get; set; }

        public string Message { get; set; }

        public byte ChatContext { get; set; }

        public string SenderName { get { return Peer.Actor.Name; } }

        public MemberAccessLevel SenderAccessLvl { get { return Peer.Actor.AccessLevel; } }

        public int SenderCmid { get { return Peer.Actor.Cmid; } }

        public int SenderActorId { get { return Peer.Actor.View.ActorId; } }
    }
}
