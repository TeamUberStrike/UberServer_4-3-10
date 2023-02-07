using Cmune.DataCenter.Common.Entities;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Synchronization;
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
using UberStrike.Core.Serialization;

namespace UberStrikeClassic.Realtime.Server.Comm
{
    public class CommEvents
    {
        private Peer peer { get; set; }

        public CommEvents(Peer p) 
        {
            peer = p;
        }

        public void SendAdminMessage(string message) 
        {

        }

        public void SendFullActorsList(List<SyncObject> players)
        { 
            Dictionary<byte, object> sendParams = new Dictionary<byte, object>();

            OperationUtil.SetArg<short>(sendParams, ParameterKeys.InstanceId, NetworkClassID.CommCenter);
            OperationUtil.SetArg<byte>(sendParams, ParameterKeys.MethodId, CommRPC.FullPlayerListUpdate);
            OperationUtil.SetBytes(sendParams, RealtimeSerialization.ToBytes(new object[]
            {
                players
            }).ToArray());

            var eventData = new EventData()
            {
                Code = 0,
                Parameters = sendParams
            };

            peer.SendEvent(eventData, new SendParameters() { Unreliable = false });
        }

        public void SendIngameChatMessage(int cmid, int actorid, string playername, string message, MemberAccessLevel accesslvl,byte context) 
        {
            Dictionary<byte, object> sendParams = new Dictionary<byte, object>();

            OperationUtil.SetArg<short>(sendParams, ParameterKeys.InstanceId,NetworkClassID.CommCenter);
            OperationUtil.SetArg<byte>(sendParams, ParameterKeys.MethodId, CommRPC.ChatMessageInGame);
            OperationUtil.SetBytes(sendParams, RealtimeSerialization.ToBytes(new object[]
            {
                cmid,
                actorid,
                playername,
                message,
                (byte)accesslvl,
                context
            }).ToArray());

            var eventData = new EventData()
            {
                Code = 0,
                Parameters = sendParams
            };

            peer.SendEvent(eventData, new SendParameters() { Unreliable = false });
        }

        public void SendActorsUpdate(List<SyncObject> syncActors) 
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object>();

            OperationUtil.SetArg<short>(parameter, ParameterKeys.InstanceId, NetworkClassID.CommCenter);
            OperationUtil.SetArg<byte>(parameter, ParameterKeys.MethodId, CommRPC.FullPlayerListUpdate);
            OperationUtil.SetBytes(parameter, RealtimeSerialization.ToBytes(syncActors).ToArray());

            var actorsEvent = new EventData()
            {
                Code = 0,
                Parameters = parameter
            };

            peer.SendEvent(actorsEvent, new SendParameters() { Unreliable = false });
        }

        public void SendLobbyMessage(int cmid, int actor, string playerName, string message) 
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object>();

            OperationUtil.SetArg<short>(parameter, ParameterKeys.InstanceId, (short)4);
            OperationUtil.SetArg<byte>(parameter, ParameterKeys.MethodId, CommRPC.ChatMessageToAll);
            OperationUtil.SetBytes(parameter, RealtimeSerialization.ToBytes(new object[]
            {
                cmid,
                actor,
                playerName,
                message
            }).ToArray());

            var eventData = new EventData()
            {
                Code = 0,
                Parameters = parameter
            };

            peer.SendEvent(eventData, new SendParameters() { Unreliable = false });
        }

        public void SendUpdateActorInfo(SyncObject syncData) 
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object>();

            OperationUtil.SetArg<short>(parameter, ParameterKeys.InstanceId, (short)4);
            OperationUtil.SetArg<byte>(parameter, ParameterKeys.MethodId, CommRPC.PlayerUpdate);
            OperationUtil.SetBytes(parameter, RealtimeSerialization.ToBytes(syncData).ToArray());

            var eventData = new EventData()
            {
                Code = 0,
                Parameters = parameter
            };

            peer.SendEvent(eventData, new SendParameters() { Unreliable = false });
        }

        public void SendPlayerLeft(int cmid) 
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object>();

            OperationUtil.SetArg<short>(parameter, ParameterKeys.InstanceId, NetworkClassID.CommCenter);
            OperationUtil.SetArg<byte>(parameter, ParameterKeys.MethodId, CommRPC.Leave);
            OperationUtil.SetBytes(parameter, RealtimeSerialization.ToBytes(new object[] { cmid }).ToArray());

            var eventData = new EventData()
            {
                Code = 0,
                Parameters = parameter
            };

            peer.SendEvent(eventData, new SendParameters() { Unreliable = false });
        }

        public void SendError(string message) 
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object>();

            OperationUtil.SetArg<short>(parameter, ParameterKeys.InstanceId, NetworkClassID.CommCenter);
            OperationUtil.SetArg<byte>(parameter, ParameterKeys.MethodId, CommRPC.DisconnectAndDisablePhoton);
            OperationUtil.SetBytes(parameter, RealtimeSerialization.ToBytes(new object[] { message }).ToArray());

            var eventData = new EventData()
            {
                Code = 0,
                Parameters = parameter
            };

            peer.SendEvent(eventData, new SendParameters() { Unreliable = false });
        }

        public void SendContactsUpdate(List<SyncObject> updated, List<int> removed)
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object>();

            OperationUtil.SetArg<short>(parameter, ParameterKeys.InstanceId, NetworkClassID.CommCenter);
            OperationUtil.SetArg<byte>(parameter, ParameterKeys.MethodId, CommRPC.UpdateContacts);
            OperationUtil.SetBytes(parameter, RealtimeSerialization.ToBytes(new object[] { updated, removed }).ToArray());

            var eventData = new EventData()
            {
                Code = 0,
                Parameters = parameter
            };

            peer.SendEvent(eventData, new SendParameters() { Unreliable = false });
        }

        public void SendModerationMessage(string message)
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object>();

            OperationUtil.SetArg<short>(parameter, ParameterKeys.InstanceId, NetworkClassID.CommCenter);
            OperationUtil.SetArg<byte>(parameter, ParameterKeys.MethodId, CommRPC.ModerationCustomMessage);
            OperationUtil.SetBytes(parameter, RealtimeSerialization.ToBytes(new object[] { message }).ToArray());

            var eventData = new EventData()
            {
                Code = 0,
                Parameters = parameter
            };

            peer.SendEvent(eventData, new SendParameters() { Unreliable = false });
        }
    }
}
