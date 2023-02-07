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
using UberStrikeClassic.Realtime.Server.Game.ActorStates;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game.Common
{
    public class GameActor
    {
        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public CharacterInfo ActorInfo { get; set; }

        public StatsCollection Stats { get; set; }

        public GamePeer Peer { get; set; }

        public GameRoom Room { get; set; }

        public int ID { get; set; }

        public byte Number { get; set; }

        public bool isPlayer { get; set; }

        public CmuneRoomID CurrentRoom { get { return Room.View.RoomID; } }

        public bool hasUpdated = false;

        public PlayerPosition Movement;

        public bool hasMoved = false;

        public ActorState State { get; }

        public bool isInfected = false;

        public GameActor(GamePeer peer, GameRoom room) 
        {
            Peer = peer;

            Room = room;

            ActorInfo = new CharacterInfo();

            isPlayer = false;

            State = new ActorState(this);
        }

        public void Tick() 
        {
            State.Current.OnTick();
        }

        public void Reset()
        {
            if (Stats != null)
                Stats = new StatsCollection();

            if(ActorInfo != null)
            {
                ActorInfo.ResetScore();
                ActorInfo.ResetState();

                if(ActorInfo.IsReadyForGame)
                {
                    ActorInfo.IsReadyForGame = false;
                }
            }
        }

        public void UpdateStatsOnKill(UberstrikeItemClass weaponClass, BodyPart bodyPart)
        {
            ushort xpAwarded = GameServerConfig.GetXpForKill(bodyPart);

            ActorInfo.Kills++;
            ActorInfo.Points += GameServerConfig.PointsPerKill;
            ActorInfo.XP += xpAwarded;
            Stats.Points += GameServerConfig.PointsPerKill;
            Stats.Xp += xpAwarded;

            switch (weaponClass)
            {
                case UberstrikeItemClass.WeaponCannon:
                    Stats.CannonKills++;
                    break;
                case UberstrikeItemClass.WeaponHandgun:
                    Stats.HandgunKills++;
                    break;
                case UberstrikeItemClass.WeaponLauncher:
                    Stats.LauncherKills++;
                    break;
                case UberstrikeItemClass.WeaponMachinegun:
                    Stats.MachineGunKills++;
                    break;
                case UberstrikeItemClass.WeaponMelee:
                    Stats.MeleeKills++;
                    break;
                case UberstrikeItemClass.WeaponShotgun:
                    Stats.ShotgunSplats++;
                    break;
                case UberstrikeItemClass.WeaponSniperRifle:
                    Stats.SniperKills++;
                    break;
                case UberstrikeItemClass.WeaponSplattergun:
                    Stats.SplattergunKills++;
                    break;
                default:
                    break;
            }
        }

        public void UpdateStatsOnDeath(UberstrikeItemClass weaponClass, bool isSuicide = false)
        {
            ActorInfo.Deaths++;
            Stats.Deaths++;

            if (isSuicide)
            {
                if (ActorInfo.XP >= GameServerConfig.XpPerKill)
                    ActorInfo.XP -= GameServerConfig.XpPerKill;

                Stats.Xp -= GameServerConfig.XpPerKill;
                ActorInfo.Kills--;

                switch (weaponClass)
                {
                    case UberstrikeItemClass.WeaponCannon:
                        Stats.CannonKills--;
                        break;
                    case UberstrikeItemClass.WeaponHandgun:
                        Stats.HandgunKills--;
                        break;
                    case UberstrikeItemClass.WeaponLauncher:
                        Stats.LauncherKills--;
                        break;
                    case UberstrikeItemClass.WeaponMachinegun:
                        Stats.MachineGunKills--;
                        break;
                    case UberstrikeItemClass.WeaponMelee:
                        Stats.MeleeKills--;
                        break;
                    case UberstrikeItemClass.WeaponShotgun:
                        Stats.ShotgunSplats--;
                        break;
                    case UberstrikeItemClass.WeaponSniperRifle:
                        Stats.SniperKills--;
                        break;
                    case UberstrikeItemClass.WeaponSplattergun:
                        Stats.SplattergunKills--;
                        break;
                    default:
                        break;
                }
            }


        }

        public SyncObject GetDeltaView(bool clearcache) 
        {
          /*  if (clearcache) 
            {
                ActorInfo.Cache.Remove(CharacterInfo.FieldTag.Health);
                
            }*/

            return SyncObjectBuilder.GetSyncData(ActorInfo, false);
        }

        public SyncObject GetViewFull() 
        {
           return SyncObjectBuilder.GetSyncData(ActorInfo, true);
        }
    }
}
