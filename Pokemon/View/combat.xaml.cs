using System;
using System.Windows;
using System.Windows.Controls;
using Pokemon.Models;
using Pokemon.Services;

namespace Pokemon.View
{
    public partial class combat : UserControl
    {
        private PokemonCombat player1;
        private PokemonCombat player2;

        private GetPokemon api = new GetPokemon();
        private Random rnd = new Random();

        private bool isPlayer1Turn;
        private bool fightEnded = false;

        public combat()
        {
            InitializeComponent();
        }

        // 🎲 Choisir Pokémon
        private async void RandomPokemonButton_Click(object sender, RoutedEventArgs e)
        {
            fightEnded = false;

            int id1 = rnd.Next(1, 1025);
            int id2 = rnd.Next(1, 1025);

            var p1 = await api.GetApiPokemon(id1.ToString());
            var p2 = await api.GetApiPokemon(id2.ToString());

            player1 = new PokemonCombat(p1);
            player2 = new PokemonCombat(p2);

            InitUI();

            // Qui commence
            isPlayer1Turn = p1.stats.vit >= p2.stats.vit;

            UpdateTurnUI();

            ResultText.Text = isPlayer1Turn
                ? "Tour du Joueur 1 ⚔️"
                : "Tour du Joueur 2 ⚔️";
        }

        // 🎨 UI
        private void InitUI()
        {
            // J1
            Pokemon1Name.Text = player1.Pokemon.name.fr;
            Pokemon1Attack.Text = player1.Pokemon.stats.atk.ToString();
            Pokemon1Defense.Text = player1.Pokemon.stats.def.ToString();
            Pokemon1Speed.Text = player1.Pokemon.stats.vit.ToString();
            Pokemon1Image.Source = PokemonHelper.GetPokemonImage(player1.Pokemon);

            // J2
            Pokemon2Name.Text = player2.Pokemon.name.fr;
            Pokemon2Attack.Text = player2.Pokemon.stats.atk.ToString();
            Pokemon2Defense.Text = player2.Pokemon.stats.def.ToString();
            Pokemon2Speed.Text = player2.Pokemon.stats.vit.ToString();
            Pokemon2Image.Source = PokemonHelper.GetPokemonImage(player2.Pokemon);

            UpdateUI();
        }

        private void UpdateUI()
        {
            // HP J1
            Pokemon1HPBar.Maximum = player1.MaxHP;
            Pokemon1HPBar.Value = player1.CurrentHP;
            Pokemon1HP.Text = $"{player1.CurrentHP} / {player1.MaxHP}";

            // HP J2
            Pokemon2HPBar.Maximum = player2.MaxHP;
            Pokemon2HPBar.Value = player2.CurrentHP;
            Pokemon2HP.Text = $"{player2.CurrentHP} / {player2.MaxHP}";
        }

        // 🔄 Gérer les tours (TRÈS IMPORTANT)
        private void UpdateTurnUI()
        {
            // Activer / désactiver boutons
            SetPlayer1Buttons(isPlayer1Turn);
            SetPlayer2Buttons(!isPlayer1Turn);
        }

        private void SetPlayer1Buttons(bool enabled)
        {
            AttackP1.IsEnabled = enabled;
            BagP1.IsEnabled = enabled;
            PokemonP1.IsEnabled = enabled;
            RunP1.IsEnabled = enabled;
        }

        private void SetPlayer2Buttons(bool enabled)
        {
            AttackP2.IsEnabled = enabled;
            BagP2.IsEnabled = enabled;
            PokemonP2.IsEnabled = enabled;
            RunP2.IsEnabled = enabled;
        }

        // 💥 Attaque
        private void Attack(PokemonCombat attacker, PokemonCombat defender, string playerName)
        {
            int atk = attacker.Pokemon.stats.atk;
            int def = defender.Pokemon.stats.def;

            int damage = Math.Max(5, atk - def / 2);

            defender.CurrentHP -= damage;
            if (defender.CurrentHP < 0)
                defender.CurrentHP = 0;

            ResultText.Text = $"{playerName} inflige {damage} dégâts !";

            UpdateUI();

            if (defender.IsDead())
            {
                ResultText.Text += $"\n{playerName} gagne ! 🏆";
                fightEnded = true;
                SetPlayer1Buttons(false);
                SetPlayer2Buttons(false);
            }
        }

        // ⚔️ Joueur 1 attaque
        private void AttackP1_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayer1Turn || fightEnded) return;

            Attack(player1, player2, "Joueur 1");

            isPlayer1Turn = false;
            UpdateTurnUI();

            if (!fightEnded)
                ResultText.Text += "\nTour du Joueur 2";
        }

        // ⚔️ Joueur 2 attaque
        private void AttackP2_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayer1Turn || fightEnded) return;

            Attack(player2, player1, "Joueur 2");

            isPlayer1Turn = true;
            UpdateTurnUI();

            if (!fightEnded)
                ResultText.Text += "\nTour du Joueur 1";
        }

        // 🎒 Soin
        private void Heal(PokemonCombat p, string playerName)
        {
            int heal = p.MaxHP / 4;

            p.CurrentHP += heal;
            if (p.CurrentHP > p.MaxHP)
                p.CurrentHP = p.MaxHP;

            ResultText.Text = $"{playerName} récupère {heal} HP ❤️";

            UpdateUI();
        }

        private void BagP1_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayer1Turn || fightEnded) return;

            Heal(player1, "Joueur 1");

            isPlayer1Turn = false;
            UpdateTurnUI();
        }

        private void BagP2_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayer1Turn || fightEnded) return;

            Heal(player2, "Joueur 2");

            isPlayer1Turn = true;
            UpdateTurnUI();
        }

        // 🐾 Changer Pokémon
        private async void PokemonP1_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayer1Turn || fightEnded) return;

            var newPoke = await api.GetApiPokemon(rnd.Next(1, 1025).ToString());
            player1 = new PokemonCombat(newPoke);

            InitUI();

            ResultText.Text = $"Joueur 1 change de Pokémon !";

            isPlayer1Turn = false;
            UpdateTurnUI();
        }

        private async void PokemonP2_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayer1Turn || fightEnded) return;

            var newPoke = await api.GetApiPokemon(rnd.Next(1, 1025).ToString());
            player2 = new PokemonCombat(newPoke);

            InitUI();

            ResultText.Text = $"Joueur 2 change de Pokémon !";

            isPlayer1Turn = true;
            UpdateTurnUI();
        }

        // 🏃 Abandon
        private void RunP1_Click(object sender, RoutedEventArgs e)
        {
            if (fightEnded) return;

            ResultText.Text = "Joueur 1 abandonne ! Joueur 2 gagne 🏆";
            fightEnded = true;
            SetPlayer1Buttons(false);
            SetPlayer2Buttons(false);
        }

        private void RunP2_Click(object sender, RoutedEventArgs e)
        {
            if (fightEnded) return;

            ResultText.Text = "Joueur 2 abandonne ! Joueur 1 gagne 🏆";
            fightEnded = true;
            SetPlayer1Buttons(false);
            SetPlayer2Buttons(false);
        }

        private void FightButton_Click(object sender, RoutedEventArgs e)
        {
            ResultText.Text = "Utilisez les boutons des joueurs 👇";
        }
    }
}