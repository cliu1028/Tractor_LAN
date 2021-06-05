using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using Duan.Xiugang.Tractor.Objects;
using Duan.Xiugang.Tractor.Player;
using Duan.Xiugang.Tractor.Properties;
using Kuaff.CardResouces;
using AutoUpdaterDotNET;
using System.Diagnostics;
using System.Reflection;

namespace Duan.Xiugang.Tractor
{
    internal partial class MainForm : Form
    {
        #region ��������

        //������ͼ��
        internal Dictionary<string, int> PlayerPosition;
        internal Dictionary<int, string> PositionPlayer;
        internal int Scores = 0;
        internal List<int> SelectedCards;
        internal TractorPlayer ThisPlayer;
        internal object[] UserAlgorithms = {null, null, null, null};
        internal Bitmap bmp = null;
        internal CalculateRegionHelper calculateRegionHelper = null;
        internal Bitmap[] cardsImages = new Bitmap[54];
        internal int cardsOrderNumber = 0;
        internal bool updateOnLoad = false;

        internal CurrentPoker[] currentAllSendPokers =
        {
            new CurrentPoker(), new CurrentPoker(), new CurrentPoker(),
            new CurrentPoker()
        };

        //ԭʼ����ͼƬ

        //��ͼ�Ĵ��������ڷ���ʱʹ�ã�
        internal int currentCount = 0;

        internal CurrentPoker[] currentPokers =
        {
            new CurrentPoker(), new CurrentPoker(), new CurrentPoker(),
            new CurrentPoker()
        };

        internal int currentRank = 0;

        //��ǰһ�ָ��ҵĳ������
        internal ArrayList[] currentSendCards = new ArrayList[4];
        internal CurrentState currentState;
        internal DrawingFormHelper drawingFormHelper = null;
        //Ӧ��˭����
        //һ�γ�����˭���ȿ�ʼ������
        internal int firstSend = 0;
        internal GameConfig gameConfig = new GameConfig();
        internal Bitmap image = null;
        internal bool isNew = true;
        internal ArrayList myCardIsReady = new ArrayList();

        //*��������
        //��ǰ�����Ƶ�����
        internal ArrayList myCardsLocation = new ArrayList();
        //��ǰ�����Ƶ���ֵ
        internal ArrayList myCardsNumber = new ArrayList();
        internal ArrayList[] pokerList = null;
        //��ǰ�����Ƶ��Ƿ񱻵��
        //��ǰ�۵׵���
        internal ArrayList send8Cards = new ArrayList();
        internal int showSuits = 0;

        //*���ҵ��Ƶĸ�������
        //����˳��
        internal long sleepMaxTime = 2000;
        internal long sleepTime;
        internal CardCommands wakeupCardCommands;

        //*�滭������
        //DrawingForm����

        //����ʱĿǰ��������һ��
        internal int whoIsBigger = 0;
        internal int whoShowRank = 0;
        internal int whoseOrder = 0; //0δ��,1�ң�2�Լң�3����,4����

        internal int timerCountDown = 0;

        //�����ļ�

        #endregion // ��������

        private readonly string roomControlPrefix = "roomControl";

        internal MainForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.StandardDoubleClick, true);


            //��ȡ��������
            InitAppSetting();

            notifyIcon.Text = Text;
            BackgroundImage = image;

            //������ʼ��
            bmp = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            ThisPlayer = new TractorPlayer();

            //���ص�ǰ����
            var nickName = FormSettings.GetSettingString(FormSettings.KeyNickName);
            this.lblSouthNickName.Text = nickName;
            ThisPlayer.PlayerId = nickName;
            ThisPlayer.MyOwnId = nickName;
            updateOnLoad = FormSettings.GetSettingBool(FormSettings.KeyUpdateOnLoad);

            //ping host
            this.progressBarPingHost.Visible = true;
            this.tmrGeneral.Start();
            this.lblSouthStarter.Text = "Connecting...";
            ThisPlayer.PingHost();
            
            ThisPlayer.PlayerOnGetCard += PlayerGetCard;
            ThisPlayer.GameOnStarted += StartGame;
            ThisPlayer.HostIsOnline += ThisPlayer_HostIsOnlineEventHandler;
            
            ThisPlayer.TrumpChanged += ThisPlayer_TrumpUpdated;
            ThisPlayer.AllCardsGot += ResortMyCards;
            ThisPlayer.PlayerShowedCards += ThisPlayer_PlayerShowedCards;
            ThisPlayer.ShowingCardBegan += ThisPlayer_ShowingCardBegan;
            ThisPlayer.GameHallUpdatedEvent += ThisPlayer_GameHallUpdatedEventHandler;
            ThisPlayer.RoomSettingUpdatedEvent += ThisPlayer_RoomSettingUpdatedEventHandler;
            ThisPlayer.ShowAllHandCardsEvent += ThisPlayer_ShowAllHandCardsEventHandler;
            ThisPlayer.NewPlayerJoined += ThisPlayer_NewPlayerJoined;
            ThisPlayer.NewPlayerReadyToStart += ThisPlayer_NewPlayerReadyToStart;
            ThisPlayer.PlayerToggleIsRobot += ThisPlayer_PlayerToggleIsRobot;
            ThisPlayer.PlayersTeamMade += ThisPlayer_PlayersTeamMade;
            ThisPlayer.TrickFinished += ThisPlayer_TrickFinished;
            ThisPlayer.TrickStarted += ThisPlayer_TrickStarted;
            ThisPlayer.HandEnding += ThisPlayer_HandEnding;
            ThisPlayer.SpecialEndingEvent += ThisPlayer_SpecialEndingEventHandler;
            ThisPlayer.StarterFailedForTrump += ThisPlayer_StarterFailedForTrump;
            ThisPlayer.StarterChangedEvent += ThisPlayer_StarterChangedEventHandler;
            ThisPlayer.NotifyMessageEvent += ThisPlayer_NotifyMessageEventHandler;
            ThisPlayer.NotifyStartTimerEvent += ThisPlayer_NotifyStartTimerEventHandler;
            ThisPlayer.NotifyCardsReadyEvent += ThisPlayer_NotifyCardsReadyEventHandler;
            ThisPlayer.ResortMyCardsEvent += ThisPlayer_ResortMyCardsEventHandler;
            ThisPlayer.Last8Discarded += ThisPlayer_Last8Discarded;
            ThisPlayer.DistributingLast8Cards += ThisPlayer_DistributingLast8Cards;
            ThisPlayer.DiscardingLast8 += ThisPlayer_DiscardingLast8;
            ThisPlayer.DumpingFail += ThisPlayer_DumpingFail;
            SelectedCards = new List<int>();
            PlayerPosition = new Dictionary<string, int>();
            PositionPlayer = new Dictionary<int, string>();
            drawingFormHelper = new DrawingFormHelper(this);
            calculateRegionHelper = new CalculateRegionHelper(this);

            for (int i = 0; i < 54; i++)
            {
                cardsImages[i] = null; //��ʼ��
            }
        }


        private void InitAppSetting()
        {
            //û�������ļ������config�ļ��ж�ȡ
            if (!File.Exists("gameConfig"))
            {
                var reader = new AppSettingsReader();
                try
                {
                    Text = (String) reader.GetValue("title", typeof (String));
                }
                catch (Exception ex)
                {
                    Text = "��������ս";
                }

                try
                {
                    gameConfig.MustRank = (String) reader.GetValue("mustRank", typeof (String));
                }
                catch (Exception ex)
                {
                    gameConfig.MustRank = ",3,8,11,12,13,";
                }

                try
                {
                    gameConfig.IsDebug = (bool) reader.GetValue("debug", typeof (bool));
                }
                catch (Exception ex)
                {
                    gameConfig.IsDebug = false;
                }

                try
                {
                    gameConfig.BottomAlgorithm = (int) reader.GetValue("bottomAlgorithm", typeof (int));
                }
                catch (Exception ex)
                {
                    gameConfig.BottomAlgorithm = 1;
                }
            }
            else
            {
                //ʵ�ʴ�gameConfig�ļ��ж�ȡ
                Stream stream = null;
                try
                {
                    IFormatter formatter = new BinaryFormatter();
                    stream = new FileStream("gameConfig", FileMode.Open, FileAccess.Read, FileShare.Read);
                    gameConfig = (GameConfig) formatter.Deserialize(stream);
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }

            //δ���л���ֵ
            var myreader = new AppSettingsReader();
            gameConfig.CardsResourceManager = Kuaff_Cards.ResourceManager;
            try
            {
                var bkImage = (String) myreader.GetValue("backImage", typeof (String));
                image = new Bitmap(bkImage);
            }
            catch (Exception ex)
            {
                image = Resources.Backgroud;
            }

            try
            {
                Text = (String) myreader.GetValue("title", typeof (String));
            }
            catch (Exception ex)
            {
            }

            gameConfig.CardImageName = "";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ThisPlayer.Quit();
            }
            catch (Exception)
            {
            }
        }

        //������ͣ�����ʱ�䣬�Լ���ͣ�������ִ������
        internal void SetPauseSet(int max, CardCommands wakeup)
        {
            sleepMaxTime = max;
            sleepTime = DateTime.Now.Ticks;
            wakeupCardCommands = wakeup;
            currentState.CurrentCardCommands = CardCommands.Pause;
        }

        #region �����¼��������

        private void init()
        {
            //��Ϸ��ʼǰ���ø��ֱ���
            this.ThisPlayer.ShowLastTrickCards = false;
            this.ThisPlayer.playerLocalCache = new PlayerLocalCache();

            //ÿ�γ�ʼ�����ػ汳��
            Graphics g = Graphics.FromImage(bmp);
            drawingFormHelper.DrawBackground(g);

            //Ŀǰ�����Է���
            showSuits = 0;
            whoShowRank = 0;

            //�÷�����
            Scores = 0;


            //����Sidebar
            drawingFormHelper.DrawSidebar(g);
            //���ƶ�������

            drawingFormHelper.Starter();

            //����Rank
            drawingFormHelper.Rank();


            //���ƻ�ɫ
            drawingFormHelper.Trump();

            send8Cards = new ArrayList();
            //������ɫ
            if (currentRank == 53)
            {
                currentState.Suit = 5;
            }

            Refresh();
        }

        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            //�Թ۲��ܴ������Ч��
            if (ThisPlayer.isObserver)
            {
                return;
            }
            //���
            //ֻ�з���ʱ�͸��ҳ���ʱ������Ӧ����¼�
            if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing ||
                ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DiscardingLast8Cards)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if ((e.X >= (int)myCardsLocation[0] &&
                         e.X <= ((int)myCardsLocation[myCardsLocation.Count - 1] + 71 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor)) && (e.Y >= 355 + drawingFormHelper.offsetY && e.Y < 472 + drawingFormHelper.offsetY + 96 * (drawingFormHelper.scaleDividend - drawingFormHelper.scaleDivisor) / drawingFormHelper.scaleDivisor))
                    {
                        if (calculateRegionHelper.CalculateClickedRegion(e, 1))
                        {
                            drawingFormHelper.DrawMyPlayingCards(ThisPlayer.CurrentPoker);
                            Refresh();

                            ThisPlayer.CardsReady(ThisPlayer.PlayerId, myCardIsReady);
                        }
                    }
                }
                else if (e.Button == MouseButtons.Right) //�Ҽ�
                {
                    if ((e.X >= (int)myCardsLocation[0] &&
                         e.X <= ((int)myCardsLocation[myCardsLocation.Count - 1] + 71 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor)) && (e.Y >= 355 + drawingFormHelper.offsetY && e.Y < 472 + drawingFormHelper.offsetY + 96 * (drawingFormHelper.scaleDividend - drawingFormHelper.scaleDivisor) / drawingFormHelper.scaleDivisor))
                    {
                        int i = calculateRegionHelper.CalculateRightClickedRegion(e);
                        if (i > -1 && i < myCardIsReady.Count)
                        {
                            int readyCount = 0;
                            for (int ri = 0; ri < myCardIsReady.Count; ri++)
                            {
                                if (ri != i && (bool)myCardIsReady[ri]) readyCount++;
                            }
                            bool b = (bool)myCardIsReady[i];
                            int x = (int)myCardsLocation[i];
                            int clickedCardNumber = (int)myCardsNumber[i];
                            //��Ӧ�Ҽ���3�������
                            //1. ����ƣ�Ĭ�ϣ�
                            int selectMoreCount = Math.Min(i, 8 - 1 - readyCount);
                            bool isLeader = false;
                            if (ThisPlayer.CurrentHandState.CurrentHandStep != HandStep.DiscardingLast8Cards)
                            {
                                //2. ����
                                if (ThisPlayer.CurrentTrickState.LeadingCards != null && ThisPlayer.CurrentTrickState.LeadingCards.Count > 0)
                                {
                                    selectMoreCount = Math.Min(i, ThisPlayer.CurrentTrickState.LeadingCards.Count - 1 - readyCount);
                                }
                                //3. �׳�
                                else if (ThisPlayer.CurrentTrickState.Learder == ThisPlayer.PlayerId)
                                {
                                    isLeader = true;
                                    selectMoreCount = i;
                                }
                            }
                            if (b)
                            {
                                var showingCardsCp = new CurrentPoker();
                                showingCardsCp.TrumpInt = (int)ThisPlayer.CurrentHandState.Trump;
                                showingCardsCp.Rank = ThisPlayer.CurrentHandState.Rank;

                                for (int j = 1; j <= selectMoreCount; j++)
                                {
                                    //�����ѡ����ͬһ��ɫ
                                    if ((int)myCardsLocation[i - j] == (x - 12 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor))
                                    {
                                        if (isLeader)
                                        {
                                            //��һ��������ѡ��Ϊ���ӣ�������
                                            int toAddCardNumber = (int)myCardsNumber[i - j];
                                            int toAddCardNumberOnRight = (int)myCardsNumber[i - j + 1];
                                            showingCardsCp.AddCard(toAddCardNumberOnRight);
                                            showingCardsCp.AddCard(toAddCardNumber);

                                            if (showingCardsCp.Count == 2 && (showingCardsCp.GetPairs().Count == 1) || //�����һ��
                                                ((showingCardsCp.GetTractorOfAnySuit().Count > 1) &&
                                                showingCardsCp.Count == showingCardsCp.GetTractorOfAnySuit().Count * 2))  //�����������
                                            {
                                                myCardIsReady[i - j] = b;
                                                myCardIsReady[i - j + 1] = b;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                            x = x - 12 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor;
                                            j++;
                                        }
                                        else
                                        {
                                            //��׻��߸���
                                            myCardIsReady[i - j] = b;
                                        }
                                        x = x - 12 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 1; j <= i; j++)
                                {
                                    if ((int)myCardsLocation[i - j] == (x - 12 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor))
                                    {
                                        myCardIsReady[i - j] = b;
                                        x = x - 12 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                            }
                            SelectedCards.Clear();
                            for (int k = 0; k < myCardIsReady.Count; k++)
                            {
                                if ((bool)myCardIsReady[k])
                                {
                                    SelectedCards.Add((int)myCardsNumber[k]);
                                }
                            }

                            drawingFormHelper.DrawMyPlayingCards(ThisPlayer.CurrentPoker);
                            Refresh();

                            ThisPlayer.CardsReady(ThisPlayer.PlayerId, myCardIsReady);
                        }
                    }
                    else
                    {
                        this.ThisPlayer.ShowLastTrickCards = !this.ThisPlayer.ShowLastTrickCards;
                        if (this.ThisPlayer.ShowLastTrickCards)
                        {
                            //������һ�ָ����������ƣ���С��һ�룬�������½ǣ������ػ���ǰ�ָ�����������
                            ThisPlayer_PlayerLastTrickShowedCards();
                            //�鿴˭����ʲô��
                            drawingFormHelper.LastTrumpMadeCardsShow();
                            //�鿴�÷���
                            drawingFormHelper.DrawScoreCards();
                        }
                        else
                        {
                            ThisPlayer_PlayerCurrentTrickShowedCards(true);
                        }
                    }
                }
            }
            else if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DistributingCards ||
                     ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DistributingCardsFinished)
            {
                ExposeTrump(e);
            }
            //һ�ֽ���ʱ�Ҽ��鿴���һ�ָ����������ƣ���С��һ�룬�������½�
            else if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Ending && e.Button == MouseButtons.Right) //�Ҽ�
            {
                this.ThisPlayer.ShowLastTrickCards = !this.ThisPlayer.ShowLastTrickCards;
                if (this.ThisPlayer.ShowLastTrickCards)
                {
                    //������һ�ָ����������ƣ���С��һ�룬�������½ǣ������ػ���ǰ��������
                    ThisPlayer_PlayerLastTrickShowedCards();
                    //�鿴˭����ʲô��
                    drawingFormHelper.LastTrumpMadeCardsShow();
                    //�鿴�÷���
                    drawingFormHelper.DrawScoreCards();
                }
                else
                {
                    ThisPlayer_ShowEnding();
                }
            }
        }

        private void ExposeTrump(MouseEventArgs e)
        {
            List<Suit> availableTrumps = ThisPlayer.AvailableTrumps();
            var trumpExposingToolRegion = new Dictionary<Suit, Region>();
            var spadeRegion = new Region(new Rectangle(443 + drawingFormHelper.offsetCenter, 327 + drawingFormHelper.offsetY, 25, 25));
            trumpExposingToolRegion.Add(Suit.Spade, spadeRegion);
            var heartRegion = new Region(new Rectangle(417 + drawingFormHelper.offsetCenter, 327 + drawingFormHelper.offsetY, 25, 25));
            trumpExposingToolRegion.Add(Suit.Heart, heartRegion);
            var clubRegion = new Region(new Rectangle(493 + drawingFormHelper.offsetCenter, 327 + drawingFormHelper.offsetY, 25, 25));
            trumpExposingToolRegion.Add(Suit.Club, clubRegion);
            var diamondRegion = new Region(new Rectangle(468 + drawingFormHelper.offsetCenter, 327 + drawingFormHelper.offsetY, 25, 25));
            trumpExposingToolRegion.Add(Suit.Diamond, diamondRegion);
            var jokerRegion = new Region(new Rectangle(518 + drawingFormHelper.offsetCenter, 327 + drawingFormHelper.offsetY, 25, 25));
            trumpExposingToolRegion.Add(Suit.Joker, jokerRegion);
            foreach (var keyValuePair in trumpExposingToolRegion)
            {
                if (keyValuePair.Value.IsVisible(e.X, e.Y))
                {
                    foreach (Suit trump in availableTrumps)
                    {
                        if (trump == keyValuePair.Key)
                        {
                            var next =
                                (TrumpExposingPoker)
                                    (Convert.ToInt32(ThisPlayer.CurrentHandState.TrumpExposingPoker) + 1);
                            if (trump == Suit.Joker)
                            {
                                if (ThisPlayer.CurrentPoker.RedJoker == 2)
                                    next = TrumpExposingPoker.PairRedJoker;
                                else if (ThisPlayer.CurrentPoker.BlackJoker == 2)
                                    next = TrumpExposingPoker.PairBlackJoker;
                            }
                            ThisPlayer.ExposeTrump(next, trump);
                        }
                    }
                }
            }
        }

        private void MainForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        private void ToDiscard8Cards()
        {
            //�ж��Ƿ��ڿ��ƽ׶�
            if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DiscardingLast8Cards &&
                ThisPlayer.CurrentHandState.Last8Holder == ThisPlayer.PlayerId) //������ҿ���
            {
                if (SelectedCards.Count == 8)
                {
                    //����,���Բ�ȥС��
                    this.btnPig.Visible = false;

                    foreach (int card in SelectedCards)
                    {
                        ThisPlayer.CurrentPoker.RemoveCard(card);
                    }

                    ThisPlayer.DiscardCards(SelectedCards.ToArray());

                    ResortMyCards();
                }
            }
        }

        private void ToShowCards()
        {
            if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing &&
                ThisPlayer.CurrentTrickState.NextPlayer() == ThisPlayer.PlayerId)
            {
                ShowingCardsValidationResult showingCardsValidationResult =
                    TractorRules.IsValid(ThisPlayer.CurrentTrickState, SelectedCards, ThisPlayer.CurrentPoker);
                //�����׼�������ƺϷ�
                if (showingCardsValidationResult.ResultType == ShowingCardsValidationResultType.Valid)
                {
                    //��ȥС��
                    this.btnPig.Visible = false;

                    foreach (int card in SelectedCards)
                    {
                        ThisPlayer.CurrentPoker.RemoveCard(card);
                    }
                    ThisPlayer.ShowCards(SelectedCards);
                    drawingFormHelper.DrawMyHandCards();
                    SelectedCards.Clear();
                }
                else if (showingCardsValidationResult.ResultType == ShowingCardsValidationResultType.TryToDump)
                {
                    //��ȥС��
                    this.btnPig.Visible = false;

                    ShowingCardsValidationResult result = ThisPlayer.ValidateDumpingCards(SelectedCards);
                    if (result.ResultType == ShowingCardsValidationResultType.DumpingSuccess) //˦�Ƴɹ�.
                    {
                        foreach (int card in SelectedCards)
                        {
                            ThisPlayer.CurrentPoker.RemoveCard(card);
                        }
                        ThisPlayer.ShowCards(SelectedCards);

                        drawingFormHelper.DrawMyHandCards();
                        SelectedCards.Clear();
                    }
					//˦��ʧ��
                    else
                    {
                        this.drawingFormHelper.DrawMessages(new string[] { string.Format("˦��{0}��ʧ��", SelectedCards.Count), string.Format("���֣�{0}", SelectedCards.Count * 10) });
                        Thread.Sleep(5000);
                        foreach (int card in result.MustShowCardsForDumpingFail)
                        {
                            ThisPlayer.CurrentPoker.RemoveCard(card);
                        }
                        ThisPlayer.ShowCards(result.MustShowCardsForDumpingFail);

                        drawingFormHelper.DrawMyHandCards();
                        SelectedCards = result.MustShowCardsForDumpingFail;
                        SelectedCards.Clear();
                    }
                }
            }
        }

        //���ڻ滭����,��������ͼ�񻭵�������
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //��bmp����������
            g.DrawImage(bmp, 0, 0);
        }

        #endregion

        #region �˵��¼�����

        internal void MenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem) sender;
            if (menuItem.Text.Equals("�˳�"))
            {
                Close();
            }
            else if (menuItem.Name.StartsWith("toolStripMenuItemBeginRank") && !ThisPlayer.isObserver)
            {
                string beginRankString = menuItem.Name.Substring("toolStripMenuItemBeginRank".Length, 1);
                ThisPlayer.SetBeginRank(beginRankString);
            }
        }

        //�����¼�����
        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            Activate();
        }
    

        #endregion // �˵��¼�����

        #region handle player event

        private void StartGame()
        {
            init();
        }

        private void PlayerGetCard(int cardNumber)
        {
            drawingFormHelper.IGetCard(cardNumber);

            //�йܴ�������
            if (gameConfig.IsDebug && !ThisPlayer.isObserver)
            {
                var availableTrump = ThisPlayer.AvailableTrumps();
                var fullDebug = FormSettings.GetSettingBool(FormSettings.KeyFullDebug);
                Suit trumpToExpose = Algorithm.TryExposingTrump(availableTrump, this.ThisPlayer.CurrentPoker, fullDebug);
                if (trumpToExpose == Suit.None) return;

                var next =
                    (TrumpExposingPoker)
                        (Convert.ToInt32(ThisPlayer.CurrentHandState.TrumpExposingPoker) + 1);
                if (trumpToExpose == Suit.Joker)
                {
                    if (ThisPlayer.CurrentPoker.BlackJoker == 2)
                        next = TrumpExposingPoker.PairBlackJoker;
                    else if (ThisPlayer.CurrentPoker.RedJoker == 2)
                        next = TrumpExposingPoker.PairRedJoker;
                }
                ThisPlayer.ExposeTrump(next, trumpToExpose);
            }
        }


        private void ThisPlayer_TrumpUpdated(CurrentHandState currentHandState)
        {
            ThisPlayer.CurrentHandState = currentHandState;
            drawingFormHelper.Trump();
            drawingFormHelper.TrumpMadeCardsShow();
            drawingFormHelper.ReDrawToolbar();
            if (ThisPlayer.CurrentHandState.IsFirstHand)
            {
                drawingFormHelper.Rank();
                drawingFormHelper.Starter();
            }
        }

        private void ResortMyCards()
        {
            drawingFormHelper.DrawMySortedCards(ThisPlayer.CurrentPoker, ThisPlayer.CurrentPoker.Count);
        }

        //��鵱ǰ�����Ƿ�Ϊ������
        private bool IsWinningWithTrump(CurrentTrickState trickState, string winnerID)
        {
            if (trickState.Learder != winnerID)
            {
                bool isLeaderTrump = PokerHelper.IsTrump(trickState.LeadingCards[0], ThisPlayer.CurrentHandState.Trump, ThisPlayer.CurrentHandState.Rank);
                bool isWinnerTrump = PokerHelper.IsTrump(trickState.ShowedCards[winnerID][0], ThisPlayer.CurrentHandState.Trump, ThisPlayer.CurrentHandState.Rank);
                if (!isLeaderTrump && isWinnerTrump) return true;
            }
            return false;
        }

        private void ThisPlayer_PlayerShowedCards()
        {
            //����µ�һ�ֿ�ʼ�����û�����Ϣ
            if (ThisPlayer.CurrentTrickState.CountOfPlayerShowedCards() == 1)
            {
                this.ThisPlayer.playerLocalCache = new PlayerLocalCache();
            }

            string latestPlayer = ThisPlayer.CurrentTrickState.LatestPlayerShowedCard();
            this.ThisPlayer.playerLocalCache.ShowedCardsInCurrentTrick = ThisPlayer.CurrentTrickState.ShowedCards.ToDictionary(entry => entry.Key, entry => entry.Value.ToList());
            
            int position = PlayerPosition[latestPlayer];
            
            //������ڻؿ����ֳ��ƣ����ػ��ոճ�����
            if (!this.ThisPlayer.ShowLastTrickCards)
            {
                //������һ��
                if (ThisPlayer.CurrentTrickState.CountOfPlayerShowedCards() == 1)
                {
                    drawingFormHelper.DrawCenterImage();
                    drawingFormHelper.DrawScoreImage();
                }

                if (position == 1)
                {
                    drawingFormHelper.DrawMyShowedCards();
                }
                else if (position == 2)
                {
                    drawingFormHelper.DrawNextUserSendedCards();
                }
                else if (position == 3)
                {
                    drawingFormHelper.DrawFriendUserSendedCards();
                }
                else if (position == 4)
                {
                    drawingFormHelper.DrawPreviousUserSendedCards();
                }
            }

            //������ڻؿ������Լ��ոճ����ƣ������ûؿ������»���
            if (this.ThisPlayer.ShowLastTrickCards && latestPlayer == ThisPlayer.PlayerId)
            {
                this.ThisPlayer.ShowLastTrickCards = false;
                ThisPlayer_PlayerCurrentTrickShowedCards(false);
            }

            if (!gameConfig.IsDebug && ThisPlayer.CurrentTrickState.NextPlayer() == ThisPlayer.PlayerId)
                drawingFormHelper.DrawMyPlayingCards(ThisPlayer.CurrentPoker);

            //��ʱ�����Թ�����
            if (ThisPlayer.isObserver && ThisPlayer.PlayerId == latestPlayer)
            {
                ThisPlayer.CurrentPoker = ThisPlayer.CurrentHandState.PlayerHoldingCards[ThisPlayer.PlayerId];
                ResortMyCards();
            }

            RobotPlayFollowing();
        }

        //������һ�ָ����������ƣ���С��һ�룬�������½�
        private void ThisPlayer_PlayerLastTrickShowedCards()
        {
            //������һ��
            drawingFormHelper.DrawCenterImage();
            drawingFormHelper.DrawScoreImage();

            this.drawingFormHelper.DrawMessages(new string[] { "�ؿ����ֳ���..." });

            string lastLeader = ThisPlayer.CurrentTrickState.serverLocalCache.lastLeader;
            if (string.IsNullOrEmpty(lastLeader) ||
                ThisPlayer.CurrentTrickState.serverLocalCache.lastShowedCards.Count == 0) return;

            CurrentTrickState trickState = new CurrentTrickState();
            trickState.Learder = lastLeader;
            trickState.Trump = ThisPlayer.CurrentTrickState.Trump;
            trickState.Rank = ThisPlayer.CurrentTrickState.Rank;

            foreach (var entry in ThisPlayer.CurrentTrickState.serverLocalCache.lastShowedCards)
            {
                trickState.ShowedCards.Add((string)entry.Key, (List<int>)entry.Value);
            }

            foreach (var entry in trickState.ShowedCards)
            {
                int position = PlayerPosition[entry.Key];
                if (position == 1)
                {
                    drawingFormHelper.DrawMyLastSendedCardsAction(new ArrayList(entry.Value));
                }
                else if (position == 2)
                {
                    drawingFormHelper.DrawNextUserLastSendedCardsAction(new ArrayList(entry.Value));
                }
                else if (position == 3)
                {
                    drawingFormHelper.DrawFriendUserLastSendedCardsAction(new ArrayList(entry.Value));
                }
                else if (position == 4)
                {
                    drawingFormHelper.DrawPreviousUserLastSendedCardsAction(new ArrayList(entry.Value));
                }
            }
            string winnerID = TractorRules.GetWinner(trickState);
            bool tempIsWinByTrump = this.IsWinningWithTrump(trickState, winnerID);
            drawingFormHelper.DrawOverridingFlag(this.PlayerPosition[winnerID], tempIsWinByTrump, 2);
            Refresh();
        }

        //���Ƶ�ǰ�ָ����������ƣ��������л��ӽǻ�ǰ�غϴ��Ʊ��ʱ��
        private void ThisPlayer_PlayerCurrentTrickShowedCards(bool fromRightClick)
        {
            //����������
            drawingFormHelper.DrawCenterImage();
            drawingFormHelper.DrawScoreImage();

            if (this.ThisPlayer.playerLocalCache.ShowedCardsInCurrentTrick != null)
            {
                foreach (var entry in this.ThisPlayer.playerLocalCache.ShowedCardsInCurrentTrick)
                {
                    string player = entry.Key;
                    int position = PlayerPosition[player];
                    if (position == 1)
                    {
                        drawingFormHelper.DrawMySendedCardsAction(new ArrayList(entry.Value));
                    }
                    else if (position == 2)
                    {
                        drawingFormHelper.DrawNextUserSendedCardsAction(new ArrayList(entry.Value));
                    }
                    else if (position == 3)
                    {
                        drawingFormHelper.DrawFriendUserSendedCardsAction(new ArrayList(entry.Value));
                    }
                    else if (position == 4)
                    {
                        drawingFormHelper.DrawPreviousUserSendedCardsAction(new ArrayList(entry.Value));
                    }
                }
            }
            if (fromRightClick)
            {
                //�ػ���������
                if (this.ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DiscardingLast8Cards)
                {
                    drawingFormHelper.TrumpMadeCardsShow();
                }

                //�ػ����Ʊ�ǣ�winner����
                if (!this.ThisPlayer.CurrentTrickState.IsStarted() && !string.IsNullOrEmpty(this.ThisPlayer.playerLocalCache.WinnderID))
                {
                    drawingFormHelper.DrawOverridingFlag(this.ThisPlayer.playerLocalCache.WinnerPosition, this.ThisPlayer.playerLocalCache.IsWinByTrump, 1);
                    drawingFormHelper.DrawWhoWinThisTime(this.ThisPlayer.playerLocalCache.WinnderID);
                }
            }
            Refresh();
        }

        //���Ƶ�ǰ�������棨�������л��ӽ�ʱ��
        private void ThisPlayer_ShowEnding()
        {
            //����������
            drawingFormHelper.DrawCenterImage();
            drawingFormHelper.DrawScoreImage();

            ThisPlayer_HandEnding();
        }

        private void ThisPlayer_ShowingCardBegan()
        {
            ThisPlayer_DiscardingLast8();
            drawingFormHelper.RemoveToolbar();
            drawingFormHelper.DrawCenterImage();
            drawingFormHelper.DrawScoreImage();

            //���ƿ�ʼǰ��ȥ������Ҫ��controls
            this.btnSurrender.Visible = false;
            this.btnRiot.Visible = false;

            Refresh();
        }

        private void ThisPlayer_PlayersTeamMade()
        {
            //set player position
            PlayerPosition.Clear();
            PositionPlayer.Clear();
            string nextPlayer = ThisPlayer.PlayerId;
            int postion = 1;
            PlayerPosition.Add(nextPlayer, postion);
            PositionPlayer.Add(postion, nextPlayer);
            nextPlayer = ThisPlayer.CurrentGameState.GetNextPlayerAfterThePlayer(nextPlayer).PlayerId;
            while (nextPlayer != ThisPlayer.PlayerId)
            {
                postion++;
                PlayerPosition.Add(nextPlayer, postion);
                PositionPlayer.Add(postion, nextPlayer);
                nextPlayer = ThisPlayer.CurrentGameState.GetNextPlayerAfterThePlayer(nextPlayer).PlayerId;
            }
        }

        private void ThisPlayer_NewPlayerJoined()
        {
            if (this.ToolStripMenuItemEnterRoom0.Enabled)
            {
                this.ToolStripMenuItemEnterRoom0.Enabled = false;
                HideRoomControls();
                init();
            }
            if (!ThisPlayer.isObserver)
            {
                this.btnReady.Show();
                this.btnRobot.Show();
                this.ToolStripMenuItemInRoom.Visible = true;
            }
            else
            {
                this.btnObserveNext.Show();
                this.ToolStripMenuItemObserve.Visible = true;
            }
            this.btnExitRoom.Show();
            this.btnRoomSetting.Show();

            int curIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                var curPlayer = ThisPlayer.CurrentGameState.Players[i];
                if (curPlayer != null && curPlayer.PlayerId == ThisPlayer.PlayerId)
                {
                    curIndex = i;
                    break;
                }
            }
            System.Windows.Forms.Label[] nickNameLabels = new System.Windows.Forms.Label[] { this.lblSouthNickName, this.lblEastNickName, this.lblNorthNickName, this.lblWestNickName };
            for (int i = 0; i < 4; i++)
            {
                var curPlayer = ThisPlayer.CurrentGameState.Players[curIndex];
                if (curPlayer != null)
                {
                    nickNameLabels[i].Text = curPlayer.PlayerId;
                    foreach (string ob in curPlayer.Observers)
                    {
                        string newLine = i == 0 ? "" : "\n";
                        nickNameLabels[i].Text += string.Format("{0}��{1}��", newLine, ob);
                    }
                }
                else
                {
                    nickNameLabels[i].Text = "";
                }
                curIndex = (curIndex + 1) % 4;
            }

            bool isHelpSeen = FormSettings.GetSettingBool(FormSettings.KeyIsHelpSeen);
            if (!isHelpSeen)
            {
                this.ToolStripMenuItemUserManual.PerformClick();
            }
        }

        private void DisplayRoomSetting(string prefix)
        {
            List<string> msgs = new List<string>();
            if (!string.IsNullOrEmpty(prefix))
            {
                msgs.Add(prefix);
                msgs.Add(string.Empty);
            }
            msgs.Add(string.Format("����Ͷ����{0}", this.ThisPlayer.CurrentRoomSetting.AllowSurrender ? "��" : "��"));
            msgs.Add(string.Format("����J���ף�{0}", this.ThisPlayer.CurrentRoomSetting.AllowJToBottom ? "��" : "��"));
            msgs.Add(this.ThisPlayer.CurrentRoomSetting.AllowRiotWithTooFewScoreCards >= 0 ? string.Format("�������С�ڵ���{0}ʱ����", this.ThisPlayer.CurrentRoomSetting.AllowRiotWithTooFewScoreCards) : "�������������");
            msgs.Add(this.ThisPlayer.CurrentRoomSetting.AllowRiotWithTooFewTrumpCards >= 0 ? string.Format("��������С�ڵ���{0}��ʱ����", this.ThisPlayer.CurrentRoomSetting.AllowRiotWithTooFewTrumpCards) : "���������Ƹ���");

            List<int> mandRanks = this.ThisPlayer.CurrentRoomSetting.GetManditoryRanks();
            string[] mandRanksStr = mandRanks.Select(x => CommonMethods.cardNumToValue[x].ToString()).ToArray();
            msgs.Add(mandRanks.Count > 0 ? string.Format("�ش�{0}", string.Join(",", mandRanksStr)) : "û�бش���");

            ThisPlayer_NotifyMessageEventHandler(msgs.ToArray());
        }

        private void ThisPlayer_NewPlayerReadyToStart(bool readyToStart)
        {
            this.btnReady.Enabled = !readyToStart;
            
            //����˭�������
            System.Windows.Forms.Label[] readyLabels = new System.Windows.Forms.Label[] { this.lblSouthStarter, this.lblEastStarter, this.lblNorthStarter, this.lblWestStarter };
            int curIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                var curPlayer = ThisPlayer.CurrentGameState.Players[i];
                if (curPlayer != null && curPlayer.PlayerId == ThisPlayer.PlayerId)
                {
                    curIndex = i;
                    break;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                var curPlayer = ThisPlayer.CurrentGameState.Players[curIndex];
                if (curPlayer != null && curPlayer.IsRobot)
                {
                    readyLabels[i].Text = "�й���";
                }
                else if (curPlayer != null && !curPlayer.IsReadyToStart)
                {
                    readyLabels[i].Text = "˼����";
                }
                else if (curPlayer != null && !string.IsNullOrEmpty(ThisPlayer.CurrentHandState.Starter) && curPlayer.PlayerId == ThisPlayer.CurrentHandState.Starter)
                {
                    readyLabels[i].Text = "ׯ��";
                }
                else
                {
                    readyLabels[i].Text = (curIndex + 1).ToString();
                }
                curIndex = (curIndex + 1) % 4;
            }
        }

        private void ThisPlayer_PlayerToggleIsRobot(bool isRobot)
        {
            bool shouldTrigger = isRobot && isRobot != gameConfig.IsDebug;
            this.ToolStripMenuItemRobot.Checked = isRobot;
            gameConfig.IsDebug = isRobot;
            this.btnRobot.Text = isRobot ? "ȡ��" : "�й�";

            //����˭���й���
            System.Windows.Forms.Label[] readyLabels = new System.Windows.Forms.Label[] { this.lblSouthStarter, this.lblEastStarter, this.lblNorthStarter, this.lblWestStarter };
            int curIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                var curPlayer = ThisPlayer.CurrentGameState.Players[i];
                if (curPlayer != null && curPlayer.PlayerId == ThisPlayer.PlayerId)
                {
                    curIndex = i;
                    break;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                var curPlayer = ThisPlayer.CurrentGameState.Players[curIndex];
                if (curPlayer != null && curPlayer.IsRobot)
                {
                    readyLabels[i].Text = "�й���";
                }
                else if (curPlayer != null && !curPlayer.IsReadyToStart)
                {
                    readyLabels[i].Text = "˼����";
                }
                else if (curPlayer != null && !string.IsNullOrEmpty(ThisPlayer.CurrentHandState.Starter) && curPlayer.PlayerId == ThisPlayer.CurrentHandState.Starter)
                {
                    readyLabels[i].Text = "ׯ��";
                }
                else
                {
                    readyLabels[i].Text = (curIndex + 1).ToString();
                }
                curIndex = (curIndex + 1) % 4;
            }

            if (shouldTrigger)
            {
                if (!ThisPlayer.CurrentTrickState.IsStarted()) this.RobotPlayStarting();
                else this.RobotPlayFollowing();
            }
        }

        private void ThisPlayer_RoomSettingUpdatedEventHandler(RoomSetting roomSetting, bool isRoomSettingModified)
        {
            this.ThisPlayer.CurrentRoomSetting = roomSetting;
            this.lblRoomName.Text = this.ThisPlayer.CurrentRoomSetting.RoomName;
            string prefix = string.Empty;
            if (isRoomSettingModified)
            {
                prefix = "���������Ѹ��ģ�";
            }
            this.DisplayRoomSetting(prefix);
        }

        private void ThisPlayer_ShowAllHandCardsEventHandler()
        {
            //����������
            drawingFormHelper.DrawCenterImage();
            drawingFormHelper.DrawScoreImage();

            foreach (var entry in this.ThisPlayer.CurrentHandState.PlayerHoldingCards)
            {
                string player = entry.Key;
                int position = PlayerPosition[player];
                if (position == 1)
                {
                    continue;
                }
                else if (position == 2)
                {
                    drawingFormHelper.DrawNextUserSendedCardsActionAllHandCards(new ArrayList(entry.Value.GetCardsInList()));
                }
                else if (position == 3)
                {
                    drawingFormHelper.DrawFriendUserSendedCardsActionAllHandCards(new ArrayList(entry.Value.GetCardsInList()));
                }
                else if (position == 4)
                {
                    drawingFormHelper.DrawPreviousUserSendedCardsActionAllHandCards(new ArrayList(entry.Value.GetCardsInList()));
                }
            }
            Refresh();

        }
        
        private void ThisPlayer_GameHallUpdatedEventHandler(List<RoomState> roomStates, List<string> names)
        {
            this.ToolStripMenuItemEnterHall.Enabled = false;
            this.btnEnterHall.Hide();
            ClearRoom();
            HideRoomControls();

            CreateRoomControls(roomStates, names);
            this.ToolStripMenuItemEnterRoom0.Enabled = true;
        }

        private void ClearRoom()
        {
            ThisPlayer.PlayerId = ThisPlayer.MyOwnId;
            ThisPlayer.isObserver = false;
            this.btnReady.Hide();
            this.btnRobot.Hide();
            this.btnExitRoom.Hide();
            this.btnRoomSetting.Hide();
            this.lblRoomName.Text = "";
            this.btnObserveNext.Hide();
            this.lblEastNickName.Text = "";
            this.lblNorthNickName.Text = "";
            this.lblWestNickName.Text = "";
            this.lblSouthNickName.Text = ThisPlayer.MyOwnId;
            this.lblEastStarter.Text = "";
            this.lblNorthStarter.Text = "";
            this.lblWestStarter.Text = "";
            this.lblSouthStarter.Text = "";
            this.ToolStripMenuItemInRoom.Visible = false;
            this.ToolStripMenuItemObserve.Visible = false;

            Graphics g = Graphics.FromImage(bmp);
            drawingFormHelper.DrawBackground(g);
            Refresh();
            g.Dispose();
        }

        private void CreateRoomControls(List<RoomState> roomStates, List<string> names)
        {
            int offsetX = 50;
            int offsetY = 150;
            int offsetYLower = offsetY + 100;

            Label labelOnline = new Label();
            labelOnline.AutoSize = true;
            labelOnline.BackColor = System.Drawing.Color.Transparent;
            labelOnline.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            labelOnline.ForeColor = System.Drawing.SystemColors.Control;
            labelOnline.Location = new System.Drawing.Point(offsetX, offsetY);
            labelOnline.Name = string.Format("{0}_Online", roomControlPrefix);
            labelOnline.Size = new System.Drawing.Size(0, 37);
            labelOnline.Text = "����";
            this.Controls.Add(labelOnline);

            Label labelNames = new Label();
            labelNames.AutoSize = true;
            labelNames.BackColor = System.Drawing.Color.Transparent;
            labelNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            labelNames.ForeColor = System.Drawing.SystemColors.Control;
            labelNames.Location = new System.Drawing.Point(offsetX, offsetYLower);
            labelNames.Name = string.Format("{0}_Names", roomControlPrefix);
            labelNames.Size = new System.Drawing.Size(0, 37);
            for (int i = 0; i < names.Count; i++)
            {
                if (i > 0) labelNames.Text += "\n";
                labelNames.Text += names[i];
            }
            this.Controls.Add(labelNames);

            offsetX += 200;
            int seatSize = 40;
            int tableSize = seatSize * 2;

            for (int roomInd = 0; roomInd < roomStates.Count; roomInd++)
            {
                int roomOffsetX = offsetX + seatSize * 8 * (roomInd % 2);
                int roomOffsetY = offsetY + seatSize * 8 * (roomInd / 2);

                RoomState room = roomStates[roomInd];
                Button btnEnterRoom = new Button();
                btnEnterRoom.Location = new System.Drawing.Point(roomOffsetX + seatSize * 5 / 4, roomOffsetY + seatSize * 5 / 4);

                btnEnterRoom.Name = string.Format("{0}_btnEnterRoom_{1}", roomControlPrefix, room.RoomID);
                btnEnterRoom.Size = new System.Drawing.Size(tableSize, tableSize);
                btnEnterRoom.BackColor = System.Drawing.Color.LightBlue;
                btnEnterRoom.Text = room.roomSetting.RoomName;
                btnEnterRoom.Click += new System.EventHandler(this.btnEnterRoom_Click);
                this.Controls.Add(btnEnterRoom);

                List<PlayerEntity> players = room.CurrentGameState.Players;
                for (int j = 0; j < players.Count; j++)
                {
                    int offsetXSeat = roomOffsetX;
                    int offsetYSeat = roomOffsetY;
                    switch (j)
                    {
                        case 0:
                            offsetXSeat += seatSize * 7 / 4;
                            break;
                        case 1:
                            offsetYSeat += seatSize * 7 / 4;
                            break;
                        case 2:
                            offsetXSeat += seatSize * 7 / 4;
                            offsetYSeat += seatSize * 7 / 2;
                            break;
                        case 3:
                            offsetXSeat += seatSize * 7 / 2;
                            offsetYSeat += seatSize * 7 / 4;
                            break;
                        default:
                            break;
                    }
                    if (players[j] == null)
                    {
                        Button btnEnterRoomByPos = new Button();
                        btnEnterRoomByPos.Location = new System.Drawing.Point(offsetXSeat, offsetYSeat);

                        btnEnterRoomByPos.Name = string.Format("{0}_btnEnterRoom_{1}_{2}", roomControlPrefix, room.RoomID, j);
                        btnEnterRoomByPos.Size = new System.Drawing.Size(seatSize, seatSize);
                        btnEnterRoomByPos.BackColor = System.Drawing.Color.LightPink;
                        btnEnterRoomByPos.Text = (j + 1).ToString();
                        btnEnterRoomByPos.Click += new System.EventHandler(this.btnEnterRoom_Click);
                        this.Controls.Add(btnEnterRoomByPos);
                    }
                    else
                    {
                        int labelOffsetX = 0;
                        int labelOffsetY = 0;
                        switch (j)
                        {
                            case 0:
                                labelOffsetY = seatSize / 4;
                                break;
                            case 1:
                                labelOffsetX = seatSize / 4;
                                break;
                            case 2:
                                labelOffsetY = -seatSize / 4;
                                break;
                            case 3:
                                labelOffsetX = -seatSize / 4;
                                break;
                            default:
                                break;
                        }
                        Label labelRoomByPos = new Label();
                        labelRoomByPos.AutoSize = true;
                        labelRoomByPos.BackColor = System.Drawing.Color.Transparent;
                        labelRoomByPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                        labelRoomByPos.ForeColor = System.Drawing.SystemColors.Control;
                        labelRoomByPos.Location = new System.Drawing.Point(offsetXSeat + labelOffsetX, offsetYSeat + labelOffsetY);
                        labelRoomByPos.Name = string.Format("{0}_lblRoom_{1}_{2}", roomControlPrefix, room.RoomID, j);
                        labelRoomByPos.Size = new System.Drawing.Size(seatSize, seatSize);

                        if (j % 2 == 0)
                        {
                            labelRoomByPos.AutoSize = false;
                            labelRoomByPos.Size = new System.Drawing.Size(seatSize * 9 / 2, seatSize);
                            labelRoomByPos.Location = new System.Drawing.Point(offsetXSeat - seatSize * 7 / 4 + labelOffsetX, offsetYSeat + labelOffsetY);
                            labelRoomByPos.TextAlign = ContentAlignment.MiddleCenter;
                        }

                        if (j == 1)
                        {
                            labelRoomByPos.Location = new System.Drawing.Point(0, 0);
                            labelRoomByPos.Size = new System.Drawing.Size(0, seatSize);

                            labelRoomByPos.Dock = System.Windows.Forms.DockStyle.Right;
                            labelRoomByPos.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

                            TableLayoutPanel tlpRoomByPos = new TableLayoutPanel();
                            tlpRoomByPos.SuspendLayout();

                            tlpRoomByPos.Anchor = System.Windows.Forms.AnchorStyles.Right;
                            tlpRoomByPos.Size = new System.Drawing.Size(0, seatSize);
                            tlpRoomByPos.AutoSize = true;
                            tlpRoomByPos.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
                            tlpRoomByPos.BackColor = System.Drawing.Color.Transparent;
                            tlpRoomByPos.ColumnCount = 1;
                            tlpRoomByPos.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
                            tlpRoomByPos.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                            tlpRoomByPos.Controls.Add(labelRoomByPos, 0, 0);
                            tlpRoomByPos.Location = new System.Drawing.Point(offsetXSeat + seatSize + labelOffsetX, offsetYSeat + labelOffsetY);
                            tlpRoomByPos.Name = string.Format("{0}_lblRoom_{1}_{2}_tlp", roomControlPrefix, room.RoomID, j);
                            tlpRoomByPos.RowCount = 1;
                            tlpRoomByPos.RowStyles.Add(new System.Windows.Forms.RowStyle());
                            tlpRoomByPos.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));

                            this.Controls.Add(tlpRoomByPos);

                            tlpRoomByPos.ResumeLayout(false);
                            tlpRoomByPos.PerformLayout();
                        }
                        else
                        {
                            this.Controls.Add(labelRoomByPos);
                        }

                        labelRoomByPos.Text += players[j].PlayerId;
                    }
                }
            }
        }

        private void HideRoomControls()
        {
            List<Control> roomCtrls = new List<Control>();
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.Name.StartsWith(this.roomControlPrefix))
                {
                    roomCtrls.Add(ctrl);
                }
            }
            foreach (Control ctrl in roomCtrls)
            {
                this.Controls.Remove(ctrl);
            }
        }

        private void Mainform_SettingsUpdatedEventHandler()
        {
            Application.Restart();
        }

        private void Mainform_RoomSettingChangedByClientEventHandler()
        {
            this.ThisPlayer.SaveRoomSetting(this.ThisPlayer.CurrentRoomSetting);
        }

        private void ThisPlayer_TrickStarted()
        {
            if (!gameConfig.IsDebug && ThisPlayer.CurrentTrickState.Learder == ThisPlayer.PlayerId)
            {
                drawingFormHelper.DrawMyPlayingCards(ThisPlayer.CurrentPoker);
            }
            RobotPlayStarting();
        }

        //�йܴ���
        private void RobotPlayFollowing()
        {
            //���һ���Զ�����
            bool isLastTrick = false;
            if (ThisPlayer.CurrentTrickState.LeadingCards.Count == this.ThisPlayer.CurrentPoker.Count)
            {
                isLastTrick = true;
            }

            //��������
            if ((isLastTrick || gameConfig.IsDebug) && !ThisPlayer.isObserver &&
                ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing &&
                ThisPlayer.CurrentTrickState.NextPlayer() == ThisPlayer.PlayerId &&
                ThisPlayer.CurrentTrickState.IsStarted())
            {
                SelectedCards.Clear();
                Algorithm.MustSelectedCards(this.SelectedCards, this.ThisPlayer.CurrentTrickState, this.ThisPlayer.CurrentPoker);
                ShowingCardsValidationResult showingCardsValidationResult =
                    TractorRules.IsValid(ThisPlayer.CurrentTrickState, SelectedCards, ThisPlayer.CurrentPoker);
                if (showingCardsValidationResult.ResultType == ShowingCardsValidationResultType.Valid)
                {
                    foreach (int card in SelectedCards)
                    {
                        ThisPlayer.CurrentPoker.RemoveCard(card);
                    }
                    ThisPlayer.ShowCards(SelectedCards);
                    drawingFormHelper.DrawMyHandCards();
                }
                else
                {
                    MessageBox.Show(string.Format("failed to auto select cards: {0}, please manually select", SelectedCards));
                }
                SelectedCards.Clear();
            }
        }

        //�йܴ�������
        private void RobotPlayStarting()
        {
            if (gameConfig.IsDebug && !ThisPlayer.isObserver &&
                (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing || ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DiscardingLast8CardsFinished))
            {
                if (string.IsNullOrEmpty(ThisPlayer.CurrentTrickState.Learder)) return;
                if (ThisPlayer.CurrentTrickState.NextPlayer() != ThisPlayer.PlayerId) return;
                if (ThisPlayer.CurrentTrickState.IsStarted()) return;

                SelectedCards.Clear();
                Algorithm.ShouldSelectedCards(this.SelectedCards, this.ThisPlayer.CurrentTrickState, this.ThisPlayer.CurrentPoker);
                ShowingCardsValidationResult showingCardsValidationResult =
                    TractorRules.IsValid(ThisPlayer.CurrentTrickState, SelectedCards, ThisPlayer.CurrentPoker);
                if (showingCardsValidationResult.ResultType == ShowingCardsValidationResultType.Valid)
                {
                    foreach (int card in SelectedCards)
                    {
                        ThisPlayer.CurrentPoker.RemoveCard(card);
                    }
                    ThisPlayer.ShowCards(SelectedCards);
                    drawingFormHelper.DrawMyHandCards();
                }
                else
                {
                    MessageBox.Show(string.Format("failed to auto select cards: {0}, please manually select", SelectedCards));
                }
                SelectedCards.Clear();
            }
        }

        private void ThisPlayer_TrickFinished()
        {
            drawingFormHelper.DrawScoreImage();

            //�ռ����Ʊ�������Ϣ
            string winnerID = TractorRules.GetWinner(ThisPlayer.CurrentTrickState);
            bool winResult = this.IsWinningWithTrump(ThisPlayer.CurrentTrickState, winnerID);
            int position = PlayerPosition[ThisPlayer.CurrentTrickState.Winner];
            this.ThisPlayer.playerLocalCache.IsWinByTrump = winResult;
            this.ThisPlayer.playerLocalCache.WinnerPosition = position;
            this.ThisPlayer.playerLocalCache.WinnderID = ThisPlayer.CurrentTrickState.Winner;
            drawingFormHelper.DrawOverridingFlag(this.ThisPlayer.playerLocalCache.WinnerPosition, this.ThisPlayer.playerLocalCache.IsWinByTrump, 1);

            drawingFormHelper.DrawWhoWinThisTime(this.ThisPlayer.CurrentTrickState.Winner);
            Refresh();
        }

        private void ThisPlayer_HandEnding()
        {
            drawingFormHelper.DrawFinishedSendedCards();
        }

        private void ThisPlayer_SpecialEndingEventHandler()
        {
            this.btnSurrender.Visible = false;
            this.btnRiot.Visible = false;
            drawingFormHelper.DrawFinishedBySpecialEnding();
        }

        private void ThisPlayer_StarterFailedForTrump()
        {
            Graphics g = Graphics.FromImage(bmp);

            //����Sidebar
            drawingFormHelper.DrawSidebar(g);
            //���ƶ�������

            drawingFormHelper.Starter();

            //����Rank
            drawingFormHelper.Rank();

            //���ƻ�ɫ
            drawingFormHelper.Trump();

            ResortMyCards();

            drawingFormHelper.ReDrawToolbar();

            Refresh();
            g.Dispose();
        }

        private void ThisPlayer_StarterChangedEventHandler()
        {
            System.Windows.Forms.Label[] starterLabels = new System.Windows.Forms.Label[] { this.lblSouthStarter, this.lblEastStarter, this.lblNorthStarter, this.lblWestStarter };
            int curIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                var curPlayer = ThisPlayer.CurrentGameState.Players[i];
                if (curPlayer != null && curPlayer.PlayerId == ThisPlayer.PlayerId)
                {
                    curIndex = i;
                    break;
                }
            }
            //����˭��ׯ��
            for (int i = 0; i < 4; i++)
            {
                var curPlayer = ThisPlayer.CurrentGameState.Players[curIndex];
                if (curPlayer != null && curPlayer.IsRobot)
                {
                    starterLabels[i].Text = "�й���";
                }
                else if (curPlayer != null && !curPlayer.IsReadyToStart)
                {
                    starterLabels[i].Text = "˼����";
                }
                else if (curPlayer != null && !string.IsNullOrEmpty(ThisPlayer.CurrentHandState.Starter) &&
                    curPlayer.PlayerId == ThisPlayer.CurrentHandState.Starter &&
                    ThisPlayer.CurrentHandState.CurrentHandStep != HandStep.Ending)
                {
                    starterLabels[i].Text = "ׯ��";
                }
                else
                {
                    starterLabels[i].Text = (curIndex + 1).ToString();
                }
                curIndex = (curIndex + 1) % 4;
            }
        }

        private void ThisPlayer_NotifyMessageEventHandler(string[] msgs)
        {
            this.drawingFormHelper.DrawMessages(msgs);
        }

        private void ThisPlayer_NotifyStartTimerEventHandler(int timerLength)
        {
            this.timerCountDown = timerLength;
            this.drawingFormHelper.DrawCountDown(true);
            this.theTimer.Start();
        }

        private void ThisPlayer_NotifyCardsReadyEventHandler(ArrayList mcir)
        {
            for (int k = 0; k < myCardIsReady.Count; k++)
            {
                myCardIsReady[k] = mcir[k];
            }

            drawingFormHelper.DrawMyPlayingCards(ThisPlayer.CurrentPoker);
            Refresh();
        }

        private void ThisPlayer_ResortMyCardsEventHandler()
        {
            ThisPlayer.CurrentPoker = ThisPlayer.CurrentHandState.PlayerHoldingCards[ThisPlayer.PlayerId];
            ResortMyCards();
        }

        private void ThisPlayer_Last8Discarded()
        {
            Graphics g = Graphics.FromImage(bmp);
            for (int i = 0; i < 8; i++)
            {
                g.DrawImage(gameConfig.BackImage, 200 + drawingFormHelper.offsetCenterHalf + i * 2 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor, 186 + drawingFormHelper.offsetCenterHalf, 71 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor, 96 * drawingFormHelper.scaleDividend / drawingFormHelper.scaleDivisor);
            }

            if (ThisPlayer.isObserver && ThisPlayer.CurrentHandState.Last8Holder == ThisPlayer.PlayerId)
            {
                ThisPlayer.CurrentPoker = ThisPlayer.CurrentHandState.PlayerHoldingCards[ThisPlayer.PlayerId];
                ResortMyCards();
            }
            if (!ThisPlayer.isObserver && 
                ThisPlayer.CurrentPoker != null && ThisPlayer.CurrentPoker.Count > 0 &&
                ThisPlayer.CurrentHandState.Last8Holder == ThisPlayer.PlayerId &&
                ThisPlayer.CurrentHandState.DiscardedCards != null &&
                ThisPlayer.CurrentHandState.DiscardedCards.Length == 8)
            {
                drawingFormHelper.DrawDiscardedCards();
            }
            Refresh();
            g.Dispose();
        }

        private void ThisPlayer_DistributingLast8Cards()
        {
            int position = PlayerPosition[ThisPlayer.CurrentHandState.Last8Holder];
            //�Լ����ײ��û�
            if (position > 1)
            {
                drawingFormHelper.DrawDistributingLast8Cards(position);
            }

            if (ThisPlayer.isObserver)
            {
                return;
            }

            //���ƽ�������������й�״̬����ȡ���й�
            if (gameConfig.IsDebug && !FormSettings.GetSettingBool(FormSettings.KeyFullDebug))
            {
                this.btnRobot.PerformClick();
            }

            //���ƽ������������Ͷ��������ʾͶ����ť
            if (ThisPlayer.CurrentRoomSetting.AllowSurrender)
            {
                this.btnSurrender.Visible = true;
            }
            
            //���ƽ������������������������ж��Ƿ����ʾ������ť
            int riotScoreCap = ThisPlayer.CurrentRoomSetting.AllowRiotWithTooFewScoreCards;
            if (ThisPlayer.CurrentPoker.GetTotalScore() <= riotScoreCap)
            {
                this.btnRiot.Visible = true;
            }

            //���ƽ���������������Ƹ��������ж��Ƿ����ʾ������ť
            int riotTrumpCap = ThisPlayer.CurrentRoomSetting.AllowRiotWithTooFewTrumpCards;
            if (ThisPlayer.CurrentPoker.GetMasterCardsCount() <= riotTrumpCap)
            {
                this.btnRiot.Visible = true;
            }
        }

        private void ThisPlayer_DiscardingLast8()
        {
            drawingFormHelper.ReDrawToolbar();
            Graphics g = Graphics.FromImage(bmp);

            g.DrawImage(image, 200 + drawingFormHelper.offsetCenterHalf, 186 + drawingFormHelper.offsetCenterHalf, 85 * drawingFormHelper.scaleDividend, 96 * drawingFormHelper.scaleDividend);
            Refresh();
            g.Dispose();

            //�йܴ������
            var fullDebug = FormSettings.GetSettingBool(FormSettings.KeyFullDebug);
            if (fullDebug && gameConfig.IsDebug && !ThisPlayer.isObserver)
            {
                if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DiscardingLast8Cards &&
                    ThisPlayer.CurrentHandState.Last8Holder == ThisPlayer.PlayerId) //������ҿ���
                {
                    SelectedCards.Clear();
                    Algorithm.ShouldSelectedLast8Cards(this.SelectedCards, this.ThisPlayer.CurrentPoker);
                    if (SelectedCards.Count == 8)
                    {
                        foreach (int card in SelectedCards)
                        {
                            ThisPlayer.CurrentPoker.RemoveCard(card);
                        }

                        ThisPlayer.DiscardCards(SelectedCards.ToArray());

                        ResortMyCards();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("failed to auto select last 8 cards: {0}, please manually select", SelectedCards));
                    }
                    SelectedCards.Clear();
                }
            }
        }

        private void ThisPlayer_DumpingFail(ShowingCardsValidationResult result)
        {
            //������һ��
            if (ThisPlayer.CurrentTrickState.AllPlayedShowedCards() || ThisPlayer.CurrentTrickState.IsStarted() == false)
            {
                drawingFormHelper.DrawCenterImage();
                drawingFormHelper.DrawScoreImage();
            }
            ThisPlayer.CurrentTrickState.ShowedCards[result.PlayerId] = result.CardsToShow;

            string latestPlayer = result.PlayerId;
            int position = PlayerPosition[latestPlayer];
            if (latestPlayer == ThisPlayer.PlayerId)
            {
                drawingFormHelper.DrawMyShowedCards();
            }
            if (position == 2)
            {
                drawingFormHelper.DrawNextUserSendedCards();
            }
            if (position == 3)
            {
                drawingFormHelper.DrawFriendUserSendedCards();
            }
            if (position == 4)
            {
                drawingFormHelper.DrawPreviousUserSendedCards();
            }
            ThisPlayer.CurrentTrickState.ShowedCards[result.PlayerId].Clear();
        }

        private void ThisPlayer_HostIsOnlineEventHandler(bool success)
        {
            string result = success ? "Host online!" : "Host offline";
            Invoke(new Action(() =>
            {
                this.lblSouthStarter.Text = result;
                this.progressBarPingHost.Value = this.progressBarPingHost.Maximum;
            }));
        }

        #endregion

        private void btnReady_Click(object sender, EventArgs e)
        {
            if (ThisPlayer.isObserver) return;
            ThisPlayer.ReadyToStart();
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings formSettings = new FormSettings();
            formSettings.SettingsUpdatedEvent += Mainform_SettingsUpdatedEventHandler;
            formSettings.Show();
        }

        private void RebootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void RestoreGameStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ThisPlayer.isObserver) return;
            ThisPlayer.RestoreGameStateFromFile(false);
        }

        private void RestoreGameStateCardsShoeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ThisPlayer.isObserver) return;
            ThisPlayer.RestoreGameStateFromFile(true);
        }

        private void theTimer_Tick(object sender, EventArgs e)
        {
            if (this.timerCountDown > 0)
            {
                this.timerCountDown--;
                this.drawingFormHelper.DrawCountDown(true);
                if (this.timerCountDown > 0) return;
            }
            Thread.Sleep(200);
            this.drawingFormHelper.DrawCountDown(false);
            this.theTimer.Stop();
        }

        private void AutoUpdaterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoUpdater.ReportErrors = true;
            AutoUpdater.Start("https://raw.githubusercontent.com/iceburgy/Tractor_LAN/master/SourceCode/TractorWinformClient/AutoUpdater.xml");
        }

        private void FeatureOverviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/iceburgy/Tractor_LAN/blob/master/README.md");
        }

        private void TeamUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ThisPlayer.isObserver) return;
            ThisPlayer.TeamUp();
        }

        private void MoveToNextPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ThisPlayer.isObserver) return;
            ThisPlayer.MoveToNextPosition(ThisPlayer.PlayerId);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (this.updateOnLoad)
            {
                AutoUpdater.ReportErrors = false;
                AutoUpdater.Start("https://raw.githubusercontent.com/iceburgy/Tractor_LAN/master/SourceCode/TractorWinformClient/AutoUpdater.xml");
            }
        }

        private void ToolStripMenuItemShowVersion_Click(object sender, EventArgs e)
        {
            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBox.Show(string.Format("��ǰ�汾��{0}", assemblyVersion));
        }

        private void ToolStripMenuItemGetReady_Click(object sender, EventArgs e)
        {
            this.btnReady.PerformClick();
        }

        private void btnEnterRoom_Click(object sender, EventArgs e)
        {
            int roomID;
            int posID = -1;
            string[] nameParts = ((Button)sender).Name.Split('_');
            string roomIDString = nameParts[2];
            if (int.TryParse(roomIDString, out roomID))
            {
                if (nameParts.Length >= 4)
                {
                    string posIDString = nameParts[3];
                    int.TryParse(posIDString, out posID);
                }
                ThisPlayer.PlayerEnterRoom(ThisPlayer.MyOwnId, roomID, posID);
            }
            else
            {
                MessageBox.Show(string.Format("failed to click button with a bad ID: {0}", roomIDString));
            }
        }

        private void ToolStripMenuItemEnterHall_Click(object sender, EventArgs e)
        {
            this.btnEnterHall.PerformClick();
        }

        private void btnEnterHall_Click(object sender, EventArgs e)
        {
            ThisPlayer.PlayerEnterHall(ThisPlayer.MyOwnId);
        }

        private void ToolStripMenuItemRobot_Click(object sender, EventArgs e)
        {
            this.btnRobot.PerformClick();
        }

        private void btnRobot_Click(object sender, EventArgs e)
        {
            if (ThisPlayer.isObserver) return;
            ThisPlayer.ToggleIsRobot();
        }

        private void ToolStripMenuItemObserverNextPlayer_Click(object sender, EventArgs e)
        {
            this.btnObserveNext.PerformClick();
        }

        private void btnObserveNext_Click(object sender, EventArgs e)
        {
            if (ThisPlayer.isObserver)
            {
                ThisPlayer.ObservePlayerById(PositionPlayer[2], ThisPlayer.MyOwnId);
            }
        }

        private void btnExitRoom_Click(object sender, EventArgs e)
        {
            ThisPlayer.ExitRoom(ThisPlayer.MyOwnId);
        }

        private void btnSurrender_Click(object sender, EventArgs e)
        {
            ThisPlayer.SpecialEndGame(ThisPlayer.MyOwnId, SpecialEndingType.Surrender);
            this.btnSurrender.Visible = false;
            this.btnRiot.Visible = false;
        }

        private void btnRiot_Click(object sender, EventArgs e)
        {
            SpecialEndingType endType = SpecialEndingType.RiotByScore;
            //���������������ж������ָ���
            int riotScoreCap = ThisPlayer.CurrentRoomSetting.AllowRiotWithTooFewScoreCards;
            if (ThisPlayer.CurrentPoker.GetTotalScore() <= riotScoreCap)
            {
                endType = SpecialEndingType.RiotByScore;
            }
            else
            {
                endType = SpecialEndingType.RiotByTrump;
            }

            ThisPlayer.SpecialEndGame(ThisPlayer.MyOwnId, endType);
            this.btnSurrender.Visible = false;
            this.btnRiot.Visible = false;
        }

        private void ToolStripMenuItemUserManual_Click(object sender, EventArgs e)
        {
            string userManual = "";
            userManual = "����������";
            userManual += "\n- ����������ׯ�ҽ����Զ���̨";
            userManual += "\n- ˦��ʧ�ܣ�������ÿ��10�ֽ��з���";
            userManual += "\n\n�����ؼ���";
            userManual += "\n- ����ʱ�����йܿ��ڴﵽ5��ʱ�Զ�����";
            userManual += "\n- �Ҽ�ѡ�ƣ��Զ�����ѡ�����кϷ��������ƣ������ڳ��ơ���ף�";
            userManual += "\n- �Ҽ������հ״��鿴�����ֳ��ơ�˭����ʲô�ơ��÷���";
            userManual += "\n- С��ͼ�����һȦ�еĴ��ƣ�������ͼ�����������";
            userManual += "\n\n����ݼ���";
            userManual += "\n- ���ƣ�����S��Show cards��";
            userManual += "\n- ���������F1";
            userManual += "\n- �����һ�����䣺F2";
            userManual += "\n- ������F3������Z��Zhunbei׼����";
            userManual += "\n- �йܣ�F4������A��Auto��";
            userManual += "\n- �Թ��¼ң�F5�������Թ�ģʽ�£�";
            userManual += "\n\n�˰�����Ϣ���ڲ˵�����������ʹ��˵�������ٴβ鿴";
            userManual += "\n\n������ʾ��";

            DialogResult dialogResult = MessageBox.Show(userManual, "ʹ��˵��", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                FormSettings.SetSetting(FormSettings.KeyIsHelpSeen, "true");
            }
            else if (dialogResult == DialogResult.No)
            {
                FormSettings.SetSetting(FormSettings.KeyIsHelpSeen, "false");
            }
        }

        private void ToolStripMenuItemEnterRoom0_Click(object sender, EventArgs e)
        {
            Control[] controls = this.Controls.Find(string.Format("{0}_btnEnterRoom_0", roomControlPrefix), false);
            if (controls != null && controls.Length > 0)
            {
                Button enterRoom0 = ((Button)controls[0]);
                if (enterRoom0.Visible)
                {
                    enterRoom0.PerformClick();
                }
            }
        }

        private void tmrGeneral_Tick(object sender, EventArgs e)
        {
            if (this.progressBarPingHost.Visible == true && this.progressBarPingHost.Value < this.progressBarPingHost.Maximum)
            {
                int step = (this.progressBarPingHost.Maximum - this.progressBarPingHost.Value) / 8;
                this.progressBarPingHost.Increment(step);
            }
            else
            {
                this.tmrGeneral.Stop();
                progressBarPingHost.Value = this.progressBarPingHost.Maximum;
                this.progressBarPingHost.Hide();
                Refresh();
            }
        }

        private void btnPig_Click(object sender, EventArgs e)
        {
            ToDiscard8Cards();
            ToShowCards();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.A:
                    if (this.btnRobot.Visible && this.btnRobot.Enabled)
                    {
                        this.btnRobot.PerformClick();
                    }
                    return true;
                case Keys.S:
                    if (this.btnPig.Visible)
                    {
                        this.btnPig.PerformClick();
                    }
                    return true;
                case Keys.Z:
                    if (this.btnReady.Visible && this.btnReady.Enabled)
                    {
                        this.btnReady.PerformClick();
                    }
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void btnRoomSetting_Click(object sender, EventArgs e)
        {
            FormRoomSetting roomSetting = new FormRoomSetting(this);
            roomSetting.RoomSettingChangedByClientEvent += Mainform_RoomSettingChangedByClientEventHandler;
            roomSetting.Show();
        }
    }
}