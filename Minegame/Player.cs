using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Minegame
{
    class PlayerEventArgs : EventArgs
    {
        public Player ? Player { get; set; }
    }
    class Player
    {
        public Position position;

        public int playerSpeed = 5;
        public string Name { get; set; }
        public int Health { get; set; }

        public int Radar { get; set; }
        public bool RadarActive { get; set; }

        public bool goLeft, goRight, goUp, goDown, wave1 = true;

        public Player(Position position, string name, int health, int radar) 
        { 
            this.position = position;
            Name = name;
            Health = health;
            Radar = radar;
        }

        public Position GetPosition()
        {
            return position;
        }

        public bool IsAlive()
        {
            return Health > 0;
        }

        public void PlayerAction(Key key, bool activate)
        {
            switch (key)
            {
                case Key.Up:
                    goUp = activate;
                    break;
                case Key.Down:
                    goDown = activate;
                    break;
                case Key.Left:
                    goLeft = activate;
                    break;
                case Key.Right:
                    goRight = activate;
                    break;
                case Key.Enter:
                    RadarActive = activate;
                    break;
                default: 
                    break;
            }
            OnPlayerMoved();
        }

        public delegate void PlayerMovedEventHandler(object source, PlayerEventArgs args);

        public event PlayerMovedEventHandler? PlayerMoved;

        protected virtual void OnPlayerMoved()
        {
            if(PlayerMoved != null)
            {
                PlayerMoved(this, new PlayerEventArgs() { Player = this});
            }
        }

        public void OnLandMineExploded(object? source, LandMineEventArgs e)
        {
            Health -= e.LandMine.Damage;
        }

        public void TakeDamage(int damage)
        {
            if(Health - damage > 0)
                Health -= damage;
            else
                OnPlayerDied();
        }

        public event EventHandler<PlayerEventArgs> PlayerDied;

        protected virtual void OnPlayerDied()
        {
            if(PlayerDied!= null)
            {
                PlayerDied(this, new PlayerEventArgs() { Player = this });
            }
        }
    }
}
