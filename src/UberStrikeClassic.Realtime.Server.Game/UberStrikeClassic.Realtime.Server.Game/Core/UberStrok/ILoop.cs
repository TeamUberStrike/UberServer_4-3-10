using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Game.Core.UberStrok
{
    public interface ILoop
    {
        float Time { get; }
        float DeltaTime { get; }

        void Setup();
        void Tick();
        void Teardown();
    }
}
