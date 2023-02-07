using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Rooms;
using UberStrikeClassic.Realtime.Server.Game.Utility;

namespace UberStrikeClassic.Realtime.Server.Game.Common
{
    public class PickupManager
    {
        public bool PickupsInitialized { get; private set; }

        private Dictionary<int, PickupItem> pickups;

        private List<int> respawningPickups;
        private List<TimeSpan> respawnTimes;

        private GameRoom currentRoom;

        public List<int> RespawningPickups
        {
            get
            {
                return respawningPickups ?? new List<int>();
            }
        }

        public PickupManager(GameRoom room)
        {
            PickupsInitialized = false;

            currentRoom = room;
        }

        public void AddPickups(List<byte> respawnDurations)
        {
            if (PickupsInitialized)
                return;

            pickups = new Dictionary<int, PickupItem>(respawnDurations.Count);
            respawningPickups = new List<int>(respawnDurations.Count);
            respawnTimes = new List<TimeSpan>(respawnDurations.Count);
           
            for(int i = 0; i < respawnDurations.Count; i++)
            {
                pickups.Add(i, new PickupItem() { Id = i, RespawnTime = TimeSpan.FromSeconds(respawnDurations[i]) });

                respawnTimes.Add(TimeSpan.FromSeconds(0));
            }

            PickupsInitialized = true;
        }

        public void Pickup(GameActor actor, int pickupId, PickupItemType type, int value)
        {
            if (!pickups.ContainsKey(pickupId))
                return;

            if (respawningPickups.Contains(pickupId))
                return;

            if (!PickupUtility.IsValueValid(type, value))
                return;

            if (type == PickupItemType.Health)
            {
                actor.ActorInfo.Health += (short)value;
                if (actor.ActorInfo.Health > GameServerConfig.MaxHealth)
                    actor.ActorInfo.Health = GameServerConfig.MaxHealth;
            }
            else if (type == PickupItemType.Armor)
            {
                actor.ActorInfo.Armor.ArmorPoints += (short)value;
                    if (actor.ActorInfo.Armor.ArmorPoints > GameServerConfig.MaxArmor)
                        actor.ActorInfo.Armor.ArmorPoints = GameServerConfig.MaxArmor;
            }

            foreach(var a in currentRoom.Actors)
            {
                if (a.ID == actor.ID)
                    continue;

                a.Peer.Events.Game.SendSetPowerUpPicked(currentRoom.View.GameMode, pickupId, false);
            }

            // Record it to statistics later

            respawningPickups.Add(pickupId);
            respawnTimes[pickupId] = pickups[pickupId].RespawnTime;
        }

        public void ResetPickups()
        {
            if (!PickupsInitialized)
                return;

            if(respawningPickups.Count > 0)
            {
                foreach(int pickupId in respawningPickups)
                {
                    respawnTimes[pickupId] = pickups[pickupId].RespawnTime;

                    foreach (var a in currentRoom.Actors)
                    {
                        a.Peer.Events.Game.SendSetPowerUpPicked(currentRoom.View.GameMode, pickupId, true);
                    }
                }

                respawningPickups.Clear();
            }
        }

        public void Tick()
        {
            if (!PickupsInitialized)
                return;

            if(respawningPickups.Count > 0)
            {
                for(int i = 0; i< respawningPickups.Count; i++)
                {
                    int pickupId = respawningPickups.ElementAt(i);

                    var time = respawnTimes[pickupId];
                    var newTime = time.Subtract(TimeSpan.FromMilliseconds(currentRoom.Loop.DeltaTime));
                    if (newTime.TotalMilliseconds <= 0)
                    {
                        respawnTimes[pickupId] = TimeSpan.FromSeconds(0);
                        foreach (var otherActor in currentRoom.Actors)
                            otherActor.Peer.Events.Game.SendSetPowerUpPicked(currentRoom.View.GameMode, pickupId, true);

                        respawningPickups.RemoveAt(i);
                    }
                    else
                    {
                        respawnTimes[pickupId] = newTime;
                    }
                }
            }
        }
    }
}
