using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Common;

namespace UberStrikeClassic.Realtime.Server.Game.ActorStates
{
    public class ActorStateEnd : State
    {
        private GameActor Actor;

        public ActorStateEnd(GameActor actor) : base(ActorStateId.End)
        {
            Actor = actor;
        }

        public override void OnEnter()
        {
            var endMatchData = new EndOfMatchData();
            endMatchData.MostEffecientWeaponId = 1004;
            endMatchData.MostValuablePlayers = new List<StatsSummary>();

            foreach (var actor in Actor.Room.Actors)
            {
                if (actor.ActorInfo == null || actor.Stats == null)
                    continue;

                if (actor.isPlayer) 
                {
                    endMatchData.MostValuablePlayers.Add(new StatsSummary()
                    {
                        Cmid = actor.ActorInfo.Cmid,
                        Deaths = actor.Stats.Deaths,
                        Kills = actor.ActorInfo.Kills,
                        Level = actor.ActorInfo.Level,
                        Name = actor.ActorInfo.PlayerName,
                        Team = actor.ActorInfo.TeamID,
                        Achievements = new Dictionary<byte, ushort>()
                    });
                }
            }

            endMatchData.PlayerStatsBestPerLife = Actor.Stats;
            endMatchData.PlayerStatsTotal = Actor.Stats;
            endMatchData.PlayerXpEarned = new Dictionary<byte, ushort>();
            endMatchData.RoundNumber = Actor.Room.RoundNumber;
            endMatchData.PlayerXpEarned.Add((byte)Actor.ActorInfo.Level, Actor.ActorInfo.XP);
            endMatchData.MostValuablePlayers = endMatchData.MostValuablePlayers.OrderByDescending(c => c.Kills).ToList();

            Actor.Peer.Events.Game.SendEndMatch(Actor.Room.View.GameMode, endMatchData);

            Actor.Peer.Events.Game.SendNextRoundCountDown(Actor.Room.View.GameMode, Actor.Room.NextRoundCountdown);
        }

        public override void OnExit()
        {
            // Actor.Reset();
        }

        public override void OnPlayerJoined(GameActor actor)
        {
           
        }

        public override void OnResume()
        {
          
        }

        public override void OnTick()
        {
           
        }
    }
}
