using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using ExitGames.Concurrency.Fibers;
using Photon.SocketServer;
using Photon.SocketServer.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Core;
using UberStrikeClassic.Realtime.Server.Game.Events;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game
{
    public class GamePeer : Peer
    {
        public CmuneRoomID CurrentRoom { get; set; }

        public GameActor Actor { get; set; }

        public GamePeerEvents Events { get; }

        public int ActorId { get; set; }

        public GamePeer(InitRequest initRequest) : base(initRequest)
        {
            Events = new GamePeerEvents(this);

            SetCurrentOperationHandler(new GlobalOperationListener());
        }
    }
}
