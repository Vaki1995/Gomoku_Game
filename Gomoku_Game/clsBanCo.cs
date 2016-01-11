using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Resources;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;


namespace Gomoku_Game
{
    enum Player // Đối tượng đánh với bạn
    {
        None = 0, // ban đầu
        Human = 1, // là người
        Com = 2, // là máy
    }

    struct Node // Thông tin một node cờ
    {
        public int Row;
        public int Column;
        public Node(int rw, int cl)
        {
            this.Row = rw;
            this.Column = cl;
        }
    }

    class clsBanCo
    {
        // Các biến chính
        private int _countRow; // số dòng
        private int _countColumn; // số cột
        private const int length = 30; //Độ dài mỗi ô
        public Player[,] board; //Mảng lưu vị trí các con cờ
        private Player end; //Biến kiểm tra trò chơi kết thúc
        private clsLuongGiaBanCo eBoard; // Lượng giá của bàn cờ
        private cls5OWin OWin; // Kiểm tra 5 ô win
        private Window frmParent; // from thực hiện
        private Canvas cvBanCo; // Nơi vẽ bàn cờ
        private TextBox tbOCo; // Nơi chứ thông tin dòng và cột của ô cờ
        private TextBox tbNamePlay; // Nơi chưa tên người chơi được đánh
        public clsMenu _menu; // tùy chọn
        // Các biến phụ
        private Rectangle rec; // Khai báo biến đánh dấu thắng hay thua
        // Điểm lượng giá
        public int[] PhongThu = new int[5] { 0, 1, 9, 81, 729 };
        public int[] TanCong = new int[5] { 0, 3, 24, 192, 1536 };



        public int Row
        {
            get { return this._countRow; }
        }
        public int Column
        {
            get { return this._countColumn; }
        }

        public Player End
        {
            get { return this.end; }
            set { this.end = value; }
        }

        public clsBanCo(Window Frm, Canvas cvBC, TextBox tb1, TextBox tb2)
        {
            _countRow = _countColumn = 12;
            frmParent = Frm;
            cvBanCo = cvBC;
            tbOCo = tb1;
            tbNamePlay = tb2;
            _menu = new clsMenu();
            OWin = new cls5OWin();
            rec = new Rectangle(); 
            end = Player.None;
            board = new Player[_countRow, _countColumn];
            ResetBoard();
            eBoard = new clsLuongGiaBanCo(this);
            CreateRec();
            cvBanCo.Children.Add(rec);
            cvBanCo.MouseMove += new System.Windows.Input.MouseEventHandler(BanCo_MouseMove);
            cvBanCo.MouseDown += new System.Windows.Input.MouseButtonEventHandler(BanCo_MouseDown);
        }

        private void CreateRec() // Khởi tạo hình vuông
        {
            rec.Width = rec.Height = 50;
            rec.HorizontalAlignment = 0;
            rec.VerticalAlignment = 0;
            rec.Opacity = 0;
        }
        public void DrawChessboard()  // Vẽ bàn cờ
        {
            for (int i = 0; i < _countRow + 1; i++)
            {
                Line line = new Line();

                line.Stroke = Brushes.Black;
                line.X1 = 0;
                line.Y1 = i * length;
                line.X2 = length * _countRow;
                line.Y2 = i * length;
                line.HorizontalAlignment = HorizontalAlignment.Left;
                line.VerticalAlignment = VerticalAlignment.Top;
                cvBanCo.Children.Add(line);
            }
            for (int i = 0; i < _countColumn + 1; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.X1 = i * length;
                line.Y1 = 0;
                line.X2 = i * length;
                line.Y2 = length * _countColumn;
                line.HorizontalAlignment = HorizontalAlignment.Left;
                line.VerticalAlignment = VerticalAlignment.Top;
                cvBanCo.Children.Add(line);
            }
        }


        public void BanCo_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int cl, rw;
            tbOCo.Clear();
            Point toado = e.GetPosition(cvBanCo);//Lấy tọa độ chuột
            if (toado.X <= 360 && toado.Y <= 360 && toado.X % length != 0 && toado.Y != 0) //Nếu tọa độ chuột nằm trong vùng kiểm soát và không trùng lên đường vẽ
            {
                //Xử lý tọa độ
                cl = ((int)toado.X / length); // cột
                rw = ((int)toado.Y / length); // dòng
                tbOCo.Text = "Row: " + (rw + 1).ToString() + "  Column: " + (cl + 1).ToString();

            }
            else
            {
                tbOCo.Text = "";
            }
        }

        public void BanCo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.GC.Collect();//Thu gôm rác
            int cl, rw;
            Point toado = e.GetPosition(cvBanCo);//Lấy tọa độ chuột
            //Xử lý tọa độ
            cl = ((int)toado.X / length); // cột
            rw = ((int)toado.Y / length); // dòng

            #region Nếu đánh với người
            if (this._menu.whoPlayWith == Player.Human) // Nếu chơi với người
            {
                //Player.Com sẽ đóng vai trò người chơi thứ 2
                if (board[rw, cl] == Player.None)//Nếu ô bấm chưa có cờ
                {
                    if (this._menu.curPlay == Player.Human && end == Player.None)//Nếu lượt đi là người và trận đấu chưa kết thúc
                    {
                        tbNamePlay.Text = "Turns play of " + this._menu.playerB;
                        board[rw, cl] = this._menu.curPlay;//Lưu loại cờ vừa đánh vào mảng
                        DrawDataBoard(rw, cl);//Vẽ con cờ theo lượt chơi
                        end = CheckEnd(rw, cl);//Kiểm tra xem trận đấu kết thúc chưa
                        if (end == Player.Human)//Nếu người chơi 1 thắng
                        {
                            this._menu.curPlay = Player.Human; //Thiết lập lại lượt chơi
                            OnWin();//Khai báo sư kiện Win
                            OnWinOrLose();//Hiển thị 5 ô Win.
                        }
                        else
                        {
                            this._menu.curPlay = Player.Com;//Thiết lập lại lượt chơi
                            OnHumanDanhXong();// Khai báo sự kiện người chơi 1 đánh xong
                        }
                    }
                    else if (this._menu.curPlay == Player.Com && end == Player.None)
                    {
                        tbNamePlay.Text = "Turns play of " + this._menu.playerA;
                        board[rw, cl] = this._menu.curPlay;//Lưu loại cờ vừa đánh vào mảng
                        DrawDataBoard(rw, cl);//Vẽ con cờ theo lượt chơi
                        end = CheckEnd(rw, cl);//Kiểm tra xem trận đấu kết thúc chưa
                        if (end == Player.Com)//Nếu người chơi 2 thắng
                        {
                            OnWin();//Khai báo sư kiện Win
                            OnWinOrLose();//Hiển thị 5 ô Win.
                        }
                        else
                        {
                            this._menu.curPlay = Player.Human;//Thiết lập lại lượt chơi
                            OnComDanhXong();// Khai báo sự kiện người chơi 2 đánh xong
                        }
                    }
                }
            }
            #endregion
            #region Nếu đánh với máy
            else if (this._menu.whoPlayWith == Player.Com) // Nếu chọn đánh với máy
            {
                Node node = new Node();
                if (board[rw, cl] == Player.None) //Nếu ô bấm chưa có cờ
                {
                    if (this._menu.curPlay == Player.Human && end == Player.None)//Nếu lượt đi là người và trận đấu chưa kết thúc
                    {
                        tbNamePlay.Text = "Turns play of  Computer";
                        board[rw, cl] = this._menu.curPlay;//Lưu loại cờ vừa đánh vào mảng        
                        DrawDataBoard(rw, cl);//Vẽ con cờ theo lượt chơi
                        end = CheckEnd(rw, cl);//Kiểm tra xem trận đấu kết thúc chưa
                        if (end == Player.Human)//Nếu người thắng cuộc là người
                        {
                            OnWin();//Khai báo sự kiện Win
                            OnWinOrLose();//Hiển thị 5 ô Win.
                        }
                        else if (end == Player.None) //Nếu trận đấu chưa kết thúc
                        {
                            this._menu.curPlay = Player.Com;//Thiết lập lại lượt chơi
                            OnHumanDanhXong(); // Khai báo sự kiện người đánh xong
                        }
                    }
                    if (this._menu.curPlay == Player.Com && end == Player.None)//Nếu lượt đi là máy và trận đấu chưa kết thúc
                    {
                        //Tìm đường đi cho máy
                        tbNamePlay.Text = "Turns play of " + this._menu.yourName;
                        eBoard.ResetBoard();
                        LuongGia(Player.Com);//Lượng giá bàn cờ cho máy
                        node = eBoard.GetMaxNode();//lưu vị trí máy sẽ đánh
                        int r, c;
                        r = node.Row;
                        c = node.Column;
                        board[r, c] = this._menu.curPlay; //Lưu loại cờ vừa đánh vào mảng     
                        DrawDataBoard(r, c);//Vẽ con cờ theo lượt chơi
                        end = CheckEnd(r, c);//Kiểm tra xem trận đấu kết thúc chưa

                        if (end == Player.Com)//Nếu máy thắng
                        {
                            OnLose();//Khai báo sư kiện Lose
                            OnWinOrLose();//Hiển thị 5 ô Lose.
                        }
                        else if (end == Player.None)
                        {
                            this._menu.curPlay = Player.Human;//Thiết lập lại lượt chơi
                            OnComDanhXong();// Khai báo sự kiện người đánh xong
                        }
                    }
                }
            }
            #endregion
        }

        // Thiết lập các giá trị lưu vị trí bàn cờ.
        public void ResetBoard()
        {
            for (int i = 0; i < _countRow; i++)
            {
                for (int j = 0; j < _countColumn; j++)
                {
                    board[i, j] = Player.None;
                }
            }
        }

        //delegate sự kiện máy đánh xong
        public delegate void ComDanhXongEventHandler();
        public event ComDanhXongEventHandler ComDanhXongEvent;
        private void OnComDanhXong()
        {
            if (ComDanhXongEvent != null)
            {
                ComDanhXongEvent();
            }
        }
        //delegate sự kiện người đánh xong
        public delegate void HumanDanhXongEventHandler();
        public event HumanDanhXongEventHandler HumanDanhXongEvent;
        private void OnHumanDanhXong()
        {
            if (HumanDanhXongEvent != null)
            {
                HumanDanhXongEvent();
            }
        }

        //delegate sự kiện Win
        public delegate void WinEventHander();
        public event WinEventHander WinEvent;
        public void OnWin()
        {
            if (WinEvent != null)
            {
                WinEvent();
            }
        }

        //delegate sự kiện Lose
        public delegate void LoseEventHander();
        public event LoseEventHander LoseEvent;
        public void OnLose()
        {
            if (LoseEvent != null)
            {
                LoseEvent();
            }
        }

        public void DiNgauNhien()
        {
            if (this._menu.curPlay == Player.Com)
            {
                board[_countRow / 2, _countColumn / 2] = this._menu.curPlay;
                DrawDataBoard(_countColumn / 2 , _countRow / 2);
                this._menu.curPlay = Player.Human;
                OnComDanhXong();//Khai báo sự kiện khi máy đánh xong
            }
        }

        public void NewGame() // Nếu lượt đi là máy
        {
            if (this._menu.curPlay == Player.Com)//Nếu chọn kiểu chơi với máy
            {
                if (this._menu.curPlay == Player.Com)//Nếu lượt đi là máy
                {
                    DiNgauNhien();
                }
            }
        }
        private Player DoiNgich(Player cur) //Hàm lấy đối thủ của người chơi hiện tại
        {
            if (cur == Player.Com) return Player.Human;
            if (cur == Player.Human) return Player.Com;
            return Player.None;
        }

        private void LuongGia(Player player)
        {
            int cntHuman = 0, cntCom = 0;//Biến đếm Human,Com
            #region Luong gia cho hang
            for (int i = 0; i < _countRow; i++)
            {
                for (int j = 0; j < _countColumn - 4; j++)
                {
                    //Khởi tạo biến đếm
                    cntHuman = cntCom = 0;
                    //Đếm số lượng con cờ trên 5 ô kế tiếp của 1 hàng
                    for (int k = 0; k < 5; k++)
                    {
                        if (board[i, j + k] == Player.Human) cntHuman++;
                        if (board[i, j + k] == Player.Com) cntCom++;
                    }
                    //Lượng giá
                    //Nếu 5 ô kế tiếp chỉ có 1 loại cờ (hoặc là Human,hoặc la Com)
                    if (cntHuman * cntCom == 0 && cntHuman != cntCom)
                    {
                        //Gán giá trị cho 5 ô kế tiếp của 1 hàng
                        for (int k = 0; k < 5; k++)
                        {
                            //Nếu ô đó chưa có quân đi
                            if (board[i, j + k] == Player.None)
                            {
                                //Nếu trong 5 ô đó chỉ tồn tại cờ của Human
                                if (cntCom == 0)
                                {
                                    //Nếu đối tượng lượng giá là Com
                                    if (player == Player.Com)
                                    {
                                        //Vì đối tượng người chơi là Com mà trong 5 ô này chỉ có Human
                                        //nên ta sẽ cộng thêm điểm phòng thủ cho Com
                                        eBoard.GiaTri[i, j + k] += PhongThu[cntHuman];
                                    }
                                    //Ngược lại cộng điểm phòng thủ cho Human
                                    else
                                    {
                                        eBoard.GiaTri[i, j + k] += TanCong[cntHuman];
                                    }
                                    //Nếu chơi theo luật Việt Nam
                                    //if (this.Option.GamePlay == LuatChoi.Vietnamese)
                                        //Xét trường hợp chặn 2 đầu
                                        //Nếu chận 2 đầu thì gán giá trị cho các ô đó bằng 0
                                        if (j - 1 >= 0 && j + 5 <= _countColumn - 1 && board[i, j - 1] == Player.Com && board[i, j + 5] == Player.Com)
                                        {
                                            eBoard.GiaTri[i, j + k] = 0;
                                        }

                                }
                                //Tương tự như trên
                                if (cntHuman == 0) //Nếu chỉ tồn tại Com
                                {
                                    if (player == Player.Human) //Nếu người chơi là Người
                                    {
                                        eBoard.GiaTri[i, j + k] += PhongThu[cntCom];
                                    }
                                    else
                                    {
                                        eBoard.GiaTri[i, j + k] += TanCong[cntCom];
                                    }
                                    //Trường hợp chặn 2 đầu
                                    //if (this.Option.GamePlay == LuatChoi.Vietnamese)
                                        if (j - 1 >= 0 && j + 5 <= _countColumn - 1 && board[i, j - 1] == Player.Human && board[i, j + 5] == Player.Human)
                                        {
                                            eBoard.GiaTri[i, j + k] = 0;
                                        }

                                }
                                if ((j + k - 1 > 0) && (j + k + 1 <= _countColumn - 1) && (cntHuman == 4 || cntCom == 4)
                                   && (board[i, j + k - 1] == Player.None || board[i, j + k + 1] == Player.None))
                                {
                                    eBoard.GiaTri[i, j + k] *= 3;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //Tương tự như lượng giá cho hàng
            #region Luong gia cho cot
            for (int i = 0; i < _countRow - 4; i++)
            {
                for (int j = 0; j < _countColumn; j++)
                {
                    cntHuman = cntCom = 0;
                    for (int k = 0; k < 5; k++)
                    {
                        if (board[i + k, j] == Player.Human) cntHuman++;
                        if (board[i + k, j] == Player.Com) cntCom++;
                    }
                    if (cntHuman * cntCom == 0 && cntCom != cntHuman)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (board[i + k, j] == Player.None)
                            {
                                if (cntCom == 0)
                                {
                                    if (player == Player.Com) eBoard.GiaTri[i + k, j] += PhongThu[cntHuman];
                                    else eBoard.GiaTri[i + k, j] += TanCong[cntHuman];
                                    // Truong hop bi chan 2 dau.
                                    if ((i - 1) >= 0 && (i + 5) <= _countRow - 1 && board[i - 1, j] == Player.Com && board[i + 5, j] == Player.Com)
                                    {
                                        eBoard.GiaTri[i + k, j] = 0;
                                    }
                                }
                                if (cntHuman == 0)
                                {
                                    if (player == Player.Human) eBoard.GiaTri[i + k, j] += PhongThu[cntCom];
                                    else eBoard.GiaTri[i + k, j] += TanCong[cntCom];
                                    // Truong hop bi chan 2 dau.
                                    if (i - 1 >= 0 && i + 5 <= _countRow - 1 && board[i - 1, j] == Player.Human && board[i + 5, j] == Player.Human)
                                        eBoard.GiaTri[i + k, j] = 0;
                                }
                                if ((i + k - 1) >= 0 && (i + k + 1) <= _countRow - 1 && (cntHuman == 4 || cntCom == 4)
                                    && (board[i + k - 1, j] == Player.None || board[i + k + 1, j] == Player.None))
                                {
                                    eBoard.GiaTri[i + k, j] *= 3;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //Tương tự như lượng giá cho hàng
            #region  Luong gia tren duong cheo chinh (\)
            for (int i = 0; i < _countRow - 4; i++)
            {
                for (int j = 0; j < _countColumn - 4; j++)
                {
                    cntHuman = cntCom = 0;
                    for (int k = 0; k < 5; k++)
                    {
                        if (board[i + k, j + k] == Player.Human) cntHuman++;
                        if (board[i + k, j + k] == Player.Com) cntCom++;
                    }
                    if (cntHuman * cntCom == 0 && cntCom != cntHuman)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (board[i + k, j + k] == Player.None)
                            {
                                if (cntCom == 0)
                                {
                                    if (player == Player.Com) eBoard.GiaTri[i + k, j + k] += PhongThu[cntHuman];
                                    else eBoard.GiaTri[i + k, j + k] += TanCong[cntHuman];
                                    // Truong hop bi chan 2 dau.
                                    if (i - 1 >= 0 && j - 1 >= 0  &&  i + 5 <= _countRow- 1 && j + 5 <= _countColumn - 1  && board[i - 1, j - 1] == Player.Com && board[i + 5, j + 5] == Player.Com)
                                            eBoard.GiaTri[i + k, j + k] = 0;
                                }
                                if (cntHuman == 0)
                                {
                                    if (player == Player.Human) eBoard.GiaTri[i + k, j + k] += PhongThu[cntCom];
                                    else eBoard.GiaTri[i + k, j + k] += TanCong[cntCom];
                                    // Truong hop bi chan 2 dau.
                                        if ((i - 1) >= 0 && j - 1 >= 0 && i + 5 <= _countRow - 1 && j + 5 <= _countColumn- 1 && board[i - 1, j - 1] == Player.Human && board[i + 5, j + 5] == Player.Human)
                                        {
                                            eBoard.GiaTri[i + k, j + k] = 0;
                                        }
                                }
                                if ((i + k - 1) >= 0 && (j + k - 1) >= 0 && (i + k + 1) <= _countRow- 1 && (j + k + 1) <= _countColumn - 1 && (cntHuman == 4 || cntCom == 4) && (board[i + k - 1, j + k - 1] == Player.None || board[i + k + 1, j + k + 1] == Player.None))
                                {
                                    eBoard.GiaTri[i + k, j + k] *= 3;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //Tương tự như lượng giá cho hàng
            #region Luong gia tren duong cheo phu (/)
            for (int i = 4; i < _countRow - 4; i++)
            {
                for (int j = 0; j < _countColumn - 4; j++)
                {
                    cntCom = 0; cntHuman = 0;
                    for (int k = 0; k < 5; k++)
                    {
                        if (board[i - k, j + k] == Player.Human) cntHuman++;
                        if (board[i - k, j + k] == Player.Com) cntCom++;
                    }
                    if (cntHuman * cntCom == 0 && cntHuman != cntCom)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (board[i - k, j + k] == Player.None)
                            {
                                if (cntCom == 0)
                                {
                                    if (player == Player.Com) eBoard.GiaTri[i - k, j + k] += PhongThu[cntHuman];
                                    else eBoard.GiaTri[i - k, j + k] += TanCong[cntHuman];
                                    // Truong hop bi chan 2 dau.
                                    if (i + 1 <= _countRow - 1 && j - 1 >= 0 && i - 5 >= 0 && j + 5 <= _countColumn - 1 && board[i + 1, j - 1] == Player.Com && board[i - 5, j + 5] == Player.Com)
                                    {
                                        eBoard.GiaTri[i - k, j + k] = 0;
                                    }
                                }
                                if (cntHuman == 0)
                                {
                                    if (player == Player.Human) eBoard.GiaTri[i - k, j + k] += PhongThu[cntCom];
                                    else eBoard.GiaTri[i - k, j + k] += TanCong[cntCom];
                                    // Truong hop bi chan 2 dau.
                                        if (i + 1 <= _countRow - 1 && j - 1 >= 0 && i - 5 >= 0 && j + 5 <= _countColumn - 1 && board[i + 1, j - 1] == Player.Human && board[i - 5, j + 5] == Player.Human)
                                        {
                                            eBoard.GiaTri[i - k, j + k] = 0;
                                        }
                                }
                                if ((i - k + 1) <= _countRow - 1 && (j + k - 1) >= 0  && (i - k - 1) >= 0 && (j + k + 1) <= _countColumn - 1 && (cntHuman == 4 || cntCom == 4) && (board[i - k + 1, j + k - 1] == Player.None || board[i - k - 1, j + k + 1] == Player.None))
                                {
                                    eBoard.GiaTri[i - k, j + k] *= 3;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
        }

        private Player CheckEnd(int rw, int cl)
        {
            int rowTemp = rw;
            int colTemp = cl;
            int count1, count2, count3, count4;
            count1 = count2 = count3 = count4 = 1;
            Player cur = board[rw, cl];
            OWin.Reset();
            OWin.Add(new Node(rowTemp, colTemp));
            #region Kiem Tra Hang Ngang
            while (colTemp - 1 >= 0 && board[rowTemp, colTemp - 1] == cur)
            {
                OWin.Add(new Node(rowTemp, colTemp - 1));
                count1++;
                colTemp--;
            }
            colTemp = cl;
            while (colTemp + 1 <= _countColumn - 1 && board[rowTemp, colTemp + 1] == cur)
            {
                OWin.Add(new Node(rowTemp, colTemp + 1));
                count1++;
                colTemp++;
            }
            if (count1 == 5)
            {
                if ((colTemp - 5 >= 0 && colTemp + 1 <= _countColumn - 1 && board[rowTemp, colTemp + 1] == board[rowTemp, colTemp - 5] && board[rowTemp, colTemp + 1] == DoiNgich(cur)) ||
                        (colTemp - 5 < 0 && (board[rowTemp, colTemp + 1] == DoiNgich(cur))) ||
                        (colTemp + 1 > _countColumn - 1 && (board[rowTemp, colTemp - 5] == DoiNgich(cur))))
                    { }
                else
                        return cur;
            }
            #endregion
            #region Kiem Tra Hang Doc
            OWin.Reset();
            colTemp = cl;
            OWin.Add(new Node(rowTemp, colTemp));

            while (rowTemp - 1 >= 0 && board[rowTemp - 1, colTemp] == cur)
            {
                OWin.Add(new Node(rowTemp - 1, colTemp));
                count2++;
                rowTemp--;
            }
            rowTemp = rw;
            while (rowTemp + 1 <= _countRow - 1 && board[rowTemp + 1, colTemp] == cur)
            {
                OWin.Add(new Node(rowTemp + 1, colTemp));
                count2++;
                rowTemp++;
            }
            if (count2 == 5)
            {
                if ((rowTemp - 5 >= 0 && rowTemp + 1 <= _countColumn - 1 && board[rowTemp + 1, colTemp] == board[rowTemp - 5, colTemp] && board[rowTemp + 1, colTemp] == DoiNgich(cur)) ||
                        (rowTemp - 5 < 0 && (board[rowTemp + 1, colTemp] == DoiNgich(cur))) ||
                        (rowTemp + 1 > _countRow - 1 && (board[rowTemp - 5, colTemp] == DoiNgich(cur))))
                    { }
                else
                    return cur;
            }
            #endregion
            #region Kiem Tra Duong Cheo Chinh (\)
            colTemp = cl;
            rowTemp = rw;
            OWin.Reset();
            OWin.Add(new Node(rowTemp, colTemp));
            while (rowTemp - 1 >= 0 && colTemp - 1 >= 0 && board[rowTemp - 1, colTemp - 1] == cur)
            {
                OWin.Add(new Node(rowTemp - 1, colTemp - 1));
                count3++;
                rowTemp--;
                colTemp--;
            }
            rowTemp = rw;
            colTemp = cl;
            while (rowTemp + 1 <= _countRow  - 1 && colTemp + 1 <= _countColumn - 1 && board[rowTemp + 1, colTemp + 1] == cur)
            {
                OWin.Add(new Node(rowTemp + 1, colTemp + 1));
                count3++;
                rowTemp++;
                colTemp++;
            }
            if (count3 == 5)
            {
                if ((colTemp - 5 >= 0 && rowTemp - 5 >= 0 && colTemp + 1 <= _countColumn - 1 && rowTemp + 1 <= _countRow - 1 && board[rowTemp + 1, colTemp + 1] == board[rowTemp - 5, colTemp - 5] && board[rowTemp + 1, colTemp + 1] == DoiNgich(cur)) ||
                           ((colTemp - 5 < 0 || rowTemp - 5 < 0) && (board[rowTemp + 1, colTemp + 1] == DoiNgich(cur))) ||
                           ((colTemp + 1 > _countColumn - 1 || rowTemp + 1 > _countRow - 1) && (board[rowTemp - 5, colTemp - 5] == DoiNgich(cur))))
                    { }
                else
                    return cur;
            }
            #endregion
            #region Kiem Tra Duong Cheo Phu
            rowTemp = rw;
            colTemp = cl;
            OWin.Reset();
            OWin.Add(new Node(rowTemp, colTemp));
            while (rowTemp + 1 <= _countRow - 1 && colTemp - 1 >= 0 && board[rowTemp + 1, colTemp - 1] == cur)
            {
                OWin.Add(new Node(rowTemp + 1, colTemp - 1));
                count4++;
                rowTemp++;
                colTemp--;
            }
            rowTemp = rw;
            colTemp = cl;
            while (rowTemp - 1 >= 0 && colTemp + 1 <= _countColumn - 1 && board[rowTemp - 1, colTemp + 1] == cur)
            {
                OWin.Add(new Node(rowTemp - 1, colTemp + 1));
                count4++;
                rowTemp--;
                colTemp++;
            }
            if (count4 == 5)
            {
                if ((rowTemp - 1 >= 0 && rowTemp + 5 <= _countRow - 1 && colTemp + 1 <= _countColumn - 1 && colTemp - 5 >= 0 && rowTemp + 1 <= _countRow- 1 && board[rowTemp - 1, colTemp + 1] == board[rowTemp + 5, colTemp - 5] && board[rowTemp - 1, colTemp + 1] == DoiNgich(cur)) ||
                          ((colTemp - 5 < 0 || rowTemp + 5 > _countRow- 1) && (board[rowTemp - 1, colTemp + 1] == DoiNgich(cur))) ||
                          ((colTemp + 1 > _countColumn - 1 || rowTemp - 1 < 0) && (board[rowTemp + 5, colTemp - 5] == DoiNgich(cur))))
                    { }
                else
                    return cur;
            }
            #endregion
            return Player.None;
        }

        private void OnWinOrLose() // Nếu thắng hay thua thì báo người thnawgs thua và đánh dấu
        {
            Node node = new Node();
            for (int i = 0; i < 5; i++)
            {
                node = OWin.GiaTri[i];
                Rectangle rec = new Rectangle();
                rec.Height = 50;
                rec.Width = 50;
                rec.Opacity = 100;
                rec.HorizontalAlignment = 0;
                rec.VerticalAlignment = 0;
                rec.Margin = new Thickness(node.Column * length - 10, node.Row * length - 10, 0, 0);
                cvBanCo.Children.Add(rec);
            }

            if(this._menu.whoPlayWith == Player.Human) // Nếu là đánh với người
            {
                if(end == Player.Human) // Nếu là người chơi thứ nhất thắng
                {
                    tbNamePlay.Text = "^_^ " + this._menu.playerA + " is the Winner!!! ^_^";
                    this._menu.playerAScore++;
                }
                else if(end == Player.Com) // Nếu là người chơi thứ 2 thắng
                {
                    tbNamePlay.Text = "^_^ " + this._menu.playerB + " is the Winner!!! ^_^";
                    this._menu.playerBScore++;
                }
            }

            else if (this._menu.whoPlayWith == Player.Com) // Nếu là đánh với máy
            {
                if (end == Player.Human) // Nếu là người chơi thắng
                {
                    tbNamePlay.Text = "^_^ You are the Winner!!! ^_^";
                    this._menu.playerAScore++;
                }
                else if (end == Player.Com) // Nếu là máy thắng
                {
                    tbNamePlay.Text = " :(  You are the Loser!!!! :(";
                    this._menu.playerBScore++;
                }
            }
        }

        private void DrawDataBoard(int rw, int cl) // Vẽ quân cờ
        {
            if (this._menu.curPlay == Player.Human)
            {
                UserControl chess;
                chess = new ChessO();
                chess.Height = length;
                chess.Width = length;
                chess.HorizontalAlignment = 0;
                chess.VerticalAlignment = 0;
                chess.Margin = new Thickness(cl * length, rw * length, 0, 0);
                cvBanCo.Children.Add(chess);
                //Ghi lại cờ vừa đánh
                rec.Opacity = 100;
                rec.Margin = new Thickness(cl * length - 10, rw * length - 10, 0, 0);
            }
            else if (this._menu.curPlay == Player.Com)
            {
                UserControl chess;
                chess = new ChessX();
                chess.Height = length;
                chess.Width = length;
                chess.HorizontalAlignment = 0;
                chess.VerticalAlignment = 0;
                chess.Margin = new Thickness(cl * length, rw * length, 0, 0);
                cvBanCo.Children.Add(chess);
                rec.Opacity = 100;
                rec.Margin = new Thickness(cl * length - 10, rw * length - 10, 0, 0);
            }
        }



        public void ResetAllBoard() //Thiết lập lại toàn bộ dữ liệu bàn cờ
        {
            OWin = new cls5OWin();
            cvBanCo.Children.Clear();
            cvBanCo.Children.Add(rec);
            ResetBoard();
            end = Player.None;
            DrawChessboard();
        }

        public void Surrender() // Nếu người chơi dầu hàng
        {
            if (this._menu.whoPlayWith == Player.Human) // Nếu là đánh với người
            {
                if (this._menu.curPlay == Player.Human) // Nếu là người chơi thứ nhất đầu hàng
                {
                    end = Player.Com; // Người chơi thứ 2 dành chiến thắng
                    OnWinOrLose();
                }
                else if (this._menu.curPlay == Player.Com) // Nếu là người chơi thứ 2đầu hàng
                {
                    end = Player.Human; // Người chơi thứ 1 dành chiến thắng
                    OnWinOrLose();
                }
            }

            else if (this._menu.whoPlayWith == Player.Com) // Nếu là đánh với máy
            {
                if (this._menu.curPlay == Player.Human) // Nếu là người chơi đầu hàng
                {
                    end = Player.Com; // Máy dành chiến thắng
                    OnWinOrLose();
                }
            }
            end = Player.None;
            OWin = new cls5OWin();
            cvBanCo.Children.Clear();
            cvBanCo.Children.Add(rec);
            ResetBoard();
            DrawChessboard();
        }

        public void PlayAgain() // Chơi lại game
        {
            end = Player.None;
            OWin = new cls5OWin();
            cvBanCo.Children.Clear();
            cvBanCo.Children.Add(rec);
            //grdBanCo.Children.Add(coAo1);
            //grdBanCo.Children.Add(coAo2);
            ResetBoard();
            DrawChessboard();
            if (this._menu.whoPlayWith == Player.Com)
            {
                if (end == Player.None)
                {
                    if ( (this._menu.playerAScore +  this._menu.playerBScore) % 2 == 1)
                    {
                        this._menu.curPlay = Player.Com;
                        tbNamePlay.Text = "Turns play of Computer";
                        DiNgauNhien();
                    }
                    else
                    {
                        this._menu.curPlay = Player.Human;
                        tbNamePlay.Text = "Turns play of " + this._menu.yourName;
                    }
                }
            }
            else if (this._menu.whoPlayWith == Player.Human)
            {
                if (end == Player.None)
                {
                    if ((this._menu.playerAScore + this._menu.playerBScore) % 2 == 1)
                    {
                        this._menu.curPlay = Player.Com;
                        tbNamePlay.Text = "Turns play of " + this._menu.playerB;
                    }
                    else
                    {
                        this._menu.curPlay = Player.Human;
                        tbNamePlay.Text = "Turns play of " + this._menu.playerA;
                    }
                }
            }
        }



    }
}