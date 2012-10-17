using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Lines
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Play : Lines.Common.LayoutAwarePage
    {
        public Play()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }


        Button[,] Field = new Button[9,9];
        Button[,] Balls = new Button[9,9];
        Brush bgBrush = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
        Brush trBrush = new SolidColorBrush(Color.FromArgb(0, 240, 240, 240));

        Brush[] Colors = new Brush[] { new SolidColorBrush(Color.FromArgb(255, 255, 127, 127)), new SolidColorBrush(Color.FromArgb(255, 127, 255, 127)), new SolidColorBrush(Color.FromArgb(255, 127, 127, 255)),
                                       new SolidColorBrush(Color.FromArgb(255, 255, 255, 127)), new SolidColorBrush(Color.FromArgb(255, 255, 127,255)), new SolidColorBrush(Color.FromArgb(255, 127, 255, 255)),     
                                       new SolidColorBrush(Color.FromArgb(255, 255, 53, 127)) };


        int dx = -1, dy = -1;


        private async void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    var b = new Button();
                    b.SetValue(Grid.RowProperty, i);
                    b.SetValue(Grid.ColumnProperty, j);
                    b.Tag = i * 9 + j;
                    b.Click += play_Click;
                    b.PointerPressed += (s, u) => play_Click(s, null);
                    b.Margin = new Thickness(1);
                    b.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                    b.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
                    b.BorderThickness = new Thickness(0);
                    b.Background = bgBrush;
                    Field[i, j] = b;
                    GameField.Children.Add(b);
                }

            var rs = ApplicationData.Current.RoamingSettings.Values;

            if (rs.ContainsKey("hiscore"))
                UpdateHighScore((int)rs["hiscore"]);

            var tosave = false;
            if (rs.ContainsKey("savedata"))
                if ((int)rs["force"] == 1) tosave = true;
                else tosave = await Question.Show("You have game saved from previous time. Do you want to continue or start from scratch?", "Lines", "Continue", "Restart");

            logic = new GameLogic();
            seed = logic.seed;

            logic.Appeared += logic_Appeared;
            logic.Disappeared += logic_Disappeared;
            logic.PointsChanged += logic_PointsChanged;
            logic.GameOver += logic_GameOver;

            if (tosave)
                logic.Load(rs["savedata"] as string);
            else
            {
                ApplicationData.Current.RoamingSettings.Values.Remove("savedata");
                logic.Start();
            }
            GameOn = true;
        }

        bool GameOn = false;

        async void logic_GameOver(object sender, int e)
        {
            GameOn = false;
            UpdateHighScore(logic.Score);
            ApplicationData.Current.RoamingSettings.Values.Remove("savedata");
            if (await Question.Show("Game over! Restart?", "Lines", "Restart", "No"))
                logic.Restart();           
        }

        int Hiscore = 0;

        void UpdateHighScore(int Value)
        {
            if (Hiscore > Value) return;

            Hiscore = Value;
            hiscoreTitle.Text = Value.ToString();
            ApplicationData.Current.RoamingSettings.Values["hiscore"] = Value;
        }

        void logic_PointsChanged(object sender, IntPoint e)
        {
            this.pointsTitle.Text = e.Data.ToString();
        }

        void play_Click(object sender, RoutedEventArgs e)
        {
            if (dx == -1 && dy == -1) return;
            var t = (int)((sender as Button).Tag);

            if (logic.Move(new IntPoint { X = dx, Y = dy }, new IntPoint { X = t / 9, Y = t % 9 })) UnselectBalls();
        }

        void logic_Disappeared(object sender, IntPoint e)
        {
            GameField.Children.Remove(Balls[e.X, e.Y]);
            Field[e.X, e.Y].Visibility = Visibility.Visible;
        }

        void logic_Appeared(object sender, IntPoint e)
        {
            var b = new Button();
            b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 127, 127, 146));
            b.Background = Colors[e.Data-1];
            b.Style = Application.Current.Resources["RoundButton"] as Style;
            b.Margin = new Thickness(1);
            b.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            b.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
            b.Tag = new Tuple<int, int>(e.X, e.Y);
            b.SetValue(Grid.RowProperty, e.X);
            b.SetValue(Grid.ColumnProperty, e.Y);
            b.Click += (s, u) => SelectBall((s as Button).Tag as Tuple<int, int>);
            GameField.Children.Add(b);
            Balls[e.X, e.Y] = b;
            Field[e.X, e.Y].Visibility = Visibility.Collapsed;

            if (GameOn) ApplicationData.Current.RoamingSettings.Values["savedata"] = logic.Save();
        }

        GameLogic logic;
        Random seed;

        private void SelectBall(int i, int j)
        {
            var thick = new Thickness(4);
            UnselectBalls();

            dx = i;
            dy = j;

            Balls[i, j].BorderThickness = thick;
        }
        
        private void UnselectBalls() {
            var noThick = new Thickness(0);
            foreach (Button b in Balls)
            {
                if (b == null) continue;
                b.BorderThickness = noThick;
            }
            dx = dy = -1;
        }

        private void SelectBall(Tuple<int, int> t) { SelectBall(t.Item1, t.Item2); }       
    }
}
