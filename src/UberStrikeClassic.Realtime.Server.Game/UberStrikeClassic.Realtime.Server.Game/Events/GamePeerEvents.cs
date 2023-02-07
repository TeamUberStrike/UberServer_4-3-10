using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Utils;
using Photon.SocketServer;

namespace UberStrikeClassic.Realtime.Server.Game.Events
{
	public class GamePeerEvents : BaseEvents
	{
		public GameRoomEvents Game { get; }

		public GamePeerEvents(GamePeer peer) : base(peer)
		{
			Game = new GameRoomEvents(peer);
		}

		public void SendFullGameListUpdate(List<RoomMetaData> rooms)
		{
			var sendParams = new Dictionary<byte, object>();

			Hashtable hashtable = new Hashtable();
			hashtable.Add(ParameterKeys.LobbyRoomUpdate, RealtimeSerialization.ToBytes(new object[] { rooms }).ToArray());
			OperationUtil.SetArg<Hashtable>(sendParams, ParameterKeys.Data, hashtable);

			SendEvent(3, sendParams);
		}

		public void SendRemovedGameList(List<CmuneRoomID> ids)
		{
			var sendParams = new Dictionary<byte, object>();

			Hashtable hashtable = new Hashtable();
			hashtable.Add(ParameterKeys.LobbyRoomDelete, RealtimeSerialization.ToBytes(new object[] { ids }).ToArray());
			OperationUtil.SetArg<Hashtable>(sendParams, ParameterKeys.Data, hashtable);

			SendEvent(5, sendParams);
		}

		public void SendServerLoadData(ServerLoadData serverdata, short invocationId)
		{
			var response = new OperationResponse() { OperationCode = CmuneOperationCodes.MessageToApplication, ReturnCode = 0 };

			var sendParams = new Dictionary<byte, object>();

			serverdata.PlayersConnected = GameApplication.Instance.PlayerCount;

			serverdata.RoomsCreated = GameApplication.Instance.Lobby.Rooms.All.Values.Count;

			OperationUtil.SetArg<short>(sendParams, ParameterKeys.InvocationId, invocationId);
			OperationUtil.SetArg<byte[]>(sendParams, ParameterKeys.Data, RealtimeSerialization.ToBytes(new object[]
			{
				serverdata
			}).ToArray());

			response.Parameters = sendParams;

			Peer.SendOperationResponse(response, new SendParameters() { Unreliable = false });
		}

		public void SendRegisterNetworkClass(int localId, short? networkIdn)
		{
			var sendParams = new Dictionary<byte, object>();

			OperationUtil.SetArg<short>(sendParams, ParameterKeys.InstanceId, (short)1);
			OperationUtil.SetArg<byte>(sendParams, ParameterKeys.MethodId, (byte)1);
			OperationUtil.SetArg<byte[]>(sendParams, ParameterKeys.Bytes, RealtimeSerialization.ToBytes(new object[] { localId, networkIdn.Value }).ToArray());

			SendEvent(0, sendParams);
		}

		public void SendPeerSpecification(short invocationId)
		{
			var response = new OperationResponse() { OperationCode = CmuneOperationCodes.MessageToApplication, ReturnCode = 0 };

			var sendParams = new Dictionary<byte, object>();

			OperationUtil.SetArg<short>(sendParams, ParameterKeys.InvocationId, invocationId);
			OperationUtil.SetBytes(sendParams, new byte[] { });

			response.Parameters = sendParams;

			Peer.SendOperationResponse(response, new SendParameters() { Unreliable = false });
		}
	}
}
