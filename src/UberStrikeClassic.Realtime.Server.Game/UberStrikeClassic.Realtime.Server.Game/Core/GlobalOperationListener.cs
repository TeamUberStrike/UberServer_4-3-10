using Cmune.DataCenter.Common.Entities;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Utils;
using Photon.SocketServer;
using Photon.SocketServer.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Operations;
using UberStrikeClassic.Realtime.Server.Game.Rooms;
using UberStrikeClassic.Realtime.Server.Game.Network;

namespace UberStrikeClassic.Realtime.Server.Game.Core
{
    public class GlobalOperationListener : IOperationHandler
    {
        private NetworkMachine NetworkMachine;

        public GlobalOperationListener()
        {
            NetworkMachine = new NetworkMachine();

            NetworkMachine.Register(INetworkClassType.GameMode, new GameOperationHandler());
            NetworkMachine.Register(INetworkClassType.PeerMode, new PeerOperationHandler());
        }

        public void OnDisconnect(PeerBase peer)
        {
            GamePeer Peer = (GamePeer)peer;

            if(Peer.Actor != null) 
            {
                GameRoom room = Peer.Actor.Room;

                if (room != null)
                {
                    room.Leave(Peer);
                }
            }

            GameApplication.Instance.Lobby.Leave(Peer);
        }

        public OperationResponse OnOperationRequest(PeerBase peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            var gamePeer = (GamePeer)peer;

            switch (operationRequest.OperationCode)
            {
                case CmuneOperationCodes.MessageToAll:
                   // HandleMessageToAll(gamePeer, operationRequest.Parameters, sendParameters); not implemented?
                    break;
                case CmuneOperationCodes.MessageToOthers:
                    HandleMessageToOthers(gamePeer, operationRequest.Parameters, sendParameters);
                    break;
                case CmuneOperationCodes.MessageToPlayer:
                    HandleMessageToPlayer(gamePeer, operationRequest.Parameters, sendParameters);
                    break;
                case CmuneOperationCodes.MessageToServer:
                    HandleMessageToServer(gamePeer, operationRequest.Parameters, sendParameters);
                    break;
                case CmuneOperationCodes.MessageToApplication:
                    HandleMessageToApplication(gamePeer, operationRequest, sendParameters);
                    break;
                case CmuneOperationCodes.PhotonGameJoin:
                    HandlePhotonJoin(gamePeer, operationRequest, sendParameters);
                    break;
                case CmuneOperationCodes.PhotonGameLeave:
                    HandlePhotonLeave(gamePeer, operationRequest, sendParameters);
                    break;
            }

            return null;
        }

        private void HandleMessageToPlayer(GamePeer sender, Dictionary<byte, object> opParams, SendParameters sendParameters)
        {
            int playerId = OperationUtil.GetArg<int>(opParams, ParameterKeys.ActorId);
            short networkId = OperationUtil.GetArg<short>(opParams, ParameterKeys.InstanceId);
            byte methodId = OperationUtil.GetArg<byte>(opParams, ParameterKeys.MethodId);
            byte[] data = OperationUtil.GetBytes(opParams);

            switch (networkId)
            {
                case GameModeID.TeamDeathMatch:
                case GameModeID.DeathMatch:
                case GameModeID.EliminationMode:
                case GameModeID.LastManStanding:
                case GameModeID.CaptureTheFlag:
                case GameModeID.FunMode:
                case GameModeID.CooperationMode:
                case GameModeID.ModerationMode:
                    NetworkMachine.ProcessMessage(INetworkClassType.GameMode, HandlerType.Player, sender, networkId, methodId, data, playerId);
                    break;
            }
        }

        private void HandleMessageToServer(GamePeer sender, Dictionary<byte, object> opParams, SendParameters sendParameters)
        {
            short networkId = OperationUtil.GetArg<short>(opParams, ParameterKeys.InstanceId);
            byte method = OperationUtil.GetArg<byte>(opParams, ParameterKeys.MethodId);
            byte[] data = OperationUtil.GetBytes(opParams);

            switch (networkId)
            {
                case GameModeID.TeamDeathMatch:
                case GameModeID.DeathMatch:
                case GameModeID.EliminationMode:
                case GameModeID.LastManStanding:
                case GameModeID.CaptureTheFlag:
                case GameModeID.FunMode:
                case GameModeID.CooperationMode:
                case GameModeID.ModerationMode:
                    NetworkMachine.ProcessMessage(INetworkClassType.GameMode, HandlerType.Server, sender, networkId, method, data);
                    break;
                case NetworkClassID.LobbyCenter:
                    NetworkMachine.ProcessMessage(INetworkClassType.PeerMode, HandlerType.Server, sender, networkId, method, data);
                    break;
                case NetworkClassID.ServerSyncCenter:
                    NetworkMachine.ProcessMessage(INetworkClassType.PeerMode, HandlerType.Server, sender, networkId, method, data);
                    break;
            }
        }

        private void HandleMessageToOthers(GamePeer sender, Dictionary<byte, object> opParams, SendParameters sendParameters)
        {
            short networkId = OperationUtil.GetArg<short>(opParams, ParameterKeys.InstanceId);
            byte method = OperationUtil.GetArg<byte>(opParams, ParameterKeys.MethodId);
            byte[] data = OperationUtil.GetBytes(opParams);

            switch (networkId)
            {
                case GameModeID.TeamDeathMatch:
                case GameModeID.DeathMatch:
                case GameModeID.EliminationMode:
                case GameModeID.LastManStanding:
                case GameModeID.CaptureTheFlag:
                case GameModeID.FunMode:
                case GameModeID.CooperationMode:
                case GameModeID.ModerationMode:
                    NetworkMachine.ProcessMessage(INetworkClassType.GameMode, HandlerType.Others, sender, networkId, method, data);
                    break;
            }
        }

        private void HandleMessageToApplication(GamePeer sender, OperationRequest operationRequest, SendParameters sendParameters) 
        {
            byte methodId = OperationUtil.GetArg<byte>(operationRequest.Parameters, ParameterKeys.MethodId);
            short invocationId = OperationUtil.GetArg<short>(operationRequest.Parameters, ParameterKeys.InvocationId);
            byte[] data = OperationUtil.GetBytes(operationRequest.Parameters);

            switch (methodId)
            {
                case GameApplicationRPC.PeerSpecification:
                    NetworkMachine.ProcessMessage(INetworkClassType.PeerMode, HandlerType.Application, sender, 0, methodId, data, 0, invocationId);
                    break;
                case GameApplicationRPC.QueryServerLoad:
                    NetworkMachine.ProcessMessage(INetworkClassType.PeerMode, HandlerType.Application, sender, 0, methodId, data, 0, invocationId);
                    break;
            }
        }

        private void HandlePhotonJoin(GamePeer peer, OperationRequest operationRequest, SendParameters sendParameters) 
        {
            RoomMetaData roomData = (RoomMetaData)RealtimeSerialization.ToObject(OperationUtil.GetBytes(operationRequest.Parameters));
            int cmid = OperationUtil.GetArg<int>(operationRequest.Parameters, ParameterKeys.Cmid);
            int accessLevel = OperationUtil.GetArg<int>(operationRequest.Parameters, ParameterKeys.AccessLevel);

            NetworkMachine.ProcessPhotonGameEvent(PhotonGameType.Join, peer, roomData, cmid, accessLevel);
        }

        private void HandlePhotonLeave(GamePeer peer, OperationRequest operationRequest, SendParameters sendParameters) 
        {
            CmuneRoomID roomId = new CmuneRoomID(OperationUtil.GetArg<byte[]>(operationRequest.Parameters, ParameterKeys.RoomId));

            NetworkMachine.ProcessPhotonGameEvent(PhotonGameType.Leave, peer, null, 0, 0, roomId);
        }
    }
}
