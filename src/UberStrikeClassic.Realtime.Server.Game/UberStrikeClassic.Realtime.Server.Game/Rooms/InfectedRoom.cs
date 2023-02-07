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
using UberStrikeClassic.Realtime.Server.Game.GameModeStates;
using UberStrikeClassic.Realtime.Server.Game.GameModeStates.Infected;

namespace UberStrikeClassic.Realtime.Server.Game.Rooms
{
    public class InfectedRoom : GameRoom
    {
        GameModeState<IGameModeStateInfected> ModeState;

        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        private List<int> infectedActors;

        public InfectedRoom(GameMetaData data, ILoopScheduler scheduler) : base(data, scheduler)
        {
            ModeState = new GameModeState<IGameModeStateInfected>();

            infectedActors = new List<int>();

            ModeState.Register(IGameModeStateInfected.WaitingForPlayers, new GameModeStateWaiting());
            ModeState.Register(IGameModeStateInfected.Countdown, new GameModeStateCountdown(this, OnCountDownComplete));
            ModeState.Register(IGameModeStateInfected.Running, new GameModeStateRunning(this));

            ModeState.SetState(IGameModeStateInfected.WaitingForPlayers);
        }

        private void OnCountDownComplete() 
        {
            ModeState.SetState(IGameModeStateInfected.Running);
        }

        public override bool CanDamage(GameActor victim, GameActor attacker)
        {
            return attacker.isInfected && !victim.isInfected || !attacker.isInfected && victim.isInfected;
        }

        public override bool CanJoin(GameActor actor, TeamID team)
        {
            if (actor.ActorInfo.AccessLevel >= (int)MemberAccessLevel.JuniorModerator)
                return true;

            return team == TeamID.NONE && !GetView().IsFull;
        }

        public override bool CanStart()
        {
            return Actors.Count >= 2;
        }

        public override void OnPlayerJoined(GameActor actor)
        {
            foreach (var others in Actors)
            {
                others.Peer.Events.Game.SendPlayerJoined(View.GameMode, actor, actor.ActorInfo.Position);
            }

            if (PickupManager.RespawningPickups.Count > 0)
            {
                actor.Peer.Events.Game.SendSetPowerUpState(View.GameMode, PickupManager.RespawningPickups);
            }

            State.Current.OnPlayerJoined(actor);

            if (CanStart() && ModeState.State == IGameModeStateInfected.WaitingForPlayers)
                ModeState.SetState(IGameModeStateInfected.Countdown);
        }

        public override void OnPlayerKilled(GameActor attacker, GameActor victim, byte itemclass, byte bodypart)
        {
            victim.State.Set(ActorStates.ActorStateId.Killed);

            foreach (var actors in Actors)
            {
                actors.Peer.Events.Game.SendPlayerKilled(View.GameMode, attacker, victim, itemclass, bodypart);
            }

            if (!victim.isInfected) 
            {
                victim.isInfected = true;

                if (!infectedActors.Contains(victim.ActorInfo.ActorId))
                    infectedActors.Add(victim.ActorInfo.ActorId);

                victim.Peer.Events.Game.SendInfected(View.GameMode);

                int endTime = Environment.TickCount + StartTime;

                EndTime = endTime;

                foreach(var actor in Actors) 
                {
                    actor.Peer.Events.Game.SendRoundTimeUpdate(View.GameMode, endTime);
                }

                foreach (var actor in Actors)
                {
                    actor.Peer.Events.Game.SendPlayerInfectedNotification(View.GameMode, victim.ActorInfo.ActorId);
                }

                if (AllPlayersInfected())
                    State.Set(RoomStates.RoomStateId.End);
            }
        }

        private bool AllPlayersInfected() 
        {
            foreach(var actor in Actors) 
            {
                if (actor.isPlayer && !actor.isInfected)
                    return false;
            }

            return true;
        }

        public override void OnPlayerLeft(int actorid, TeamID team)
        {
            foreach (var others in Actors)
            {
                if (others.ActorInfo.ActorId == actorid) continue;

                others.Peer.Events.Game.SendPlayerLeft(View.GameMode, actorid);
            }

            View.InGamePlayers--;

            if (State.Current.RoomStateID == RoomStates.RoomStateId.Running && View.InGamePlayers <= 1)
                State.Set(RoomStates.RoomStateId.End);

            if (infectedActors.Count == 1 && infectedActors.Contains(actorid) && View.InGamePlayers >= 2 && ModeState.State == IGameModeStateInfected.Running)
                ModeState.SetState(IGameModeStateInfected.Countdown);
            else if(ModeState.State == IGameModeStateInfected.Countdown && View.InGamePlayers < 2)
                ModeState.SetState(IGameModeStateInfected.WaitingForPlayers);
            else
                ModeState.SetState(IGameModeStateInfected.WaitingForPlayers);

            infectedActors.Remove(actorid);
        }

        public override void OnReset()
        {
            infectedActors.Clear();

            foreach(var actor in Actors) 
            {
                if (actor.isPlayer) { actor.isInfected = false; }
            }

            foreach(var actor in Actors) 
            {
                actor.Peer.Events.Game.SendResetInfectedPlayers(View.GameMode);
            }

            if (CanStart())
                ModeState.SetState(IGameModeStateInfected.Countdown);
            else
                ModeState.SetState(IGameModeStateInfected.WaitingForPlayers);
        }

        public override void OnPrepare(GameActor actor)
        {
            // Only used by TDM
        }

        public override void Update()
        {
            ModeState.Tick();
        }
    }
}
