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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Pokemon.Models;
using Pokemon.Services;


namespace Pokemon.View
{
    /// <summary>
    /// Logique d'interaction pour combat.xaml
    /// </summary>
    public partial class combat : UserControl
    {
        // ⚡ Champ de classe accessible dans toutes les méthodes
        private List<Root> poke;

        public combat()
        {
            InitializeComponent();

            // Initialisation de la liste de Pokémon
            poke = LoadAllPokemons();

            RandomPokemonButton.Click += RandomPokemonButton_Click;
        }

        private void RandomPokemonButton_Click(object sender, RoutedEventArgs e)
        {
            // Pokémon 1 aléatoire
            var pokemon1 = PokemonHelper.GetRandomPokemon(poke);
            if (pokemon1 != null)
            {
                Pokemon1Name.Text = pokemon1.name?.fr ?? "???";
                Pokemon1HP.Text = pokemon1.stats?.hp.ToString() ?? "0";
                Pokemon1Attack.Text = pokemon1.stats?.atk.ToString() ?? "0";
                Pokemon1Defense.Text = pokemon1.stats?.def.ToString() ?? "0";
                Pokemon1Speed.Text = pokemon1.stats?.vit.ToString() ?? "0";
                Pokemon1Image.Source = PokemonHelper.GetPokemonImage(pokemon1);
            }

            // Pokémon 2 aléatoire
            var pokemon2 = PokemonHelper.GetRandomPokemon(poke);
            if (pokemon2 != null)
            {
                Pokemon2Name.Text = pokemon2.name?.fr ?? "???";
                Pokemon2HP.Text = pokemon2.stats?.hp.ToString() ?? "0";
                Pokemon2Attack.Text = pokemon2.stats?.atk.ToString() ?? "0";
                Pokemon2Defense.Text = pokemon2.stats?.def.ToString() ?? "0";
                Pokemon2Speed.Text = pokemon2.stats?.vit.ToString() ?? "0";
                Pokemon2Image.Source = PokemonHelper.GetPokemonImage(pokemon2);
            }

            ResultText.Text = "Pokémons choisis !";
        }

        private List<Root> LoadAllPokemons()
        {
            // Exemple : charger depuis JSON ou API
            // return JsonConvert.DeserializeObject<List<Root>>(File.ReadAllText("pokemons.json"));
            return new List<Root>(); // Placeholder
        }
    }
}
