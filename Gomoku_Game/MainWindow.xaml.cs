using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;


namespace Gomoku_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        clsBanCo banco;
        
        public MainWindow()
        {
            InitializeComponent();
            //this.ConnectServer(); // Kết nối tới server
            gridChessBoard.Visibility = Visibility.Hidden; // Ẩn màn hình đánh cờ
            gridPlwCom.Visibility = Visibility.Hidden; // Ẩn màn hình sau khi chọn đánh với máy
            gridPlwPl.Visibility = Visibility.Hidden; // Ẩn màn hình sau khi chọn đánh với người
            banco = new clsBanCo(this, cv_BanCo, text_TTOCo,textCurPlay);
            banco.DrawChessboard();
            cv_BanCo.MouseMove += new System.Windows.Input.MouseEventHandler(banco.BanCo_MouseMove);
            cv_BanCo.MouseDown += new System.Windows.Input.MouseButtonEventHandler(banco.BanCo_MouseDown);
        }


        public void PlvsPl_Click(object sender, RoutedEventArgs e) // sự kiện khi click chọn đánh với người
        {
            gridMenu.Visibility = Visibility.Hidden; // Ẩn màn hình menu
            gridPlwPl.Visibility = Visibility.Visible; // Hiện màn hình sau khi chọn đánh với người
            banco._menu.whoPlayWith = Player.Human;
            banco.ResetAllBoard();
        }

        public void PlvsCom_Click(object sender, RoutedEventArgs e) /// sự kiện khi click chọn đánh với máy
        {
            gridMenu.Visibility = Visibility.Hidden; // Ẩn màn hình menu
            gridPlwCom.Visibility = Visibility.Visible; // Hiện màn hình sau khi chọn đánh với máy
            banco._menu.whoPlayWith = Player.Com;
            banco.ResetAllBoard();
            banco.NewGame();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e) // trở lại sau khi chọn đánh với người
        {
            gridPlwPl.Visibility = Visibility.Hidden; //// Ẩn màn hình sau khi chọn đánh với người
            gridMenu.Visibility = Visibility.Visible; // Hiện màn hình menu
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e) // sau khi nhấn play khi chịn đánh với người
        {
            gridPlwPl.Visibility = Visibility.Hidden; //// Ẩn màn hình sau khi chọn đánh với người
            gridChessBoard.Visibility = Visibility.Visible; // Hiện màn hình đánh cờ chính
            banco._menu.playerA = textPlayerA.Text;
            banco._menu.playerB = textPlayerB.Text;
            textCurChat.Text = textPlayerB.Text + " can Chat ...";
            textCurPlay.Text = "Turns play of " + textPlayerA.Text;
            banco._menu.curPlay = Player.Human; // Lượt chơi thuộc về người chơi thứ nhất
            banco._menu.curChat = Player.Com; // Lượt chat thuộc về người chơi thứ hai
            lbPlayerA.Content = textPlayerA.Text;
            lbPlayerB.Content = textPlayerB.Text;
            lbScoreA.Content = banco._menu.playerAScore.ToString();
            lbScoreB.Content = banco._menu.playerBScore.ToString();
        }

        private void btnBack1_Click(object sender, RoutedEventArgs e) // trở lại sau khi chọn đánh với máy
        {
            gridPlwCom.Visibility = Visibility.Hidden; //// Ẩn màn hình sau khi chọn đánh với người
            gridMenu.Visibility = Visibility.Visible; // Hiện màn hình menu
        }

        private void btnPlay1_Click(object sender, RoutedEventArgs e) // sau khi nhấn play khi chọn đánh với máy
        {
            gridPlwCom.Visibility = Visibility.Hidden; //// Ẩn màn hình sau khi chọn đánh với người
            gridChessBoard.Visibility = Visibility.Visible; // Hiện màn hình đánh cờ chính
            banco._menu.yourName = textYourName.Text;
            textCurChat.Text = "Computer can Chat ...";
            textCurPlay.Text = "Turns play of " + textYourName.Text;
            banco._menu.curPlay = Player.Human; // Lượt chơi thuộc về người chơi
            banco._menu.curChat = Player.Com; // Lượt chat thuộc về máy
            lbPlayerA.Content = textYourName.Text;
            lbPlayerB.Content = "Computer";
            lbScoreA.Content = banco._menu.playerAScore.ToString();
            lbScoreB.Content = banco._menu.playerBScore.ToString();
        }


        private void click_Send(object sender, RoutedEventArgs e)
        {
            if (banco._menu.whoPlayWith == Player.Human) // Nếu chơi với người
            {
                if (banco._menu.curChat == Player.Human) // nếu là người chơi thứ nhất
                {
                    text_ChatHistory.SelectionStart = 0;
                    text_ChatHistory.SelectionLength = 0;
                    text_ChatHistory.SelectedText = textPlayerA.Text + ":   (" + DateTime.Now.ToString("hh:mm:ss") + ")" + "\n" + text_WriteChat.Text + "\n" + "  ------------------------------------------" + "\n";
                    text_WriteChat.Clear();
                }
                else if (banco._menu.curChat == Player.Com) // Nếu là người chơi thứ 2
                {
                    text_ChatHistory.SelectionStart = 0;
                    text_ChatHistory.SelectionLength = 0;
                    text_ChatHistory.SelectedText = textPlayerB.Text + ":   (" + DateTime.Now.ToString("hh:mm:ss") + ")" + "\n" + text_WriteChat.Text + "\n" + "  ------------------------------------------" + "\n";
                    text_WriteChat.Clear();
                }
            }
            else if (banco._menu.whoPlayWith == Player.Com) // Nếu chơi với máy
            {
                if (banco._menu.curChat == Player.Human) // nếu là người chơi thứ nhất
                {
                    text_ChatHistory.SelectionStart = 0;
                    text_ChatHistory.SelectionLength = 0;
                    text_ChatHistory.SelectedText = textYourName.Text + ":   (" + DateTime.Now.ToString("hh:mm:ss") + ")" + "\n" + text_WriteChat.Text + "\n" + "  ------------------------------------------" + "\n";
                    text_WriteChat.Clear();
                }
                else if (banco._menu.curChat == Player.Com) // Nếu là máy
                {
                    text_ChatHistory.SelectionStart = 0;
                    text_ChatHistory.SelectionLength = 0;
                    text_ChatHistory.SelectedText = "Computer:  (" + DateTime.Now.ToString("hh:mm:ss") + ")" + "\n" + text_WriteChat.Text + "\n" + "  ------------------------------------------" + "\n";
                    text_WriteChat.Clear();
                }
            }
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            gridChessBoard.Visibility = Visibility.Hidden; // Ẩn màn hình đánh cờ 
            gridMenu.Visibility = Visibility.Visible; // Hiện màn hình menu
            text_ChatHistory.Clear();
            textPlayerA.Clear();
            textPlayerB.Clear();
            textYourName.Clear();
            text_TTOCo.Clear();
            textCurPlay.Clear();
            textCurChat.Clear();
            banco._menu.playerAScore = 0;
            banco._menu.playerBScore = 0;
            lbPlayerA.Content = "";
            lbPlayerB.Content = "";
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)  //chuyển dổi người muốn chat
        {
            if (banco._menu.whoPlayWith == Player.Human) // Nếu chơi với người
            {
                if (banco._menu.curChat == Player.Human) // nếu là người chơi thứ nhất chat
                {
                    banco._menu.curChat = Player.Com;
                    textCurChat.Text = textPlayerB.Text + " can Chat ...";
                }
                else if (banco._menu.curChat == Player.Com) // nếu là người chơi thứ 2 chat
                {
                    banco._menu.curChat = Player.Human;
                    textCurChat.Text = textPlayerA.Text + " can Chat ...";
                }
            }
            else if (banco._menu.whoPlayWith == Player.Com) // Nếu chơi với máy
            {
                if (banco._menu.curChat == Player.Human) // nếu là người chat
                {
                    banco._menu.curChat = Player.Com;
                    textCurChat.Text = "Computer can Chat ...";
                }
                else if (banco._menu.curChat == Player.Com) // nếu là máy chat
                {
                    banco._menu.curChat = Player.Human;
                    textCurChat.Text = textYourName.Text + " can Chat ...";
                }
            }
        }

        private void btnSur_Click(object sender, RoutedEventArgs e) // Nếu người chơi đầu hàng
        {
            System.GC.Collect();
            banco.Surrender();
            lbScoreA.Content = banco._menu.playerAScore.ToString();
            lbScoreB.Content = banco._menu.playerBScore.ToString();
        }

        private void btnPlayAgain_Click(object sender, RoutedEventArgs e)
        {
            System.GC.Collect();
            banco.PlayAgain();
            lbScoreA.Content = banco._menu.playerAScore.ToString();
            lbScoreB.Content = banco._menu.playerBScore.ToString();
        }

        // Phần kết nối với server
        //private void ConnectServer()
        //{
        //    var socket = IO.Socket("ws://gomoku-lajosveres.rhcloud.com:8000");
        //    socket.On(Socket.EVENT_CONNECT, () =>
        //    {
        //         text_ChatHistory.SelectionStart = 0;
        //            text_ChatHistory.SelectionLength = 0;
        //            text_ChatHistory.SelectedText = "Connected";
        //    });
        //}


    }
}
