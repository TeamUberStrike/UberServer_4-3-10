using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.DataCenter.Common.Entities;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Utils;
using ExitGames.Logging;
using Photon.SocketServer;
using UberStrike.Core.Types;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Core;
using UberStrikeClassic.Realtime.Server.Game.Events;
using UberStrikeClassic.Realtime.Server.Game.Operations;
using UberStrikeClassic.Realtime.Server.Game.Rooms;
using UberStrikeClassic.Realtime.Server.Game.Network;

namespace UberStrikeClassic.Realtime.Server.Game.Operations
{
    public class GameOperationHandler : NetworkClass
    {
        public GameOperationHandler()
        {

        }

        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public override void MessageToOthers(GamePeer peer, short networkId, byte RpcId, byte[] data)
        {
            switch (RpcId)
            {
                case FpsGameRPC.EmitProjectile:
                    EmitProjectile(peer, data);
                    break;
                case FpsGameRPC.EmitQuickItem:
                    EmitQuickItem(peer, data);
                    break;
                case FpsGameRPC.ExplodeProjectile:
                    ExplodeProjectile(peer, data);
                    break;
                case FpsGameRPC.SingleBulletFire:
                    SingleBulletFire(peer, data);
                    break;
                case FpsGameRPC.QuickItemEvent:
                    QuickItemEvent(peer, data);
                    break;
                default:
                    break;
            }
        }

        private void EmitProjectile(GamePeer peer, byte[] data) 
        {
            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnEmitProjectile(peer.Actor, data));
            }
        }

        private void ExplodeProjectile(GamePeer peer, byte[] data) 
        {
            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnExplodeProjectile(peer.Actor, data));
            }
        }

        private void EmitQuickItem(GamePeer peer, byte[] data) 
        {
            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnEmitQuickItem(peer.Actor, data));
            }
        }

        private void SingleBulletFire(GamePeer peer, byte[] data) 
        {
            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnSingleBulletFire(peer.Actor, data));
            }
        }

        private void QuickItemEvent(GamePeer peer, byte[] data) 
        {
            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnQuickItemEvent(peer.Actor, data));
            }
        }

        public override void MessageToPlayer(GamePeer peer, int playerId, short networkId, byte RpcId, byte[] data)
        {
            switch (RpcId)
            {
                case FpsGameRPC.PlayerHit:
                    HitPlayer(peer, data, playerId);
                    break;
            }
        }

        private void HitPlayer(GamePeer peer, byte[] data, int playerId) 
        {
            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnHitPlayer(peer.Actor, playerId, data));
            }
        }

		#region Server
		public override void MessageToServer(GamePeer peer, short networkId, byte RpcId, byte[] data)
        {

            switch (RpcId)
            {
                case RPC.Join:
                    Join(peer, data);
                    break;
                case RPC.Leave:
                    Leave(peer, data);
                    break;
                case RPC.PlayerUpdate:
                    PlayerUpdate(peer, data);
                    break;
                case RPC.ResetPlayer:
                    ResetPlayer(peer);
                    break;
                case FpsGameRPC.SetSpawnPoints:
                    SetSpawnPoints(peer, data);
                    break;
                case FpsGameRPC.SetPowerUpCount: // Cant find this shit on client
                    SetPowerUpCount(peer, data);
                    break;
                case FpsGameRPC.SetPlayerSpawnPosition:
                    SetPlayerSpawnPosition(peer, data);
                    break;
                case FpsGameRPC.PositionUpdate:
                    PositionUpdate(peer, data);
                    break;
                case FpsGameRPC.PowerUpPicked:
                    PowerUpPicked(peer, data);
                    break;
                case FpsGameRPC.DoorOpen:
                    DoorOpen(peer, data);
                    break;
                case FpsGameRPC.IncreaseHealthAndArmor:
                    IncreaseHealthAndArmor(peer, data);
                    break;
                case FpsGameRPC.PlayerHit:
                    PlayerHit(peer, data);
                    break;
                case FpsGameRPC.RequestRespawnForPlayer:
                    RequestSpawnForPlayer();
                    break;
                case FpsGameRPC.SetShotCounts: // Used for weapon statistics
                    SetShotsCount(peer ,data);
                    break;
                case FpsGameRPC.SetPlayerReadyForNextRound:
                    SetPlayerReadyForNextRound(peer, data);
                    break;
            }
        }

        private void Join(GamePeer peer, byte[] data) 
        {
            CharacterInfo characterInfo = (CharacterInfo)RealtimeSerialization.ToObject(data);

            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnJoin(peer.Actor, characterInfo));
            }
        }

        private void Leave(GamePeer peer, byte[] data) 
        {
            int actorId = Convert.ToInt32(RealtimeSerialization.ToObject(data));

            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnLeave(peer, actorId));
            }
        }

        private void PlayerUpdate(GamePeer peer, byte[] data) 
        {
            var syncObj = (SyncObject)RealtimeSerialization.ToObject(data);

            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnPlayerUpdate(peer.Actor, syncObj, data));
            }
        }

        private void ResetPlayer(GamePeer peer) 
        {
            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnResetPlayer(peer.Actor));
            }
        }

        private void SetSpawnPoints(GamePeer peer, byte[] data) 
        {
            var objs = RealtimeSerialization.ToObjects(data);
            int spawnPointsNone = (byte)objs[0];
            int spawnPointsRed = (byte)objs[1];
            int spawnPointsBlue = (byte)objs[2];

            if (peer.Actor != null && peer.Actor.Room != null) 
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnSetSpawnPoints(spawnPointsNone, spawnPointsBlue, spawnPointsRed));
            }
        }

        private void SetPlayerSpawnPosition(GamePeer peer, byte[] data) 
        {
            var objs = RealtimeSerialization.ToObjects(data);
            int actorId = Convert.ToInt32(objs[0]);
            UnityEngine.Vector3 position = (UnityEngine.Vector3)objs[1];

            if (peer.Actor != null && peer.Actor.Room != null) 
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnSetPlayerSpawnPosition(peer.Actor, position));
            }
        }

        private void PositionUpdate(GamePeer peer, byte[] data) 
        {
            byte[] receivedPosBytes = ((List<byte>)RealtimeSerialization.ToObject(data)).ToArray();

            int i = 0;
            int actorId = DefaultByteConverter.ToInt(receivedPosBytes, ref i);
            ShortVector3 position = new ShortVector3(receivedPosBytes, ref i);
            int time = DefaultByteConverter.ToInt(receivedPosBytes, ref i);

            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnPositionUpdate(peer.Actor, position, time));
            }
        }

        private void PowerUpPicked(GamePeer peer, byte[] data) 
        {
            var objs = RealtimeSerialization.ToObjects(data);

            int pickerId = Convert.ToInt32(objs[0]);
            int pickupId = Convert.ToInt32(objs[1]);
            PickupItemType itemType = (PickupItemType)((byte)objs[2]);
            int value = (byte)objs[3];

            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

               peer.Actor.Room.Enqueue(() => handler.OnPowerUpPicked(peer.Actor, pickupId, itemType, value));
            }
        }

        private void SetPowerUpCount(GamePeer peer, byte[] data)
        {
            var objs = RealtimeSerialization.ToObjects(data);

            int actorId = (int)objs[0];
            List<byte> respawnDurations = (List<byte>)objs[1];

            if(peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnSetPowerUpCount(peer.Actor, respawnDurations));
            }
        }

        private void DoorOpen(GamePeer peer, byte[] data) 
        {
            int doorId = Convert.ToInt32(RealtimeSerialization.ToObject(data));

            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnDoorOpen(doorId));
            }
        }

        private void IncreaseHealthAndArmor(GamePeer peer, byte[] data) 
        {
            var objs = RealtimeSerialization.ToObjects(data);

            int actorId = Convert.ToInt32(objs[0]);
            byte health = (byte)objs[1];
            byte armor = (byte)objs[2];

            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnIncreaseHealthAndArmor(peer.Actor, health, armor));
            }
        }

        private void PlayerHit(GamePeer peer, byte[] data) 
        {
            var objs = RealtimeSerialization.ToObjects(data);

            int attackerId = (int)objs[0];
            int victimId = (int)objs[1];
            short damage = (short)objs[2];
            BodyPart bodyPart = (BodyPart)((byte)objs[3]);
            int shotCount = (int)objs[4];
            byte lookRotationY = (byte)objs[5];
            int weaponid = (int)objs[6];
            UberstrikeItemClass itemClass = (UberstrikeItemClass)((byte)objs[7]);
            int damageEffect = (int)objs[8];
            float damageEffectValue = (float)objs[9];

            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnPlayerHit(peer.Actor, victimId, damage, bodyPart, shotCount, lookRotationY, weaponid, itemClass, damageEffect, damageEffectValue));
            }
        }

        private void RequestSpawnForPlayer() 
        {
            // not implemented
        }

        private void SetShotsCount(GamePeer peer, byte[] data) 
        {
            var objs = RealtimeSerialization.ToObjects(data);

            int cmid = (int)objs[0];
            List<int> shots = (List<int>)objs[1];

            //TODO: finnish implementation
        }

        private void SetPlayerReadyForNextRound(GamePeer peer, byte[] data)
        {
            int actorId = (int)RealtimeSerialization.ToObject(data);

            if (peer.Actor != null && peer.Actor.Room != null)
            {
                IRoomOperations handler = peer.Actor.Room;

                peer.Actor.Room.Enqueue(() => handler.OnSetPlayerReadyForNextRound(peer.Actor));
            }
        }
		#endregion
	}
}
