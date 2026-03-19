using System;
using System.Windows;
using System.Windows.Controls;
using Pokemon.Models;
using Pokemon.Services;

namespace Pokemon.View
{
    public partial class combat : UserControl
    {
        private Root currentPokemon1;
        private Root currentPokemon2;

        private GetPokemon api = new GetPokemon();
        private Random rnd = new Random();

        public combat()
        {
            InitializeComponent();
        }

        // 🎲 Bouton aléatoire
        private async void RandomPokemonButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id1 = rnd.Next(1, 1025);
                int id2 = rnd.Next(1, 1025);

                currentPokemon1 = await api.GetApiPokemon(id1.ToString());
                currentPokemon2 = await api.GetApiPokemon(id2.ToString());

                // Pokémon 1
                if (currentPokemon1 != null)
                {
                    Pokemon1Name.Text = currentPokemon1.name?.fr ?? "???";
                    Pokemon1HP.Text = currentPokemon1.stats?.hp.ToString() ?? "0";
                    Pokemon1Attack.Text = currentPokemon1.stats?.atk.ToString() ?? "0";
                    Pokemon1Defense.Text = currentPokemon1.stats?.def.ToString() ?? "0";
                    Pokemon1Speed.Text = currentPokemon1.stats?.vit.ToString() ?? "0";
                    Pokemon1Image.Source = PokemonHelper.GetPokemonImage(currentPokemon1);
                }

                // Pokémon 2
                if (currentPokemon2 != null)
                {
                    Pokemon2Name.Text = currentPokemon2.name?.fr ?? "???";
                    Pokemon2HP.Text = currentPokemon2.stats?.hp.ToString() ?? "0";
                    Pokemon2Attack.Text = currentPokemon2.stats?.atk.ToString() ?? "0";
                    Pokemon2Defense.Text = currentPokemon2.stats?.def.ToString() ?? "0";
                    Pokemon2Speed.Text = currentPokemon2.stats?.vit.ToString() ?? "0";
                    Pokemon2Image.Source = PokemonHelper.GetPokemonImage(currentPokemon2);
                }

                ResultText.Text = "Pokémons choisis ! Clique sur combattre ⚔️";
            }
            catch (Exception ex)
            {
                ResultText.Text = "Erreur : " + ex.Message;
            }
        }

        // ⚔️ Combat
        private void FightButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPokemon1 == null || currentPokemon2 == null)
            {
                ResultText.Text = "Choisis d'abord les Pokémon !";
                return;
            }

            string result = SimulateFight(currentPokemon1, currentPokemon2);
            ResultText.Text = result;
        }

        // 💥 Logique combat
        private string SimulateFight(Root p1, Root p2)
        {
            int hp1 = p1.stats?.hp ?? 0;
            int hp2 = p2.stats?.hp ?? 0;

            int atk1 = p1.stats?.atk ?? 0;
            int def1 = p1.stats?.def ?? 0;
            int vit1 = p1.stats?.vit ?? 0;

            int atk2 = p2.stats?.atk ?? 0;
            int def2 = p2.stats?.def ?? 0;
            int vit2 = p2.stats?.vit ?? 0;

            bool p1Turn = vit1 >= vit2;

            while (hp1 > 0 && hp2 > 0)
            {
                if (p1Turn)
                {
                    int damage = Math.Max(1, atk1 - def2 / 2);
                    hp2 -= damage;
                }
                else
                {
                    int damage = Math.Max(1, atk2 - def1 / 2);
                    hp1 -= damage;
                }

                p1Turn = !p1Turn;
            }

            return hp1 > 0
                ? $"{p1.name?.fr} gagne ! 🏆"
                : $"{p2.name?.fr} gagne ! 🏆";
        }
    }
}