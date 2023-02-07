using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game.ActorStates
{
    public class ActorState
    {
        private Dictionary<ActorStateId, State> states;

        public State Current { get; set; }

        public ActorState(GameActor actor)
        {
            states = new Dictionary<ActorStateId, State>();

            states.Add(ActorStateId.Overview, new ActorStateOverview(actor));
            states.Add(ActorStateId.Playing, new ActorStatePlaying(actor));
            states.Add(ActorStateId.Killed, new ActorStateKilled(actor));
            states.Add(ActorStateId.End, new ActorStateEnd(actor));
        }

        public void Set(ActorStateId stateId)
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
            if (Current != null)
            {
                Current.OnTick();
            }
        }

        public void Previous() 
        {
            if(Current.ActorStateID == ActorStateId.Killed) 
            {
                Current.OnExit();

                if (states.TryGetValue(ActorStateId.Playing, out State state))
                {
                    Current = state;

                    Current.OnResume();
                }
            }
        }
    }

    public enum ActorStateId 
    {
        Overview,
        Playing,
        Killed,
        End
    }
}
