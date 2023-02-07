using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Game.GameModeStates
{
    public interface IModeState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
    }
}
