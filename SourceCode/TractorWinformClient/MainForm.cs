using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
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
        private string musicFile = "";
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
            ThisPlayer.NewPlayerJoined += ThisPlayer_NewPlayerJoined;
            ThisPlayer.NewPlayerReadyToStart += ThisPlayer_NewPlayerReadyToStart;
            ThisPlayer.PlayerToggleIsRobot += ThisPlayer_PlayerToggleIsRobot;
            ThisPlayer.PlayersTeamMade += ThisPlayer_PlayersTeamMade;
            ThisPlayer.TrickFinished += ThisPlayer_TrickFinished;
            ThisPlayer.TrickStarted += ThisPlayer_TrickStarted;
            ThisPlayer.HandEnding += ThisPlayer_HandEnding;
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
                    else if ((e.X >= 20 && e.X <= (20 + 70) || e.X >= drawingFormHelper.offsetSideBar && e.X <= (drawingFormHelper.offsetSideBar + 70)) && (e.Y >= 30 && e.Y < 30 + 80))
                    {
                        //���Ϸ���һ���ƿ�鿴˭����ʲô��
                        if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing || ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DiscardingLast8Cards)
                        {
                            drawingFormHelper.LastTrumpMadeCardsShow();
                        }
                    }
                    else if (e.X >= drawingFormHelper.offsetSideBar - 56 && e.X <= drawingFormHelper.offsetSideBar && e.Y >= 128 && e.Y < 128 + 56)
                    {
                        //��÷�ͼ��鿴�÷���
                        drawingFormHelper.DrawScoreCards();
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
                        //������һ�ָ����������ƣ���С��һ�룬�������½�
                        ThisPlayer_PlayerLastTrickShowedCards();
                        Refresh();
                    }
                }


                //�ж��Ƿ�����С��*********�����ϵĵ����ͬ
                var pigRect = new Rectangle(296 + drawingFormHelper.offsetXPig, 300 + drawingFormHelper.offsetY, 53, 46);
                var region = new Region(pigRect);
                if (region.IsVisible(e.X, e.Y))
                {
                    if (SelectedCards.Count > 0)
                    {
                        ToDiscard8Cards();
                        ToShowCards();
                    }
                }
            }
            else if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DistributingCards ||
                     ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DistributingCardsFinished)
            {
                ExposeTrump(e);
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
                                if (ThisPlayer.CurrentPoker.BlackJoker == 2)
                                    next = TrumpExposingPoker.PairBlackJoker;
                                else if (ThisPlayer.CurrentPoker.RedJoker == 2)
                                    next = TrumpExposingPoker.PairRedJoker;
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
            var pigRect = new Rectangle(296 + drawingFormHelper.offsetXPig, 300 + drawingFormHelper.offsetY, 53, 46);
            var pigRectEmpty = new Rectangle(296, 300, 53, 46);
            //�ж��Ƿ��ڿ��ƽ׶�
            if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DiscardingLast8Cards &&
                ThisPlayer.CurrentHandState.Last8Holder == ThisPlayer.PlayerId) //������ҿ���
            {
                if (SelectedCards.Count == 8)
                {
                    //����,���Բ�ȥС��
                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(image, pigRect, pigRectEmpty, GraphicsUnit.Pixel);
                    g.Dispose();

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
            var pigRect = new Rectangle(296 + drawingFormHelper.offsetXPig, 300 + drawingFormHelper.offsetY, 53, 46);
            var pigRectEmpty = new Rectangle(296, 300, 53, 46);
            Graphics g = Graphics.FromImage(bmp);
            if (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing &&
                ThisPlayer.CurrentTrickState.NextPlayer() == ThisPlayer.PlayerId)
            {
                ShowingCardsValidationResult showingCardsValidationResult =
                    TractorRules.IsValid(ThisPlayer.CurrentTrickState, SelectedCards, ThisPlayer.CurrentPoker);
                //�����׼�������ƺϷ�
                if (showingCardsValidationResult.ResultType == ShowingCardsValidationResultType.Valid)
                {
                    //��ȥС��
                    g.DrawImage(image, pigRect, pigRectEmpty, GraphicsUnit.Pixel);

                    foreach (int card in SelectedCards)
                    {
                        ThisPlayer.CurrentPoker.RemoveCard(card);
                    }
                    ThisPlayer.ShowCards(SelectedCards);
                    drawingFormHelper.DrawMyShowedCards();
                    SelectedCards.Clear();
                }
                else if (showingCardsValidationResult.ResultType == ShowingCardsValidationResultType.TryToDump)
                {
                    //��ȥС��
                    g.DrawImage(image, pigRect, pigRectEmpty, GraphicsUnit.Pixel);

                    ShowingCardsValidationResult result = ThisPlayer.ValidateDumpingCards(SelectedCards);
                    if (result.ResultType == ShowingCardsValidationResultType.DumpingSuccess) //˦�Ƴɹ�.
                    {
                        foreach (int card in SelectedCards)
                        {
                            ThisPlayer.CurrentPoker.RemoveCard(card);
                        }
                        ThisPlayer.ShowCards(SelectedCards);

                        drawingFormHelper.DrawMyShowedCards();
                        SelectedCards.Clear();
                    }
                        //˦��ʧ��
                    else
                    {
                        foreach (int card in result.MustShowCardsForDumpingFail)
                        {
                            ThisPlayer.CurrentPoker.RemoveCard(card);
                        }
                        Thread.Sleep(2000);
                        ThisPlayer.ShowCards(result.MustShowCardsForDumpingFail);

                        SelectedCards = result.MustShowCardsForDumpingFail;
                        SelectedCards.Clear();
                    }
                }
            }
            g.Dispose();
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


        private void ThisPlayer_PlayerShowedCards()
        {
            //������һ��
            if (ThisPlayer.CurrentTrickState.CountOfPlayerShowedCards() == 1)
            {
                drawingFormHelper.DrawCenterImage();
                drawingFormHelper.DrawScoreImage();
            }

            string latestPlayer = ThisPlayer.CurrentTrickState.LatestPlayerShowedCard();
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

            if (ThisPlayer.CurrentTrickState.NextPlayer() == ThisPlayer.PlayerId)
                drawingFormHelper.DrawMyPlayingCards(ThisPlayer.CurrentPoker);

            //��ʱ�����Թ�����
            if (ThisPlayer.isObserver && ThisPlayer.PlayerId == latestPlayer)
            {
                ThisPlayer.CurrentPoker = ThisPlayer.CurrentHandState.PlayerHoldingCards[ThisPlayer.PlayerId];
                ResortMyCards();
            }

            //���һ���Զ�����
            bool isLastTrick = false;
            if (ThisPlayer.CurrentTrickState.LeadingCards.Count == this.ThisPlayer.CurrentPoker.Count)
            {
                isLastTrick = true;
            }

            //�йܴ��򣬸���
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
                    drawingFormHelper.DrawMyShowedCards();
                }
                else
                {
                    MessageBox.Show(string.Format("failed to auto select cards: {0}, please manually select", SelectedCards));
                }
                SelectedCards.Clear();
            }
        }

        //������һ�ָ����������ƣ���С��һ�룬�������½�
        private void ThisPlayer_PlayerLastTrickShowedCards()
        {
            if (ThisPlayer.CurrentTrickState.ShowedCardsInLastTrick.Count == 0) return;
            foreach (var entry in ThisPlayer.CurrentTrickState.ShowedCardsInLastTrick)
            {
                string player = entry.Key;
                int position = PlayerPosition[player];
                if (position == 1)
                {
                    drawingFormHelper.DrawMyLastSendedCardsAction(new ArrayList(entry.Value));
                }
                if (position == 2)
                {
                    drawingFormHelper.DrawNextUserLastSendedCardsAction(new ArrayList(entry.Value));
                }
                if (position == 3)
                {
                    drawingFormHelper.DrawFriendUserLastSendedCardsAction(new ArrayList(entry.Value));
                }
                if (position == 4)
                {
                    drawingFormHelper.DrawPreviousUserLastSendedCardsAction(new ArrayList(entry.Value));
                }
            }
        }

        private void ThisPlayer_ShowingCardBegan()
        {
            ThisPlayer_DiscardingLast8();
            drawingFormHelper.RemoveToolbar();
            drawingFormHelper.DrawCenterImage();
            drawingFormHelper.DrawScoreImage();
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
            int offsetXDelta = 200;
            int offsetY = 100;
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

            offsetX += offsetXDelta;
            int seatSize = 40;
            int tableSize = seatSize * 2;

            for (int roomInd = 0; roomInd < roomStates.Count; roomInd++)
            {
                int roomOffsetX = offsetX + seatSize * 7 * (roomInd % 2);
                int roomOffsetY = offsetY + seatSize * 7 * (roomInd / 2);

                RoomState room = roomStates[roomInd];
                Button btnEnterRoom = new Button();
                btnEnterRoom.Location = new System.Drawing.Point(roomOffsetX + seatSize * 3 / 2, roomOffsetY + seatSize * 3 / 2);

                btnEnterRoom.Name = string.Format("{0}_btnEnterRoom_{1}", roomControlPrefix, room.RoomID);
                btnEnterRoom.Size = new System.Drawing.Size(tableSize, tableSize);
                btnEnterRoom.Text = room.RoomName;
                btnEnterRoom.UseVisualStyleBackColor = true;
                btnEnterRoom.Click += new System.EventHandler(this.btnEnterRoom_Click);
                this.Controls.Add(btnEnterRoom);

                List<PlayerEntity> players = room.CurrentGameState.Players;
                for (int j = 0; j < players.Count; j++)
                {
                    int offsetXSeat = roomOffsetX;
                    switch (j)
                    {
                        case 0:
                        case 2:
                            offsetXSeat += seatSize * 2;
                            break;
                        case 3:
                            offsetXSeat += seatSize * 4;
                            break;
                        default:
                            break;
                    }
                    int offsetYSeat = roomOffsetY;
                    switch (j)
                    {
                        case 1:
                        case 3:
                            offsetYSeat += seatSize * 2;
                            break;
                        case 2:
                            offsetYSeat += seatSize * 4;
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
                        btnEnterRoomByPos.Text = (j + 1).ToString();
                        btnEnterRoomByPos.UseVisualStyleBackColor = true;
                        btnEnterRoomByPos.Click += new System.EventHandler(this.btnEnterRoom_Click);
                        this.Controls.Add(btnEnterRoomByPos);
                    }
                    else
                    {
                        Label labelRoomByPos = new Label();
                        labelRoomByPos.AutoSize = false;
                        labelRoomByPos.BackColor = System.Drawing.Color.Transparent;
                        labelRoomByPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                        labelRoomByPos.ForeColor = System.Drawing.SystemColors.Control;
                        labelRoomByPos.Location = new System.Drawing.Point(offsetXSeat, offsetYSeat);
                        labelRoomByPos.Name = string.Format("{0}_lblRoom_{1}_{2}", roomControlPrefix, room.RoomID, j);
                        labelRoomByPos.Size = new System.Drawing.Size(seatSize, seatSize);
                        this.Controls.Add(labelRoomByPos);

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

        private void ThisPlayer_TrickStarted()
        {
            //�йܴ�������
            if (gameConfig.IsDebug && !ThisPlayer.isObserver &&
                (ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing || ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DiscardingLast8CardsFinished) &&
                ThisPlayer.CurrentTrickState.NextPlayer() == ThisPlayer.PlayerId &&
                !ThisPlayer.CurrentTrickState.IsStarted())
            {
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

            drawingFormHelper.DrawWhoWinThisTime();
            Refresh();
        }

        private void ThisPlayer_HandEnding()
        {
            drawingFormHelper.DrawFinishedSendedCards();
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

        private void ThisPlayer_NotifyMessageEventHandler(string msg)
        {
            MessageBox.Show(msg);
        }

        private void ThisPlayer_NotifyStartTimerEventHandler(int timerLength)
        {
            this.lblTheTimer.Text = timerLength.ToString();
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
            int timeRemaining = 0;
            if (int.TryParse(this.lblTheTimer.Text, out timeRemaining))
            {
                if (timeRemaining > 0)
                {
                    this.lblTheTimer.Text = (timeRemaining - 1).ToString();
                    if (timeRemaining - 1 > 0) return;
                }
            }
            Refresh();
            Thread.Sleep(200);
            this.lblTheTimer.Text = "";
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

        private void ToolStripMenuItemUserManual_Click(object sender, EventArgs e)
        {
            string userManual = "�����ؼ���";
            userManual += "\n�Ҽ�ѡ�ƣ��Զ�����ѡ�����кϷ��������ƣ������ڳ��ơ���ף�";
            userManual += "\n�鿴���ƣ������·��ġ�ׯ�ҡ�������ׯ�ң�";
            userManual += "\n�鿴���ֳ��ƣ��Ҽ������հ״�";
            userManual += "\n�鿴�÷��ƣ���÷�ͼ��";
            userManual += "\n�鿴˭����ʲô�ƣ����Ϸ���һ���ƿ򣨶���/�ϱ���";
            userManual += "\n\n����ݼ���";
            userManual += "\n���������F1";
            userManual += "\n�����һ�����䣺F2";
            userManual += "\n������F3";
            userManual += "\n�йܣ�F4";
            userManual += "\n�Թ��¼ң�F5�������Թ�ģʽ�£�";
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

        //���Լ���ʾ����
        private void lblSouthStarter_Click(object sender, EventArgs e)
        {
            //�Թ۲��ܴ������Ч��
            if (ThisPlayer.isObserver)
            {
                return;
            }
            if (ThisPlayer.CurrentPoker != null && ThisPlayer.CurrentPoker.Count > 0 &&
                ThisPlayer.CurrentHandState.Last8Holder == ThisPlayer.PlayerId &&
                ThisPlayer.CurrentHandState.DiscardedCards != null &&
                ThisPlayer.CurrentHandState.DiscardedCards.Length == 8)
            {
                drawingFormHelper.DrawDiscardedCards();
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
    }
}