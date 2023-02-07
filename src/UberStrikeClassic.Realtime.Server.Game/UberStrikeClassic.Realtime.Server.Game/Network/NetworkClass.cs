using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.Realtime.Common;

namespace UberStrikeClassic.Realtime.Server.Game.Network
{
    public class NetworkClass
    {
        public NetworkClass() 
        {

        }

        public virtual void MessageToServer(GamePeer peer, short networkId, byte RpcId, byte[] data) 
        {

        }

        public virtual void MessageToOthers(GamePeer peer, short networkId, byte RpcId, byte[] data) 
        {

        }
        public virtual void MessageToPlayer(GamePeer peer, int playerId, short networkId, byte RpcId, byte[] data) 
        {

        }

        public virtual void MessageToApplication(GamePeer peer, short invocationid, byte RpcId) 
        {

        }

        public virtual void OnLeaveGame(GamePeer peer, CmuneRoomID room) 
        {

        }

        public virtual void OnJoinGame(GamePeer peer, RoomMetaData roomdata, int cmid, int accesslvl) 
        {

        }
    }
}
