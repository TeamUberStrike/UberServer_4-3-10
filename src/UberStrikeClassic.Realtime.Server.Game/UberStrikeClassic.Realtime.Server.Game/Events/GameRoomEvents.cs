using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Synchronization;
using ExitGames.Logging;
using Photon.SocketServer;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Rooms;
using UnityEngine;

namespace UberStrikeClassic.Realtime.Server.Game.Events
{
	public class GameRoomEvents : BaseEvents
	{
		public GameRoomEvents(GamePeer peer) : base(peer)
		{
		}

		public void SendDoorOpen(short gameMode, int id)
		{
			SendEvent(gameMode, FpsGameRPC.DoorOpen, new object[] { id });
		}

		public void SendPlayerJoined(short gameMode, GameActor actor, Vector3 position)
		{
			SendEvent(gameMode, FpsGameRPC.Join, new object[] { SyncObjectBuilder.GetSyncData(actor.ActorInfo, true), actor.ActorInfo.Position });
		}

		public void SendPlayerLeft(short gameMode, int actorId)
		{
			SendEvent(gameMode, FpsGameRPC.Leave, new object[] { actorId });
		}

		public void SendSetPlayerSpawnPosition(short gameMode, byte playerNumber, Vector3 position)
		{
			SendEvent(gameMode, FpsGameRPC.SetPlayerSpawnPosition, new object[] { playerNumber, position });
		}

		public void SendAllPlayerDeltas(short gameMode, byte[] deltas, bool reliable)
		{
			SendEvent(gameMode, FpsGameRPC.DeltaPlayerListUpdate, deltas, !reliable);
		}

		public void SendPlayerDelta(short gameMode, SyncObject delta, bool reliable)
		{
			Dictionary<byte, object> sendParams = OperationFactory.Create(CmuneOperationCodes.MessageToAll, new object[]
			{
				gameMode,
				GameRPC.DeltaPlayerListUpdate,
				RealtimeSerialization.ToBytes(delta).ToArray()
			});

			var eventData = new EventData()
			{
				Code = 0,
				Parameters = sendParams
			};

			Peer.SendEvent(eventData, new SendParameters() { ChannelId = 0, Unreliable = !reliable });
		}

		public void SendAllPositionUpdates(short gameMode, byte[] positionBytes)
		{
			SendEvent(gameMode, FpsGameRPC.PositionUpdate, positionBytes, true);
		}

		public void SendKillsRemaining(short gameMode, short kills, short otherkills, bool isLead)
		{
			SendEvent(gameMode, FpsGameRPC.UpdateSplatCount, new object[] { kills, otherkills, isLead });
		}

		public void SendBegin(short gameMode)
		{
			SendEvent(gameMode, FpsGameRPC.Begin, new object[] { });
		}

		public void SendMatchStart(short gameMode, int endtime)
		{
			SendEvent(gameMode, FpsGameRPC.MatchStart, new object[] { 0, endtime });
		}

		public void SendNextSpawnPoint(short gameMode, int spawnTime, GamePeer peer, TeamID team)
		{
			int randomSpawn = Peer.Actor.Room.SpawnPoints.RequestSpawnPointForTeam(team);

			SendEvent(gameMode, FpsGameRPC.SetNextSpawnPointForPlayer, new object[] { randomSpawn, spawnTime });
		}

		public void SendFullActorListUpdate(GameActor actor)
		{
			List<SyncObject> syncObjs = new List<SyncObject>();
			List<UnityEngine.Vector3> positions = new List<UnityEngine.Vector3>();

			foreach (var a in actor.Room.Actors)
			{
				if (a.ActorInfo == null || actor.Peer.ConnectionId == a.Peer.ConnectionId || !a.isPlayer)
					continue;

				syncObjs.Add(SyncObjectBuilder.GetSyncData(a.ActorInfo, true));
				positions.Add(a.ActorInfo.Position);
			}

			SendEvent(actor.Room.View.GameMode, FpsGameRPC.FullPlayerListUpdate, new object[] { syncObjs, positions });
		}

		public void SendEmitProjectile(short gamemode, byte[] data)
		{
			SendEvent(gamemode, FpsGameRPC.EmitProjectile, data);
		}

		public void SendExplodeProjectile(short gameMode, byte[] data)
		{
			SendEvent(gameMode, FpsGameRPC.ExplodeProjectile, data);
		}

		public void SendEmitQuickItem(short gameMode, byte[] data)
		{
			SendEvent(gameMode, FpsGameRPC.EmitQuickItem, data);
		}

		public void SendSingleBulletFire(short gameMode, byte[] data)
		{
			SendEvent(gameMode, FpsGameRPC.SingleBulletFire, data);
		}

		public void SendQuickItemEvent(short gameMode, byte[] data)
		{
			SendEvent(gameMode, FpsGameRPC.QuickItemEvent, data);
		}

		public void SendDamageEffect(short gameMode, DamageEvent damage)
		{
			SendEvent(gameMode, FpsGameRPC.PlayerEvent, new object[] { damage });
		}

		public void SendPlayerKilled(short gameMode, GameActor attacker, GameActor victim, byte item, byte body)
		{
			SendEvent(gameMode, FpsGameRPC.SplatGameEvent, new object[] { attacker.ActorInfo.ActorId, victim.ActorInfo.ActorId, item, body });
		}

		public void SendPlayerUpdate(short gameMode, SyncObject syncObj)
		{
			SendEvent(gameMode, FpsGameRPC.PlayerUpdate, new object[] { syncObj });
		}

		public void SendEndMatch(short gameMode, EndOfMatchData data)
		{
			SendEvent(gameMode, FpsGameRPC.MatchEnd, new object[] { data });
		}

		public void SendNextRoundCountDown(short gameMode, int time)
		{
			SendEvent(gameMode, FpsGameRPC.SetEndOfRoundCountdown, new object[] { time });
		}

		public void SendUpdateRoundScore(short gameMode, int red, int blue, bool isLead)
		{
			SendEvent(gameMode, FpsGameRPC.UpdateSplatCount, new object[] { blue, red, isLead });
		}

		public void SendTeamBalancingUpdate(short gameMode, int red, int blue)
		{
			SendEvent(gameMode, FpsGameRPC.TeamBalanceUpdate, new object[] { blue, red });
		}

		/// <summary>
		/// This RPC on client hides all sent pickup IDs. It is probably used when client joins and there are some pickups picked up.
		/// </summary>
		/// <param name="pickupIds"></param>
		public void SendSetPowerUpState(short gameMode, List<int> pickupIds)
		{
			SendEvent(gameMode, FpsGameRPC.SetPowerupState, new object[] { pickupIds });
		}

		public void SendSetPowerUpPicked(short gameMode, int pickupId, bool showPickup)
		{
			SendEvent(gameMode, FpsGameRPC.PowerUpPicked, new object[] { pickupId, (byte)(showPickup ? 0 : 1) });
		}

		public void SendRoundTimeUpdate(short gameMode, int serverticks)
		{
			SendEvent(gameMode, FpsGameRPC.UpdateInfectedRoundTime, new object[] { serverticks });
		}

		public void SendInfectedCountDown(short gameMode, int count)
		{
			SendEvent(gameMode, FpsGameRPC.InfectedCountDown, new object[] { count });
		}

		public void SendInfected(short gameMode)
		{
			SendEvent(gameMode, FpsGameRPC.Infected, new object[] { });
		}

		public void SendPlayerInfectedNotification(short gameMode, int actorid)
		{
			SendEvent(gameMode, FpsGameRPC.PlayerInfected, new object[] { actorid });
		}

		public void SendResetInfectedPlayers(short gameMode)
		{
			SendEvent(gameMode, FpsGameRPC.ResetInfectedPlayers, new object[] { });
		}
	}
}
