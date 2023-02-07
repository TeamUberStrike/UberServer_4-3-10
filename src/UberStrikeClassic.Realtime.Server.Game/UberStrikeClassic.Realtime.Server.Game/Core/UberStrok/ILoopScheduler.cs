using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Game.Core.UberStrok
{
    public interface ILoopScheduler
    {
        void Schedule(ILoop loop);
        bool Unschedule(ILoop loop);
        float GetLoad();
    }
}
