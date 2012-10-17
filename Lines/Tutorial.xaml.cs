using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class Tutorial : Lines.Common.LayoutAwarePage
    {
        public Tutorial()
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

        Brush bgBrush = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
        Button[,] Field = new Button[9, 9];
        Button[,] Balls = new Button[9, 9];
        Brush[] Colors = new Brush[] { new SolidColorBrush(Color.FromArgb(255, 255, 127, 127)), new SolidColorBrush(Color.FromArgb(255, 127, 255, 127)), new SolidColorBrush(Color.FromArgb(255, 127, 127, 255)),
                                       new SolidColorBrush(Color.FromArgb(255, 255, 255, 127)), new SolidColorBrush(Color.FromArgb(255, 255, 127,255)), new SolidColorBrush(Color.FromArgb(255, 127, 255, 255)),     
                                       new SolidColorBrush(Color.FromArgb(255, 255, 53, 127)) };


        private void Tutorial_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    var b = new Button();
                    b.SetValue(Grid.RowProperty, i);
                    b.SetValue(Grid.ColumnProperty, j);
                    b.Tag = i * 9 + j;
                    b.Margin = new Thickness(1);
                    b.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                    b.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
                    b.BorderThickness = new Thickness(0);
                    b.Background = bgBrush;
                    Field[i, j] = b;
                    GameField.Children.Add(b);
                }
            Stage1();
        }

        void Remove(int i, int j)
        {
            GameField.Children.Remove(Balls[i,j]);
            Field[i,j].Visibility = Visibility.Visible;
        }

        void Add(int i, int j, int c, Action<Button> action)
        {
            var b = new Button();
            b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 127, 127, 146));
            b.Background = Colors[c];
            b.Style = Application.Current.Resources["RoundButton"] as Style;
            b.Margin = new Thickness(1);
            b.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            b.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
            b.Tag = 1;
            b.SetValue(Grid.RowProperty, i);
            b.SetValue(Grid.ColumnProperty, j);
            b.Click += (s, e) => action(s as Button);
            GameField.Children.Add(b);
            Balls[i,j] = b;
            Field[i,j].Visibility = Visibility.Collapsed;
        }

        int dx, dy;

        private void SelectBall(Button p)
        {            
            var thick = new Thickness(4);
            UnselectBalls();

            dx = (int) p.GetValue(Grid.RowProperty);
            dy = (int) p.GetValue(Grid.ColumnProperty);

            p.BorderThickness = thick;
        }

        private void UnselectBalls()
        {
            var noThick = new Thickness(0);
            foreach (Button b in Balls)
            {
                if (b == null) continue;
                b.BorderThickness = noThick;
            }
            dx = dy = -1;
        }

        private void Stage1()
        {
            tutorialToDo.Text = "To move a ball, select it:";
            GameField.Children.Clear();
            foreach (Button b in Field) GameField.Children.Add(b);
            Button[,] Balls = new Button[9, 9];
            Add(4, 5, 3, b => { SelectBall(b); Stage2(); });
        }

        private void Stage2()
        {
            tutorialToDo.Text = "Now click on the destination field to move the ball:";
            Field[6, 7].Background = Colors[4];
            Field[6, 7].Click += Stage2_DestClick;
        }

        private void Stage3()
        {
            Field[6, 7].Visibility = Visibility.Visible;
            tutorialToDo.Text = "To remove the line, get 5 in the row:";
            GameField.Children.Clear();
            Button[,] Balls = new Button[9, 9];
            foreach (Button b in Field) GameField.Children.Add(b);
            Add(4, 0, 3, b => { SelectBall(b); });
            Add(4, 1, 3, b => { SelectBall(b); });
            Add(4, 2, 3, b => { SelectBall(b); });
            Add(4, 3, 3, b => { SelectBall(b); });
            Add(7, 5, 3, b => { SelectBall(b); });
            Field[4, 4].Click += Stage3_DestClick;
        }

        private async void Stage3_DestClick(object sender, RoutedEventArgs e)
        {
            if (dx != 7 || dy != 5) return;
            tutorialToDo.Text = "Good Job!";
            Remove(7, 5);
            Add(4, 4, 3, (b) => { });
            await Task.Delay(2000);
            await new MessageDialog("Remember that you need a path to move balls between cells. Good luck!", "Lines").ShowAsync();
            Frame.GoBack();
        }

        private async void Stage2_DestClick(object sender, RoutedEventArgs e)
        {
            tutorialToDo.Text = "Good Job!";
            Field[6, 7].Background = bgBrush;
            Remove(4, 5);
            Add(6, 7, 3, (b) => { });
            await Task.Delay(2000);
            Stage3();
        }
    }
}
