using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Game
{
    class cls5OWin
    {
        public Node[] GiaTri;
        int thuTu;
        public cls5OWin()
        {
            GiaTri = new Node[10];
            for (int i = 0; i < 10; i++)
            {
                GiaTri[i] = new Node();
            }
            thuTu = 0;
        }
        public void Add(Node n)
        {
            GiaTri[thuTu] = n;
            thuTu++;
        }
        public void Reset()
        {
            thuTu = 0;
            GiaTri = new Node[10];
            for (int i = 0; i < 10; i++)
            {
                GiaTri[i] = new Node();
            }
        }
    }
}
