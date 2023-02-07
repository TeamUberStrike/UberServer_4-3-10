using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.Realtime.Common;
using UberStrike.Core.Types;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UnityEngine;

namespace UberStrikeClassic.Realtime.Server.Game.Events
{
	interface IRoomOperations
	{
		void OnJoin(GameActor actor, CharacterInfo info);
		void OnLeave(GamePeer peer, int actorId);
		void OnPlayerUpdate(GameActor actor, SyncObject info, byte[] data);
		void OnSetSpawnPoints(int none, int blue, int red);
		void OnSetPlayerSpawnPosition(GameActor actor, Vector3 pos);
		void OnPositionUpdate(GameActor actor, ShortVector3 pos, int time);
		void OnPowerUpPicked(GameActor actor, int pickup, PickupItemType itemtype, int value);
		void OnSetPowerUpCount(GameActor actor, List<byte> respawnDurations);
		void OnDoorOpen(int doorId);
		void OnIncreaseHealthAndArmor(GameActor actor, byte health, byte armor);
		void OnPlayerHit(GameActor attacker, int victimId, short damage, BodyPart bodyPart, int shotcount, byte lookRY, int weaponId, UberstrikeItemClass itemClass, int dmgEffect, float dmgEffectValue);
		void OnSetPlayerReadyForNextRound(GameActor actor);
		void OnHitPlayer(GameActor actor, int target, byte[] data);
		void OnEmitProjectile(GameActor actor, byte[] data);
		void OnExplodeProjectile(GameActor actor, byte[] data);
		void OnEmitQuickItem(GameActor actor, byte[] data);
		void OnSingleBulletFire(GameActor actor, byte[] data);
		void OnQuickItemEvent(GameActor actor, byte[] data);
		void OnResetPlayer(GameActor actor);
	}
}
