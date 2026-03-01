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
using Pokemon.Models;

namespace Pokemon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public GetPokemon getPokemon;
        public GetIDFromName getIDFromName;
        public MainWindow()
        {
            InitializeComponent();
            getIDFromName = new GetIDFromName();
            getPokemon = new GetPokemon();
            string idPoke = "1";
        _: GetPokemon(idPoke);
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
        public async Task GetPokemon(string numPoke)
        {  
            var asyncTask = await getPokemon.GetApiPokemon(numPoke);
            if (asyncTask == null || asyncTask.stats == null)
            {
                MessageBox.Show("Pokémon introuvable, veuillez vérifier le nom.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
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
                NumPokemon.Text = "#" + root.pokedex_id.ToString();
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
                PVNumPokemon.Text = stats.hp.ToString();
                AttPokemon.Value = stats.atk;
                AttNumPokemon.Text = stats.atk.ToString();
                DefPokemon.Value = stats.def;
                DefNumPokemon.Text = stats.def.ToString();
                DefSpePokemon.Value = stats.spe_def;
                DefSpeNumPokemon.Text = stats.spe_def.ToString();
                AttSpePokemon.Value = stats.spe_atk;
                AttSpeNumPokemon.Text = stats.spe_atk.ToString();
                VitPokemon.Value = stats.vit;
                VitNumPokemon.Text = stats.vit.ToString();
            }
        }

        private async void BttRecherche_Click(object sender, RoutedEventArgs e)
        {

            string idPoke = NumPokeRecherche.Text.Trim().ToLower();


            if (string.IsNullOrWhiteSpace(idPoke))
            {
                MessageBox.Show("Veuillez entrer un nom ou un numéro de Pokémon valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (int.TryParse(idPoke, out int numero))
            {
                if (numero < 1 || numero > 1025)
                {
                    MessageBox.Show("Veuillez entrer un numéro de Pokémon valide (entre 1 et 1025).", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _: GetPokemon(numero.ToString());
            }
            else
            { 
                   await GetPokemon(idPoke);
                
                //if ((string.IsNullOrWhiteSpace(idPoke)))
                //{
                //// On a trouvé le Pokémon, on appelle GetPokemon avec l'ID
                //_: GetPokemon(idPoke);
                //}
                //else
                //{
                //    MessageBox.Show("Pokémon introuvable, veuillez vérifier le nom.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }
        }
    }
}