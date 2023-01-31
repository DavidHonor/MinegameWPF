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
        Player player;
        List<LandMine> mines;
        List<Projectile> projectiles;

        public MainWindow()
        {
            InitializeComponent();
            StartGame();
        }

        private void StartGame()
        {
            myCanvas.Children.Clear();

            //Init variables
            GameTimer = new DispatcherTimer();
            player = new Player(new Position(1, 1), "Levi", 100, 3);
            mines = new List<LandMine>();
            projectiles = new List<Projectile>();

            //Mines
            mines.Add(new LandMine(new Position(50, 100), false, 50));
            mines.Add(new LandMine(new Position(50, 170), false, 50));
            InitGameElements();

            player.PlayerDied += Player_PlayerDied;
            GameTimer.Tick += GameTimer_Tick;
            GameTimer.Interval = TimeSpan.FromMilliseconds(20);

            myCanvas.Focus();
            GameTimer.Start();
        }

        private void InitGameElements()
        {
            foreach (LandMine mine in mines)
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
            if (player.goLeft && Canvas.GetLeft(Player) > 0)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - player.playerSpeed);
            }
            if(player.goRight && Canvas.GetLeft(Player) + (Player.Width + 20) < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + player.playerSpeed);
            }
            if(player.goUp && Canvas.GetTop(Player) > 0)
            {
                Canvas.SetTop(Player, Canvas.GetTop(Player) - player.playerSpeed);
            }
            if(player.goDown && Canvas.GetTop(Player) + (Player.Height * 2) < Application.Current.MainWindow.Height)
            {
                Canvas.SetTop(Player, Canvas.GetTop(Player) + player.playerSpeed);
            }

            //Update player position
            player.position.left = Canvas.GetLeft(Player);
            player.position.top = Canvas.GetTop(Player);

            //Check landmines distance from player
            foreach (LandMine mine in mines)
            {
                double distance = mine.IsNearby(player.position);
                if (distance < mine.ellipse.Width + 2)
                    mine.ellipse.Fill = new SolidColorBrush(Colors.Blue);
                else if (distance < 400 && player.wave1) LaunchProjectile(mine);

                mine.text.Text = $"{Math.Round(distance, 0)}";
            }
            debugLabel.Content = $"active projectiles: {projectiles.Count}";


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
                    player.TakeDamage(projectile.Damage);
                    healthBar.Value = player.Health;
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
                projectile.Target = player.position;
                projectile.Speed = player.playerSpeed * 1.2;
                projectile.Ellipse = new Ellipse();

                projectile.Ellipse.Width = 20;
                projectile.Ellipse.Height = 20;
                projectile.Ellipse.Fill = new SolidColorBrush(Colors.Green);
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
            player.PlayerAction(e.Key, true);
        }

        private void myCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            player.PlayerAction(e.Key, false);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(player != null)
                player.wave1 = true;
        }

        private void enableAttack_Unchecked(object sender, RoutedEventArgs e)
        {
            player.wave1 = false;
        }
    }
}
