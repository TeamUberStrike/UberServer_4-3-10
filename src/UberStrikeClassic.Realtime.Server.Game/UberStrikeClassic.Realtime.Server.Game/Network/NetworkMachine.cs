using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.Realtime.Common;

namespace UberStrikeClassic.Realtime.Server.Game.Network
{
    public class NetworkMachine
    {
        private Dictionary<INetworkClassType, NetworkClass> handlers;

        public NetworkMachine() 
        {
            handlers = new Dictionary<INetworkClassType, NetworkClass>();
        }

        public void Register(INetworkClassType type, NetworkClass handler) 
        {
            if (!handlers.ContainsKey(type))
                handlers.Add(type, handler);
        }

        public void ProcessMessage(INetworkClassType handlertype, HandlerType type, GamePeer peer, short networkID, byte RpcId, byte[] data,
            int playerid = default, short invoc = default) 
        {
            if(handlers.TryGetValue(handlertype, out NetworkClass handler)) 
            {
                switch (type) 
                {
                    case HandlerType.Server:
                        handler.MessageToServer(peer, networkID, RpcId, data);
                        break;
                    case HandlerType.Others:
                        handler.MessageToOthers(peer, networkID, RpcId, data);
                        break;
                    case HandlerType.Player:
                        handler.MessageToPlayer(peer, playerid, networkID, RpcId, data);
                        break;
                    case HandlerType.Application:
                        handler.MessageToApplication(peer, invoc, RpcId);
                        break;
                }
            }
        }

        public void ProcessPhotonGameEvent(PhotonGameType gametype, GamePeer peer, RoomMetaData roomdata=default, int cmid=default, 
            int accesslevel=default, CmuneRoomID id = default) 
        {

            if (handlers.TryGetValue(INetworkClassType.PeerMode, out NetworkClass handler))
            {
                switch (gametype)
                {
                    case PhotonGameType.Join:
                        handler.OnJoinGame(peer, roomdata, cmid, accesslevel);
                        break;
                    case PhotonGameType.Leave:
                        handler.OnLeaveGame(peer, id);
                        break;
                }
            }
        }
    }
}
