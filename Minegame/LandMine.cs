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
    class LandMineEventArgs : EventArgs
    {
        public LandMine ? LandMine { get; set; }
    }
    class LandMine
    {
        public Position position;
        public bool Exploded { get; set; }

        public int Damage { get; set; }

        public Ellipse? ellipse { get; set; }
        public TextBlock ? text { get; set; }
        public Stopwatch ? stopwatch { get; set; }

        public LandMine(Position position, bool exploded, int damage)
        {
            this.position = position;
            this.Exploded = exploded;
            this.Damage = damage;
        }

        public double IsNearby(Position other, double maxdistance = 28.5)
        {
            double distance = Math.Sqrt(Math.Pow(other.left - position.left, 2) + Math.Pow(other.top - position.top, 2));
            return distance;
        }

        public delegate void LandMineExplodedEventHandler(object source, LandMineEventArgs args);

        public event LandMineExplodedEventHandler? LandMineExploded;

        protected virtual void OnLandMineExploded()
        {
            if (LandMineExploded != null)
            {
                LandMineExploded(this, new LandMineEventArgs() { LandMine = this });
            }
        }

        public void OnVehicleMoved(object ?source, PlayerEventArgs e)
        {
            if (position.PositionMatch(e.player?.position) && !Exploded)
            {
                Exploded = true;
                OnLandMineExploded();
            }
        }
    }
}
