using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game.RoomStates
{
    public class RoomStateRunning : State
    {
        private GameRoom Room;

        public RoomStateRunning(GameRoom room) : base(RoomStateId.Running)
        {
            Room = room;
        }

        public override void OnEnter()
        {
            if (Room.View.GameMode == GameModeID.InfectedMode)
                Room.StartTime = Room.View.RoundTime / 3 * 1000;

            Room.EndTime = Environment.TickCount + Room.View.RoundTime * 1000;

            foreach(var actor in Room.Actors) 
            {
                if (actor.isPlayer) 
                {
                    actor.State.Set(ActorStates.ActorStateId.Playing);
                }
            }
        }

        public override void OnExit()
        {
            
        }

        public override void OnPlayerJoined(GameActor actor)
        {
            actor.State.Set(ActorStates.ActorStateId.Playing);
        }

        public override void OnResume()
        {
           
        }

        public override void OnTick()
        {
            if(Environment.TickCount >= Room.EndTime) 
            {
                Room.State.Set(RoomStateId.End);
            }
        }
    }
}
