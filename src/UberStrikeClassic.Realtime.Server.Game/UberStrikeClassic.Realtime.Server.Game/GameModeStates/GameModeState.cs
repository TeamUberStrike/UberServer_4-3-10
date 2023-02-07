using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Game.GameModeStates
{
    public class GameModeState<T> where T : struct
    {
        private Dictionary<T, IModeState> states;

        private IModeState Current;

        public T State;

        public GameModeState() 
        {
            states = new Dictionary<T, IModeState>();
        }

        public void Register(T Id, IModeState state) 
        {
            states.Add(Id, state);
        }

        public void SetState(T state) 
        {
            if (Current != null)
                Current.OnExit();

            if(states.TryGetValue(state, out IModeState modestate)) 
            {
                Current = modestate;

                Current.OnEnter();

                State = state;
            }
        }

        public void Tick() 
        {
            if(Current != null)
                Current.OnUpdate();
        }
    }
}
