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
        public Sprites sprites;
        public Gmax gmax;
        public Mega mega;
        public Evolution evolution;
        public MainWindow()
        {
            InitializeComponent();
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
                evolution = asyncTask.evolution;
                Root root = asyncTask;        
                Sexe sexe = asyncTask.sexe;
                sprites = asyncTask.sprites;
                Stats stats = asyncTask.stats;
                Talent talent = asyncTask.talents[0];
                TypePokemon typePokemon = asyncTask.types[0];
                List<Forme> formes = asyncTask.formes;
                gmax = asyncTask.gmax;
                mega = asyncTask.mega;
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
                NomEN.Text = name.en;
                NomJP.Text = name.jp;
                CategoriePokemon.Text=root.category;
                GenerationPokemon.Text=root.generation.ToString();
                TaillePokemon.Text = root.height.ToString();
                PoidsPokemon.Text = root.weight.ToString();
                string talent1 = root.talents[0].name;
                bool tc1=root.talents[0].tc;
                if (talent1 != "Null")
                {
                    if (tc1)
                    {
                        TalentsPokemon.Items.Add(talent1 + " Talent caché");
                    }
                    else
                    {
                        TalentsPokemon.Items.Add(talent1 + " Talent non caché");
                    }
                }
                string talent2 = root.talents[1].name;
                bool tc2 = root.talents[1].tc;
                if (talent1 != "Null")
                {
                    if (tc1)
                    {
                        TalentsPokemon.Items.Add(talent2 + " Talent caché");
                    }
                    else
                    {
                        TalentsPokemon.Items.Add(talent2 + " Talent non caché");
                    }
                }


            }
        }

        private async void BttRecherche_Click(object sender, RoutedEventArgs e)
        {
            ShinyCheckBox.IsChecked = false;
            GmaxCheckBox.IsChecked = false;
            MegaCheckBox.IsChecked = false;
            TalentsPokemon.Items.Clear();
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
            _: GetPokemon(idPoke);
            }
        }

        private void ShinyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (GmaxCheckBox.IsChecked == false && MegaCheckBox.IsChecked == false)
            {
                if (!string.IsNullOrEmpty(sprites?.shiny))
                {
                    ImgPokemon.Source = new BitmapImage(new Uri(sprites.shiny));
                }
                else
                {
                    ShinyCheckBox.IsChecked = false;
                    ImgPokemon.Source = new BitmapImage(new Uri(sprites.regular));
                    MessageBox.Show("Ce pokemon ne dispose pas de version Shiny.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (GmaxCheckBox.IsChecked == true)
            {
                if (!string.IsNullOrEmpty(sprites.gmax?.shiny))
                {
                    ImgPokemon.Source = new BitmapImage(new Uri(sprites.gmax.shiny));
                }
                else
                {
                    ShinyCheckBox.IsChecked = false;
                    ImgPokemon.Source = new BitmapImage(new Uri(sprites.gmax.regular));
                    MessageBox.Show("Ce pokemon ne dispose pas de version Shiny pour la version Gmax.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(sprites?.shiny))
                {
                    ImgPokemon.Source = new BitmapImage(new Uri(evolution.mega[0].sprites.shiny));
                }
                else
                {
                    ShinyCheckBox.IsChecked = false;
                    ImgPokemon.Source = new BitmapImage(new Uri(evolution.mega[0].sprites.regular));
                    MessageBox.Show("Ce pokemon ne dispose pas de version Shiny pour sa Mega Evolution.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ShinyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ImgPokemon.Source = new BitmapImage(new Uri(sprites.regular));
        }

        private void GmaxCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShinyCheckBox.IsChecked=false;
            MegaCheckBox.IsChecked=false;
            if (sprites?.gmax != null && !string.IsNullOrEmpty(sprites.gmax?.regular))
            {
                ImgPokemon.Source = new BitmapImage(new Uri(sprites.gmax.regular));
            }
            else
            {
                GmaxCheckBox.IsChecked = false;
                ShinyCheckBox.IsChecked = false;
                ImgPokemon.Source = new BitmapImage(new Uri(sprites.regular));
                MessageBox.Show("Ce pokemon ne dispose pas de version Gmax.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GmaxCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ImgPokemon.Source = new BitmapImage(new Uri(sprites.regular));
        }

        private void MegaCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShinyCheckBox.IsChecked = false;
            GmaxCheckBox.IsChecked=false;
            if (evolution?.mega != null &&
    evolution.mega.Count > 0 &&
    !string.IsNullOrEmpty(evolution.mega[0]?.sprites?.regular))
            {
                ImgPokemon.Source = new BitmapImage(new Uri(evolution.mega[0].sprites.regular));
            }
            else
            {
                MegaCheckBox.IsChecked = false;
                ShinyCheckBox.IsChecked = false;
                ImgPokemon.Source = new BitmapImage(new Uri(sprites.regular));
                MessageBox.Show("Ce pokemon ne peut pas Mega Evolué.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MegaCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ImgPokemon.Source = new BitmapImage(new Uri(sprites.regular));
        }
    }
}