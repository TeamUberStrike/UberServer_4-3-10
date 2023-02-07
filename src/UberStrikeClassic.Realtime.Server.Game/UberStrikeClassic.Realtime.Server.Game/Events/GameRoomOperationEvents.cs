using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Synchronization;
using ExitGames.Logging;
using UberStrike.Core.Types;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Rooms;
using UnityEngine;

namespace UberStrikeClassic.Realtime.Server.Game.Events
{
	public class GameRoomOperationEvents : IRoomOperations
	{
		private GameRoom Room;

		public GameRoomOperationEvents() 
		{
		
		}

		private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

		public void OnInitialize(GameRoom room) 
		{
			Room = room;

			log.Error("--------------------------- GameRoomOperationEvents Initialized! ---------------------------");
		}

		public void OnResetPlayer(GameActor actor) 
		{
			actor.ActorInfo.ResetState();

			actor.State.Previous();
		}

		public void OnDoorOpen(int doorId)
		{
			foreach (GameActor actor in Room.Actors) 
			{
				actor.Peer.Events.Game.SendDoorOpen(Room.View.GameMode, doorId);
			}
		}

		public void OnEmitProjectile(GameActor actor, byte[] data)
		{
			foreach(var other in actor.Room.Actors) 
			{
				if (other.ActorInfo.ActorId == actor.ActorInfo.ActorId) continue;

				other.Peer.Events.Game.SendEmitProjectile(actor.Room.View.GameMode, data);
			}
		}

		public void OnEmitQuickItem(GameActor actor, byte[] data)
		{
			foreach(var other in actor.Room.Actors) 
			{
				if (other.ActorInfo.ActorId == actor.ActorInfo.ActorId) continue;

				other.Peer.Events.Game.SendEmitQuickItem(actor.Room.View.GameMode, data);
			}
		}

		public void OnExplodeProjectile(GameActor actor, byte[] data)
		{
			foreach(var other in actor.Room.Actors) 
			{
				other.Peer.Events.Game.SendExplodeProjectile(actor.Room.View.GameMode, data);
			}
		}

		public void OnHitPlayer(GameActor actor, int target, byte[] data)
		{
			
		}

		public void OnIncreaseHealthAndArmor(GameActor actor, byte health, byte armor)
		{
			
		}

		public void OnJoin(GameActor actor, CharacterInfo info)
		{
			actor.ActorInfo = info;

			Room.View.InGamePlayers++;

			actor.ActorInfo.PlayerNumber = actor.Number;

			actor.ActorInfo.CurrentRoom = Room.View.RoomID;

			actor.Stats = new StatsCollection();

			actor.ActorInfo.ResetScore();

			actor.isPlayer = true;

			Room.OnPlayerJoined(actor);

			log.ErrorFormat("OnJoin() RoomID: [{0}] ActorID: [{1}] Name: [{2}]", Room.RoomID.Number, actor.ActorInfo.ActorId, actor.ActorInfo.PlayerName);

		}

		public void OnLeave(GamePeer peer, int actorId)
		{
			peer.Disconnect();
		}

		public void OnPlayerHit(GameActor attacker, int victimId, short damage, BodyPart bodyPart, int shotcount, 
			byte lookRY, int weaponId, UberstrikeItemClass itemClass, int dmgEffect, float dmgEffectValue)
		{
			DamageEvent dmgevent = new DamageEvent();
			dmgevent.AddDamage(lookRY, damage, (byte)bodyPart, dmgEffect, dmgEffectValue);

			foreach(var victim in attacker.Room.Actors) 
			{
				if (victim.ActorInfo.ActorId != victimId)
					continue;

				if (Room.DoDamage(attacker, victim, damage, bodyPart, itemClass, dmgevent)) 
				{
					Room.OnPlayerKilled(attacker, victim,(byte)itemClass,(byte)bodyPart);

					break;
				}
			}
		}

		public void OnPlayerUpdate(GameActor actor, SyncObject syncObj, byte[] data)
		{
			if (actor.State.Current.ActorStateID == ActorStates.ActorStateId.Killed || !actor.ActorInfo.IsAlive)
				return;

			if (syncObj.IsEmpty) return;

			if((syncObj.DeltaCode & CharacterInfo.FieldTag.Stats) != 0)
			{
				if(syncObj.Data.ContainsKey(CharacterInfo.FieldTag.Stats))
				{
					syncObj.Data.Remove(CharacterInfo.FieldTag.Stats);
				}

				syncObj.DeltaCode &= ~CharacterInfo.FieldTag.Stats;
			}

			if ((syncObj.DeltaCode & CharacterInfo.FieldTag.Health) != 0)
			{
				if (syncObj.Data.ContainsKey(CharacterInfo.FieldTag.Health))
				{
					syncObj.Data.Remove(CharacterInfo.FieldTag.Health);
				}

				syncObj.DeltaCode &= ~CharacterInfo.FieldTag.Health;
			}

			SyncObjectBuilder.ReadSyncData(syncObj, false, syncObj.DeltaCode, actor.ActorInfo);
		}

		public void OnPositionUpdate(GameActor actor, ShortVector3 pos, int time)
		{
			actor.Movement.Position = pos.Vector3;
			actor.Movement.Player = actor.ActorInfo.PlayerNumber;
			actor.Movement.Time = time;

			actor.hasMoved = true;
		}

		/// <summary>
		/// This is used for pickup picked RPC.
		/// </summary>
		/// <param name="actor"></param>
		/// <param name="pickup"></param>
		/// <param name="itemtype"></param>
		/// <param name="value"></param>
		public void OnPowerUpPicked(GameActor actor, int pickup, PickupItemType itemtype, int value)
		{
			Room.PickupManager.Pickup(actor, pickup, itemtype, value);
		}

		/// <summary>
		/// In client FpsGameMode class there is a method called OnNormalJoin which sends respawn durations for all pickups in a map.
		/// This can be used to initialize all pickups for that map on server.
		/// For pickup IDs this system is same as set spawn points, there is a count and pickup IDs are sequential between 0 and count that is received.
		/// </summary>
		/// <param name="actor"></param>
		/// <param name="respawnDurations"></param>
		public void OnSetPowerUpCount(GameActor actor, List<byte> respawnDurations)
		{
			Room.PickupManager.AddPickups(respawnDurations);
		}

		public void OnQuickItemEvent(GameActor actor, byte[] data)
		{
			foreach (var other in actor.Room.Actors)
			{
				if (other.ActorInfo.ActorId == actor.ActorInfo.ActorId) continue;

				other.Peer.Events.Game.SendQuickItemEvent(actor.Room.View.GameMode, data);
			}
		}

		public void OnSetPlayerReadyForNextRound(GameActor actor)
		{
			if (actor.ActorInfo == null || actor.ActorInfo.IsReadyForGame)
				return;

			actor.ActorInfo.IsReadyForGame = true;
		}

		public void OnSetPlayerSpawnPosition(GameActor actor, Vector3 pos)
		{
			actor.ActorInfo.Position = pos;

			foreach(GameActor others in Room.Actors) 
			{
				if (others.ActorInfo.PlayerNumber == actor.ActorInfo.PlayerNumber) continue;

				others.Peer.Events.Game.SendSetPlayerSpawnPosition(Room.View.GameMode, actor.ActorInfo.PlayerNumber, pos);
			}
		}

		public void OnSetSpawnPoints(int none, int blue, int red)
		{
			if (!Room.SpawnPoints.IsLoaded)
				Room.SpawnPoints.AddPoints(blue, red, none);
		}

		public void OnSingleBulletFire(GameActor actor, byte[] data)
		{
			foreach (var other in actor.Room.Actors)
			{
				if (other.ActorInfo.ActorId == actor.ActorInfo.ActorId) continue;

				other.Peer.Events.Game.SendSingleBulletFire(actor.Room.View.GameMode, data);
			}
		}
	}
}
