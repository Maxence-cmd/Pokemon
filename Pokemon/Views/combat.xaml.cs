using Newtonsoft.Json;
using Pokemon.Models;
using Pokemon.Models;
using Pokemon.Services;
using Pokemon.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shapes;

namespace Pokemon.Views
{
    /// <summary>
    /// Logique d'interaction pour combat.xaml
    /// </summary>
    public partial class combat : Window
    {
        public List<Resistance> resistances1;
        public List<Resistance> resistances2;
        public GetPokemon getPokemon;
        public Sprites sprites;
        public Gmax gmax;
        public Mega mega;
        public Evolution evolution1;
        public Root root;
        public  Root root1;
        public Root root2;
        public Stats stats1;
        public  Stats stats2;
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
             root1 = asyncTask1;
             root2 = asyncTask2;
            Sprites sprites1 = asyncTask1.sprites;
            Sprites sprites2 = asyncTask2.sprites;
             stats1 = asyncTask1.stats;
             stats2 = asyncTask2.stats;
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
            try
            {
                TypePokemon types12 =asyncTask1.types[1];
            }
            catch (Exception ex)
            {
                TypePokemon types12 = null;
            }
            try
            {
                TypePokemon types22 = asyncTask1.types[1];
            }
            catch (Exception ex)
            {
                TypePokemon types22 = null;
            }
            resistances1 = asyncTask1.resistances;
            resistances2 = asyncTask2.resistances;
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
        public double GetEfficaciteDepuisAPI(List<Resistance> resistances, string typeAttaque)
        {
            var res = resistances.FirstOrDefault(r => r.name == typeAttaque);

            if (res != null)
                return res.multiplier;

            return 1.0;
        }
        private async void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            GererViePoke gestion = new GererViePoke();
            int niveauJoueur;
            int niveauEnnemi;
            Random rnd = new Random();
            niveauJoueur = rnd.Next(30, 60);
            niveauEnnemi = rnd.Next(30, 60);
            int puissance = 60;
            int attaqueEnnemi = stats2.atk;
            int defenseJoueur = stats1.def;
            int attaqueJoueur = stats1.atk;
            int defenseEnnemi = stats2.def;
            // ========================
            // ATTAQUE JOUEUR
            // ========================
            int pvEnnemi = (int)EnemyPokemonHPBar.Value;
            double efficacite = GetEfficaciteDepuisAPI(resistances2, "Feu");
            var result = gestion.CalculerDegats(
    niveauEnnemi,
    attaqueEnnemi,
    defenseJoueur,
    puissance,
     stab: 1.0,
     efficacite: efficacite
 );

            int degatsJoueur = result.degats;
            bool critique = result.critique;
            bool esquive = result.esquive;
           
            pvEnnemi = gestion.AppliquerDegats(pvEnnemi, degatsJoueur);

            EnemyPokemonHPBar.Value = pvEnnemi;
            EnemyPokemonHPText.Text = pvEnnemi + " / " + EnemyPokemonHPBar.Maximum;
            string message = "";
            if (esquive)
            {
                message = "L'ennemi a esquivé l'attaque !";
            }
            else
            {
                ShakePokemon(EnemyShakeTransform);
                if (critique)
                    message += "💥 Coup critique !";
                else
                    message += "Coup normal.\n";
                if (efficacite > 1)
                    message += "C'est super efficace !";
                else if (efficacite < 1)
                    message += "Ce n'est pas très efficace...";
            }

                if (message != "")
                    MessageBox.Show(message);
            
                if (gestion.EstKO(pvEnnemi))
                {
                    MessageBox.Show(root2.name.fr + " est KO !");

                }
            
                await Task.Delay(500); // Pause avant l'attaque ennemie
                                       // ========================
                                       // ATTAQUE ENNEMI
                                       // ========================



                int pvJoueur = (int)PlayerPokemonHPBar.Value;
                var degatsEnnemi = gestion.CalculerDegats(
                    niveauJoueur,
                    attaqueJoueur,
                    defenseEnnemi,
                    puissance
                );

                int degatsEne = degatsEnnemi.degats;
                bool critiqueEn = degatsEnnemi.critique;
                bool esquiveEne = degatsEnnemi.esquive;

                pvJoueur = gestion.AppliquerDegats(pvJoueur, degatsEne);
                
                PlayerPokemonHPBar.Value = pvJoueur;
                PlayerPokemonHPText.Text = pvJoueur + " / " + PlayerPokemonHPBar.Maximum;
                string message2 = "";
                if (esquiveEne)
                {
                    message2 = "Vous avez esquivé l'attaque !";
                }
                else
                {
                ShakePokemon(PlayerShakeTransform);
                if (critique)
                        message2 += "💥 Coup critique !\n";

                    if (efficacite > 1)
                        message2 += "C'est super efficace !";
                    else if (efficacite < 1)
                        message2 += "Ce n'est pas très efficace...";
                }
                if (message2 != "")
                    MessageBox.Show(message2);
                if (gestion.EstKO(pvJoueur))
                {
                    MessageBox.Show(root1.name.fr + " est KO !");
                    return;
                }

            }
        
        public void ShakePokemon(TranslateTransform transform)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = -15,
                To = 15,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(5)
            };

            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
    }
}
