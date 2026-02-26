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
        public GetPokemon getPokemon;
        public MainWindow()
        {
            InitializeComponent();
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
        public async void callMeteo(string ville)
        {
            var asyncTask = await getPokemon.GetApiPokemon(ville);
            Evolution evolution = asyncTask.evolution;
            Root root = asyncTask;
            Sexe sexe = asyncTask.sexe;
            Sprites sprites = asyncTask.sprites;
            Stats stats = asyncTask.stats;
            Talent talent = asyncTask.talents[0];
            TypePokemon typePokemon = asyncTask.types[0];
            List<Forme> formes = asyncTask.formes; 
            Gmax gmax = asyncTask.gmax;
            Mega mega = asyncTask.mega;
            Name name = asyncTask.name;
            Next next = asyncTask.next;
            Pre pre = asyncTask.pre;
            List<Resistance> resistances = asyncTask.resistances;
            NomPokemon.Text = name.fr;
            NumPokemon.Text = root.pokedex_id.ToString();
            ImgPokemon.Source = new BitmapImage(new Uri(sprites.regular));
            Type1Pokemon.Source = new BitmapImage(new Uri(root.types[0].image));
            try 
            {
                Type2Pokemon.Source = new BitmapImage(new Uri(root.types[1].image));
            }
                catch (Exception ex)
            {
                Type2Pokemon.Source = null;
            }
            PVPokemon.Value = stats.hp;
            AttPokemon.Value = stats.atk;
            DefPokemon.Value = stats.def;
            DefSpePokemon.Value = stats.spe_def;
            AttSpePokemon.Value = stats.spe_atk;
            VitPokemon.Value = stats.vit;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}