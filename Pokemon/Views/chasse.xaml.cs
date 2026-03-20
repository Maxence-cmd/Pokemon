using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pokemon.Views
{
    /// <summary>
    /// Logique d'interaction pour chasse.xaml
    /// </summary>
    public partial class chasse : Window
    {
        public chasse()
        {
            InitializeComponent();
        }
        double speed = 5;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            double x = Canvas.GetLeft(perso);
            double y = Canvas.GetTop(perso);

            if (e.Key == Key.Left)
            {
                Canvas.SetLeft(perso, x - speed);
            }
            else if (e.Key == Key.Right)
            {
                Canvas.SetLeft(perso, x + speed);
            }
            else if (e.Key == Key.Up)
            {
                Canvas.SetTop(perso, y - speed);
            }
            else if (e.Key == Key.Down)
            {
                Canvas.SetTop(perso, y + speed);
            }
        }
    }
}
