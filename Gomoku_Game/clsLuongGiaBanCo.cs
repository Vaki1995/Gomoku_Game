using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Game
{
    class clsLuongGiaBanCo
    {
        //Bảng lượng giá bàn cờ.
        private int height;
        private int width;
        public int[,] GiaTri;
        //Contructor
        public clsLuongGiaBanCo(clsBanCo cls)
        {
            height = cls.Row;
            width = cls.Column;
            GiaTri = new int[height, width];
            ResetBoard();
        }
        //Public Methods
        public void ResetBoard()
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    GiaTri[i, j] = 0;
                }
            }
        }
        //Hàm lấy vị trí có giá trị cao nhất trên bàn cờ
        public Node GetMaxNode()
        {
            Node n = new Node();
            int maxValue = GiaTri[0, 0];
            Node[] arrMaxNodes = new Node[289];
            for (int i = 0; i < 289; i++)
            {
                arrMaxNodes[i] = new Node();
            }
            int count = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (GiaTri[i, j] > maxValue)
                    {
                        n.Row = i;
                        n.Column = j;
                        maxValue = GiaTri[i, j];
                    }
                }
            }

            //Với mục đích không lặp lại bước đi giống như lần trước
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (GiaTri[i, j] == maxValue)
                    {
                        n.Row = i;
                        n.Column = j;
                        arrMaxNodes[count] = n;
                        count++;
                    }
                }
            }
            //Đường đi sẽ là ngẫu nhiên

            Random r = new Random();
            int soNgauNhien = r.Next(count);
            return arrMaxNodes[soNgauNhien];
        }
    }
}
