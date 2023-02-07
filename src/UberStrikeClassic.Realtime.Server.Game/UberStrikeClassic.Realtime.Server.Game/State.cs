using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Game.ActorStates;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.RoomStates;

namespace UberStrikeClassic.Realtime.Server.Game
{
    public abstract class State
    {
        public RoomStateId RoomStateID { get; }

        public ActorStateId ActorStateID { get; }

        public State(RoomStateId id) 
        {
            RoomStateID = id;
        }

        public State(ActorStateId id)
        {
            ActorStateID = id;
        }

        public abstract void OnEnter();
        public abstract void OnExit();
        public abstract void OnTick();
        public abstract void OnPlayerJoined(GameActor actor);
        public abstract void OnResume();
    }
}
