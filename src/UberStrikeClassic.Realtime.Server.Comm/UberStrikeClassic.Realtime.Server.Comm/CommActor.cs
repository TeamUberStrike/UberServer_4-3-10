using Cmune.DataCenter.Common.Entities;
using Cmune.Realtime.Common;
using ExitGames.Logging;
using Photon.SocketServer;
using Photon.SocketServer.Rpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Core.Serialization;

namespace UberStrikeClassic.Realtime.Server.Comm
{
    public class CommActor
    {
        public CommPeer Peer { get; }

        public CommActorInfo View { get; }

        public bool IsMuted { get; set; }

        public DateTime MuteEndTime { get; set; }

        public int Cmid => View.Cmid;

        public string Name => View.PlayerName;

        public MemberAccessLevel AccessLevel => (MemberAccessLevel)View.AccessLevel;

        public HashSet<int> ContactList { get; set; }

        public CommActor(CommPeer peer, CommActorInfo info) 
        {
            Peer = peer ?? throw new ArgumentNullException(nameof(peer));

            View = info ?? throw new ArgumentNullException(nameof(info));

            ContactList = new HashSet<int>();
        }
    }
}
