using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using Cmune.Realtime.Common.Synchronization;
using Cmune.Realtime.Common.Utils;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using Photon.SocketServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Core.Types;
using UberStrike.Realtime.Common;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Core.UberStrok;
using UberStrikeClassic.Realtime.Server.Game.Events;
using UberStrikeClassic.Realtime.Server.Game.RoomStates;

namespace UberStrikeClassic.Realtime.Server.Game.Rooms
{
    public abstract class GameRoom : GameRoomOperationEvents, IDisposable
    {
        private bool _disposed = false;

        public GameMetaData View;

        public List<GameActor> actors;

        private List<PlayerPosition> actorMovements;

        private List<SyncObject> actorDeltas;

        public ICollection<GameActor> Actors;

        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public byte nextplayer { get; set; }
        public bool Updated { get; set; }
        public int RoundNumber { get; set; }
        public int NextRoundCountdown { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public CmuneRoomID RoomID { get { return View.RoomID; } }
       
        private readonly Timer _frametimer;
        public Loop Loop { get; }
        public ILoopScheduler Scheduler { get; }
        public RoomState State { get; }
        public SpawnPointManager SpawnPoints { get; }
        public PickupManager PickupManager { get; }

        public GameRoom(GameMetaData data, ILoopScheduler scheduler) : base()
        {
            Scheduler = scheduler;

            Loop = new Loop(OnTick, OnTickError);

            const float UBZ_INTERVAL = 1000f / 9.8f;

            _frametimer = new Timer(Loop, UBZ_INTERVAL);

            View = data;

            nextplayer = 0;

            actorMovements = new List<PlayerPosition>();

            actorDeltas = new List<SyncObject>();

            actors = new List<GameActor>();

            Actors = actors;

            base.OnInitialize(this);

            State = new RoomState(this);

            SpawnPoints = new SpawnPointManager();

            PickupManager = new PickupManager(this);

            Reset();

            State.Set(RoomStateId.WaitingForPlayers);

            Scheduler.Schedule(Loop);

            RoundNumber = 0;

            NextRoundCountdown = 25;
        }

        public abstract bool CanDamage(GameActor victim, GameActor attacker);
        public abstract bool CanJoin(GameActor actor, TeamID team);
        public abstract bool CanStart();
        public abstract void OnPlayerJoined(GameActor actor);
        public abstract void OnPlayerLeft(int actorid, TeamID team);
        public abstract void OnPlayerKilled(GameActor attacker, GameActor victim, byte itemclass, byte bodypart);
        public abstract void OnReset();
        public abstract void OnPrepare(GameActor actor);

        public virtual void Update() 
        {

        }

        public void Join(GamePeer peer) 
        {
            Enqueue(() => DoJoin(peer));
        }

        public bool Find(int actorId) 
        {
            foreach(var ac in Actors) 
            {
                if(ac.ActorInfo.ActorId == actorId) 
                {
                    return true;
                }
            }

            return false;
        }

        public GameActor GetActor(int id) 
        {
            foreach (var ac in Actors)
            {
                if (ac.ActorInfo.ActorId == id)
                {
                    return ac;
                }
            }

            return null;
        }

        public void Leave(GamePeer peer) 
        {
            Enqueue(() => DoLeave(peer));
        }

        private void OnTick() 
        {
            bool updateMovements = _frametimer.Tick();

            State.Tick();

            PickupManager.Tick();

            Update();

            foreach (GameActor actor in Actors)
            {
                if (actor.isPlayer)
                    actor.Tick();
            }


            if (updateMovements) 
            {
                try 
                {
                    foreach (GameActor actor in Actors)
                    {
                        if (actor.hasMoved && actor.ActorInfo.IsAlive && actor.isPlayer)
                        {
                            actorMovements.Add(actor.Movement);

                            actor.hasMoved = false;
                        }
                    }

                    foreach (GameActor actor in Actors)
                    {
                        if (actor.isPlayer)
                        {
                            actorDeltas.Add(actor.GetDeltaView(updateMovements));
                        }
                    }

                    if (actorDeltas.Count > 0)
                    {
                        byte[] deltas = RealtimeSerialization.ToBytes(actorDeltas).ToArray();

                        foreach (var all in Actors)
                        {
                            all.Peer.Events.Game.SendAllPlayerDeltas(View.GameMode, deltas, true);
                        }

                        actorDeltas.Clear();
                    }

                    if (actorMovements.Count > 0) 
                    {
                        List<byte> positions = PlayerPositionsUpdateBytes();

                        byte[] data = RealtimeSerialization.ToBytes(positions).ToArray();

                        foreach (var all in Actors) 
                        {
                            all.Peer.Events.Game.SendAllPositionUpdates(View.GameMode, data);
                        }

                        actorMovements.Clear();
                    }

                }
                catch(Exception ex) 
                {
                    log.Error(ex);
                }
            }

        }

        public List<SyncObject> GetActorsDeltas()
        {
            List<SyncObject> list = new List<SyncObject>();
            foreach(var a in Actors)
            {
                if (a.ActorInfo == null) continue;

                list.Add(a.GetViewFull());
            }

            return list;
        }

        private void OnTickError(Exception ex)
        {
            log.Error(ex);
        }

        private List<byte> PlayerPositionsUpdateBytes() 
        {
            if(actorMovements.Count > 0) 
            {
                List<byte> positionBytes = new List<byte>();
                positionBytes.Add((byte)actorMovements.Count);

                foreach (PlayerPosition position in actorMovements)
                {
                    positionBytes.Add(position.Player);
                    DefaultByteConverter.FromInt(position.Time, ref positionBytes);
                    ShortVector3.Bytes(positionBytes, position.Position);
                }

                return positionBytes;
            }

            return null;
        }

        public void Enqueue(Action a)
        {
            Loop.Enqueue(a);
        }

        public void Reset() 
        {
            _frametimer.Restart();

            PickupManager.ResetPickups();
            SpawnPoints.ResetSpawns();

            NextRoundCountdown = 25;
        }

        public bool DoDamage(GameActor attacker, GameActor victim, short damage, BodyPart part, UberstrikeItemClass itemClass, DamageEvent dmgevent) 
        {
            bool selfDamage = attacker.ActorInfo.Cmid == victim.ActorInfo.Cmid;

            if (State.Current.RoomStateID == RoomStateId.End)
                return false;

            if (!victim.ActorInfo.IsAlive)
                return false;

            /*if (!attacker.ActorInfo.IsAlive)
                return false;*/

            if (!CanDamage(victim, attacker))
                return false;        

            if(View.GameMode == GameModeID.InfectedMode) 
            {
                return true;
            }
            else 
            {
                short realDamage = victim.ActorInfo.Armor.AbsorbDamage(damage, part);

                victim.ActorInfo.Health -= realDamage;

                victim.Peer.Events.Game.SendDamageEffect(View.GameMode, dmgevent);

                if (victim.ActorInfo.Health <= 0)
                {
                    if (!selfDamage)
                    {
                        attacker.UpdateStatsOnKill(itemClass, part);
                        victim.UpdateStatsOnDeath(itemClass);
                    }
                    else
                    {
                        victim.UpdateStatsOnDeath(itemClass, true);
                    }

                    return true;
                }
            }

            return false;
        }

        public void Dispose() 
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if(disposing)
            {
                Scheduler.Unschedule(Loop);

                foreach (var actor in Actors)
                {
                    foreach (var player in Actors)
                    {
                        actor.Peer.Events.Game.SendPlayerLeft(View.GameMode, player.ActorInfo.ActorId);
                    }
                }

                foreach (var actor in Actors)
                {
                    var peer = actor.Peer;

                    peer.Actor = null;

                    peer.Disconnect();
                }

                actors.Clear();
            }

            _disposed = true;
        }

        private void DoLeave(GamePeer peer) 
        {
            GameActor leaver = peer.Actor;

            try
            {
                if (Find(leaver.ActorInfo.ActorId))
                {
                    log.ErrorFormat("Player Left the Game [CLEANUP STARTED] Name: {0}", leaver.ActorInfo.PlayerName);

                    OnPlayerLeft(leaver.ActorInfo.ActorId, leaver.ActorInfo.TeamID);

                    actors.Remove(leaver);

                    peer.Actor.ActorInfo = null;

                    peer.Actor.Stats = null;

                    peer.Actor = null;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void DoJoin(GamePeer peer) 
        {
            try
            {
                GameActor joinActor = new GameActor(peer, this);

                peer.Actor = joinActor;

                int actorId = 1;
                byte playerNum = 1;

                if (Actors != null && Actors.Count > 0)
                {
                    actorId = Actors.Max(c => c.ID) + 1;
                    playerNum = Convert.ToByte(Actors.Max(c => c.ActorInfo.PlayerNumber) + 1);
                }

                peer.Actor.ID = actorId;

                peer.Actor.Number = playerNum;

                actors.Add(peer.Actor);

                peer.Actor.ActorInfo.PlayerNumber = playerNum;

                peer.Actor.State.Set(ActorStates.ActorStateId.Overview);

                log.ErrorFormat("GamePeer joined room. [RoomID: {0}]", RoomID.Number);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public RoomMetaData GetView()
        {
            if(View.ConnectedPlayers != Actors.Count) 
            {
                View.ConnectedPlayers = Actors.Count;
                Updated = true;
            }

            return View;
        }
    }
}
