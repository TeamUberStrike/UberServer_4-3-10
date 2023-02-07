using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using ExitGames.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Core.UberStrok;

namespace UberStrikeClassic.Realtime.Server.Game.ActorStates
{
    public class ActorStatePlaying : State
    {
        private GameActor Actor;

        FixedTimer healthTimer { get; set; }

        public ActorStatePlaying(GameActor actor) : base(ActorStateId.Playing) 
        {
            Actor = actor;

            healthTimer = new FixedTimer(Actor.Room.Loop, 1000f);
        }

        public override void OnEnter()
        {
            Actor.Peer.Events.Game.SendMatchStart(Actor.Room.View.GameMode, Actor.Room.EndTime);

            Actor.Reset();

            foreach(var a in Actor.Room.Actors)
            {
                a.Peer.Events.Game.SendAllPlayerDeltas(Actor.Room.View.GameMode, RealtimeSerialization.ToBytes(new List<SyncObject>() { Actor.GetViewFull() }).ToArray(), true);
            }

            Actor.Peer.Events.Game.SendNextSpawnPoint(Actor.Room.View.GameMode, 0, Actor.Peer, Actor.ActorInfo.TeamID);
        }

        public override void OnExit()
        {
         
        }

        public override void OnPlayerJoined(GameActor actor)
        {
           
        }

        public override void OnTick()
        {
            int armorcapacity = Actor.ActorInfo.Armor.ArmorPointCapacity;

            healthTimer.IsEnabled = Actor.ActorInfo.Health > GameServerConfig.DefaultHealth || Actor.ActorInfo.Armor.ArmorPoints > armorcapacity;

            while (healthTimer.Tick())
            {
                if (Actor.ActorInfo.Health > GameServerConfig.DefaultHealth)
                    Actor.ActorInfo.Health--;
                if (Actor.ActorInfo.Armor.ArmorPoints > armorcapacity)
                    Actor.ActorInfo.Armor.ArmorPoints--;
            }
        }

        public override void OnResume()
        {
            healthTimer.Reset();
        }
    }
}
