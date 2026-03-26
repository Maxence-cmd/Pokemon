using Microsoft.VisualBasic;
using Pokemon.Models;
using Pokemon.Services;
using Pokemon.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace Pokemon.Views
{
    public partial class chasse : Window
    {
        public GetPokemon getPokemon;
        Attaque attaque1;
        Attaque attaque2;
        Attaque attaque3;
        Attaque attaque4;
        Root playerData;
        Root enemyData;
        int currentHpPlayer;
        int currentHpEnemy;
        int maxHpEnemy;
        int maxHpPlayer;
        Stats statsPlayer;
        Stats statsEnemy;
        bool left, right, up, down;
        double speed = 1.5;
        DispatcherTimer timer;
        bool interactionActive = false;
        bool enCombat = false;
        Rectangle panneauActuel = null;
        Random random = new Random();
        int pokemonId;
        int pokemonIdEn;
        int frame = 0;
        int maxFrame = 3; // nombre d'images
        int animationCounter = 0;
        string direction = "front";
        private bool _attackPanelOpen = false;
        public chasse()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += GameLoop;
            getPokemon = new GetPokemon();
            timer.Start();
        }
        public async Task<Root> LoadPokemonFromFile(string path)
        {
            string json = await File.ReadAllTextAsync(path);
            return System.Text.Json.JsonSerializer.Deserialize<Root>(json);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (interactionActive) return;

            double x = Canvas.GetLeft(perso);
            double y = Canvas.GetTop(perso);

            double newX = x;
            double newY = y;

            if (left) newX -= speed;
            if (right) newX += speed;
            if (up) newY -= speed;
            if (down) newY += speed;

            double hitboxWidth = perso.Width * 0.6;
            double hitboxHeight = perso.Height * 0.6;

            double offsetX = (perso.Width - hitboxWidth) / 2;
            double offsetY = (perso.Height - hitboxHeight) / 2;

            Rect futurPerso = new Rect(
                newX + offsetX,
                newY + offsetY,
                hitboxWidth,
                hitboxHeight
            );
            if (!enCombat && DansHerbe(futurPerso))
            {
                LancerCombat();
                return;
            }

            if (!CollisionAvecObstacle(futurPerso))
            {
                Canvas.SetLeft(perso, newX);
                Canvas.SetTop(perso, newY);
                bool isMoving = left || right || up || down;

                if (isMoving)
                {
                    animationCounter++;

                    if (animationCounter >= 10) // vitesse animation
                    {
                        frame++;
                        if (frame >= maxFrame)
                            frame = 0;

                        animationCounter = 0;
                    }

                    // direction
                    if (left) direction = "left";
                    if (right) direction = "right";
                    if (up) direction = "back";
                    if (down) direction = "front";

                    // changer image
                    perso.Source = new BitmapImage(
                        new Uri($"pack://application:,,,/Image/sprites garcon/{direction}_{frame}.png")
                    );
                }
                else
                {
                    // idle (frame 0)
                    frame = 0;

                    perso.Source = new BitmapImage(
                        new Uri($"pack://application:,,,/Image/sprites garcon/{direction}_1.png")
                    );
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                // Si déjà ouvert → on ferme
                if (interactionActive)
                {
                    DialogueBox.Visibility = Visibility.Hidden;
                    interactionActive = false;
                    panneauActuel = null;
                    return;
                }

                // Sinon → on cherche une zone
                Rectangle interaction = GetInteraction();

                if (interaction != null)
                {
                    DialogueText.Text = interaction.Tag.ToString() ?? "...";
                    DialogueBox.Visibility = Visibility.Visible;

                    panneauActuel = interaction;
                    interactionActive = true;

                    left = right = up = down = false;
                }

                return;
            }

            if (interactionActive) return;

            if (e.Key == Key.Left) left = true;
            if (e.Key == Key.Right) right = true;
            if (e.Key == Key.Up) up = true;
            if (e.Key == Key.Down) down = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (interactionActive) return;

            if (e.Key == Key.Left) left = false;
            if (e.Key == Key.Right) right = false;
            if (e.Key == Key.Up) up = false;
            if (e.Key == Key.Down) down = false;
        }

        private bool CollisionAvecObstacle(Rect futurPerso)
        {
            foreach (UIElement element in ZoneBloque.Children)
            {
                if (element is Rectangle rect)
                {
                    double x = Canvas.GetLeft(rect);
                    double y = Canvas.GetTop(rect);

                    Rect obstacle = new Rect(x, y, rect.Width, rect.Height);

                    if (futurPerso.IntersectsWith(obstacle))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Rectangle GetInteraction()
        {
            double x = Canvas.GetLeft(perso);
            double y = Canvas.GetTop(perso);

            double hitboxWidth = perso.Width * 0.6;
            double hitboxHeight = perso.Height * 0.6;

            double offsetX = (perso.Width - hitboxWidth) / 2;
            double offsetY = (perso.Height - hitboxHeight) / 2;

            Rect persoRect = new Rect(
                x + offsetX,
                y + offsetY,
                hitboxWidth,
                hitboxHeight
            );

            foreach (UIElement element in Interaction.Children)
            {
                if (element is Rectangle rect)
                {
                    double rx = Canvas.GetLeft(rect);
                    double ry = Canvas.GetTop(rect);

                    Rect zone = new Rect(rx, ry, rect.Width, rect.Height);

                    if (persoRect.IntersectsWith(zone))
                    {
                        return rect;
                    }
                }
            }

            return null;
        }
        private void ShowActionButtons(bool show)
        {
            BtnCombat.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            BtnPokemon.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            BtnSac.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            BtnFuite.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }


        private void BtnCombat_Click(object sender, RoutedEventArgs e)
        {
            TextBox.Visibility = Visibility.Collapsed;
            AttackPanel.Visibility = Visibility.Visible;
            ShowActionButtons(false);
            _attackPanelOpen = true;
        }

        private void BtnPokemon_Click(object sender, RoutedEventArgs e)
        {
            SetCombatText("Choisis un Pokémon !");
            // TODO : ouvrir écran équipe
        }

        private void BtnSac_Click(object sender, RoutedEventArgs e)
        {
            SetCombatText("Tu ouvres ton sac...");
            // TODO : ouvrir inventaire

        }
        public void UpdateHpBar(ProgressBar bar, TextBlock label, int current, int max)
        {
            bar.Maximum = max;

            // 🔥 FORCE UPDATE
            bar.Value = 0;
            bar.Value = current;

            double pct = (double)current / max;

            bar.Foreground = pct > 0.5
                ? new SolidColorBrush(Color.FromRgb(88, 208, 80))    // vert
                : pct > 0.2
                    ? new SolidColorBrush(Color.FromRgb(248, 224, 56))  // jaune
                    : new SolidColorBrush(Color.FromRgb(240, 56, 56));   // rouge

            label.Text = $"{current}/{max}";
        }

        public void UpdateExpBar(int current, int max)
        {
            PlayerExpBar.Maximum = max;
            PlayerExpBar.Value = current;
        }
        private async void BtnFuite_Click(object sender, RoutedEventArgs e)
        {
            ShowActionButtons(false);
            SetCombatText("Tu prends la fuite...");
            await Task.Delay(1200);
            CombatScreen.Visibility = Visibility.Hidden;
            // TODO : retour map
        }
        public void SetCombatText(string text)
        {
            CombatText.Text = text;
            TextBox.Visibility = Visibility.Visible;
            AttackPanel.Visibility = Visibility.Collapsed;
            _attackPanelOpen = false;
           ShowActionButtons(true);
        }

        private bool DansHerbe(Rect futurPerso)
        {
            foreach (UIElement element in ZoneHerbe.Children)
            {
                if (element is Rectangle rect)
                {
                    double x = Canvas.GetLeft(rect);
                    double y = Canvas.GetTop(rect);

                    Rect herbe = new Rect(x, y, rect.Width, rect.Height);

                    if (futurPerso.IntersectsWith(herbe))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private async void LancerCombat()
        {
            enCombat = true;

            left = right = up = down = false;

            CombatScreen.Visibility = Visibility.Visible;
            pokemonId = random.Next(1, 152);
            pokemonIdEn = random.Next(1, 152);
            playerData = await getPokemon.GetApiPokemon(pokemonIdEn.ToString());
            enemyData = await getPokemon.GetApiPokemon(pokemonId.ToString());

            statsPlayer = playerData.stats;
            statsEnemy = enemyData.stats;
            PlayerName.Text = playerData.name.fr.ToUpper();
            EnemyName.Text = enemyData.name.fr.ToUpper();
            currentHpPlayer = statsPlayer.hp;
             maxHpPlayer = statsPlayer.hp;
            currentHpEnemy = statsEnemy.hp;
            maxHpEnemy = statsEnemy.hp;
            var talents = playerData.talents;

            // attaque 1 API
            attaque1 = new Attaque(
                talents[0].name,
                playerData.types[0].name,
                50
            );

            // attaque 2 API (si existe)
            if (talents.Count > 1)
            {
                attaque2 = new Attaque(
                    talents[1].name,
                    playerData.types[0].name,
                    60
                );
            }
            else
            {
                attaque2 = null;
            }

            // attaques fixes
            attaque3 = new Attaque("Vive-Attaque", "Normal", 40);
            attaque4 = new Attaque("Rugissement", "Normal", 0);
            BtnMove1.Content = attaque1.Nom.ToUpper();

            // si attaque 2 existe
            if (attaque2 != null)
            {
                BtnMove2.Content = attaque2.Nom.ToUpper();
                BtnMove2.Visibility = Visibility.Visible;
            }
            else
            {
                BtnMove2.Visibility = Visibility.Collapsed;
            }

            // attaques fixes
            BtnMove3.Content = attaque3.Nom.ToUpper();
            BtnMove4.Content = attaque4.Nom.ToUpper();
            UpdateHpCustom(EnemyHpBar, currentHpEnemy, maxHpEnemy, 160);
            UpdateHpCustom(PlayerHp, currentHpPlayer, maxHpPlayer, 180);
            // TEXTE HP
            PlayerHpText.Text = statsPlayer.hp + "/" + statsPlayer.hp;
            EnemyHpText.Text = statsEnemy.hp + "/" + statsEnemy.hp;

            // TEXTE COMBAT
            CombatText.Text = $"Un {enemyData.name.fr.ToUpper()} sauvage apparaît !";
            // pokemonId = 25;
            //pokemonIdEn = 25;
            // cacher au début
            EnemyPokemon.Visibility = Visibility.Hidden;
            PlayerPokemon.Visibility = Visibility.Hidden;

            // 🐲 Apparition ennemi
            await Task.Delay(500);

            // Hors du Dispatcher (téléchargement en background)
            var url = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/animated/back/{pokemonIdEn}.gif";

            byte[] gifBytes;
            using (var client = new HttpClient())
                gifBytes = await client.GetByteArrayAsync(url);

            // Sur le UI thread — charge depuis MemoryStream
            await Dispatcher.InvokeAsync(() =>
            {
                using var ms = new MemoryStream(gifBytes);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;  // copie tout en mémoire avant de fermer le stream
                bitmap.EndInit();
                bitmap.Freeze();  // thread-safe

                ImageBehavior.SetAnimatedSource(PlayerPokemon, bitmap);
                ImageBehavior.SetRepeatBehavior(PlayerPokemon, RepeatBehavior.Forever);
            });

            EnemyPokemon.Visibility = Visibility.Visible;

            // 🎬 pause
            await Task.Delay(1000);

            // 🎬 lancer pokeball (TON GIF)
            LancerAnimationPokeball();

            // 🔥 apparition joueur
            await Task.Delay(1000);

            url = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/animated/{pokemonId}.gif";

            byte[] enemyGifBytes;
            using (var client = new HttpClient())
                enemyGifBytes = await client.GetByteArrayAsync(url);

            await Dispatcher.InvokeAsync(() =>
            {
                using var ms = new MemoryStream(enemyGifBytes);

                var enemyImage = new BitmapImage();
                enemyImage.BeginInit();
                enemyImage.StreamSource = ms;
                enemyImage.CacheOption = BitmapCacheOption.OnLoad;
                enemyImage.EndInit();
                enemyImage.Freeze();

                ImageBehavior.SetAnimatedSource(EnemyPokemon, enemyImage);
                ImageBehavior.SetRepeatBehavior(EnemyPokemon, RepeatBehavior.Forever);
            });

            PlayerPokemon.Visibility = Visibility.Visible;
        }
        private void LancerAnimationPokeball()
        {
            var uri = new Uri("pack://application:,,,/Image/Gif/pokeball_throw_back.gif");
            var bitmap = new BitmapImage(uri);
            ImageBehavior.SetAnimatedSource(PlayerTrainer, bitmap);
            ImageBehavior.SetRepeatBehavior(PlayerTrainer, new RepeatBehavior(1));
        }

        private async void BtnMove1_Click(object sender, RoutedEventArgs e)
        {
            if (currentHpPlayer <= 0 || currentHpEnemy <= 0)
                return;

            await TourCombat(attaque1);
        }

        private async void BtnMove2_Click(object sender, RoutedEventArgs e)
        {
            if (currentHpPlayer <= 0 || currentHpEnemy <= 0)
                return;

            if (attaque2 != null)
                await TourCombat(attaque2);
        }

        private async void BtnMove3_Click(object sender, RoutedEventArgs e)
        {
            if (currentHpPlayer <= 0 || currentHpEnemy <= 0)
                return;

            await TourCombat(attaque3);
        }
        double GetMultiplier(List<Resistance> resistances, string attackType)
        {
            foreach (var r in resistances)
            {
                if (r.name == attackType)
                    return r.multiplier;
            }

            return 1; // défaut
        }

        private async  void BtnMove4_Click(object sender, RoutedEventArgs e)
        {
            if (currentHpPlayer <= 0 || currentHpEnemy <= 0)
                return;

            await TourCombat(attaque4);
        }

        private async Task TourCombat(Attaque attaqueJoueur)
        {
            // 🟢 JOUEUR attaque
            int degats = CalculDegatsAvecAttaque(statsPlayer, statsEnemy, attaqueJoueur);
            currentHpEnemy -= degats;

            if (currentHpEnemy < 0)
                currentHpEnemy = 0;

            UpdateHpCustom(EnemyHpBar, currentHpEnemy, maxHpEnemy, 160);
           

            SetCombatText($"{playerData.name.fr} utilise {attaqueJoueur.Nom} ! -{degats} PV");

            await Task.Delay(1000);

            // KO ennemi
            if (currentHpEnemy <= 0)
            {
                SetCombatText($"{enemyData.name.fr} est KO !");
                await Task.Delay(1500);
                FinCombat();
                return;
            }

            // 🔴 ENNEMI attaque (attaque simple)
            Attaque attaqueEnnemi = new Attaque("Charge", enemyData.types[0].name, 40);

            degats = CalculDegatsAvecAttaque(statsEnemy, statsPlayer, attaqueEnnemi);
            currentHpPlayer -= degats;

            if (currentHpPlayer < 0)
                currentHpPlayer = 0;

          
            UpdateHpCustom(PlayerHp, currentHpPlayer, maxHpPlayer, 180);

            SetCombatText($"{enemyData.name.fr} utilise {attaqueEnnemi.Nom} ! -{degats} PV");

            await Task.Delay(1000);

            if (currentHpPlayer <= 0)
            {
                SetCombatText($"{playerData.name.fr} est KO !");
                await Task.Delay(1500);
                FinCombat();
            }
        }
        private int CalculDegatsAvecAttaque(Stats attaquant, Stats defenseur, Attaque attaque)
        {
            Random rnd = new Random();

            if (attaque.Puissance <= 0)
                return 0;

            double critique = rnd.Next(0, 100) < 10 ? 1.5 : 1.0;
            double aleatoire = rnd.Next(85, 101) / 100.0;

            double degats = (((attaque.Puissance * Math.Max(attaquant.atk, 1))
                            / (double)Math.Max(defenseur.def, 1)) / 5.0) + 2;

            degats *= critique * aleatoire;

            return Math.Max(1, (int)degats);
        }
        void UpdateHpCustom(Border bar, int current, int max, double fullWidth)
        {
            double ratio = (double)current / max;

            bar.Width = fullWidth * ratio;

            if (ratio > 0.5)
                bar.Background = new SolidColorBrush(Color.FromRgb(88, 208, 80)); // vert
            else if (ratio > 0.2)
                bar.Background = new SolidColorBrush(Color.FromRgb(248, 224, 56)); // jaune
            else
                bar.Background = new SolidColorBrush(Color.FromRgb(240, 56, 56)); // rouge
        }
        int CalculDegats(Stats attaquant, Stats defenseur)
        {
            Random rnd = new Random();

            int baseDamage = attaquant.atk - (defenseur.def / 2);

            if (baseDamage < 5)
                baseDamage = 5;

            int randomBonus = rnd.Next(0, 5);

            return baseDamage + randomBonus;
        }
        private async void FinCombat()
        {
            await Task.Delay(1500);

            CombatScreen.Visibility = Visibility.Hidden;
            enCombat = false;
        }
    }
}