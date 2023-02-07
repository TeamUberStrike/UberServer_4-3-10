using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.DataCenter.Common.Entities;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Utils;
using Photon.SocketServer;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Core;
using UberStrikeClassic.Realtime.Server.Game.Rooms;
using UberStrikeClassic.Realtime.Server.Game.Network;

namespace UberStrikeClassic.Realtime.Server.Game.Operations
{
	public class PeerOperationHandler : NetworkClass
	{
		public PeerOperationHandler() 
		{

		}

		private ServerLoadData serverLoadData = new ServerLoadData()
		{
			Latency = 10, // 10
			MaxPlayerCount = 50,
			PeersConnected = 0,
			PlayersConnected = 0,
			State = ServerLoadData.Status.Alive,
			TimeStamp = DateTime.UtcNow
		};

		public override void MessageToApplication(GamePeer peer, short invocationid, byte RpcId)
		{
			switch (RpcId)
			{
				case GameApplicationRPC.PeerSpecification:
					 OnPeerSpecification(peer, invocationid);
					break;
				case GameApplicationRPC.QueryServerLoad:
					 OnServerLoad(peer, invocationid);
					break;
			}
		}

		public override void MessageToServer(GamePeer peer, short networkId, byte RpcId, byte[] data)
		{
			if (networkId == NetworkClassID.LobbyCenter)
			{
				switch (RpcId)
				{
					case LobbyRPC.Join:
						JoinLobby(peer, data);
						break;
				}
			}
			else if (networkId == NetworkClassID.ServerSyncCenter)
			{
				switch (RpcId)
				{
					case ServerSyncCenterRPC.RegisterStaticNetworkClass:
						ServerSyncCenter(peer, data);
						break;
				}
			}
		}

		public override void OnJoinGame(GamePeer peer, RoomMetaData roomData, int cmid, int accessLvl)
		{
			OperationResponse response = new OperationResponse() { OperationCode = 88, ReturnCode = 0 };

			if(roomData.RoomID.Number == 0) /* Create game request */
			{
				GameRoom room;

				room = GameApplication.Instance.Lobby.Rooms.Create((GameMetaData)roomData);

				if (room != null)
				{
					room.Join(peer);

					var sendParams = new Dictionary<byte, object>();
					OperationUtil.SetArg<int>(sendParams, ParameterKeys.ActorNr,cmid);
					OperationUtil.SetArg<byte[]>(sendParams, ParameterKeys.GameId, room.View.RoomID.GetBytes());
					OperationUtil.SetArg<bool>(sendParams, ParameterKeys.InitRoom, true);
					OperationUtil.SetArg<long>(sendParams, ParameterKeys.ServerTicks, (long)Environment.TickCount);

					response.Parameters = sendParams;
					response.ReturnCode = 0;
				}
				else { /* Failed to create room */ }
			}
			else if(roomData.RoomID.Number == 66) /* Join Lobby Room */
			{
				var sendParams = new Dictionary<byte, object>();
				OperationUtil.SetArg<int>(sendParams, ParameterKeys.ActorNr, cmid);
				OperationUtil.SetArg<byte[]>(sendParams, ParameterKeys.GameId, roomData.RoomID.GetBytes());
				OperationUtil.SetArg<bool>(sendParams, ParameterKeys.InitRoom, false);
				OperationUtil.SetArg<long>(sendParams, ParameterKeys.ServerTicks, (long)Environment.TickCount);

				response.Parameters = sendParams;
				response.ReturnCode = 0;
			}
			else /* Join existing room */
			{
				GameRoom room = GameApplication.Instance.Lobby.Rooms.Get(roomData.RoomID.Number);

				if (room != null)
				{
					room.Join(peer);

					var sendParams = new Dictionary<byte, object>();
					OperationUtil.SetArg<int>(sendParams, ParameterKeys.ActorNr,cmid);
					OperationUtil.SetArg<byte[]>(sendParams, ParameterKeys.GameId, room.View.RoomID.GetBytes());
					OperationUtil.SetArg<bool>(sendParams, ParameterKeys.InitRoom, true);
					OperationUtil.SetArg<long>(sendParams, ParameterKeys.ServerTicks, (long)Environment.TickCount);

					response.Parameters = sendParams;
					response.ReturnCode = 0;
				}
				else { /* Room does not exist anymore */ }
			}

			peer.SendOperationResponse(response, new SendParameters() { Unreliable = false });
		}

		public void OnJoinLobby(GamePeer peer)
		{
			GameApplication.Instance.Lobby.Join(peer);

			List<RoomMetaData> allRooms = new List<RoomMetaData>();

			foreach(GameRoom room in GameApplication.Instance.Lobby.Rooms.All.Values) 
			{
				allRooms.Add(room.GetView());
			}

			peer.Events.SendFullGameListUpdate(allRooms);
		}

		public override void OnLeaveGame(GamePeer peer, CmuneRoomID roomID)
		{
			var response = new OperationResponse()
			{
				OperationCode = 89,
				ReturnCode = 0,
				Parameters = new Dictionary<byte, object>()
			};

			peer.SendOperationResponse(response, new SendParameters() { Unreliable = false });
		}

		public void OnPeerSpecification(GamePeer peer, short invocId)
		{
			peer.Events.SendPeerSpecification(invocId);
		}

		public void OnServerLoad(GamePeer peer, short invocId)
		{
			peer.Events.SendServerLoadData(serverLoadData, invocId);
		}

		public void OnServerSyncCenter(GamePeer peer, int actor, int localId, short? networkIdn)
		{
			peer.Events.SendRegisterNetworkClass(localId, networkIdn);
		}

		private void JoinLobby(GamePeer peer, byte[] data)
		{
			OnJoinLobby(peer);
		}

		private void ServerSyncCenter(GamePeer peer, byte[] data)
		{
			object[] objs = RealtimeSerialization.ToObjects(data);
			int actorIdSecure = (int)objs[0];
			int localId = (int)objs[1];
			short? networkIdN = (short?)objs[2];

			OnServerSyncCenter(peer, actorIdSecure, localId, networkIdN);
		}
	}
}
