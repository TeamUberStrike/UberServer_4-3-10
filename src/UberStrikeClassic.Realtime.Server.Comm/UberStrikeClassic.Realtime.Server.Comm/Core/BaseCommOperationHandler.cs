using Cmune.DataCenter.Common.Entities;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Utils;
using ExitGames.Logging;
using Photon.SocketServer;
using Photon.SocketServer.Rpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Comm.Helper;

namespace UberStrikeClassic.Realtime.Server.Comm.Core
{
    public abstract class BaseCommOperationHandler : IOperationHandler
    {
        private static readonly ILogger Logging = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public const int ID = 1;

        public abstract void OnJoinCommPeer(CommPeer peer, CommActorInfo actor);
        public abstract void OnPlayerBanned(CommPeer peer, int cmid);
        public abstract void OnLobbyChatMessage(int actorId, string message);
        public abstract void OnUpdateActorInfo(int actorId, SyncObject syncActor);
        public abstract void OnUpdatePlayerRoom(int actorId, CmuneRoomID room);
        public abstract void OnResetPlayerRoom(int actorId);
        public abstract void OnGameIngameChatMessage(GameChatEvent args);
        public abstract void OnSetContactList(int cmid, HashSet<int> contacts);
        public abstract void OnUpdateContacts(int cmid);
        public abstract void OnModerationCustomMessage(int actorId, string message);

        public void OnDisconnect(PeerBase peer)
        {
           var CommPeer = CommServerApplication.Instance.Room.FindPhotonBasePeer(peer.ConnectionId);

            if(CommPeer != null) 
            {
                CommServerApplication.Instance.Room.Leave(CommPeer);
            }
        }

        public OperationResponse OnOperationRequest(PeerBase peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            LobbyOperationRequestCode OpCode = (LobbyOperationRequestCode)operationRequest.OperationCode;

            if (Logging.IsDebugEnabled)
            {
                Logging.DebugFormat("New Operation request of type: {0}", OpCode.ToString());
            }

            var commPeer = (CommPeer)peer;

            switch (OpCode) 
            {
                case LobbyOperationRequestCode.Server:
                    return ServerRequest(commPeer, operationRequest);
                case LobbyOperationRequestCode.Player:
                    return PlayerRequest(commPeer, operationRequest);
                case LobbyOperationRequestCode.Application:
                    break;
                case LobbyOperationRequestCode.JoinRoom:
                    return ConnectCommLobbyRequest(commPeer, operationRequest);
                case LobbyOperationRequestCode.LeaveRoom:
                    break;
            }

            return null;
        }

        private OperationResponse ServerRequest(CommPeer peer, OperationRequest request) 
        {
            short networkId = OperationUtil.GetArg<short>(request.Parameters, ParameterKeys.InstanceId);
            byte RpcId = OperationUtil.GetArg<byte>(request.Parameters, ParameterKeys.MethodId);
            byte[] data = OperationUtil.GetBytes(request.Parameters);

            if (Logging.IsDebugEnabled)
            {
                Logging.DebugFormat("New 'Server' Operation Request with the networkID: {0} and RPCID: {1}",networkId,RpcId);
            }

            if (networkId == NetworkClassID.ServerSyncCenter) 
            {
                object[] objs = RealtimeSerialization.ToObjects(data);
                int actorIdSecure = (int)objs[0];
                int localId = (int)objs[1];
                short? networkIdN = (short?)objs[2];

                var sendParams = new Dictionary<byte, object>();
                OperationUtil.SetArg<short>(sendParams, ParameterKeys.InstanceId, (short)1);
                OperationUtil.SetArg<byte>(sendParams, ParameterKeys.MethodId, (byte)1);
                OperationUtil.SetArg<byte[]>(sendParams, ParameterKeys.Bytes, RealtimeSerialization.ToBytes(new object[] { localId, networkIdN.Value }).ToArray());

                peer.SendEvent(new EventData()
                {
                    Code = 0,
                    Parameters = sendParams
                },new SendParameters());
            }
            else if(networkId == NetworkClassID.CommCenter) 
            {
                switch (RpcId)
                {
                    case CommRPC.Join:
                        JoinCommPeer(peer, data);
                        break;
                    case CommRPC.ModerationBanPlayer:
                        PlayerBanned(data);
                        break;
                    case CommRPC.ChatMessageToAll:
                        LobbyChatMessage(data);
                        break;
                    case CommRPC.PlayerUpdate:
                        UpdateActorInfo(data);
                        break;
                    case CommRPC.UpdatePlayerRoom:
                        UpdatePlayerRoom(data);
                        break;
                    case CommRPC.ResetPlayerRoom:
                        ResetPlayerRoom(data);
                        break;
                    case RPC.Leave:
                        peer.Disconnect();
                        break;
                    case CommRPC.ChatMessageInGame:
                        InGameChatMessage(data);
                        break;
                    case CommRPC.SetContactList:
                        SetContactList(data);
                        break;
                    case CommRPC.UpdateContacts:
                        UpdateContacts(data);
                        break;
                }
            }

            return null;
        }

        private OperationResponse PlayerRequest(CommPeer peer, OperationRequest request)
        {
            int playerId = OperationUtil.GetArg<int>(request.Parameters, ParameterKeys.ActorId);
            short instanceId = OperationUtil.GetArg<short>(request.Parameters, ParameterKeys.InstanceId);
            byte RpcId = OperationUtil.GetArg<byte>(request.Parameters, ParameterKeys.MethodId);
            byte[] receivedData = OperationUtil.GetBytes(request.Parameters);

            switch (RpcId) 
            {
                case CommRPC.ModerationCustomMessage:
                    CustomMessage(peer, playerId, receivedData);
                    break;
            }

            return null;
        }

        // Peer connects to Comm Lobby Room (I still dont get why it has to go through server)
        private OperationResponse ConnectCommLobbyRequest(CommPeer peer, OperationRequest request) 
        {
            var parameter = new Dictionary<byte, object>();

            RoomMetaData roomData = (RoomMetaData)RealtimeSerialization.ToObject(OperationUtil.GetBytes(request.Parameters));
            int cmid = OperationUtil.GetArg<int>(request.Parameters,ParameterKeys.Cmid);
            MemberAccessLevel accessLevel = (MemberAccessLevel)(OperationUtil.GetArg<int>(request.Parameters,ParameterKeys.AccessLevel));

            if (Logging.IsDebugEnabled)
            {
                Logging.DebugFormat("Client joined Comm Lobby Room. RoomID: {0} PeerID: {1}",roomData.RoomID,peer.PeerID);
            }

            OperationUtil.SetArg<int>(parameter, ParameterKeys.ActorNr, cmid);
            OperationUtil.SetArg<byte[]>(parameter, ParameterKeys.GameId, roomData.RoomID.GetBytes());
            OperationUtil.SetArg<bool>(parameter, ParameterKeys.InitRoom,false);
            OperationUtil.SetArg<long>(parameter, ParameterKeys.ServerTicks,(long)Environment.TickCount);

           var opResponse = new OperationResponse()
           {
               OperationCode = 88,
               ReturnCode = 0,
               Parameters = parameter,
               DebugMessage = string.Empty
           };

            return opResponse;
        }

        private void JoinCommPeer(CommPeer peer, byte[] data) 
        {
            CommActorInfo actorInfo = (CommActorInfo)RealtimeSerialization.ToObject(data);

            OnJoinCommPeer(peer,actorInfo);
        }

        private void InGameChatMessage(byte[] data) 
        {
            object[] objs = RealtimeSerialization.ToObjects(data);
            int senderActor = (int)objs[0];
            string message = (string)objs[1];
            byte chatContext = (byte)objs[2];

            CommPeer peer = CommServerApplication.Instance.Room.Find(senderActor);

            GameChatEvent chatEvent = new GameChatEvent(peer, message, chatContext);

            OnGameIngameChatMessage(chatEvent);
        }

        private void PlayerBanned(byte[] data) 
        {
            object[] objs = RealtimeSerialization.ToObjects(data);
            int adminCmid = (int)objs[0];
            int victimCmid = (int)objs[1];

            CommPeer sender = CommServerApplication.Instance.Room.Find(adminCmid);

            OnPlayerBanned(sender, victimCmid);
        }

        private void LobbyChatMessage(byte[] data) 
        {
            object[] objs = RealtimeSerialization.ToObjects(data);
            int senderActor = (int)objs[0];
            string message = (string)objs[1];

            OnLobbyChatMessage(senderActor, message);
        }

        private void UpdateActorInfo(byte[] data) 
        {
            SyncObject syncActorData = (SyncObject)RealtimeSerialization.ToObject(data);

            var actorID = syncActorData.Id;

            OnUpdateActorInfo(actorID, syncActorData);
        }

        private void UpdatePlayerRoom(byte[] data) 
        {
            object[] objs = RealtimeSerialization.ToObjects(data);

            int actorID = (int)objs[0];
            CmuneRoomID Room = (CmuneRoomID)objs[1];

            OnUpdatePlayerRoom(actorID, Room);
        }

        private void ResetPlayerRoom(byte[] data) 
        {
            object[] objs = RealtimeSerialization.ToObjects(data);

            int actorID = (int)objs[0];

            OnResetPlayerRoom(actorID);
        }

        private void SetContactList(byte[] data)
        {
            object[] objs = RealtimeSerialization.ToObjects(data);

            int cmid = (int)objs[0];
            HashSet<int> contacts = (HashSet<int>)objs[1];

            OnSetContactList(cmid, contacts);
        }

        private void UpdateContacts(byte[] data)
        {
            int cmid = (int)RealtimeSerialization.ToObject(data);

            OnUpdateContacts(cmid);
        }

        private void CustomMessage(CommPeer sender, int targetId, byte[] data)
        {
            if (sender.Actor != null && sender.Actor.AccessLevel < MemberAccessLevel.ChatModerator)
                return;

            string msg = (string)RealtimeSerialization.ToObject(data);

            OnModerationCustomMessage(targetId, msg);
        }
    }
}
