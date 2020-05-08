using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot1
{
    class Station
    {
        private double _x;
        private double _y;
        private string _name;

        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Station(string name, double x, double y)
        {
            Name = name;
            X = x;
            Y = y;
        }
    }
}
