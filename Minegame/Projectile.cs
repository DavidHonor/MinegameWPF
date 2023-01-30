using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Minegame
{
    class Projectile
    {
        public Position Target { get; set; }
        public Ellipse Ellipse { get; set; }
        public Stopwatch Lifetime { get; set; }
        public double Speed { get; set; }

        public int Damage = 20;

        public double GetTargetDistance()
        {
            return Math.Sqrt(Math.Pow(Target.left - Canvas.GetLeft(Ellipse), 2) + Math.Pow(Target.top - Canvas.GetTop(Ellipse), 2));
        }
        public double IsNearby(Position other, double maxdistance = 28.5)
        {
            double distance = Math.Sqrt(Math.Pow(other.left - Canvas.GetLeft(Ellipse), 2) + Math.Pow(other.top - Canvas.GetTop(Ellipse), 2));
            return distance;
        }
    }
}
