using Pokemon.Services;
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
    /// Logique d'interaction pour combat.xaml
    /// </summary>
    public partial class combat : Window
    {
        public GetPokemon getPokemon;
        public Sprites sprites;
        public Gmax gmax;
        public Mega mega;
        public Evolution evolution1;
        public Root root;
        public combat()
        {
            InitializeComponent();
            getPokemon = new GetPokemon();
            Choisirpoke();
        }
        private async Task Choisirpoke()
        {
            Random rnd = new Random();
            int poke1 = rnd.Next(1, 1026); 
            int poke2 = rnd.Next(1, 1026);
            var asyncTask1 = await getPokemon.GetApiPokemon(poke1.ToString());
            var asyncTask2 = await getPokemon.GetApiPokemon(poke2.ToString());
            Evolution evolution1 = asyncTask1.evolution;
            Evolution evolution = asyncTask2.evolution;
            Root root1 = asyncTask1;
            Root root2 = asyncTask2;
            Sprites sprites1 = asyncTask1.sprites;
            Sprites sprites2 = asyncTask2.sprites;
            Stats stats1 = asyncTask1.stats;
            Stats stats2 = asyncTask2.stats;
            Talent talent1 = asyncTask1.talents[0];
            Talent talent2 = asyncTask2.talents[0];
            Gmax gmax1 = asyncTask1.gmax;
            Gmax gmax2 = asyncTask2.gmax;
            Mega mega1 = asyncTask1.mega;
            Mega mega2 = asyncTask2.mega;
            Name name1 = asyncTask1.name;
            Name name2 = asyncTask2.name;
            TypePokemon types1 = asyncTask1.types[0];
            TypePokemon types2 = asyncTask2.types[0];
            Type1Pokemon.Source = new BitmapImage(new Uri(root.types[0].image));
            try
            {
                Type2Pokemon.Source = new BitmapImage(new Uri(root.types[1].image));
            }
            catch (Exception ex)
            {
                Type2Pokemon.Source = null;
            }
            List<Resistance> resistances1 = asyncTask1.resistances;
            List<Resistance> resistances2 = asyncTask2.resistances;
            PlayerPokemonSprite.Source = new BitmapImage(new Uri(sprites1.regular));
            EnemyPokemonSprite.Source = new BitmapImage(new Uri(sprites2.regular));
            PlayerPokemonName.Text=name1.fr;
            EnemyPokemonName.Text=name2.fr;
            PlayerPokemonHPBar.Value = stats1.hp;
            EnemyPokemonHPBar.Value = stats2.hp;
            PlayerPokemonHPBar.Maximum = stats1.hp;
            EnemyPokemonHPBar.Maximum = stats2.hp;
            PlayerPokemonHPText.Text= stats1.hp.ToString()+"/"+stats1.hp.ToString();
            EnemyPokemonHPText.Text= stats2.hp.ToString() + "/" + stats2.hp.ToString();

        }
        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
