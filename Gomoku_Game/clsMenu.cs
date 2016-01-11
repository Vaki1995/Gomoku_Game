using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Game
{
    class clsMenu
    {
        private Player WhoPlayWith;
        private string PlayerA; // Tên người chơi thứ nhất
        private string PlayerB; // Tên người chơi thứ hai
        private string YourName; // Tên người chơi đánh với máy
        private Player CurPlay;//Lượt đi
        private Player CurChat; //Lượt chat
        private int PlayerAScore;//Điểm người chơi thứ 1
        private int PlayerBScore;//Điểm người chơi thứ 2

        public clsMenu()
        {
            whoPlayWith = Player.None;
            playerA = "Player A";
            playerB = "Player B";
            yourName = "Your Name";
            curPlay = Player.None;
            curChat = Player.None;
            playerAScore = 0;
            playerBScore = 0;
        }

        public Player whoPlayWith
        {
            get { return this.WhoPlayWith; }
            set { this.WhoPlayWith = value; }
        }

        public string playerA
        {
            get { return this.PlayerA; }
            set { this.PlayerA = value; }
        }

        public string playerB
        {
            get { return this.PlayerB; }
            set { this.PlayerB = value; }
        }

        public string yourName
        {
            get { return this.YourName; }
            set { this.YourName = value; }
        }


        public Player curPlay
        {
            get { return this.CurPlay; }
            set { this.CurPlay = value; }
        }

        public Player curChat
        {
            get { return this.CurChat; }
            set { this.CurChat = value; }
        }

        public int playerAScore
        {
            get { return this.PlayerAScore; }
            set { this.PlayerAScore = value; }
        }
        public int playerBScore
        {
            get { return this.PlayerBScore; }
            set { this.PlayerBScore = value; }
        }
    }
}
