using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using Kuaff.CardResouces;
using Duan.Xiugang.Tractor.Player;
using Duan.Xiugang.Tractor.Objects;

namespace Duan.Xiugang.Tractor
{
    /// <summary>
    /// ʵ�ִ󲿷ֵĻ滭����
    /// </summary>
    class DrawingFormHelper
    {
        MainForm mainForm;
        public int scaleDividend = 3;
        public int scaleDivisor = 2;
        public int offsetY = 245;
        public int offsetCenterHalf = 90;
        public int offsetCenter = 180;
        public int offsetSideBar = 0;
        public int offsetWinnerYMe = 21;
        public int offsetXPig = 240;
        public PictureBox[] overridingFlagLabels;
        public Bitmap[] overridingFlagPictures;
        public int[][][] overridingFlagLocations;
        public int[][] overridingFlagSizes;

        private int suitSequence = 0;
        private Font suitSequenceFont = new Font("Arial", 9, FontStyle.Bold);

        internal DrawingFormHelper(MainForm mainForm)
        {
            this.mainForm = mainForm;
            this.offsetSideBar = mainForm.Width - 70 - 20;
            initOverridingLabels();
        }

        private void initOverridingLabels()
        {
            int width = 75;
            int height = 50;
            this.overridingFlagLabels = new PictureBox[] { this.mainForm.imbOverridingFlag_1, this.mainForm.imbOverridingFlag_2, this.mainForm.imbOverridingFlag_3, this.mainForm.imbOverridingFlag_4 };
            this.overridingFlagPictures = new Bitmap[] { Properties.Resources.bagua, Properties.Resources.zhugong, Properties.Resources.sha, Properties.Resources.huosha, Properties.Resources.leisha };
            this.overridingFlagLocations = new int[2][][];
            this.overridingFlagSizes = new int[2][];

            //half size
            this.overridingFlagLocations[0] = new int[4][];
            this.overridingFlagLocations[0][0] = new int[] { 285 + offsetCenterHalf, 244 + offsetCenter + 96 * scaleDividend / scaleDivisor - height / 2 };
            this.overridingFlagLocations[0][1] = new int[] { 326 + offsetCenter, 187 + offsetCenterHalf + 96 * scaleDividend / scaleDivisor - height / 2 };
            this.overridingFlagLocations[0][2] = new int[] { 285 + offsetCenterHalf, 130 + 96 * scaleDividend / scaleDivisor - height / 2 };
            this.overridingFlagLocations[0][3] = new int[] { 245, 187 + offsetCenterHalf + 96 * scaleDividend / scaleDivisor - height / 2 };

            this.overridingFlagSizes[0] = new int[] { width / 2, height / 2 };

            //full size
            this.overridingFlagLocations[1] = new int[4][];
            this.overridingFlagLocations[1][0] = new int[] { 285 + offsetCenterHalf, 244 + offsetCenter + 96 * scaleDividend / scaleDivisor - height };
            this.overridingFlagLocations[1][1] = new int[] { 326 + offsetCenter, 187 + offsetCenterHalf + 96 * scaleDividend / scaleDivisor - height };
            this.overridingFlagLocations[1][2] = new int[] { 285 + offsetCenterHalf, 130 + 96 * scaleDividend / scaleDivisor - height };
            this.overridingFlagLocations[1][3] = new int[] { 245, 187 + offsetCenterHalf + 96 * scaleDividend / scaleDivisor - height };

            this.overridingFlagSizes[1] = new int[] { width, height };
        }

        private void setOverridingLabel(int position, int sizeLevel) {
            this.overridingFlagLabels[position - 1].Location = new System.Drawing.Point(this.overridingFlagLocations[sizeLevel][position - 1][0], this.overridingFlagLocations[sizeLevel][position - 1][1]);
            this.overridingFlagLabels[position - 1].Size = new System.Drawing.Size(this.overridingFlagSizes[sizeLevel][0], this.overridingFlagSizes[sizeLevel][1]);
        }

        #region ���ƶ���

        internal void IGetCard(int cardNumber)
        {
            //�õ�������ͼ���Graphics
            Graphics g = Graphics.FromImage(mainForm.bmp);
            DrawMyCards(g, mainForm.ThisPlayer.CurrentPoker, mainForm.ThisPlayer.CurrentPoker.Count);

            ReDrawToolbar();

            mainForm.Refresh();
            g.Dispose();
        }

        internal void TrumpMadeCardsShow()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            var trumpMadeCard = (int)mainForm.ThisPlayer.CurrentHandState.Trump * 13 - 13 + mainForm.ThisPlayer.CurrentHandState.Rank;
            if (mainForm.ThisPlayer.CurrentHandState.TrumpExposingPoker == TrumpExposingPoker.PairBlackJoker)
                trumpMadeCard = 52;
            else if (mainForm.ThisPlayer.CurrentHandState.TrumpExposingPoker == TrumpExposingPoker.PairRedJoker)
                trumpMadeCard = 53;

            if (mainForm.ThisPlayer.CurrentHandState.TrumpExposingPoker ==  TrumpExposingPoker.SingleRank)
            {
                if (mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.TrumpMaker] == 3)
                {
                    g.DrawImage(getPokerImageByNumber(trumpMadeCard), 294 + offsetCenterHalf, 130, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                else if (mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.TrumpMaker] == 4)
                {
                    g.DrawImage(getPokerImageByNumber(trumpMadeCard), 80, 158 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                else if (mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.TrumpMaker] == 2)
                {
                    g.DrawImage(getPokerImageByNumber(trumpMadeCard), 480 + offsetCenterHalf, 200 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
            }
            else if (mainForm.ThisPlayer.CurrentHandState.TrumpExposingPoker > TrumpExposingPoker.SingleRank)
            {
                if (mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.TrumpMaker] == 3)
                {
                    ClearSuitCards(g);
                    g.DrawImage(getPokerImageByNumber(trumpMadeCard), 280 + offsetCenterHalf, 130, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                    g.DrawImage(getPokerImageByNumber(trumpMadeCard), 280 + 12 * scaleDividend / scaleDivisor + offsetCenterHalf, 130, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                else if (mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.TrumpMaker] == 4)
                {
                    ClearSuitCards(g);
                    g.DrawImage(getPokerImageByNumber(trumpMadeCard), 80, 158 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                    g.DrawImage(getPokerImageByNumber(trumpMadeCard), 80, 158 + 20 * scaleDividend / scaleDivisor + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);

                }
                else if (mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.TrumpMaker] == 2)
                {
                    ClearSuitCards(g);
                    g.DrawImage(getPokerImageByNumber(trumpMadeCard), 480 + offsetCenterHalf, 200 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                    g.DrawImage(getPokerImageByNumber(trumpMadeCard), 480 + offsetCenterHalf, 200 + 20 * scaleDividend / scaleDivisor + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                else if (mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.TrumpMaker] == 1)
                {
                    ClearSuitCards(g);                    
                }
            }
            mainForm.Refresh();
            g.Dispose();
        }
        //���������
        internal void ClearSuitCards(Graphics g)
        {
            g.DrawImage(mainForm.image, new Rectangle(80, 158 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 116 * scaleDividend / scaleDivisor), new Rectangle(80, 158, 71, 116), GraphicsUnit.Pixel);
            g.DrawImage(mainForm.image, new Rectangle(480 + offsetCenterHalf, 200 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 116 * scaleDividend / scaleDivisor), new Rectangle(480, 200, 71, 116), GraphicsUnit.Pixel);
            g.DrawImage(mainForm.image, new Rectangle(280 + offsetCenterHalf, 130, 85 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor), new Rectangle(280, 80, 85, 96), GraphicsUnit.Pixel);
        }

        //�鿴��˭����������Сһ��
        internal void LastTrumpMadeCardsShow()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            Dictionary<string, Dictionary<Suit, TrumpState>> trumpDict = new Dictionary<string, Dictionary<Suit, TrumpState>>();
            foreach (var lastHandState in mainForm.ThisPlayer.CurrentHandState.LastTrumpStates)
            {
                string key1 = lastHandState.TrumpMaker;
                if (!trumpDict.ContainsKey(key1))
                {
                    trumpDict[key1] = new Dictionary<Suit, TrumpState>();
                }
                Dictionary<Suit, TrumpState> val1 = trumpDict[key1];

                Suit key2 = lastHandState.Trump;
                if (!val1.ContainsKey(key2))
                {
                    val1[key2] = lastHandState;
                }
                TrumpState val2 = val1[key2];
                val2.TrumpExposingPoker = (TrumpExposingPoker)Math.Max((int)val2.TrumpExposingPoker, (int)lastHandState.TrumpExposingPoker);
            }
            foreach (var trumpDict2Entry in trumpDict)
            {
                string player = trumpDict2Entry.Key;
                Dictionary<Suit, TrumpState> suitToTrumInfo = trumpDict2Entry.Value;
                int x = 0, y = 0;
                int wid = 71 * scaleDividend / scaleDivisor / 2;
                int hei = 96 * scaleDividend / scaleDivisor / 2;
                switch (mainForm.PlayerPosition[player])
                {
                    case 3:
                        x = 280 + offsetCenterHalf;
                        y = 130;
                        break;
                    case 4:
                        x = 80;
                        y = 200 + offsetCenterHalf;
                        break;
                    case 2:
                        x = offsetSideBar - wid;
                        y = 200 + offsetCenterHalf;
                        break;
                    case 1:
                        x = 280 - offsetCenterHalf;
                        y = 250 + offsetY;
                        break;
                    default:
                        break;
                }

                int offset = 0;
                int offsetDelta = 12 * scaleDividend / scaleDivisor;
                int totalCount = 0;
                if (mainForm.PlayerPosition[player] == 2)
                {
                    foreach (var suitToTrumInfoEntry in suitToTrumInfo)
                    {
                        int baseCount = 1;
                        TrumpState trumpInfo = suitToTrumInfoEntry.Value;
                        if (trumpInfo.TrumpExposingPoker > TrumpExposingPoker.SingleRank) baseCount = 2;
                        totalCount += baseCount;
                    }
                }
                x -= offsetDelta * (totalCount - 1);
                foreach (var suitToTrumInfoEntry in suitToTrumInfo)
                {
                    Suit trump = suitToTrumInfoEntry.Key;
                    TrumpState trumpInfo = suitToTrumInfoEntry.Value;

                    var trumpMadeCard = ((int)trump - 1) * 13 + mainForm.ThisPlayer.CurrentHandState.Rank;

                    if (trumpInfo.TrumpExposingPoker == TrumpExposingPoker.PairBlackJoker)
                        trumpMadeCard = 52;
                    else if (trumpInfo.TrumpExposingPoker == TrumpExposingPoker.PairRedJoker)
                        trumpMadeCard = 53;

                    int count = 1;
                    if (trumpInfo.TrumpExposingPoker > TrumpExposingPoker.SingleRank)
                    {
                        count = 2;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        g.DrawImage(getPokerImageByNumber(trumpMadeCard), x + offset, y, wid, hei);
                        offset += offsetDelta;
                    }
                }
            }
            mainForm.Refresh();
            g.Dispose();
        }

        #endregion // ���ƶ���

        #region ������λ�õ���
        /// <summary>
        /// ����ʱ���������.
        /// ���ȴӵ�ͼ��ȡ��Ӧ��λ�ã��ػ���鱳����
        /// Ȼ�����Ƶı��滭58-count*2���ơ�
        /// 
        /// </summary>
        /// <param name="g">������ͼƬ��Graphics</param>
        /// <param name="num">�Ƶ�����=58-���ƴ���*2</param>
        internal void DrawCenterAllCards(Graphics g, int num)
        {
            Rectangle rect = new Rectangle(200, 186, (num + 1) * 2 + 71, 96);
            g.DrawImage(mainForm.image, rect, rect, GraphicsUnit.Pixel);

            for (int i = 0; i < num; i++)
            {
                g.DrawImage(mainForm.gameConfig.BackImage, 200 + i * 2, 186, 71, 96);
            }
        }

        /// <summary>
        /// ����һ���ƣ���Ҫ�����������
        /// </summary>
        internal void DrawCenterImage()
        {
            this.HideOverridingLabels();
            Graphics g = Graphics.FromImage(mainForm.bmp);
            Rectangle rect = new Rectangle(20, 120, offsetSideBar, 224 + offsetCenter + 75);
            g.DrawImage(mainForm.image, 20, 120, rect.Width, rect.Height);
            g.Dispose();
            mainForm.Refresh();
        }

        /// <summary>
        /// ������ͼƬ
        /// </summary>
        internal void DrawPassImage()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            Rectangle rect = new Rectangle(110, 150, 400, 199);
            g.DrawImage(Properties.Resources.Pass, rect);
            g.Dispose();
            mainForm.Refresh();
        }
        #endregion // ������λ�õ���

        #region ���ƴ���

        internal void DrawDiscardedLast8Cards()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            Rectangle rect = new Rectangle(200, 186, 90, 96);
            Rectangle backRect = new Rectangle(77, 121, 477, 254);
            //���8�ŵ�ͼ��ȡ����
            Bitmap backup = mainForm.bmp.Clone(rect, PixelFormat.DontCare);
            //����λ���ñ�������
            //g.DrawImage(mainForm.image, rect, rect, GraphicsUnit.Pixel);
            g.DrawImage(mainForm.image, backRect, backRect, GraphicsUnit.Pixel);
            mainForm.Refresh();
            g.Dispose();
        }
        //�յ��ƵĶ���
        /// <summary>
        /// ����25�κ����ʣ��8����.
        /// ��ʱ�Ѿ�ȷ����ׯ�ң���8���ƽ���ׯ��,
        /// ͬʱ�Զ����ķ�ʽ��ʾ��
        /// </summary>
        internal void DrawCenter8Cards()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            Rectangle rect = new Rectangle(200, 186, 90, 96);
            Rectangle backRect = new Rectangle(77, 121, 477, 254);
            //���8�ŵ�ͼ��ȡ����
            Bitmap backup = mainForm.bmp.Clone(rect, PixelFormat.DontCare);
            //����λ���ñ�������
            //g.DrawImage(mainForm.image, rect, rect, GraphicsUnit.Pixel);
            g.DrawImage(mainForm.image, backRect, backRect, GraphicsUnit.Pixel);

            //������8�Ž���ׯ�ң�������ʽ��
            if (mainForm.currentState.Master == 1)
            {
                DrawAnimatedCard(backup, 300, 330, 90, 96);
                Get8Cards(mainForm.pokerList[0], mainForm.pokerList[1], mainForm.pokerList[2], mainForm.pokerList[3]);
            }
            else if (mainForm.currentState.Master == 2)
            {
                DrawAnimatedCard(backup, 200, 80, 90, 96);
                Get8Cards(mainForm.pokerList[1], mainForm.pokerList[0], mainForm.pokerList[2], mainForm.pokerList[3]);
            }
            else if (mainForm.currentState.Master == 3)
            {
                DrawAnimatedCard(backup, 70, 186, 90, 96);
                Get8Cards(mainForm.pokerList[2], mainForm.pokerList[1], mainForm.pokerList[0], mainForm.pokerList[3]);
            }
            else if (mainForm.currentState.Master == 4)
            {
                DrawAnimatedCard(backup, 400, 186, 90, 96);
                Get8Cards(mainForm.pokerList[3], mainForm.pokerList[1], mainForm.pokerList[2], mainForm.pokerList[0]);
            }
            mainForm.Refresh();

            g.Dispose();
        }
        //�����8�Ž���ׯ��
        private void Get8Cards(ArrayList list0, ArrayList list1, ArrayList list2, ArrayList list3)
        {
            list0.Add(list1[25]);
            list0.Add(list1[26]);
            list0.Add(list2[25]);
            list0.Add(list2[26]);
            list0.Add(list3[25]);
            list0.Add(list3[26]);
            list1.RemoveAt(26);
            list1.RemoveAt(25);
            list2.RemoveAt(26);
            list2.RemoveAt(25);
            list3.RemoveAt(26);
            list3.RemoveAt(25);
        }

        internal void DrawBottomCards(ArrayList bottom)
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);

            //������,��169��ʼ��
            for (int i = 0; i < 8; i++)
            {
                if (i == 2)
                {
                    g.DrawImage(getPokerImageByNumber((int)bottom[i]), 230 + i * 14, 146, 71, 96);
                }
                else
                {
                    g.DrawImage(getPokerImageByNumber((int)bottom[i]), 230 + i * 14, 186, 71, 96);
                }
            }

            mainForm.Refresh();

            g.Dispose();
        }
        #endregion // ���ƴ���


        #region ����Sidebar��toolbar
        /// <summary>
        /// ����Sidebar
        /// </summary>
        /// <param name="g"></param>
        internal void DrawSidebar(Graphics g)
        {
            DrawMyImage(g, Properties.Resources.Sidebar, 20, 30, 70, 89);
            DrawMyImage(g, Properties.Resources.Sidebar, offsetSideBar, 30, 70, 89);
        }
        /// <summary>
        /// �������ϱ�
        /// </summary>
        /// <param name="g">������ͼ���Graphics</param>
        /// <param name="who">��˭</param>
        /// <param name="b">�Ƿ���ɫ</param>
        internal void DrawMaster(Graphics g, int who, int start)
        {
            if (who < 1 || who > 4)
            {
                return;
            }

            start = start * 80;

            int X = 0;

            if (who == 1)
            {
                start += 40;
                X = offsetSideBar + 8;
            }
            else if (who == 2)
            {
                start += 60;
                X = offsetSideBar + 40;
            }
            else if (who == 3)
            {
                start += 0;
                X = 30;
            }
            else if (who == 4)
            {
                start += 20;
                X = 60;
            }

            Rectangle destRect = new Rectangle(X, 45, 20, 20);
            Rectangle srcRect = new Rectangle(start, 0, 20, 20);

            g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);

        }

        /// <summary>
        /// ��������ɫ
        /// </summary>
        /// <param name="g"></param>
        /// <param name="who"></param>
        /// <param name="start"></param>
        internal void DrawOtherMaster(Graphics g, int who, int start)
        {


            if (who != 1)
            {
                Rectangle destRect = new Rectangle(offsetSideBar + 8, 45, 20, 20);
                Rectangle srcRect = new Rectangle(40, 0, 20, 20);
                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }
            if (who != 2)
            {
                Rectangle destRect = new Rectangle(offsetSideBar + 40, 45, 20, 20);
                Rectangle srcRect = new Rectangle(60, 0, 20, 20);
                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }
            if (who != 3)
            {
                Rectangle destRect = new Rectangle(31, 45, 20, 20);
                Rectangle srcRect = new Rectangle(0, 0, 20, 20);
                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }
            if (who != 4)
            {
                Rectangle destRect = new Rectangle(61, 45, 20, 20);
                Rectangle srcRect = new Rectangle(20, 0, 20, 20);
                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }

        }


        /// <summary>
        /// ����Rank
        /// </summary>
        internal void Rank()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);

            int rankofNorthSouth = 0;
            int rankofEastWest = 0;

            if (mainForm.ThisPlayer.CurrentGameState.Players.Find(p => p != null && p.PlayerId == mainForm.ThisPlayer.PlayerId) != null)
                rankofNorthSouth = mainForm.ThisPlayer.CurrentGameState.Players.Find(p => p != null && p.PlayerId == mainForm.ThisPlayer.PlayerId).Rank;
            if (mainForm.ThisPlayer.CurrentGameState.GetNextPlayerAfterThePlayer(mainForm.ThisPlayer.PlayerId) != null)
                rankofEastWest = mainForm.ThisPlayer.CurrentGameState.GetNextPlayerAfterThePlayer(mainForm.ThisPlayer.PlayerId).Rank;

            bool starterIsKnown = false;
            bool starterInMyTeam = false;
            if (!string.IsNullOrEmpty(mainForm.ThisPlayer.CurrentHandState.Starter))
            {
                starterInMyTeam = mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.Starter] == 1 ||
                          mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.Starter] == 3;
                starterIsKnown = true;
            }


            Rectangle northSouthRect = new Rectangle(offsetSideBar + 26, 68, 20, 20);
            Rectangle eastWestRect = new Rectangle(46, 68, 20, 20);



            //Ȼ��������д��
            if (!starterIsKnown)
            {
                g.DrawImage(Properties.Resources.Sidebar, northSouthRect, new Rectangle(26, 38, 20, 20), GraphicsUnit.Pixel);
                g.DrawImage(Properties.Resources.Sidebar, eastWestRect, new Rectangle(26, 38, 20, 20), GraphicsUnit.Pixel);

                g.DrawImage(Properties.Resources.CardNumber, northSouthRect, getCardNumberImage(rankofNorthSouth, false), GraphicsUnit.Pixel);
                g.DrawImage(Properties.Resources.CardNumber, eastWestRect, getCardNumberImage(rankofEastWest, false), GraphicsUnit.Pixel);
            }
            else
            {
                g.DrawImage(Properties.Resources.Sidebar, northSouthRect, new Rectangle(26, 38, 20, 20), GraphicsUnit.Pixel);
                g.DrawImage(Properties.Resources.Sidebar, eastWestRect, new Rectangle(26, 38, 20, 20), GraphicsUnit.Pixel);

                if (starterInMyTeam)
                {
                    g.DrawImage(Properties.Resources.CardNumber, northSouthRect, getCardNumberImage(rankofNorthSouth, true), GraphicsUnit.Pixel);
                    g.DrawImage(Properties.Resources.CardNumber, eastWestRect, getCardNumberImage(rankofEastWest, false), GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(Properties.Resources.CardNumber, northSouthRect, getCardNumberImage(rankofNorthSouth, false), GraphicsUnit.Pixel);
                    g.DrawImage(Properties.Resources.CardNumber, eastWestRect, getCardNumberImage(rankofEastWest, true), GraphicsUnit.Pixel);
                }
            }

            mainForm.Refresh();
            g.Dispose();
        }

        private Rectangle getCardNumberImage(int number, bool b)
        {
            Rectangle result = new Rectangle(0, 0, 0, 0);

            if (number >= 0 && number <= 12)
            {
                if (b)
                {
                    number += 14;
                }
                result = new Rectangle(number * 20, 0, 20, 20);
            }


            if ((number == 53) && (b))
            {
                result = new Rectangle(540, 0, 20, 20);
            }
            if ((number == 53) && (!b))
            {
                result = new Rectangle(260, 0, 20, 20);
            }

            return result;
        }
        /// <summary>
        /// ����
        /// </summary>
        internal void Trump()
        {
            CurrentHandState currentHandState = mainForm.ThisPlayer.CurrentHandState;

            Graphics g = Graphics.FromImage(mainForm.bmp);

            Rectangle northSouthRect = new Rectangle(offsetSideBar + 23, 88, 25, 25);
            Rectangle eastWestRect = new Rectangle(43, 88, 25, 25);

            Rectangle trumpRect = new Rectangle(23, 58, 25, 25);//backGroud
            Rectangle backGroupRect = new Rectangle(23, 58, 25, 25);//backGroud
            Rectangle noTrumpRect = new Rectangle(250, 0, 25, 25);

            g.DrawImage(Properties.Resources.Sidebar, northSouthRect, backGroupRect, GraphicsUnit.Pixel);
            g.DrawImage(Properties.Resources.Sidebar, eastWestRect, backGroupRect, GraphicsUnit.Pixel);
            g.DrawImage(Properties.Resources.Suit, northSouthRect, noTrumpRect, GraphicsUnit.Pixel);
            g.DrawImage(Properties.Resources.Suit, eastWestRect, noTrumpRect, GraphicsUnit.Pixel);

            if (currentHandState == null)
                return;

            bool trumpMade = false;
            bool trumpMadeByMyTeam = false;
            if (mainForm.ThisPlayer.CurrentHandState.Trump != Suit.None)
            {
                trumpMade = true;
                trumpMadeByMyTeam = mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.TrumpMaker] == 1 ||
                          mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.TrumpMaker] == 3;
            }


            if (!trumpMade)
                return;

            if (currentHandState.Trump == Suit.Heart)
            {
                trumpRect = new Rectangle(0, 0, 25, 25);
            }
            else if (currentHandState.Trump == Suit.Spade)
            {
                trumpRect = new Rectangle(25, 0, 25, 25);
            }
            else if (currentHandState.Trump == Suit.Diamond)
            {
                trumpRect = new Rectangle(50, 0, 25, 25);
            }
            else if (currentHandState.Trump == Suit.Club)
            {
                trumpRect = new Rectangle(75, 0, 25, 25);
            }
            else if (currentHandState.Trump == Suit.Joker)
            {
                trumpRect = new Rectangle(100, 0, 25, 25);
            }

            if (trumpMadeByMyTeam)
            {
                g.DrawImage(Properties.Resources.Sidebar, northSouthRect, backGroupRect, GraphicsUnit.Pixel);
                g.DrawImage(Properties.Resources.Suit, northSouthRect, trumpRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.DrawImage(Properties.Resources.Sidebar, eastWestRect, backGroupRect, GraphicsUnit.Pixel);
                g.DrawImage(Properties.Resources.Suit, eastWestRect, trumpRect, GraphicsUnit.Pixel);
            }

            mainForm.Refresh();
            g.Dispose();
        }


        /// <summary>
        /// ��������
        /// </summary>
        internal void DrawToolbar()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            g.DrawImage(Properties.Resources.Toolbar, new Rectangle(415 + offsetCenter, 325 -14 + offsetY, 129 * scaleDividend / scaleDivisor, 29 * scaleDividend / scaleDivisor), new Rectangle(0, 0, 129, 29), GraphicsUnit.Pixel);
            //�����ְ���ɫ
            g.DrawImage(Properties.Resources.Suit, new Rectangle(418 + offsetCenter, 327 -14 + offsetY, 25 * scaleDividend / scaleDivisor, 25 * scaleDividend / scaleDivisor), new Rectangle(125, 0, 25, 25), GraphicsUnit.Pixel);
            g.DrawImage(Properties.Resources.Suit, new Rectangle(417 + offsetCenter + 25 * scaleDividend / scaleDivisor, 327 -14 + offsetY, 100 * scaleDividend / scaleDivisor, 25 * scaleDividend / scaleDivisor), new Rectangle(125, 0, 100, 25), GraphicsUnit.Pixel);
            g.Dispose();
        }

        /// <summary>
        /// ��ȥ������
        /// </summary>
        internal void RemoveToolbar()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            g.DrawImage(mainForm.image, new Rectangle(415 + offsetCenter, 325 -14 + offsetY, 129 * scaleDividend / scaleDivisor, 29 * scaleDividend / scaleDivisor), new Rectangle(415, 325, 129, 29), GraphicsUnit.Pixel);
            g.Dispose();
        }


        #endregion // ����Sidebar��toolbar



        //�ж����Ƿ�����
        internal void ReDrawToolbar()
        {
            //������������������ж�
            if (mainForm.ThisPlayer.CurrentHandState.Rank == 53)
                return;
            var availableTrump = mainForm.ThisPlayer.AvailableTrumps();
            ReDrawToolbar(availableTrump);


        }

        //���ҵĹ�����
        internal void ReDrawToolbar(List<Suit> suits)
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            g.DrawImage(Properties.Resources.Toolbar, new Rectangle(415 + offsetCenter, 325 -14 + offsetY, 129 * scaleDividend / scaleDivisor, 29 * scaleDividend / scaleDivisor), new Rectangle(0, 0, 129, 29), GraphicsUnit.Pixel);
            //�����ְ���ɫ
            for (int i = 0; i < 5; i++)
            {
                if (suits.Exists(s=> (int)s ==i+1))
                {
                    int offsetXForHeart = i == 0 ? 1 : 0;
                    g.DrawImage(Properties.Resources.Suit, new Rectangle(418 + offsetXForHeart + offsetCenter + i * 25 * scaleDividend / scaleDivisor, 327 -14 + offsetY, 25 * scaleDividend / scaleDivisor, 25 * scaleDividend / scaleDivisor), new Rectangle(i * 25, 0, 25, 25), GraphicsUnit.Pixel);
                }
                else
                {
                    int offsetXForHeart = i == 0 ? 1 : 0;
                    g.DrawImage(Properties.Resources.Suit, new Rectangle(417 + offsetXForHeart + offsetCenter + i * 25 * scaleDividend / scaleDivisor, 327 -14 + offsetY, 25 * scaleDividend / scaleDivisor, 25 * scaleDividend / scaleDivisor), new Rectangle(125 + i * 25, 0, 25, 25), GraphicsUnit.Pixel);
                }
            }
            g.Dispose();
        }


        /// <summary>
        /// �ж�����Ƿ���������.
        /// �����㷨����������������򱾾����֣����·���
        /// </summary>
        /// <returns></returns>
        internal bool DoRankNot()
        {

            if (mainForm.currentState.Suit == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


  


        #region �ڸ�������»��Լ�����

        /// <summary>
        /// �����ڼ���л����ҵ�����.
        /// ���ջ�ɫ�����ƽ������֡�
        /// </summary>
        /// <param name="g">������ͼƬ��Graphics</param>
        /// <param name="currentPoker">�ҵ�ǰ�õ�����</param>
        /// <param name="index">�����Ƶ�����</param>
        internal void DrawMyCards(Graphics g, CurrentPoker currentPoker, int index)
        {
            int j = 0;

            //���������Ļ
            Rectangle rect = new Rectangle(30, 360 + offsetY, 560 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
            g.DrawImage(mainForm.image, rect.X, rect.Y, rect.Width, rect.Height);

            //ȷ���滭��ʼλ��
            int start = (int)((2780 - index * 75) / 10);

            //����
            j = DrawMyHearts(g, currentPoker, j, start);
            //��ɫ֮��ӿ�϶
            j++;


            //����
            j = DrawMyPeachs(g, currentPoker, j, start);
            //��ɫ֮��ӿ�϶
            j++;


            //����
            j = DrawMyDiamonds(g, currentPoker, j, start);
            //��ɫ֮��ӿ�϶
            j++;


            //÷��
            j = DrawMyClubs(g, currentPoker, j, start);
            //��ɫ֮��ӿ�϶
            j++;

            //Rank(�ݲ���������Rank)
            j = DrawHeartsRank(g, currentPoker, j, start);
            j = DrawPeachsRank(g, currentPoker, j, start);
            j = DrawClubsRank(g, currentPoker, j, start);
            j = DrawDiamondsRank(g, currentPoker, j, start);

            //С��
            j = DrawSmallJack(g, currentPoker, j, start);
            //����
            j = DrawBigJack(g, currentPoker, j, start);


        }

        //���Լ�����õ���,һ���������ƺ����,�ͳ�һ���ƺ����
        /// <summary>
        /// �ڳ���ײ������Ѿ�����õ���.
        /// ��������»�ʹ�����������
        /// 1.�����׼������ʱ
        /// 2.����һ����,��Ҫ�ػ��ײ�
        /// </summary>
        /// <param name="currentPoker"></param>
        internal void DrawMySortedCards(CurrentPoker currentPoker, int index)
        {

            //����ʱ�������
            //��������ʱ������¼�����е��Ƶ�λ�á���С���Ƿ񱻵��
            mainForm.myCardsLocation = new ArrayList();
            mainForm.myCardsNumber = new ArrayList();
            mainForm.myCardIsReady = new ArrayList();


            Graphics g = Graphics.FromImage(mainForm.bmp);

            //���������Ļ
            Rectangle rect = new Rectangle(30, 355 + offsetY, 600 * scaleDividend / scaleDivisor, 116 * scaleDividend / scaleDivisor);
            g.DrawImage(mainForm.image, rect.X, rect.Y, rect.Width, rect.Height);

            //�����ʼλ��
            int start = (int)((2780 - index * 75) / 10);


            //��¼ÿ���Ƶ�Xֵ
            int j = 0;
            //��ʱ���������������ж��Ƿ�ĳ��ɫȱʧ
            int k = 0;
            if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Heart)//����
            {
                j = DrawMyPeachs(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyDiamonds(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyClubs(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyHearts(g, currentPoker, j, start);

                j = DrawPeachsRank(g, currentPoker, j, start);
                j = DrawDiamondsRank(g, currentPoker, j, start);
                j = DrawClubsRank(g, currentPoker, j, start);
                j = DrawHeartsRank(g, currentPoker, j, start);
            }
            else if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Spade) //����
            {

                j = DrawMyDiamonds(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyClubs(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyHearts(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyPeachs(g, currentPoker, j, start);


                j = DrawDiamondsRank(g, currentPoker, j, start);
                j = DrawClubsRank(g, currentPoker, j, start);
                j = DrawHeartsRank(g, currentPoker, j, start);
                j = DrawPeachsRank(g, currentPoker, j, start);
            }
            else if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Diamond)  //��Ƭ
            {

                j = DrawMyClubs(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyHearts(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyPeachs(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyDiamonds(g, currentPoker, j, start);


                j = DrawClubsRank(g, currentPoker, j, start);
                j = DrawHeartsRank(g, currentPoker, j, start);
                j = DrawPeachsRank(g, currentPoker, j, start);
                j = DrawDiamondsRank(g, currentPoker, j, start);//����
            }
            else if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Club)
            {

                j = DrawMyHearts(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyPeachs(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyDiamonds(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyClubs(g, currentPoker, j, start);


                j = DrawHeartsRank(g, currentPoker, j, start);
                j = DrawPeachsRank(g, currentPoker, j, start);
                j = DrawDiamondsRank(g, currentPoker, j, start);
                j = DrawClubsRank(g, currentPoker, j, start);//÷��
            }
            else if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Joker || mainForm.ThisPlayer.CurrentHandState.Trump == Suit.None)
            {
                j = DrawMyHearts(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyPeachs(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyDiamonds(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyClubs(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);

                j = DrawHeartsRank(g, currentPoker, j, start);
                j = DrawPeachsRank(g, currentPoker, j, start);
                j = DrawDiamondsRank(g, currentPoker, j, start);
                j = DrawClubsRank(g, currentPoker, j, start);
            }


            //С��
            j = DrawSmallJack(g, currentPoker, j, start);

            //����
            j = DrawBigJack(g, currentPoker, j, start);

            mainForm.Refresh();
            g.Dispose();
        }

        private static void IsSuitLost(ref int j, ref int k)
        {
            if ((j - k) <= 1)
            {
                j--;
            }
            k = j;
        }

        /// <summary>
        /// �ػ������е���.
        /// ���������˵��������һ�֮����л��ơ�
        /// </summary>
        /// <param name="currentPoker">��ǰ�����е���</param>
        /// <param name="index">�Ƶ�����</param>
        internal void DrawMyPlayingCards(CurrentPoker currentPoker)
        {
            int index = currentPoker.Count;


            mainForm.cardsOrderNumber = 0;

            Graphics g = Graphics.FromImage(mainForm.bmp);

            //���������Ļ
            Rectangle rect = new Rectangle(30, 355 + offsetY, 600 * scaleDividend / scaleDivisor, 116 * scaleDividend / scaleDivisor);
            g.DrawImage(mainForm.image, rect.X, rect.Y, rect.Width, rect.Height);
            DrawScoreImageAndCards();

            int start = (int)((2780 - index * 75) / 10);

            //Rank(��������Rank)
            //��¼ÿ���Ƶ�Xֵ
            int j = 0;
            //��ʱ���������������ж��Ƿ�ĳ��ɫȱʧ
            int k = 0;

            if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Heart)
            {
                j = DrawMyPeachs2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyDiamonds2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyClubs2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyHearts2(g, currentPoker, j, start);

                j = DrawPeachsRank2(g, currentPoker, j, start);
                j = DrawDiamondsRank2(g, currentPoker, j, start);
                j = DrawClubsRank2(g, currentPoker, j, start);
                j = DrawHeartsRank2(g, currentPoker, j, start);//����
            }
            else if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Spade)
            {

                j = DrawMyDiamonds2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyClubs2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyHearts2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyPeachs2(g, currentPoker, j, start);

                j = DrawDiamondsRank2(g, currentPoker, j, start);
                j = DrawClubsRank2(g, currentPoker, j, start);
                j = DrawHeartsRank2(g, currentPoker, j, start);
                j = DrawPeachsRank2(g, currentPoker, j, start);//����
            }
            else if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Diamond)
            {

                j = DrawMyClubs2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyHearts2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyPeachs2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyDiamonds2(g, currentPoker, j, start);

                j = DrawClubsRank2(g, currentPoker, j, start);
                j = DrawHeartsRank2(g, currentPoker, j, start);
                j = DrawPeachsRank2(g, currentPoker, j, start);
                j = DrawDiamondsRank2(g, currentPoker, j, start);//����
            }
            else if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Club)
            {

                j = DrawMyHearts2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyPeachs2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyDiamonds2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyClubs2(g, currentPoker, j, start);

                j = DrawHeartsRank2(g, currentPoker, j, start);
                j = DrawPeachsRank2(g, currentPoker, j, start);
                j = DrawDiamondsRank2(g, currentPoker, j, start);
                j = DrawClubsRank2(g, currentPoker, j, start);//÷��
            }
            else if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Joker)
            {
                j = DrawMyHearts2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyPeachs2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyDiamonds2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);
                j = DrawMyClubs2(g, currentPoker, j, start) + 1;
                IsSuitLost(ref j, ref k);

                j = DrawHeartsRank2(g, currentPoker, j, start);
                j = DrawPeachsRank2(g, currentPoker, j, start);
                j = DrawDiamondsRank2(g, currentPoker, j, start);
                j = DrawClubsRank2(g, currentPoker, j, start);
            }

            //С��
            j = DrawBlackJoker2(g, currentPoker, j, start);

            //����
            j = DrawRedJoker2(g, currentPoker, j, start);


            //�жϵ�ǰ�ĳ������Ƿ���Ч,�����Ч����С��
            if (mainForm.SelectedCards.Count > 0)
            {
                var selectedCardsValidationResult = TractorRules.IsValid(mainForm.ThisPlayer.CurrentTrickState,
                                                                         mainForm.SelectedCards,
                                                                         mainForm.ThisPlayer.CurrentPoker);

                if ((mainForm.ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing
                     && mainForm.ThisPlayer.CurrentTrickState.NextPlayer() == mainForm.ThisPlayer.PlayerId)
                    &&
                    (selectedCardsValidationResult.ResultType == ShowingCardsValidationResultType.Valid ||
                     selectedCardsValidationResult.ResultType == ShowingCardsValidationResultType.TryToDump))
                {
                    this.mainForm.btnPig.Visible = true;
                }
                else if ((mainForm.ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing
                 && mainForm.ThisPlayer.CurrentTrickState.NextPlayer() == mainForm.ThisPlayer.PlayerId))
                {
                    this.mainForm.btnPig.Visible = false;
                }    

            }
            else if ((mainForm.ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.Playing
             && mainForm.ThisPlayer.CurrentTrickState.NextPlayer() == mainForm.ThisPlayer.PlayerId))
            {
                this.mainForm.btnPig.Visible = false;
            }    


            My8CardsIsReady(g);

            mainForm.Refresh();
            g.Dispose();
        }

        private void My8CardsIsReady(Graphics g)
        {
            if (mainForm.ThisPlayer.isObserver) return;
            //������ҿ���
            if (mainForm.ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DiscardingLast8Cards && mainForm.ThisPlayer.CurrentHandState.Last8Holder == mainForm.ThisPlayer.PlayerId)
            {
                int total = 0;
                for (int i = 0; i < mainForm.myCardIsReady.Count; i++)
                {
                    if ((bool)mainForm.myCardIsReady[i])
                    {
                        total++;
                    }
                }
                if (total == 8)
                {
                    this.mainForm.btnPig.Visible = true;
                }
                else
                {
                    this.mainForm.btnPig.Visible = false;
                }
            }
        }


        /// <summary>
        /// ����Ļ��������ҳ�����
        /// </summary>
        /// <param name="readys">�ҳ����Ƶ��б�</param>
        internal void DrawMySendedCardsAction(ArrayList readys)
        {
            int start = 285;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), start + offsetCenterHalf, 244 + offsetCenter, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                start += 12 * scaleDividend / scaleDivisor;
            }
            g.Dispose();


        }

        /// <summary>
        /// ����Ļ��������ҳ�����
        /// </summary>
        /// <param name="cards">the card numbers of showed cards</param>
        internal void DrawMySendedCardsAction(List<int> cards)
        {
            int start = 285;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            foreach (var card in cards)
            {
                DrawMyImage(g, getPokerImageByNumber(card), start + offsetCenterHalf, 244 + offsetCenter, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                start += 12 * scaleDividend / scaleDivisor;
            }

            g.Dispose();
        }

        /// <summary>
        /// ���Լҵ���
        /// </summary>
        /// <param name="readys"></param>
        internal void DrawFriendUserSendedCardsAction(ArrayList readys)
        {
            int start = 285;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), start + offsetCenterHalf, 130, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                start += 12 * scaleDividend / scaleDivisor;
            }

            g.Dispose();
        }



        /// <summary>
        /// ���ϼ�Ӧ�ó�����
        /// </summary>
        /// <param name="readys"></param>
        internal void DrawPreviousUserSendedCardsAction(ArrayList readys)
        {
            int start = 245;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), start + i * 12 * scaleDividend / scaleDivisor, 187 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
            }

            g.Dispose();
        }



        /// <summary>
        /// ���¼�Ӧ�ó�����
        /// </summary>
        /// <param name="readys"></param>
        internal void DrawNextUserSendedCardsAction(ArrayList readys)
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), 326 + offsetCenter + i * 12 * scaleDividend / scaleDivisor, 187 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
            }

            g.Dispose();
        }

        // sizeLevel: 0-half, 1-full
        public void DrawOverridingFlag(int position, int winType, int sizeLevel)
        {
            HideOverridingLabels();
            this.setOverridingLabel(position, sizeLevel);
            this.overridingFlagLabels[position - 1].Show();
            this.overridingFlagLabels[position - 1].BackgroundImage = this.overridingFlagPictures[winType];
        }

        private void HideOverridingLabels()
        {
            for (int i = 0; i < this.overridingFlagLabels.Length; i++)
            {
                this.overridingFlagLabels[i].Hide();
            }
        }

        #endregion // �ڸ�������»��Լ�����

        #region // ����������軭����������


        /// <summary>
        /// ���Լҵ���
        /// </summary>
        /// <param name="readys"></param>
        internal void DrawFriendUserSendedCardsActionAllHandCards(ArrayList readys)
        {
            int start = 90;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), start, 130, 71 * scaleDividend / scaleDivisor * 2 / 3, 96 * scaleDividend / scaleDivisor * 2 / 3);
                start += 12 * scaleDividend / scaleDivisor * 2 / 3;
            }

            g.Dispose();
        }



        /// <summary>
        /// ���ϼ�Ӧ�ó�����
        /// </summary>
        /// <param name="readys"></param>
        internal void DrawPreviousUserSendedCardsActionAllHandCards(ArrayList readys)
        {
            int start = 90;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), start + i * 12 * scaleDividend / scaleDivisor * 2 / 3, 260, 71 * scaleDividend / scaleDivisor * 2 / 3, 96 * scaleDividend / scaleDivisor * 2 / 3);
            }

            g.Dispose();
        }



        /// <summary>
        /// ���¼�Ӧ�ó�����
        /// </summary>
        /// <param name="readys"></param>
        internal void DrawNextUserSendedCardsActionAllHandCards(ArrayList readys)
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), 150 + offsetCenter + i * 12 * scaleDividend / scaleDivisor * 2 / 3, 496, 71 * scaleDividend / scaleDivisor * 2 / 3, 96 * scaleDividend / scaleDivisor * 2 / 3);
            }

            g.Dispose();
        }
        #endregion


        #region ���Լ�������(���ֻ�ɫ�����ֻ�ɫRank����С��)
        private int DrawBigJack(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            j = DrawMyOneOrTwoCards(g, currentPoker.RedJoker, 53, j, start, false);
            return j;
        }


        private int DrawSmallJack(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            j = DrawMyOneOrTwoCards(g, currentPoker.BlackJoker, 52, j, start, false);
            return j;
        }

        private int DrawDiamondsRank(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            j = DrawMyOneOrTwoCards(g, currentPoker.DiamondsRankTotal, mainForm.ThisPlayer.CurrentHandState.Rank + 26, j, start, false);
            return j;
        }

        private int DrawClubsRank(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            j = DrawMyOneOrTwoCards(g, currentPoker.ClubsRankTotal, mainForm.ThisPlayer.CurrentHandState.Rank + 39, j, start, false);
            return j;
        }

        private int DrawPeachsRank(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            j = DrawMyOneOrTwoCards(g, currentPoker.SpadesRankCount, mainForm.ThisPlayer.CurrentHandState.Rank + 13, j, start, false);
            return j;
        }

        private int DrawHeartsRank(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            j = DrawMyOneOrTwoCards(g, currentPoker.HeartsRankTotal, mainForm.ThisPlayer.CurrentHandState.Rank, j, start, mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Joker || mainForm.ThisPlayer.CurrentHandState.Trump == Suit.None);
            return j;
        }

        private int DrawMyClubs(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            for (int i = 0; i < 13; i++)
            {
                j = DrawMyOneOrTwoCards(g, currentPoker.ClubsNoRank[i], i + 39, j, start, i == 0);
            }
            return j;
        }

        private int DrawMyDiamonds(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            for (int i = 0; i < 13; i++)
            {
                j = DrawMyOneOrTwoCards(g, currentPoker.DiamondsNoRank[i], i + 26, j, start, i == 0);
            }
            return j;
        }

        private int DrawMyPeachs(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            for (int i = 0; i < 13; i++)
            {
                j = DrawMyOneOrTwoCards(g, currentPoker.PeachsNoRank[i], i + 13, j, start, i == 0);

            }
            return j;
        }

        private int DrawMyHearts(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            for (int i = 0; i < 13; i++)
            {
                j = DrawMyOneOrTwoCards(g, currentPoker.HeartsNoRank[i], i, j, start, i == 0);
            }
            return j;
        }

        //��������
        private int DrawMyOneOrTwoCards(Graphics g, int count, int number, int j, int start, bool resetSuitSequence)
        {
            if (resetSuitSequence) suitSequence = 1;

            //�������������������Ҫ��������������һ��
            bool b = (number == 52) || (number == 53);
            b = b & (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Joker);
            if (mainForm.ThisPlayer.CurrentHandState.Trump != Suit.Joker)
            {
                if (number ==((int) mainForm.ThisPlayer.CurrentHandState.Trump - 1)*13 +mainForm.ThisPlayer.CurrentHandState.Rank)
                {
                    b = true;
                }

            }
            b = b && (mainForm.ThisPlayer.CurrentHandState.CurrentHandStep == HandStep.DistributingCards);

            if (count == 1)
            {
                SetCardsInformation(start + j * 12 * scaleDividend / scaleDivisor, number, false);
                if (mainForm.ThisPlayer.PlayerId == mainForm.ThisPlayer.CurrentHandState.TrumpMaker && b)
                {
                    if (number == 52 || number == 53)
                    {
                        g.DrawImage(getPokerImageByNumber(number), start + j * 12 * scaleDividend / scaleDivisor, 375 + offsetY, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor); //����������������
                    }
                    else
                    {
                        g.DrawImage(getPokerImageByNumber(number), start + j * 12 * scaleDividend / scaleDivisor, 360 + offsetY, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                    }
                }
                else
                {
                    g.DrawImage(getPokerImageByNumber(number), start + j * 12 * scaleDividend / scaleDivisor, 375 + offsetY, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                //���Ƶ�����
                if (this.mainForm.showSuitSeq) g.DrawString(suitSequence.ToString(), suitSequenceFont, Brushes.Gray, start + j * 12 * scaleDividend / scaleDivisor + 1, 375 + offsetY + 54 * scaleDividend / scaleDivisor);
                
                j++;
                suitSequence++;
            }
            else if (count == 2)
            {
                SetCardsInformation(start + j * 12 * scaleDividend / scaleDivisor, number, false);

                if (mainForm.ThisPlayer.PlayerId == mainForm.ThisPlayer.CurrentHandState.TrumpMaker && b &&
                    mainForm.ThisPlayer.CurrentHandState.TrumpExposingPoker >= TrumpExposingPoker.SingleRank)
                {
                    g.DrawImage(getPokerImageByNumber(number), start + j * 12 * scaleDividend / scaleDivisor, 360 + offsetY, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                else
                {
                    g.DrawImage(getPokerImageByNumber(number), start + j * 12 * scaleDividend / scaleDivisor, 375 + offsetY, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                if (this.mainForm.showSuitSeq) g.DrawString(suitSequence.ToString(), suitSequenceFont, Brushes.Gray, start + j * 12 * scaleDividend / scaleDivisor + 1, 375 + offsetY + 54 * scaleDividend / scaleDivisor);

                j++;
                suitSequence++;
                SetCardsInformation(start + j * 12 * scaleDividend / scaleDivisor, number, false);
                if (mainForm.ThisPlayer.PlayerId == mainForm.ThisPlayer.CurrentHandState.TrumpMaker && b &&
                    mainForm.ThisPlayer.CurrentHandState.TrumpExposingPoker >= TrumpExposingPoker.PairRank)
                {
                    g.DrawImage(getPokerImageByNumber(number), start + j * 12 * scaleDividend / scaleDivisor, 360 + offsetY, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                else
                {
                    g.DrawImage(getPokerImageByNumber(number), start + j * 12 * scaleDividend / scaleDivisor, 375 + offsetY, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                if (this.mainForm.showSuitSeq) g.DrawString(suitSequence.ToString(), suitSequenceFont, Brushes.Gray, start + j * 12 * scaleDividend / scaleDivisor + 1, 375 + offsetY + 54 * scaleDividend / scaleDivisor);

                j++;
                suitSequence++;
            }
            return j;
        }


        #endregion // ���Լ�������

        #region ���Լ�����ķ���
        private int DrawRedJoker2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            if (currentPoker.RedJoker == 1)
            {
                j = DrawMyOneOrTwoCards2(g, j, 53, start, 355, 71, 96) + 1;
            }
            else if (currentPoker.RedJoker == 2)
            {
                j = DrawMyOneOrTwoCards2(g, j, 53, start, 355, 71, 96) + 1;
                j = DrawMyOneOrTwoCards2(g, j, 53, start, 355, 71, 96) + 1;
            }
            return j;
        }

        private int DrawBlackJoker2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            if (currentPoker.BlackJoker == 1)
            {
                j = DrawMyOneOrTwoCards2(g, j, 52, start, 355, 71, 96) + 1;
            }
            else if (currentPoker.BlackJoker == 2)
            {
                j = DrawMyOneOrTwoCards2(g, j, 52, start, 355, 71, 96) + 1;
                j = DrawMyOneOrTwoCards2(g, j, 52, start, 355, 71, 96) + 1;
            }
            return j;
        }

        private int DrawDiamondsRank2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            if (currentPoker.DiamondsRankTotal == 1)
            {
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank + 26, start, 355, 71, 96) + 1;
            }
            else if (currentPoker.DiamondsRankTotal == 2)
            {
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank + 26, start, 355, 71, 96) + 1;
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank + 26, start, 355, 71, 96) + 1;
            }
            return j;
        }

        private int DrawClubsRank2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            if (currentPoker.ClubsRankTotal == 1)
            {
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank + 39, start, 355, 71, 96) + 1;
            }
            else if (currentPoker.ClubsRankTotal == 2)
            {
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank + 39, start, 355, 71, 96) + 1;
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank + 39, start, 355, 71, 96) + 1;
            }
            return j;
        }

        private int DrawPeachsRank2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            if (currentPoker.SpadesRankCount == 1)
            {
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank + 13, start, 355, 71, 96) + 1;
            }
            else if (currentPoker.SpadesRankCount == 2)
            {
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank + 13, start, 355, 71, 96) + 1;
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank + 13, start, 355, 71, 96) + 1;
            }
            return j;
        }

        private int DrawHeartsRank2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            if (mainForm.ThisPlayer.CurrentHandState.Trump == Suit.Joker || mainForm.ThisPlayer.CurrentHandState.Trump == Suit.None) suitSequence = 1;
            if (currentPoker.HeartsRankTotal == 1)
            {
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank, start, 355, 71, 96) + 1;
            }
            else if (currentPoker.HeartsRankTotal == 2)
            {
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank, start, 355, 71, 96) + 1;
                j = DrawMyOneOrTwoCards2(g, j, mainForm.ThisPlayer.CurrentHandState.Rank, start, 355, 71, 96) + 1;
            }
            return j;
        }

        private int DrawMyClubs2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            suitSequence = 1;
            for (int i = 0; i < 13; i++)
            {
                if (currentPoker.ClubsNoRank[i] == 1)
                {
                    j = DrawMyOneOrTwoCards2(g, j, i + 39, start, 355, 71, 96) + 1;
                }
                else if (currentPoker.ClubsNoRank[i] == 2)
                {
                    j = DrawMyOneOrTwoCards2(g, j, i + 39, start, 355, 71, 96) + 1;
                    j = DrawMyOneOrTwoCards2(g, j, i + 39, start, 355, 71, 96) + 1;
                }
            }
            return j;
        }

        private int DrawMyDiamonds2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            suitSequence = 1;
            for (int i = 0; i < 13; i++)
            {
                if (currentPoker.DiamondsNoRank[i] == 1)
                {
                    j = DrawMyOneOrTwoCards2(g, j, i + 26, start, 355, 71, 96) + 1;
                }
                else if (currentPoker.DiamondsNoRank[i] == 2)
                {
                    j = DrawMyOneOrTwoCards2(g, j, i + 26, start, 355, 71, 96) + 1;
                    j = DrawMyOneOrTwoCards2(g, j, i + 26, start, 355, 71, 96) + 1;
                }
            }
            return j;
        }

        private int DrawMyPeachs2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            suitSequence = 1;
            for (int i = 0; i < 13; i++)
            {
                if (currentPoker.PeachsNoRank[i] == 1)
                {
                    j = DrawMyOneOrTwoCards2(g, j, i + 13, start, 355, 71, 96) + 1;
                }
                else if (currentPoker.PeachsNoRank[i] == 2)
                {
                    j = DrawMyOneOrTwoCards2(g, j, i + 13, start, 355, 71, 96) + 1;
                    j = DrawMyOneOrTwoCards2(g, j, i + 13, start, 355, 71, 96) + 1;
                }
            }
            return j;
        }

        private int DrawMyHearts2(Graphics g, CurrentPoker currentPoker, int j, int start)
        {
            suitSequence = 1;
            for (int i = 0; i < 13; i++)
            {
                if (currentPoker.HeartsNoRank[i] == 1)
                {
                    j = DrawMyOneOrTwoCards2(g, j, i, start, 355, 71, 96) + 1;
                }
                else if (currentPoker.HeartsNoRank[i] == 2)
                {
                    j = DrawMyOneOrTwoCards2(g, j, i, start, 355, 71, 96) + 1;
                    j = DrawMyOneOrTwoCards2(g, j, i, start, 355, 71, 96) + 1;
                }
            }
            return j;
        }

        //��������
        private int DrawMyOneOrTwoCards2(Graphics g, int j, int number, int start, int y, int width, int height)
        {
            if ((bool)mainForm.myCardIsReady[mainForm.cardsOrderNumber])
            {
                g.DrawImage(getPokerImageByNumber(number), start + j * 12 * scaleDividend / scaleDivisor, y + offsetY, width * scaleDividend / scaleDivisor, height * scaleDividend / scaleDivisor);
            }
            else
            {
                g.DrawImage(getPokerImageByNumber(number), start + j * 12 * scaleDividend / scaleDivisor, y + offsetY + 20, width * scaleDividend / scaleDivisor, height * scaleDividend / scaleDivisor);
            }
            //���Ƶ�����
            if (this.mainForm.showSuitSeq) g.DrawString(suitSequence.ToString(), suitSequenceFont, Brushes.Gray, start + j * 12 * scaleDividend / scaleDivisor + 1, 375 + offsetY + 54 * scaleDividend / scaleDivisor);
            suitSequence++;

            mainForm.cardsOrderNumber++;
            return j;
        }
        #endregion // ���ƵĻ��Լ�����ķ���

        #region ���Ƹ��ҳ����ƣ�������������֪ͨ��һ��
        /// <summary>
        /// ���Լ�������
        /// </summary>
        internal void DrawMyFinishSendedCards()
        {
            //�����뻭���������
            DrawMySendedCardsAction(mainForm.currentSendCards[0]);



            //�ػ��Լ����е���
            if (mainForm.currentPokers[0].Count > 0)
            {
                DrawMySortedCards(mainForm.currentPokers[0], mainForm.currentPokers[0].Count);
            }
            else //�����²��ռ�
            {
                Rectangle rect = new Rectangle(30, 355 + offsetY, 560 * scaleDividend / scaleDivisor, 116 * scaleDividend / scaleDivisor);
                Graphics g = Graphics.FromImage(mainForm.bmp);
                g.DrawImage(mainForm.image, rect.X, rect.Y, rect.Width, rect.Height);
                g.Dispose();
            }

            mainForm.Refresh();

        }

        /// <summary>
        /// �¼ҳ���
        /// </summary>
        internal void DrawNextUserSendedCards()
        {
            var latestCards = mainForm.ThisPlayer.CurrentTrickState.ShowedCards[mainForm.ThisPlayer.CurrentTrickState.LatestPlayerShowedCard()];
            DrawNextUserSendedCardsAction(new ArrayList(latestCards));

            // DrawScoreImage(mainForm.Scores);
            mainForm.Refresh();

        }

        /// <summary>
        /// �Լҳ���
        /// </summary>
        internal void DrawFriendUserSendedCards()
        {

            var latestCards = mainForm.ThisPlayer.CurrentTrickState.ShowedCards[mainForm.ThisPlayer.CurrentTrickState.LatestPlayerShowedCard()];
            DrawFriendUserSendedCardsAction(new ArrayList(latestCards));

            //DrawScoreImage(mainForm.Scores);
            mainForm.Refresh();

        }

        /// <summary>
        /// �ϼҳ���
        /// </summary>
        internal void DrawPreviousUserSendedCards()
        {
            var latestCards = mainForm.ThisPlayer.CurrentTrickState.ShowedCards[mainForm.ThisPlayer.CurrentTrickState.LatestPlayerShowedCard()];
            DrawPreviousUserSendedCardsAction(new ArrayList(latestCards));

            //DrawScoreImage(mainForm.Scores);
            mainForm.Refresh();

        }

        internal void DrawScoreImageAndCards()
        {
            //���÷�ͼ��
            int scores = mainForm.ThisPlayer.CurrentHandState.Score;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            Bitmap bmp = global::Duan.Xiugang.Tractor.Properties.Resources.scores;
            Font font = new Font("����", 12, FontStyle.Bold);

            Rectangle rect = new Rectangle(20, 128, 56, 56);
            g.DrawImage(mainForm.image, rect, rect, GraphicsUnit.Pixel);
            g.DrawImage(bmp, rect);
            int x = 20 + 16;
            if (scores.ToString().Length == 2)
            {
                x -= 4;
            }
            else if (scores.ToString().Length == 3)
            {
                x -= 8;
            }
            g.DrawString(scores + "", font, Brushes.White, x, 138);

            //���÷��ƣ����ڵ÷�ͼ����±�
            int wid = 71 * scaleDividend / scaleDivisor * 2 / 3;
            int hei = 96 * scaleDividend / scaleDivisor * 2 / 3;
            int cardsX = 20;

            int cardsY = 130 + 50;
            for (int i = 0; i < mainForm.ThisPlayer.CurrentHandState.ScoreCards.Count; i++)
            {
                //����2���������һ��
                g.DrawImage(getPokerImageByNumber(mainForm.ThisPlayer.CurrentHandState.ScoreCards[i]), cardsX + i * (2 + 12 * scaleDividend / scaleDivisor * 2 / 3), cardsY, wid, hei);
            }

            g.Dispose();
        }

        internal void DrawCountDown(bool shouldDraw)
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);

            int x = 300, y = 500, size=70;
            Rectangle rectsrc = new Rectangle(0, 0, size, size);
            Rectangle rect = new Rectangle(x, y, size, size);
            g.DrawImage(mainForm.image, rect, rectsrc, GraphicsUnit.Pixel);

            if (shouldDraw)
            {
                Font font = new Font("����", 50, FontStyle.Bold);
                int countDown = mainForm.timerCountDown;
                g.DrawString(countDown.ToString(), font, Brushes.Yellow, x, y);
            }
            mainForm.Refresh();
            g.Dispose();
        }

        internal void DrawMessages(string[] msgs)
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);

            int x = 300, y = 496, width = 300, height = (msgs.Length) * 21 + 8;
            y = y - height;
            Rectangle rectsrc = new Rectangle(0, 0, width, height);
            Rectangle rect = new Rectangle(x, y, width, height);
            g.DrawImage(mainForm.image, rect, rectsrc, GraphicsUnit.Pixel);

            if (msgs != null)
            {
                y += 4;
                int fontSize = 16;
                Font font = new Font("����", fontSize, FontStyle.Bold);
                for (int i = 0; i < msgs.Length; i++)
                {
                    g.DrawString(msgs[i], font, Brushes.Yellow, x, y + i * (fontSize + 5));
                }
            }
            mainForm.Refresh();
            g.Dispose();
        }

        internal void DrawFinishedScoreImage()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);

            Pen pen = new Pen(Color.White, 2);
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.White)), 90, 124, 463 + offsetCenter, 244 * scaleDividend / scaleDivisor);
            g.DrawRectangle(pen, 90, 124, 463 + offsetCenter, 244 * scaleDividend / scaleDivisor);

            //������,��169��ʼ��
            for (int i = 0; i < 8; i++)
            {
                g.DrawImage(getPokerImageByNumber((int)mainForm.ThisPlayer.CurrentHandState.DiscardedCards[i]), 230 + i * 14 * scaleDividend / scaleDivisor, 130, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
            }

            //��СѾ
            g.DrawImage(global::Duan.Xiugang.Tractor.Properties.Resources.Logo, 160 + offsetCenterHalf, 237 + offsetCenterHalf, 110, 112);

            //���÷�
            Font font = new Font("����", 16, FontStyle.Bold);
            g.DrawString("�ܵ÷� " + mainForm.ThisPlayer.CurrentHandState.Score, font, Brushes.Blue, 310 + offsetCenterHalf, 286 + offsetCenterHalf);

            g.Dispose();

        }

        //��С��2/3�����ס֮ǰ������
        internal void DrawDiscardedCards()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);

            //������
            for (int i = 0; i < 8; i++)
            {
                g.DrawImage(getPokerImageByNumber((int)mainForm.ThisPlayer.CurrentHandState.DiscardedCards[i]), 100 + i * 12, 30, 70, 89);
            }

            g.Dispose();
            mainForm.Refresh();
        }

        //���÷���,��С��2/3�����ס֮ǰ������
        internal void DrawScoreCards()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);

            //���ڵ÷�ͼ������
            int wid = 71 * scaleDividend / scaleDivisor * 2 / 3;
            int hei = 96 * scaleDividend / scaleDivisor * 2 / 3;
            //����2���������һ��
            int x = offsetSideBar - wid - 5 - (mainForm.ThisPlayer.CurrentHandState.ScoreCards.Count - 1) * (2 + 12 * scaleDividend / scaleDivisor * 2 / 3);

            int y = 130 + 50;
            for (int i = 0; i < mainForm.ThisPlayer.CurrentHandState.ScoreCards.Count; i++)
            {
                g.DrawImage(getPokerImageByNumber(mainForm.ThisPlayer.CurrentHandState.ScoreCards[i]), x + i * (2 + 12 * scaleDividend / scaleDivisor * 2 / 3), y, wid, hei);
            }

            //alert if score cards does not match actual scores
            int points = 0;
            foreach (int card in mainForm.ThisPlayer.CurrentHandState.ScoreCards)
            {
                if (card % 13 == 3)
                    points += 5;
                else if (card % 13 == 8)
                    points += 10;
                else if (card % 13 == 11)
                    points += 10;
            }
            if (points + mainForm.ThisPlayer.CurrentHandState.ScoreAdjustment != mainForm.ThisPlayer.CurrentHandState.Score)
            {
                MessageBox.Show(string.Format("bug report: mismatch! score cards score: {0}, actual score: {1}", points, mainForm.ThisPlayer.CurrentHandState.Score));
            }

            g.Dispose();
            mainForm.Refresh();
        }

        //��Ҷ������ƣ������÷ֶ��٣��´θ�˭����
        internal void DrawFinishedSendedCards()
        {
            DrawCenterImage();
            DrawFinishedScoreImage();
            DrawScoreCards();
            mainForm.Refresh();
        }

        //����Ͷ��
        internal void DrawFinishedBySpecialEnding()
        {
            DrawCenterImage();
            mainForm.Refresh();
        }
        #endregion // ���Ƹ��ҳ����ƣ�������������֪ͨ��һ��

        #region ������һ�ָ����������ƣ���С��һ�룬�������½�
        /// <summary>
        /// �������ֵ���
        /// </summary>
        /// <param name="readys">�����ֵ��Ƶ��б�</param>
        internal void DrawMyLastSendedCardsAction(ArrayList readys)
        {
            int halfHeigh = 96 * scaleDividend / scaleDivisor / 2;
            int start = 285;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), start + offsetCenterHalf, 244 + offsetCenter + halfHeigh, 71 * scaleDividend / scaleDivisor / 2, 96 * scaleDividend / scaleDivisor / 2);
                start += 12 * scaleDividend / scaleDivisor;
            }
            g.Dispose();
        }

        /// <summary>
        /// ���Լ����ֵ���
        /// </summary>
        /// <param name="readys"></param>
        internal void DrawFriendUserLastSendedCardsAction(ArrayList readys)
        {
            int halfHeigh = 96 * scaleDividend / scaleDivisor / 2;
            int start = 285;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), start + offsetCenterHalf, 130 + halfHeigh, 71 * scaleDividend / scaleDivisor / 2, 96 * scaleDividend / scaleDivisor / 2);
                start += 12 * scaleDividend / scaleDivisor;
            }
            g.Dispose();
        }



        /// <summary>
        /// ���ϼ����ֵ���
        /// </summary>
        /// <param name="readys"></param>
        internal void DrawPreviousUserLastSendedCardsAction(ArrayList readys)
        {
            int halfHeigh = 96 * scaleDividend / scaleDivisor / 2;
            int start = 245;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), start, 187 + offsetCenterHalf + halfHeigh, 71 * scaleDividend / scaleDivisor / 2, 96 * scaleDividend / scaleDivisor / 2);
                start += 12 * scaleDividend / scaleDivisor;
            }
            g.Dispose();
        }



        /// <summary>
        /// ���¼����ֵ���
        /// </summary>
        /// <param name="readys"></param>
        internal void DrawNextUserLastSendedCardsAction(ArrayList readys)
        {
            int halfHeigh = 96 * scaleDividend / scaleDivisor / 2;
            int start = 326;
            Graphics g = Graphics.FromImage(mainForm.bmp);
            for (int i = 0; i < readys.Count; i++)
            {
                DrawMyImage(g, getPokerImageByNumber((int)readys[i]), start + offsetCenter, 187 + offsetCenterHalf + halfHeigh, 71 * scaleDividend / scaleDivisor / 2, 96 * scaleDividend / scaleDivisor / 2);
                start += 12 * scaleDividend / scaleDivisor;
            }
            g.Dispose();
        }
        #endregion // ������һ�ָ����������ƣ���С��һ�룬�������½�

        #region ����ʱ�ĸ�������

        //�����ƺŵõ���Ӧ���Ƶ�ͼƬ
        private Bitmap getPokerImageByNumber(int number)
        {
            Bitmap bitmap = null;

            if (mainForm.gameConfig.CardImageName.Length == 0) //����Ƕ��ͼ���ж�ȡ
            {
                bitmap = (Bitmap)mainForm.gameConfig.CardsResourceManager.GetObject("_" + number, Kuaff_Cards.Culture);
            }
            else
            {
                bitmap = mainForm.cardsImages[number]; //���Զ����ͼƬ�ж�ȡ
            }

            return bitmap;
        }

        /// <summary>
        /// �ػ����򱳾�
        /// </summary>
        /// <param name="g">������ͼ���Graphics</param>
        internal void DrawBackground(Graphics g)
        {
            this.mainForm.btnPig.Hide();
            this.HideOverridingLabels();
            //Bitmap image = global::Kuaff.Tractor.Properties.Resources.Backgroud;
            g.DrawImage(mainForm.image, 0, 0, mainForm.ClientRectangle.Width, mainForm.ClientRectangle.Height);
        }

        //�����ƶ��������м�֡�������ú���ȥ��
        private void DrawAnimatedCard(Bitmap card, int x, int y, int width, int height)
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);
            Bitmap backup = mainForm.bmp.Clone(new Rectangle(x, y, width, height), PixelFormat.DontCare);
            g.DrawImage(card, x, y, width, height);
            mainForm.Refresh();
            g.DrawImage(backup, x, y, width, height);
            g.Dispose();
        }

        //��ͼ�ķ���
        private void DrawMyImage(Graphics g, Bitmap bmp, int x, int y, int width, int height)
        {
            g.DrawImage(bmp, x, y, width, height);
        }

        //���õ�ǰ���Ƶ���Ϣ
        private void SetCardsInformation(int x, int number, bool ready)
        {
            mainForm.myCardsLocation.Add(x);
            mainForm.myCardsNumber.Add(number);
            mainForm.myCardIsReady.Add(ready);
        }
        #endregion // ����ʱ�ĸ�������

        public void DrawMyShowedCards()
        {
            //�����뻭���������
            DrawMySendedCardsAction(mainForm.ThisPlayer.CurrentTrickState.ShowedCards[mainForm.ThisPlayer.PlayerId]);

            mainForm.Refresh();
        }

        public void DrawMyHandCards()
        {
            //�ػ��Լ����е���
            if (mainForm.ThisPlayer.CurrentPoker.Count > 0)
                DrawMySortedCards(mainForm.ThisPlayer.CurrentPoker, mainForm.ThisPlayer.CurrentPoker.Count);
            else //���е��ƶ�������
            {
                Rectangle rect = new Rectangle(30, 355 + offsetY, 560 * scaleDividend / scaleDivisor, 116 * scaleDividend / scaleDivisor);
                Graphics g = Graphics.FromImage(mainForm.bmp);
                g.DrawImage(mainForm.image, rect.X, rect.Y, rect.Width, rect.Height);
                g.Dispose();
            }

            mainForm.Refresh();

        }

        //����ׯ��������Լ����ڵ�λ�ã���ׯ�һ�õ��ƵĶ���
        public void DrawDistributingLast8Cards(int position)
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);

            //��8�ŵ���
            for (int i = 0; i < 8; i++)
            {
                g.DrawImage(mainForm.gameConfig.BackImage, 200 + offsetCenterHalf + i * 2 * scaleDividend / scaleDivisor, 186 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
            }
            mainForm.Refresh();
            Thread.Sleep(100);

            //�ַ�
            for (int i = 1; i <= 8; i++)
            {
                Rectangle rect = new Rectangle(200 + offsetCenterHalf, 186 + offsetCenterHalf, 16 * scaleDividend / scaleDivisor + 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                g.DrawImage(mainForm.image, rect.X, rect.Y, rect.Width, rect.Height);
                for (int j = 0; j < 8 - i; j++)
                {
                    g.DrawImage(mainForm.gameConfig.BackImage, 200 + offsetCenterHalf + j * 2 * scaleDividend / scaleDivisor, 186 + offsetCenterHalf, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                }
                switch (position)
                {
                    case 2:
                        //�����ҵ�λ��
                        DrawAnimatedCard(mainForm.gameConfig.BackImage, 520 + offsetCenter, 220 + offsetCenterHalf - 12 * 4, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                        break;
                    case 3:
                        //���Լҵ�λ��
                        DrawAnimatedCard(mainForm.gameConfig.BackImage, 400 + offsetCenterHalf - 12 * 12, 60, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                        break;
                    case 4:
                        //�����ҵ�λ��
                        DrawAnimatedCard(mainForm.gameConfig.BackImage, 50, 160 + offsetCenterHalf + 12 * 4, 71 * scaleDividend / scaleDivisor, 96 * scaleDividend / scaleDivisor);
                        break;
                }
                mainForm.Refresh();
                Thread.Sleep(100);
            }
            g.Dispose();
        }

        internal void Starter()
        {
            Graphics g = Graphics.FromImage(mainForm.bmp);

            int starterPostion = 0;
            if (!string.IsNullOrEmpty(mainForm.ThisPlayer.CurrentHandState.Starter))
            {
                starterPostion = mainForm.PlayerPosition[mainForm.ThisPlayer.CurrentHandState.Starter];
            }
                                   
            Rectangle destRect;
            Rectangle srcRect;

            //south
            if (starterPostion == 1)
            {

                destRect = new Rectangle(offsetSideBar + 8, 45, 20, 20);
                srcRect = new Rectangle(120, 0, 20, 20);

                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                destRect = new Rectangle(offsetSideBar + 8, 45, 20, 20);
                srcRect = new Rectangle(40, 0, 20, 20);
                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }

            //north
            if (starterPostion == 3)
            {
                destRect = new Rectangle(offsetSideBar + 40, 45, 20, 20);
                srcRect = new Rectangle(140, 0, 20, 20);

                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                destRect = new Rectangle(offsetSideBar + 40, 45, 20, 20);
                srcRect = new Rectangle(60, 0, 20, 20);
                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }
            //west
            if (starterPostion == 4)
            {
                destRect = new Rectangle(30, 45, 20, 20);
                srcRect = new Rectangle(80, 0, 20, 20);

                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                destRect = new Rectangle(31, 45, 20, 20);
                srcRect = new Rectangle(0, 0, 20, 20);
                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }
            //east
            if (starterPostion == 2)
            {
                destRect = new Rectangle(60, 45, 20, 20);
                srcRect = new Rectangle(100, 0, 20, 20);

                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                destRect = new Rectangle(61, 45, 20, 20);
                srcRect = new Rectangle(20, 0, 20, 20);
                g.DrawImage(Properties.Resources.Master, destRect, srcRect, GraphicsUnit.Pixel);
            }

            mainForm.Refresh();
            g.Dispose();
        }
    }
}
