using Pokemon.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Pokemon.Services;
using Newtonsoft.Json;

namespace Pokemon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            API api = new API();
            GetPokemon getPokemon = new GetPokemon();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == "Nom ou numéro")
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black; // texte utilisateur en noir
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "Nom ou numéro";
                tb.Foreground = Brushes.Gray; // placeholder gris
            }
        }

    }
}