using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game.RoomStates
{
    public class RoomStateWaitingForPlayers : State
    {
        private GameRoom Room { get; }

        public RoomStateWaitingForPlayers(GameRoom room) : base(RoomStateId.WaitingForPlayers)
        {
            Room = room;
        }

        public override void OnEnter()
        {
            
        }

        public override void OnExit()
        {
            
        }

        public override void OnTick()
        {
           
        }

        public override void OnPlayerJoined(GameActor actor)
        {
            Room.State.Set(RoomStateId.Running);
        }

        public override void OnResume()
        {
            
        }
    }
}
