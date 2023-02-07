using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game.RoomStates
{
    public class RoomState
    {
        private Dictionary<RoomStateId, State> states;

        public State Current { get; set; }

        public RoomState(GameRoom room) 
        {
            states = new Dictionary<RoomStateId, State>();

            states.Add(RoomStateId.WaitingForPlayers, new RoomStateWaitingForPlayers(room));
            states.Add(RoomStateId.Running, new RoomStateRunning(room));
            states.Add(RoomStateId.End, new RoomStateEnd(room));
        }

        public void Set(RoomStateId stateId) 
        {
            if (Current != null) { Current.OnExit(); }

            if (states.TryGetValue(stateId, out State state))
            {
                Current = state;

                Current.OnEnter();
            }
        }

        public void Tick() 
        {
            if (Current != null && Current.RoomStateID != RoomStateId.WaitingForPlayers)
            {
                Current.OnTick();
            }
        }
    }

    public enum RoomStateId 
    {
        WaitingForPlayers,
        Running,
        End
    }
}
