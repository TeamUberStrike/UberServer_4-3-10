using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.Realtime.Common;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Core.UberStrok;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game
{
	public class LobbyRoomManager : IDisposable
	{
		private bool _disposed = false;

		private readonly object sync = new object();

		private int _roomId = 0;

		private ConcurrentDictionary<int, GameRoom> _rooms;

		private readonly List<CmuneRoomID> removedRooms;

		private readonly List<RoomMetaData> updatedRooms;

		private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

		private readonly BalancingLoopScheduler _loopScheduler;

		public ConcurrentDictionary<int, GameRoom> All 
		{
			get { return _rooms; }
		}

		public LobbyRoomManager()
		{
			_loopScheduler = new BalancingLoopScheduler(64);

			_rooms = new ConcurrentDictionary<int, GameRoom>();

			removedRooms = new List<CmuneRoomID>();

			updatedRooms = new List<RoomMetaData>();
		}

		public GameRoom Get(int roomID)
		{
			if (_rooms.TryGetValue(roomID, out GameRoom room))
				return room;

			return null;
		}

		public GameRoom Create(GameMetaData data) 
		{
			GameRoom room = null;

			GameMetaData GameData;
			lock(sync)
			{
				_roomId++;

				GameData = new GameMetaData(_roomId, data.RoomName, data.ServerConnection, data.MapID, data.Password, data.RoundTime, data.MaxPlayers, data.GameMode)
				{
					GameModifierFlags = data.GameModifierFlags,
					LevelMin = data.LevelMin,
					LevelMax = data.LevelMax,
					SplatLimit = data.SplatLimit,
					Tag = data.Tag,
					InGamePlayers = 0,
					ConnectedPlayers = 0,
				};
			}

			switch (data.GameMode) 
			{
				case GameModeID.TeamDeathMatch:
					room = new TeamDeathMatchRoom(GameData, _loopScheduler);
					break;
				case GameModeID.DeathMatch:
					room = new DeathMatchRoom(GameData, _loopScheduler);
					break;
				case GameModeID.InfectedMode:
					room = new InfectedRoom(GameData, _loopScheduler);
					break;
			}

			if(_rooms.TryAdd(_roomId, room))
			{
				lock (sync)
				{
					updatedRooms.Add(room.GetView());

					room.Updated = false;
				}

				log.InfoFormat("New Game Created. [Name: {0}] [RoomID: {1}]", room.View.RoomName, room.View.RoomID.Number);

				return room;
			}

			room?.Dispose();

			return null;
		}

		public void Tick()
		{ 
			lock(sync)
			{
				foreach (var room in _rooms.Values)
				{
					var view = room.GetView();

					if (room.Actors.Count <= 0 && room.Loop.Time >= 15 * 1000)
					{
						removedRooms.Add(room.View.RoomID);
					}
					else if (room.Updated)
					{
						updatedRooms.Add(view);

						room.Updated = false;
					}
				}

				if (updatedRooms.Count > 0)
				{
					foreach (var peer in GameApplication.Instance.Lobby.Peers)
					{
						peer.Events.SendFullGameListUpdate(updatedRooms);
					}

					updatedRooms.Clear();
				}

				if (removedRooms.Count > 0)
				{
					foreach (var peer in GameApplication.Instance.Lobby.Peers)
					{
						peer.Events.SendRemovedGameList(removedRooms);
					}


					foreach (var roomId in removedRooms)
					{
						if (_rooms.TryRemove(roomId.Number, out GameRoom Room))
						{
							Room.Dispose();

							log.ErrorFormat("Room has been removed. Name: {0}", Room.View.RoomName);
						}
					}

					removedRooms.Clear();
				}
			}
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_loopScheduler.Dispose();
			_disposed = true;
		}
	}
}
