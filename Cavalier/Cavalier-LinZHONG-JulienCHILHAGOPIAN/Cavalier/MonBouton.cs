using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavalier
{
    class MonBouton : System.Windows.Forms.Button
    {
        public int L { get; set; }
        public int C { get; set; }
        public bool atteignable = false;
        public bool dejaPassé = false;
        public int ordre { get; set; }

        public MonBouton(int l, int c) : base()
        {
            this.L = l;
            this.C = c;
            this.ordre = 0;
        }

    }
}
