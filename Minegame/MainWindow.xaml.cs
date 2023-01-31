using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Minegame
{
    public partial class MainWindow : Window
    {

        DispatcherTimer GameTimer;
        Player Player;
        List<LandMine> Mines;
        List<Projectile> projectiles;

        public MainWindow()
        {
            InitializeComponent();
            StartGame();
        }

        private void StartGame()
        {
            //Clear variables
            myCanvas.Children.Clear();
            if(GameTimer != null)
                GameTimer.Stop();

            //Init variables
            GameTimer = new DispatcherTimer();
            Player = new Player(new Position(1, 1), "Levi", 100, 3);
            Mines = new List<LandMine>();
            projectiles = new List<Projectile>();

            //Create game elements
            InitGameElements();

            Player.PlayerDied += Player_PlayerDied;
            GameTimer.Tick += GameTimer_Tick;
            GameTimer.Interval = TimeSpan.FromMilliseconds(20);

            myCanvas.Focus();
            GameTimer.Start();
        }

        private void InitGameElements()
        {
            //Create player
            Ellipse playerEllipse = new Ellipse();
            playerEllipse.Width = 40;
            playerEllipse.Height = 40;
            playerEllipse.Fill = new SolidColorBrush(Colors.Green);

            Canvas.SetLeft(playerEllipse, 420);
            Canvas.SetTop(playerEllipse, 135);

            Player.Ellipse = playerEllipse;
            myCanvas.Children.Add(playerEllipse);

            //Create mines
            Mines.Add(new LandMine(new Position(50, 100), false, 50));
            Mines.Add(new LandMine(new Position(50, 170), false, 50));

            foreach (LandMine mine in Mines)
            {
                TextBlock text = new TextBlock();
                text.Text = "debug";
                text.Foreground = new SolidColorBrush(Colors.White);

                Ellipse ellipse = new Ellipse();
                ellipse.Width = 40;
                ellipse.Height = 40;
                ellipse.Stroke = new SolidColorBrush(Colors.Black);
                ellipse.StrokeThickness = 2;
                ellipse.Fill = new SolidColorBrush(Colors.Red);

                mine.ellipse = ellipse;
                mine.text = text;

                Canvas.SetLeft(text, mine.position.left + (ellipse.Height / 2) - text.FontSize);
                Canvas.SetTop(text, mine.position.top + (ellipse.Height / 2) - text.FontSize);

                Canvas.SetLeft(ellipse, mine.position.left);
                Canvas.SetTop(ellipse, mine.position.top);

                Canvas.SetZIndex(text, 2);

                myCanvas.Children.Add(text);
                myCanvas.Children.Add(ellipse);
            }
        }

        private void Player_PlayerDied(object? sender, PlayerEventArgs e)
        {
            var Result = MessageBox.Show("Would you like to play again?", "You died!", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (Result == MessageBoxResult.Yes)
                StartGame();
            else
                Environment.Exit(0);
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            debugLabel.Content = $"active Uid: {Canvas.GetLeft(Player.Ellipse)}";
            if (Player.goLeft && Canvas.GetLeft(Player.Ellipse) > 0)
            {
                Canvas.SetLeft(Player.Ellipse, Canvas.GetLeft(Player.Ellipse) - Player.playerSpeed);
            }
            if(Player.goRight && Canvas.GetLeft(Player.Ellipse) + (Player.Ellipse.Width + 20) < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(Player.Ellipse, Canvas.GetLeft(Player.Ellipse) + Player.playerSpeed);
            }
            if(Player.goUp && Canvas.GetTop(Player.Ellipse) > 0)
            {
                Canvas.SetTop(Player.Ellipse, Canvas.GetTop(Player.Ellipse) - Player.playerSpeed);
            }
            if(Player.goDown && Canvas.GetTop(Player.Ellipse) + (Player.Ellipse.Height * 2) < Application.Current.MainWindow.Height)
            {
                Canvas.SetTop(Player.Ellipse, Canvas.GetTop(Player.Ellipse) + Player.playerSpeed);
            }

            //Update player position
            Player.position.left = Canvas.GetLeft(Player.Ellipse);
            Player.position.top = Canvas.GetTop(Player.Ellipse);

            //Check landMines distance from player
            foreach (LandMine mine in Mines)
            {
                double distance = mine.IsNearby(Player.position);
                if (distance < mine.ellipse.Width + 2)
                    mine.ellipse.Fill = new SolidColorBrush(Colors.Blue);
                else if (distance < 400 && Player.wave1) LaunchProjectile(mine);

                mine.text.Text = $"{Math.Round(distance, 0)}";
            }
            


            //Move projectiles
            List<Projectile> ToRemove = new List<Projectile>();
            foreach (Projectile projectile in projectiles)
            {
                TimeSpan TElapsed = projectile.Lifetime.Elapsed;
                double distance = projectile.GetTargetDistance();

                if (TElapsed.Seconds > 2)
                {
                    myCanvas.Children.Remove(projectile.Ellipse);
                    ToRemove.Add(projectile);
                }
                else if(distance > 20)
                {
                    double vx, vy, d;
                    vx = projectile.Target.left - Canvas.GetLeft(projectile.Ellipse);
                    vy = projectile.Target.top - Canvas.GetTop(projectile.Ellipse);
                    d = Math.Max(Math.Abs(vx), Math.Abs(vy));
                    vx /= d;
                    vy /= d;

                    Canvas.SetLeft(projectile.Ellipse, Canvas.GetLeft(projectile.Ellipse) + vx * projectile.Speed);
                    Canvas.SetTop(projectile.Ellipse, Canvas.GetTop(projectile.Ellipse) + vy * projectile.Speed);
                }
                else
                {
                    Player.TakeDamage(projectile.Damage);
                    healthBar.Value = Player.Health;
                    myCanvas.Children.Remove(projectile.Ellipse);
                    ToRemove.Add(projectile);
                }
            }
            //Remove projectiles
            foreach (Projectile projectile in ToRemove)
            {
                projectiles.Remove(projectile);
            }
        }

        private void LaunchProjectile(LandMine mine)
        {
            if (mine.stopwatch == null)
            {
                mine.stopwatch = new Stopwatch();
                mine.stopwatch.Start();
            }

            TimeSpan elapsed = mine.stopwatch.Elapsed;
            
            if (elapsed.TotalSeconds > 5)
            {

                Projectile projectile = new Projectile();
                projectile.Target = Player.position;
                projectile.Speed = Player.playerSpeed * 1.2;
                projectile.Ellipse = new Ellipse();

                projectile.Ellipse.Width = 20;
                projectile.Ellipse.Height = 20;
                projectile.Ellipse.Fill = new SolidColorBrush(Colors.DarkRed);
                projectile.Lifetime = new Stopwatch();
                projectile.Lifetime.Start();

                Canvas.SetLeft(projectile.Ellipse, mine.position.left);
                Canvas.SetTop(projectile.Ellipse, mine.position.top);

                Canvas.SetZIndex(projectile.Ellipse, 2);

                projectiles.Add(projectile);
                myCanvas.Children.Add(projectile.Ellipse);

                mine.stopwatch.Restart();
            }
        }

        private void myCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            Player.PlayerAction(e.Key, true);
        }

        private void myCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            Player.PlayerAction(e.Key, false);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(Player != null)
                Player.wave1 = true;
        }

        private void enableAttack_Unchecked(object sender, RoutedEventArgs e)
        {
            Player.wave1 = false;
        }
    }
}
