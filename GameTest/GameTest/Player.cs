using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTest
{
    internal class Player
    {
        public int x { get; set; }
        public int y { get; set; }
        
        public string color { get; set; }
        public int score { get; set; }
        public string name { get; set; }

        public Player(int x,int y,string color, int score, string name)
        {
            this.x = x;
            this.y = y;
            this.color = color;
            this.score = score;
            this.name = name;
        }


    }
}
