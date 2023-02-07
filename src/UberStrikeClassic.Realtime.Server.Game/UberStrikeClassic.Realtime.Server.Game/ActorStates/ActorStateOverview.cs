using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game.ActorStates
{
    public class ActorStateOverview : State
    {
        private GameActor Actor;

        public ActorStateOverview(GameActor actor) : base(ActorStateId.Overview) 
        {
           Actor = actor;
        }

        public override void OnEnter()
        {
            Actor.Peer.Events.Game.SendBegin(Actor.Room.View.GameMode);

            if (Actor.Room.Actors.Count > 0) 
            {
                Actor.Peer.Events.Game.SendFullActorListUpdate(Actor);
            }

            if(Actor.Room.View.GameMode == GameModeID.TeamDeathMatch) 
            {
                Actor.Room.OnPrepare(Actor);
            }
        }

        public override void OnExit()
        {
            
        }

        public override void OnPlayerJoined(GameActor actor)
        {
            
        }

        public override void OnResume()
        {
           
        }

        public override void OnTick()
        {
           
        }
    }
}
