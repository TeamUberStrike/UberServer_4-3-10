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
    public class CommPeer : Peer
    {
        public CommActor Actor { get; set; }

        public CommEvents Events { get; set; }

        public int PeerID { get { return ConnectionId; } }

        public CommPeer(InitRequest initRequest) : base(initRequest)
        {
            SetCurrentOperationHandler(new CommOperationHandler());

            Events = new CommEvents(this);
        } 

        public void AddActorInfo(CommActorInfo info) 
        {
            Actor = new CommActor(this, info);
        }

        public void UpdateActorRoom(CmuneRoomID roomId) 
        {
            Actor.View.CurrentRoom = roomId;
        }

        public void ResetActorRoom() 
        {
            //Actor.View.CurrentRoom = CmuneRoomID.Empty;

            Actor.View.CurrentRoom = new CmuneRoomID(StaticRoomID.CommCenter, string.Empty);
        }
    }
}
