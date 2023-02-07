using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Synchronization;
using UberStrikeClassic.Realtime.Server.Game.Common;

namespace UberStrikeClassic.Realtime.Server.Game.ActorStates
{
    public class ActorStateKilled : State
    {

        private GameActor Actor;

        public ActorStateKilled(GameActor actor) : base(ActorStateId.Killed)
        {
            Actor = actor;
        }

        public override void OnEnter()
        {
            Actor.ActorInfo.Health = 0;

            Actor.ActorInfo.Armor.ArmorPoints = 0;

            Actor.ActorInfo.PlayerState = UberStrike.Realtime.Common.PlayerStates.DEAD;

            List<SyncObject> sync = new List<SyncObject>();

            sync.Add(Actor.GetViewFull());

            byte[] data = RealtimeSerialization.ToBytes(sync).ToArray();

            foreach (var actor in Actor.Room.Actors)
            {
                actor.Peer.Events.Game.SendAllPlayerDeltas(Actor.Room.View.GameMode, data, true);
            }

            
            Actor.Peer.Events.Game.SendNextSpawnPoint(Actor.Room.View.GameMode, 5, Actor.Peer, Actor.ActorInfo.TeamID);
        }

        public override void OnExit()
        {
            List<SyncObject> sync = new List<SyncObject>();

            sync.Add(Actor.GetViewFull());

            byte[] data = RealtimeSerialization.ToBytes(sync).ToArray();

            foreach(var actor in Actor.Room.Actors) 
            {
                actor.Peer.Events.Game.SendAllPlayerDeltas(Actor.Room.View.GameMode, data, true);
            }
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
