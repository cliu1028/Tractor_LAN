﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using Duan.Xiugang.Tractor.Objects;
using System.Threading;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.ServiceModel.Channels;
using System.Configuration;


[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace TractorServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class TractorHost : ITractorHost
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
    (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string[] RoomNames = new string[] { "桃园结义", "五谷丰登", "无中生有" };
        internal int MaxRoom = 0;
        internal bool AllowSameIP = false;
        public string KeyMaxRoom = "maxRoom";
        public string KeyAllowSameIP = "allowSameIP";

        public CardsShoe CardsShoe { get; set; }

        public Dictionary<string, IPlayer> PlayersProxy { get; set; }
        public List<GameRoom> GameRooms { get; set; }
        public Dictionary<string, GameRoom> SessionIDGameRoom { get; set; }

        public TractorHost()
        {
            var myreader = new AppSettingsReader();
            try
            {
                MaxRoom = (int)myreader.GetValue(KeyMaxRoom, typeof(int));
                AllowSameIP = (bool)myreader.GetValue(KeyAllowSameIP, typeof(bool));
            }
            catch (Exception ex)
            {
                log.Debug(string.Format("reading config {0} failed with exception: {1}", KeyAllowSameIP, ex.Message));
            }

            CardsShoe = new CardsShoe();
            PlayersProxy = new Dictionary<string, IPlayer>();
            GameRooms = new List<GameRoom>();
            for (int i = 0; i < this.MaxRoom; i++)
            {
                GameRoom gameRoom = new GameRoom(i, RoomNames[i]);
                GameRooms.Add(gameRoom);
                if (!Directory.Exists(gameRoom.LogsByRoomFolder))
                {
                    Directory.CreateDirectory(gameRoom.LogsByRoomFolder);
                }
            }
            SessionIDGameRoom = new Dictionary<string, GameRoom>();
        }

        #region implement interface ITractorHost

        public void PlayerEnterHall(string playerID)
        {
            if (!PlayersProxy.Keys.Contains(playerID))
            {
                IPlayer player = OperationContext.Current.GetCallbackChannel<IPlayer>();
                PlayersProxy.Add(playerID, player);
                log.Debug(string.Format("player {0} entered hall.", playerID));
                string clientIP = GetClientIP();
                GameRoom.LogClientInfo(clientIP, playerID, false);
                UpdateGameHall();
            }
            else
            {
                PlayersProxy[playerID].NotifyMessage("已在游戏大厅里或重名");
            }
        }

        public void PlayerEnterRoom(string playerID, int roomID)
        {
            string clientIP = GetClientIP();
            string sessionID = GetSessionID();
            IPlayer player = PlayersProxy[playerID];
            if (player != null)
            {
                GameRoom gameRoom = this.GameRooms.Single((room) => room.RoomID == roomID);
                if (gameRoom == null)
                {
                    player.NotifyMessage(string.Format("加入房间失败，房间号【{0}】不存在", roomID));
                }
                else
                {
                    lock (gameRoom)
                    {
                        bool entered = gameRoom.PlayerEnterRoom(playerID, clientIP, player, AllowSameIP);
                        if (entered)
                        {
                            SessionIDGameRoom[sessionID] = gameRoom;
                            Thread.Sleep(500);
                            Thread thr = new Thread(new ThreadStart(this.UpdateGameHall));
                            thr.Start();
                        }
                    }
                }
            }
            else
            {
                log.Debug(string.Format("PlayerEnterRoom failed, playerID: {0}, roomID: {1}", playerID, roomID));
            }
        }

        //玩家退出房间
        public void PlayerExitRoom(string playerID)
        {
            string sessionID = GetSessionID();
            if (this.SessionIDGameRoom.ContainsKey(sessionID))
            {
                GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
                //如果退出的是正常玩家，则先将旁观玩家移出房间
                if (gameRoom.PlayersProxy.ContainsKey(playerID))
                {
                    List<string> obs = gameRoom.ObserversProxy.Keys.ToList<string>();
                    foreach (string ob in obs)
                    {
                        gameRoom.PlayerQuit(ob);
                    }
                }

                //再将正常玩家移出房间
                gameRoom.PlayerQuit(playerID);
                SessionIDGameRoom.Remove(sessionID);
            }
            log.Debug(string.Format("player {0} exited room.", playerID));
            Thread.Sleep(500);
            Thread thr = new Thread(new ThreadStart(this.UpdateGameHall));
            thr.Start();
        }

        //玩家退出游戏
        public IAsyncResult BeginPlayerQuit(string playerID, AsyncCallback callback, object state)
        {
            string sessionID = GetSessionID();
            if (this.SessionIDGameRoom.ContainsKey(sessionID))
            {
                GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
                //如果退出的是正常玩家，则先将旁观玩家移出房间
                if (gameRoom.PlayersProxy.ContainsKey(playerID))
                {
                    List<string> obs = gameRoom.ObserversProxy.Keys.ToList<string>();
                    foreach (string ob in obs)
                    {
                        gameRoom.PlayerQuit(ob);
                    }
                }

                //再将正常玩家移出房间
                gameRoom.PlayerQuit(playerID);
                SessionIDGameRoom.Remove(sessionID);
            }
            PlayersProxy.Remove(playerID);
            string result = string.Format("player {0} quit.", playerID);
            log.Debug(result);
            Thread.Sleep(500);
            Thread thr = new Thread(new ThreadStart(this.UpdateGameHall));
            thr.Start();

            return new CompletedAsyncResult<string>(result);
        }
        public string EndPlayerQuit(IAsyncResult ar)
        {
            CompletedAsyncResult<string> result = ar as CompletedAsyncResult<string>;
            log.Debug(string.Format("EndServiceAsyncMethod called with: \"{0}\"", result.Data));
            return result.Data;
        }

        public void PlayerIsReadyToStart(string playerID)
        {
            string sessionID = GetSessionID();
            if (!this.SessionIDGameRoom.ContainsKey(sessionID))
            {
                IPlayer player = PlayersProxy[playerID];
                player.NotifyMessage(string.Format("就绪失败，sessionID【{0}】不匹配任何房间", sessionID));
            }
            else
            {
                GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
                gameRoom.PlayerIsReadyToStart(playerID);
            }
        }

        public void PlayerToggleIsRobot(string playerID)
        {
            string sessionID = GetSessionID();
            if (!this.SessionIDGameRoom.ContainsKey(sessionID))
            {
                IPlayer player = PlayersProxy[playerID];
                player.NotifyMessage(string.Format("托管失败，sessionID【{0}】不匹配任何房间", sessionID));
            }
            else
            {
                GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
                gameRoom.PlayerToggleIsRobot(playerID);
            }
        }

        //player discard last 8 cards
        public void StoreDiscardedCards(int[] cards)
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.StoreDiscardedCards(cards);
        }

        //亮主
        public void PlayerMakeTrump(Duan.Xiugang.Tractor.Objects.TrumpExposingPoker trumpExposingPoker, Duan.Xiugang.Tractor.Objects.Suit trump, string trumpMaker)
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.PlayerMakeTrump(trumpExposingPoker, trump, trumpMaker);
        }

        public void PlayerShowCards(CurrentTrickState currentTrickState)
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.PlayerShowCards(currentTrickState);
        }

        public ShowingCardsValidationResult ValidateDumpingCards(List<int> selectedCards, string playerId)
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            return gameRoom.ValidateDumpingCards(selectedCards, playerId);
        }

        public void RefreshPlayersCurrentHandState()
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.RefreshPlayersCurrentHandState();
        }

        //随机组队
        public void TeamUp()
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.TeamUp();
        }

        //和下家互换座位
        public void MoveToNextPosition(string playerId)
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.MoveToNextPosition(playerId);
        }

        //旁观：选牌
        public void CardsReady(string playerId, ArrayList myCardIsReady)
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.CardsReady(playerId, myCardIsReady);
        }

        //读取牌局
        public void RestoreGameStateFromFile()
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.RestoreGameStateFromFile();
        }

        //设置从几打起
        public void SetBeginRank(string beginRankString)
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.SetBeginRank(beginRankString);
        }

        //旁观玩家 by id
        public void ObservePlayerById(string playerId, string observerId)
        {
            string sessionID = GetSessionID();
            GameRoom gameRoom = this.SessionIDGameRoom[sessionID];
            gameRoom.ObservePlayerById(playerId, observerId);
        }
        #endregion

        #region Update Client State
        public void UpdateGameHall()
        {
            foreach (IPlayer player in PlayersProxy.Values)
            {
                bool isInRoom = false;
                foreach (GameRoom room in this.GameRooms)
                {
                    if (room.PlayersProxy.ContainsValue(player) ||
                        room.ObserversProxy.ContainsValue(player))
                    {
                        isInRoom = true;
                        break;
                    }
                }

                if (!isInRoom)
                {
                    List<string> names = PlayersProxy.Keys.ToList<string>();
                    player.NotifyGameHall(this.GameRooms, names);
                }
            }
        }

        #endregion

        private string GetClientIP()
        {
            string ip = "";
            OperationContext context = OperationContext.Current;
            MessageProperties prop = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint =
                   prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            ip = endpoint.Address;
            return ip;
        }

        private string GetSessionID()
        {
            return OperationContext.Current.SessionId;
        }
    }

    // Simple async result implementation.
    class CompletedAsyncResult<T> : IAsyncResult
    {
        T data;

        public CompletedAsyncResult(T data)
        { this.data = data; }

        public T Data
        { get { return data; } }

        #region IAsyncResult Members
        public object AsyncState
        { get { return (object)data; } }

        public WaitHandle AsyncWaitHandle
        { get { throw new Exception("The method or operation is not implemented."); } }

        public bool CompletedSynchronously
        { get { return true; } }

        public bool IsCompleted
        { get { return true; } }
        #endregion
    }
}