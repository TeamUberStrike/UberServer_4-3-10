using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Logging;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Core.UberStrok;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game.RoomStates
{
    public class RoomStateEnd : State
    {
        private GameRoom Room;

        private Countdown restartCountdown;

        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public RoomStateEnd(GameRoom room) : base(RoomStateId.End)
        {
            Room = room;

            restartCountdown = new Countdown(Room.Loop, 25, 0);
            restartCountdown.Completed += OnRestartCountdownCompleted;
            restartCountdown.Counted += OnCountdown;
        }

        public override void OnEnter()
        {
            restartCountdown.Restart();

            Room.NextRoundCountdown = 25;

            foreach(var actor in Room.Actors) 
            {
                if(actor.isPlayer)
                    actor.State.Set(ActorStates.ActorStateId.End);
            }
        }

        public override void OnExit()
        {
           
        }

        public override void OnPlayerJoined(GameActor actor)
        {
            actor.State.Set(ActorStates.ActorStateId.End);
        }

        public override void OnResume()
        {
            
        }

        public override void OnTick()
        {
            restartCountdown.Tick();
        }

        private void OnCountdown(int count)
        {
            Room.NextRoundCountdown = count;

            if(Room.NextRoundCountdown > 0)
            {
                int readyPlayersCount = Room.Actors.Where(c => c.ActorInfo != null && c.ActorInfo.IsReadyForGame).Count();

                if(readyPlayersCount >= Room.View.InGamePlayers)
                {
                    Room.Reset();
                    Room.RoundNumber++;

                    Room.State.Set(RoomStateId.Running);

                    Room.OnReset();
                }
            }
        }

        private void OnRestartCountdownCompleted()
        {
            Room.Reset();
            Room.RoundNumber++;

            Room.State.Set(RoomStateId.Running);

            Room.OnReset();
        }
    }
}
