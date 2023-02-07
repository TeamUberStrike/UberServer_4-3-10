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

namespace UberStrikeClassic.Realtime.Server.Game.Rooms
{
    public class TeamDeathMatchRoom : GameRoom
    {
        public int BlueTeamScore { get; set; }
        public int RedTeamScore { get; set; }
        public int BlueTeamCount { get; set; }
        public int RedTeamCount { get; set; }

        public TeamDeathMatchRoom(GameMetaData data, ILoopScheduler scheduler) : base(data, scheduler)
        {
            BlueTeamCount = 0;
            RedTeamCount = 0;
            BlueTeamScore = 0;
            RedTeamScore = 0;
        }

        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public override bool CanDamage(GameActor victim, GameActor attacker)
        {
            return victim == attacker || victim.ActorInfo.TeamID != attacker.ActorInfo.TeamID;
        }

        public override bool CanJoin(GameActor actor, TeamID team)
        {
            if (actor.ActorInfo.AccessLevel >= (int)MemberAccessLevel.JuniorModerator)
                return true;

            return team == TeamID.NONE && !GetView().IsFull;
        }

        public override bool CanStart()
        {
            return true;
        }

        public override void OnPlayerJoined(GameActor actor)
        {
            foreach (var others in Actors)
            {
                others.Peer.Events.Game.SendPlayerJoined(View.GameMode, actor, actor.ActorInfo.Position);
            }

            if (actor.ActorInfo.TeamID == TeamID.RED)
                RedTeamCount++;
            else if (actor.ActorInfo.TeamID == TeamID.BLUE)
                BlueTeamCount++;

            if (PickupManager.RespawningPickups.Count > 0)
            {
                actor.Peer.Events.Game.SendSetPowerUpState(View.GameMode, PickupManager.RespawningPickups);
            }

            State.Current.OnPlayerJoined(actor);

            foreach (var others in Actors)
            {
                others.Peer.Events.Game.SendTeamBalancingUpdate(View.GameMode, RedTeamCount, BlueTeamCount);
            }
        }

        public override void OnPlayerKilled(GameActor attacker, GameActor victim, byte itemclass, byte bodypart)
        {
            victim.State.Set(ActorStates.ActorStateId.Killed);

            foreach (var actors in Actors)
            {
                actors.Peer.Events.Game.SendPlayerKilled(View.GameMode, attacker, victim, itemclass, bodypart);
            }

            if(attacker.ActorInfo.Cmid != victim.ActorInfo.Cmid) 
            {
                if (attacker.ActorInfo.TeamID == TeamID.BLUE)
                    BlueTeamScore++;
                else if (attacker.ActorInfo.TeamID == TeamID.RED)
                    RedTeamScore++;

                foreach(var actor in Actors) 
                {
                    if (actor.isPlayer) 
                    {
                        bool isLeading = IsTeamLeading(actor.ActorInfo.TeamID);

                        actor.Peer.Events.Game.SendUpdateRoundScore(View.GameMode, RedTeamScore, BlueTeamScore, isLeading);
                    }
                }

                if (BlueTeamScore >= View.SplatLimit || RedTeamScore >= View.SplatLimit)
                    State.Set(RoomStates.RoomStateId.End);
            }
        }

        private bool IsTeamLeading(TeamID team) 
        {
            if(team == TeamID.BLUE) 
            {
                return BlueTeamScore > RedTeamScore;
            }
            else if(team == TeamID.RED) 
            {
                return RedTeamScore > BlueTeamScore;
            }

            return false;
        }

        public override void OnPlayerLeft(int actorid, TeamID team)
        {
            foreach (var others in Actors)
            {
                if (others.ActorInfo.ActorId == actorid) continue;

                others.Peer.Events.Game.SendPlayerLeft(View.GameMode, actorid);
            }

            if (team == TeamID.BLUE)
                BlueTeamCount--;
            else if (team == TeamID.RED)
                RedTeamCount--;

            foreach(var actor in Actors) 
            {
                actor.Peer.Events.Game.SendTeamBalancingUpdate(View.GameMode, RedTeamCount, BlueTeamCount);
            }

            View.InGamePlayers--;

            if (State.Current.RoomStateID == RoomStates.RoomStateId.Running && RedTeamCount == 0 || BlueTeamCount == 0)
                State.Set(RoomStates.RoomStateId.End);
        }

        public override void OnReset()
        {
            BlueTeamScore = 0;
            RedTeamScore = 0;

            foreach(var actor in Actors) 
            {
                if (actor.isPlayer) 
                {
                    actor.Peer.Events.Game.SendUpdateRoundScore(View.GameMode, RedTeamScore, BlueTeamScore, false);
                }
            }
        }

        public override void OnPrepare(GameActor actor)
        {
            if (Actors.Count <= 1) return;

            actor.Peer.Events.Game.SendTeamBalancingUpdate(View.GameMode, RedTeamCount, BlueTeamCount);
        }
    }
}
