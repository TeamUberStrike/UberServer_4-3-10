using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.DataCenter.Common.Entities;
using ExitGames.Logging;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Core.UberStrok;
using UberStrikeClassic.Realtime.Server.Game.Events;

namespace UberStrikeClassic.Realtime.Server.Game.Rooms
{
    public class DeathMatchRoom : GameRoom
    {
        public DeathMatchRoom(GameMetaData data, ILoopScheduler scheduler) : base(data, scheduler)
        {
            
        }

        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public override bool CanDamage(GameActor victim, GameActor attacker)
        {
            return true;
        }

        public override bool CanJoin(GameActor actor, TeamID team)
        {
            if (actor.ActorInfo.AccessLevel >= (int)MemberAccessLevel.JuniorModerator)
                return true;

            return team == TeamID.NONE && !GetView().IsFull;
        }

        public override bool CanStart()
        {
            return Actors.Count > 1;
        }

        public override void OnPlayerJoined(GameActor actor)
        {
            foreach (var others in Actors)
            {
                others.Peer.Events.Game.SendPlayerJoined(View.GameMode, actor, actor.ActorInfo.Position);
            }

            int killsremain = GetKillsRemaining();

            GameActor highest = HighestKillActor();

            bool isLead = (highest.ActorInfo.ActorId == actor.ActorInfo.ActorId) ? true : false;

            actor.Peer.Events.Game.SendKillsRemaining(View.GameMode, actor.ActorInfo.Kills, highest.ActorInfo.Kills, isLead);

            if(PickupManager.RespawningPickups.Count > 0)
            {
                actor.Peer.Events.Game.SendSetPowerUpState(View.GameMode, PickupManager.RespawningPickups);
            }

            State.Current.OnPlayerJoined(actor);
        }

        public override void OnPlayerKilled(GameActor attacker, GameActor victim, byte itemclass, byte bodypart)
        {
            victim.State.Set(ActorStates.ActorStateId.Killed);

            foreach(var actors in Actors) 
            {
                actors.Peer.Events.Game.SendPlayerKilled(View.GameMode, attacker, victim, itemclass, bodypart);
            }

            var highestKillActor = HighestKillActor();
            if (attacker.ActorInfo.Cmid != victim.ActorInfo.Cmid) 
            {
                foreach (var actor in Actors)
                {
                    bool isLeading = highestKillActor.ActorInfo.Kills == actor.ActorInfo.Kills ? true : false;

                    actor.Peer.Events.Game.SendKillsRemaining(View.GameMode, actor.ActorInfo.Kills, highestKillActor.ActorInfo.Kills, isLeading);
                }
            }

            if(GetKillsRemaining() <= 0) 
            {
                State.Set(RoomStates.RoomStateId.End);
            }
        }

        public override void OnPlayerLeft(int actorid, TeamID team)
        {
            foreach (var others in Actors)
            {
                if (others.ActorInfo.ActorId == actorid) continue;

                others.Peer.Events.Game.SendPlayerLeft(View.GameMode, actorid);
            }

            foreach (var actor in Actors)
            {
                if (actor.ActorInfo.ActorId == actorid) continue;

                bool isLeading = HighestKillActor().ActorInfo.Kills == actor.ActorInfo.Kills ? true : false;

                actor.Peer.Events.Game.SendKillsRemaining(View.GameMode, actor.ActorInfo.Kills, HighestKillActor().ActorInfo.Kills, isLeading);
            }

            View.InGamePlayers--;

            if (State.Current.RoomStateID == RoomStates.RoomStateId.Running && View.InGamePlayers <= 1)
                State.Set(RoomStates.RoomStateId.End);
        }

        private GameActor HighestKillActor()
        {
            return Actors.Aggregate((a1, a2) => a1.ActorInfo != null && a2.ActorInfo != null && a1.ActorInfo.Kills > a2.ActorInfo.Kills ? a1 : a2);
        }

        private int GetKillsRemaining()
        {
            return View.SplatLimit - (Actors.Count > 0 ? Actors.Aggregate((a, b) => a.ActorInfo.Kills > b.ActorInfo.Kills ? a : b).ActorInfo.Kills : 0);
        }

        public override void OnReset()
        {
           foreach(var actor in Actors) 
           {
                if (actor.isPlayer) 
                {
                    GameActor highest = HighestKillActor();

                    bool isLead = (highest.ActorInfo.ActorId == actor.ActorInfo.ActorId) ? true : false;

                    actor.Peer.Events.Game.SendKillsRemaining(View.GameMode, actor.ActorInfo.Kills, highest.ActorInfo.Kills, isLead);
                }
           }
        }

        public override void OnPrepare(GameActor actor)
        {
            // Only used by TDM
        }
    }
}
