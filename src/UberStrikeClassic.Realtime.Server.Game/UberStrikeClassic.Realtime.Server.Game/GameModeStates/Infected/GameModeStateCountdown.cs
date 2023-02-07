using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Game.Core.UberStrok;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game.GameModeStates.Infected
{
    public class GameModeStateCountdown : IModeState
    {
        private Countdown restartCountdown;

        private GameRoom Room;

        public GameModeStateCountdown(GameRoom room, Action callback) 
        {
            Room = room;

            restartCountdown = new Countdown(Room.Loop, 15, 0);
            restartCountdown.Completed += callback;
            restartCountdown.Counted += OnCountdown;
        }

        public void OnEnter()
        {
            restartCountdown.Restart();
        }

        public void OnExit()
        {
            restartCountdown.Reset();
        }

        public void OnUpdate()
        {
            restartCountdown.Tick();
        }

        private void OnCountdown(int count)
        {
            foreach(var actor in Room.Actors) 
            {
                if (actor.isPlayer) 
                {
                    actor.Peer.Events.Game.SendInfectedCountDown(Room.View.GameMode, count);
                }
            }
        }
    }
}
