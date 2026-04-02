using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.VisualBasic;
using Pokemon.Models;
using Pokemon.Services;
using WpfAnimatedGif;

namespace Pokemon.Views
{
    public partial class chasse : Window
    {
        public GetPokemon getPokemon;
        int nbPokeBall = 50;
        int nbSuperBall = 15;
        int nbHyperBall = 5;
        int nbPotion = 5;
        int nbSuperPotion = 3;
        int nbHyperPotion = 1;
        bool modeRemplacement = false;
        GererEquipe pokemonEnAttente = null;
        List<GererEquipe> team = new List<GererEquipe>();
        Attaque attaque1;
        Attaque attaque2;
        Attaque attaque3;
        Attaque attaque4;
        Root playerData;
        Root enemyData;
        int currentHpPlayer;
        int currentHpEnemy;
        string starterChoisi = "";
        int maxHpEnemy;
        int maxHpPlayer;
        Stats statsPlayer;
        Stats statsEnemy;
        bool left, right, up, down;
        double speed = 10;
        DispatcherTimer timer;
        bool interactionActive = false;
        bool enCombat = false;
        Rectangle panneauActuel = null;
        Random random = new Random();
        int pokemonId;
        int pokemonIdEn;
        int frame = 0;
        int catchrate = 120; // taux de capture de base (ex : 120 pour un Pokémon rare)
        int maxFrame = 3; // nombre d'images
        int animationCounter = 0;
        string direction = "front";
        private bool _attackPanelOpen = false;
        bool escapeSuccess;
        bool isTeleporting = false;
        string pokemonpremier = "nope";
        public chasse()
        {
            InitializeComponent();
            UpdateBagUI();
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
                CheckTeleport();
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
            if (ZoneBloque0.Visibility == Visibility.Visible || ZoneBloque0.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in ZoneBloque0.Children)
                {
                    if (element is Rectangle rect)
                    {
                        Rect obstacle = new Rect(
                            Canvas.GetLeft(rect),
                            Canvas.GetTop(rect),
                            rect.Width,
                            rect.Height);

                        if (futurPerso.IntersectsWith(obstacle))
                            return true;
                    }
                }
            }

            if (ZoneBloque1.Visibility == Visibility.Visible || ZoneBloque1.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in ZoneBloque1.Children)
                {
                    if (element is Rectangle rect)
                    {
                        Rect obstacle = new Rect(
                            Canvas.GetLeft(rect),
                            Canvas.GetTop(rect),
                            rect.Width,
                            rect.Height);

                        if (futurPerso.IntersectsWith(obstacle))
                            return true;
                    }
                }
            }
            if (ZoneBloque2.Visibility == Visibility.Visible || ZoneBloque2.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in ZoneBloque2.Children)
                {
                    if (element is Rectangle rect)
                    {
                        Rect obstacle = new Rect(
                            Canvas.GetLeft(rect),
                            Canvas.GetTop(rect),
                            rect.Width,
                            rect.Height);

                        if (futurPerso.IntersectsWith(obstacle))
                            return true;
                    }
                }
            }
            if (ZoneBloque3.Visibility == Visibility.Visible || ZoneBloque3.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in ZoneBloque3.Children)
                {
                    if (element is Rectangle rect)
                    {
                        Rect obstacle = new Rect(
                            Canvas.GetLeft(rect),
                            Canvas.GetTop(rect),
                            rect.Width,
                            rect.Height);

                        if (futurPerso.IntersectsWith(obstacle))
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
            if (Interaction0.Visibility == Visibility.Visible || Interaction0.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in Interaction0.Children)
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
            }
            if (Interaction1.Visibility == Visibility.Visible || Interaction1.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in Interaction1.Children)
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
            }
            if (Interaction2.Visibility == Visibility.Visible || Interaction2.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in Interaction2.Children)
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
            }
            if (Interaction3.Visibility == Visibility.Visible || Interaction3.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in Interaction3.Children)
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
            //TextBox.Visibility = Visibility.Collapsed;
            AttackPanel.Visibility = Visibility.Visible;
            ShowActionButtons(false);
            _attackPanelOpen = true;
        }

        private void BtnPokemon_Click(object sender, RoutedEventArgs e)
        {
            UpdateTeamUI();
            SetCombatText("Choisis un Pokémon !");
            TeamScreen.Visibility = Visibility.Visible;
            CombatScreen.Visibility = Visibility.Hidden;
            ZoneJeu1.Visibility = Visibility.Hidden;
            modeRemplacement = false; // ⚠️ important (on switch, pas remplacement équipe)
        }

        private void BtnSac_Click(object sender, RoutedEventArgs e)
        {
            SetCombatText("Tu ouvres ton sac...");
            AttackPanel.Visibility = Visibility.Collapsed;

            BagPanel.Visibility = Visibility.Visible;

        }
        public void UpdateHpBar(ProgressBar bar, TextBlock label, int current, int max)
        {
            bar.Maximum = max;

              
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
            ZoneJeu1.Visibility = Visibility.Visible;
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
            if (ZoneHerbe0.Visibility == Visibility.Visible || ZoneHerbe0.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in ZoneHerbe0.Children)
                {
                    if (element is Rectangle rect)
                    {
                        double rx = Canvas.GetLeft(rect);
                        double ry = Canvas.GetTop(rect);

                        Rect herbe = new Rect(rx, ry, rect.Width, rect.Height);
                        if (futurPerso.IntersectsWith(herbe))
                        {
                            return true;
                        }
                    }
                }
            }
            if (ZoneHerbe1.Visibility == Visibility.Visible || ZoneHerbe1.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in ZoneHerbe1.Children)
                {
                    if (element is Rectangle rect)
                    {
                        double rx = Canvas.GetLeft(rect);
                        double ry = Canvas.GetTop(rect);

                        Rect herbe = new Rect(rx, ry, rect.Width, rect.Height);
                        if (futurPerso.IntersectsWith(herbe))
                        {
                            return true;
                        }
                    }
                }
            }
            if (ZoneHerbe3.Visibility == Visibility.Visible || ZoneHerbe3.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in ZoneHerbe3.Children)
                {
                    if (element is Rectangle rect)
                    {
                        double rx = Canvas.GetLeft(rect);
                        double ry = Canvas.GetTop(rect);

                        Rect herbe = new Rect(rx, ry, rect.Width, rect.Height);
                        if (futurPerso.IntersectsWith(herbe))
                        {
                            return true;
                        }
                    }
                }
            }
            if (ZoneHerbe2.Visibility == Visibility.Visible || ZoneHerbe2.Visibility == Visibility.Hidden)
            {
                foreach (UIElement element in ZoneHerbe2.Children)
                {
                    if (element is Rectangle rect)
                    {
                        double rx = Canvas.GetLeft(rect);
                        double ry = Canvas.GetTop(rect);

                        Rect herbe = new Rect(rx, ry, rect.Width, rect.Height);
                        if (futurPerso.IntersectsWith(herbe))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private void CheckTeleport()
        {
            Rect playerRect = new Rect(
                Canvas.GetLeft(perso),
                Canvas.GetTop(perso),
                perso.Width,
                perso.Height);

            // 🔥 liste de toutes les zones TP
            var zonesTP = new List<Canvas> { ZoneTP0, ZoneTP1, ZoneTP2 };

            foreach (var zone in zonesTP)
            {
                if (zone.Visibility != Visibility.Visible && zone.Visibility != Visibility.Hidden)
                    continue;

                foreach (var rect in zone.Children.OfType<Rectangle>())
                {
                    Rect tpRect = new Rect(
                        Canvas.GetLeft(rect),
                        Canvas.GetTop(rect),
                        rect.Width,
                        rect.Height);

                    if (!isTeleporting && playerRect.IntersectsWith(tpRect))
                    {
                        string tag = rect.Tag as string;

                        isTeleporting = true;

                        switch (tag)
                        {
                            case "0to1":
                                SwitchToMap1(98, 347, "0");
                                break;

                            case "2to1":
                                SwitchToMap1(105, 126, "2");
                                break;

                            case "3to0":
                                SwitchToMap0(107, 218, "3");
                                break;

                            case "2to0":
                                SwitchToMap0(400, 46, "2");
                                break;

                            case "1to0":
                                SwitchToMap0(672, 195, "1");
                                break;

                            case "0to2":
                                SwitchToMap2(426, 420, "0");
                                break;

                            case "31to21":
                                SwitchToMap2(416, 310, "3");
                                break;

                            case "32to22":
                                SwitchToMap2(416, 196, "3");
                                break;

                            case "1to2":
                                SwitchToMap2(672, 355, "1");
                                break;

                            case "0to3":
                                SwitchToMap3(658, 330, "0");
                                break;

                            case "21to31":
                                SwitchToMap3(569, 24, "2");
                                break;

                            case "22to32":
                                SwitchToMap3(482, 24, "2");
                                break;
                        }

                        Task.Delay(500).ContinueWith(_ =>
                        {
                            Dispatcher.Invoke(() => isTeleporting = false);
                        });

                        return;
                    }
                }
            }
        }
        private void SwitchToMap1(int x,int y, string dep)
        {
            // 🔁 zones collision
            (this.FindName($"ZoneBloque{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneBloque1.Visibility = Visibility.Hidden;

            // 🌿 herbe
            (this.FindName($"ZoneHerbe{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneHerbe1.Visibility = Visibility.Hidden;

            // 🚪 TP
            (this.FindName($"ZoneTP{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneTP1.Visibility = Visibility.Hidden;

            // 💬 interactions
            (this.FindName($"Interaction{dep}") as Canvas).Visibility = Visibility.Collapsed;
            Interaction1.Visibility = Visibility.Hidden;
            ImgBck.Source = new BitmapImage(
    new Uri("/Image/map poke/safari1.png", UriKind.Relative)
);
            // 🧍 joueur position
            Canvas.SetLeft(perso, x);
            Canvas.SetTop(perso, y);
        }
        private void SwitchToMap2(int x, int y, string dep)
        {
            // 🔁 zones collision
            (this.FindName($"ZoneBloque{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneBloque2.Visibility = Visibility.Hidden;

            // 🌿 herbe
            (this.FindName($"ZoneHerbe{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneHerbe2.Visibility = Visibility.Hidden;

            // 🚪 TP
            (this.FindName($"ZoneTP{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneTP2.Visibility = Visibility.Hidden;

            // 💬 interactions
            (this.FindName($"Interaction{dep}") as Canvas).Visibility = Visibility.Collapsed;
            Interaction2.Visibility = Visibility.Hidden;
            ImgBck.Source = new BitmapImage(
    new Uri("/Image/map poke/safari2.png", UriKind.Relative)
);
            // 🧍 joueur position
            Canvas.SetLeft(perso, x);
            Canvas.SetTop(perso, y);
        }
        private void SwitchToMap3(int x, int y, string dep)
        {
            // 🔁 zones collision
            (this.FindName($"ZoneBloque{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneBloque3.Visibility = Visibility.Hidden;

            // 🌿 herbe
            (this.FindName($"ZoneHerbe{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneHerbe3.Visibility = Visibility.Hidden;

            // 🚪 TP
            (this.FindName($"ZoneTP{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneTP3.Visibility = Visibility.Hidden;

            // 💬 interactions
            (this.FindName($"Interaction{dep}") as Canvas).Visibility = Visibility.Collapsed;
            Interaction3.Visibility = Visibility.Hidden;
            ImgBck.Source = new BitmapImage(
    new Uri("/Image/map poke/safari3.png", UriKind.Relative)
);
            // 🧍 joueur position
            Canvas.SetLeft(perso, x);
            Canvas.SetTop(perso, y);
        }
        private void SwitchToMap0(int x, int y, string dep)
        {
            // 🔁 zones collision
            (this.FindName($"ZoneBloque{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneBloque0.Visibility = Visibility.Hidden;

            // 🌿 herbe
            (this.FindName($"ZoneHerbe{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneHerbe0.Visibility = Visibility.Hidden;

            // 🚪 TP
            (this.FindName($"ZoneTP{dep}") as Canvas).Visibility = Visibility.Collapsed;
            ZoneTP0.Visibility = Visibility.Hidden;

            // 💬 interactions
            (this.FindName($"Interaction{dep}") as Canvas).Visibility = Visibility.Collapsed;
            Interaction0.Visibility = Visibility.Hidden;
            ImgBck.Source = new BitmapImage(
    new Uri("/Image/map poke/safaricentral.png", UriKind.Relative)
);
            // 🧍 joueur position
            Canvas.SetLeft(perso, x);
            Canvas.SetTop(perso, y);
        }
        private async void LancerCombat()
        {
            PlayerPokemon.Visibility = Visibility.Hidden;
            EnemyPokemon.Visibility = Visibility.Visible;
            enCombat = true;
           PokeballAnim.Visibility = Visibility.Hidden;
            left = right = up = down = false;

            CombatScreen.Visibility = Visibility.Visible;
            pokemonId = random.Next(1, 152);
            // pokemonIdEn = random.Next(1, 152);
            if (pokemonpremier == "nope")
                pokemonpremier = starterChoisi;
            playerData = await getPokemon.GetApiPokemon(pokemonpremier);
            enemyData = await getPokemon.GetApiPokemon(pokemonId.ToString());
            pokemonIdEn =playerData.pokedex_id;
            statsPlayer = playerData.stats;
            statsEnemy = enemyData.stats;
            catchrate = enemyData.catch_rate ?? 45;
            PlayerName.Text = playerData.name.fr.ToUpper();
            EnemyName.Text = enemyData.name.fr.ToUpper();
            currentHpPlayer = statsPlayer.hp;
             maxHpPlayer = statsPlayer.hp;
            currentHpEnemy = statsEnemy.hp;
            maxHpEnemy = statsEnemy.hp;
            var url = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/animated/{pokemonId}.gif";
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
            ApparaitrePokemon(EnemyPokemon);
            EnemyPokemon.Visibility = Visibility.Visible;
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
            //M
            // TEXTE COMBAT
            CombatText.Text = $"Un {enemyData.name.fr.ToUpper()} sauvage apparaît !";
            // pokemonId = 25;
            //pokemonIdEn = 25;
            // cacher au début
           
            PlayerPokemon.Visibility = Visibility.Hidden;

            // 🐲 Apparition ennemi
            await Task.Delay(500);

            // Hors du Dispatcher (téléchargement en background)
             url = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/animated/back/{pokemonIdEn}.gif";

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

           

            // 🎬 pause
            await Task.Delay(1000);

            // 🎬 lancer pokeball (TON GIF)
            LancerAnimationPokeball();

            // 🔥 apparition joueur
            await Task.Delay(1300);

            

           

            PlayerPokemon.Visibility = Visibility.Visible;
        }
        private void LancerAnimationPokeball()
        {
            var uri = new Uri("pack://application:,,,/Image/Gif/pokeball_throw_back.gif");
            var bitmap = new BitmapImage(uri);
            ImageBehavior.SetAnimatedSource(PlayerTrainer, bitmap);
            ImageBehavior.SetRepeatBehavior(PlayerTrainer, new RepeatBehavior(1));
        }
        private void StartGame()
        {
            StartMenu.Visibility = Visibility.Collapsed;
            GameView.Visibility = Visibility.Visible;
            ZoneJeu1.Focus(); // important pour les touches
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

        private async Task TourCombat(Attaque attaqueJoueur, bool poke = false)
        {
            int degats;
            if (!poke==true)
            { 
            // 🟢 JOUEUR attaque
             degats = CalculDegatsAvecAttaque(statsPlayer, statsEnemy, attaqueJoueur);
            currentHpEnemy -= degats;

            if (currentHpEnemy < 0)
                currentHpEnemy = 0;

            UpdateHpCustom(EnemyHpBar, currentHpEnemy, maxHpEnemy, 160);


            SetCombatText($"{playerData.name.fr} utilise {attaqueJoueur.Nom} ! -{degats} PV");
             }

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

        private void UsePokeBall_Click(object sender, RoutedEventArgs e)
        {

            if (nbPokeBall <= 0)
            {
                SetCombatText("Tu n'as plus de Poké Ball !");
                return;
            }

            nbPokeBall--;
            UpdateBagUI();

            TryCatch("pokeball");
        }

        private async void UseSuperBall_Click(object sender, RoutedEventArgs e)
        {

            if (nbSuperBall <= 0)
            {
                SetCombatText("Tu n'as plus de Super Ball !");
                return;
            }

            nbSuperBall--;
            UpdateBagUI();

            TryCatch("megaball");
        }

        private void UseHyperBall_Click(object sender, RoutedEventArgs e)
        {
            if (nbHyperBall <= 0)
            {
                SetCombatText("Tu n'as plus de Hyper Ball !");
                return;
            }

            nbHyperBall--;
            UpdateBagUI();

            TryCatch("hyperball");
        }

        private void UsePotion_Click(object sender, RoutedEventArgs e)
        {
            if (nbPotion <= 0)
            {
                SetCombatText("Plus de potion !");
                return;
            }

            nbPotion--;
            UpdateBagUI();

            HealPokemon(20);
        }

        private void UseSuperPotion_Click(object sender, RoutedEventArgs e)
        {
            if (nbSuperPotion <= 0)
            {
                SetCombatText("Plus de Super Potion !");
                return;
            }

            nbSuperPotion--;
            UpdateBagUI();

            HealPokemon(50);
        }

        private void UseHyperPotion_Click(object sender, RoutedEventArgs e)
        {
            if (nbHyperPotion <= 0)
            {
                SetCombatText("Plus de Hyper Potion !");
                return;
            }

            nbHyperPotion--;
            UpdateBagUI();

            HealPokemon(200);
        }

        private void BtnRetourCombat_Click(object sender, RoutedEventArgs e)
        {
            BagPanel.Visibility = Visibility.Collapsed;
            AttackPanel.Visibility = Visibility.Collapsed;
            TextBox.Visibility = Visibility.Visible;
            ShowActionButtons(true);
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
            ZoneJeu1.Visibility = Visibility.Visible;
            CombatScreen.Visibility = Visibility.Hidden;
            enCombat = false;
        }
        void HealPokemon(int amount)
        {
                if (currentHpPlayer <= 0)
                    return;

                if (currentHpPlayer >= maxHpPlayer)
                {
                    CombatText.Text = "Ton Pokémon est déjà full PV !";
                    return;
                }

                currentHpPlayer += amount;

                if (currentHpPlayer > maxHpPlayer)
                    currentHpPlayer = maxHpPlayer;

                // TEXTE
                PlayerHpText.Text = $"{currentHpPlayer}/{maxHpPlayer}";

                // BARRE (UNE SEULE FOIS)
                UpdateHpCustom(PlayerHp, currentHpPlayer, maxHpPlayer, 180);

                // MESSAGE
                CombatText.Text = $"Ton Pokémon récupère {amount} PV !";
        }

        private void ChooseBulbasaur_Click(object sender, RoutedEventArgs e)
        {
            starterChoisi = "Bulbizarre";
            AjouterPokemonEquipe("Bulbizarre", 45, "1");
            StartGame();
        }

        private void ChooseCharmander_Click(object sender, RoutedEventArgs e)
        {
            starterChoisi = "Salameche";
            AjouterPokemonEquipe("Salameche",39, "4");
            StartGame();
        }

        private void ChooseSquirtle_Click(object sender, RoutedEventArgs e)
        {
            starterChoisi = "Carapuce";
            AjouterPokemonEquipe("Carapuce",44,"7");
            StartGame();
        }

        private void bttReglage_Click(object sender, RoutedEventArgs e)
        {
            TeamScreen.Visibility = Visibility.Visible;
            GameView.Visibility = Visibility.Hidden;

            UpdateTeamUI();
        }

        double GetBallMultiplier(string type)
        {
            return type switch
            {
                "poke" => 1.0,
                "super" => 1.5,
                "hyper" => 2.0,
                _ => 1.0
            };
        }
        bool TryCatchPokemon(int catchRate, string ballType)
        {
            double ballMultiplier = GetBallMultiplier(ballType);

            double hpFactor = (double)(maxHpEnemy - currentHpEnemy) / maxHpEnemy;

            double chance = catchRate * ballMultiplier * hpFactor;

            // normaliser (optionnel)
            if (chance > 255) chance = 255;

            int roll = random.Next(0, 256);

            return roll < chance;
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            TeamScreen.Visibility = Visibility.Collapsed;
            GameView.Visibility = Visibility.Visible;
        }

        private async void Slot_Click(object sender, MouseButtonEventArgs e)
        {
            Border clickedSlot = sender as Border;

            int index = clickedSlot == Slot1 ? 0 :
                        clickedSlot == Slot2 ? 1 :
                        clickedSlot == Slot3 ? 2 :
                        clickedSlot == Slot4 ? 3 :
                        clickedSlot == Slot5 ? 4 :
                        clickedSlot == Slot6 ? 5 : -1;

            if (index == -1 || index >= team.Count)
                return;

            var nouveauPokemon = team[index];

            // 🔥 retour combat
            TeamScreen.Visibility = Visibility.Collapsed;
            CombatScreen.Visibility = Visibility.Visible;

            SetCombatText($"Go {nouveauPokemon.Name} !");

            // 🔥 animation switch complète
            await SwitchPokemon(nouveauPokemon);
        }
        private async Task SwitchPokemon(GererEquipe nouveau)
        {
            // 🔴 ancien disparaît
            await DisparaitrePokemon(PlayerPokemon);

            // 🎯 lancer pokeball (même anim)
            LancerAnimationPokeball();

            // 🧠 update data joueur
            playerData = await getPokemon.GetApiPokemon(nouveau.Id);
            statsPlayer = playerData.stats;

            currentHpPlayer = nouveau.CurrentHP;
            maxHpPlayer = nouveau.MaxHP;

            PlayerName.Text = nouveau.Name.ToUpper();

            UpdateHpCustom(PlayerHp, currentHpPlayer, maxHpPlayer, 180);
            PlayerHpText.Text = $"{currentHpPlayer}/{maxHpPlayer}";

            // 🎥 charger sprite
            var url = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/animated/back/{nouveau.Id}.gif";

            byte[] gifBytes;
            using (var client = new HttpClient())
                gifBytes = await client.GetByteArrayAsync(url);

            await Dispatcher.InvokeAsync(() =>
            {
                using var ms = new MemoryStream(gifBytes);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                ImageBehavior.SetAnimatedSource(PlayerPokemon, bitmap);
            });

            // 🟢 apparition
            await ApparaitrePokemon(PlayerPokemon);
        }

        async Task TryCatch(string ballType)
        {
            int catchRate = enemyData.catch_rate ?? 45; // ⚠️ adapte selon ton modèle

            bool success = TryCatchPokemon(catchRate, ballType);
           // bool success = true;
            BagPanel.Visibility = Visibility.Collapsed;
            SetCombatText("Tu lances une Poké Ball...");
        _: LancerPokeball(ballType,success);
            await Task.Delay(5500);
            if (success)
            {
                SetCombatText($"Bravo ! {enemyData.name.fr} est capturé !");
                bool remplacement = await AjouterPokemonEquipe(
                enemyData.name.fr,
                enemyData.stats.hp,
                enemyData.pokedex_id.ToString()
                );

                if (!remplacement)
                {
                    await Task.Delay(1500);
                    FinCombat();
                }
            }
            else
            {
                SetCombatText("Oh non ! Le Pokémon s'est échappé !");
                await Task.Delay(1000);
                escapeSuccess = true;
                // L'ennemi attaque après échec
                await TourCombat(new Attaque("Charge", enemyData.types[0].name, 40),escapeSuccess);
            }
            escapeSuccess = false;
        }
        private async void UpdateTeamUI()
        {
            var slots = new[]
            {
        (Slot1, Name1, Hp1, Img1),
        (Slot2, Name2, Hp2, Img2),
        (Slot3, Name3, Hp3, Img3),
        (Slot4, Name4, Hp4, Img4),
        (Slot5, Name5, Hp5, Img5),
        (Slot6, Name6, Hp6, Img6),
    };

            for (int i = 0; i < slots.Length; i++)
            {
              
                if (i < team.Count)
                {
                    var p = team[i];

                    slots[i].Item2.Text = p.Name;
                    slots[i].Item3.Text = $"{p.CurrentHP}/{p.MaxHP}";
                    var recupId = await getPokemon.GetApiPokemon(slots[i].Item2.Text);
                    string url = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/animated/{p.Id}.gif";
                    slots[i].Item4.Source = new BitmapImage(new Uri(url));

                    // 🔥 Couleur selon HP
                    double ratio = (double)p.CurrentHP / p.MaxHP;

                    if (ratio <= 0)
                        slots[i].Item1.Background = Brushes.Red;
                    else if (ratio < 0.5)
                        slots[i].Item1.Background = Brushes.Orange;
                    else
                        slots[i].Item1.Background = Brushes.Green;
                }
                else
                {
                    // N/A
                    slots[i].Item2.Text = "N/A";
                    slots[i].Item3.Text = "";
                    slots[i].Item4.Source = null;
                    slots[i].Item1.Background = Brushes.Gray;
                }
            }
        }
        private async Task<bool> AjouterPokemonEquipe(string nom, int hpMax, string IdNouveau)
        {
            var pokemon = new GererEquipe
            {
                Name = nom,
                CurrentHP = hpMax,
                MaxHP = hpMax,
                Id = IdNouveau
            };

            if (team.Count < 6)
            {
                team.Add(pokemon);
                return false; // pas de remplacement
            }
            else
            {
                
                pokemonEnAttente = pokemon;
                modeRemplacement = true;

                MessageBox.Show("Ton équipe est pleine ! Choisis un Pokémon à remplacer.");

                TeamScreen.Visibility = Visibility.Visible;
                GameView.Visibility = Visibility.Hidden;
                CombatScreen.Visibility = Visibility.Hidden;

                UpdateTeamUI();

                return true; // remplacement en cours
            }
        }
        private async Task LancerPokeball(string ball, bool reussi)
        {
            string gifPath;
            int nombre = random.Next(1, 4); // ✅ utilise ton Random global

            if (reussi)
            {
                gifPath = $"/Image/Gif/{ball}_catch.gif";
            }
            else
            {
                gifPath = nombre == 3
                    ? $"/Image/Gif/{ball}_open_direct.gif"
                    : $"/Image/Gif/{ball}_open_{nombre}shake.gif";
            }

            RestartGif(PokeballAnim, gifPath);
            PokeballAnim.Visibility = Visibility.Visible;

            // 🔥 TRAJECTOIRE
            PathGeometry path = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                StartPoint = new Point(50, 400)
            };

            figure.Segments.Add(new QuadraticBezierSegment(
                new Point(300, 0),
                new Point(550, 120),
                true));

            path.Figures.Add(figure);

            int duration = 1900;

            var animX = new DoubleAnimationUsingPath
            {
                PathGeometry = path,
                Duration = TimeSpan.FromMilliseconds(duration),
                Source = PathAnimationSource.X
            };

            var animY = new DoubleAnimationUsingPath
            {
                PathGeometry = path,
                Duration = TimeSpan.FromMilliseconds(duration),
                Source = PathAnimationSource.Y
            };

            // 🔥 LANCER TOUJOURS (succès OU échec)
            PokeballAnim.BeginAnimation(Canvas.LeftProperty, animX);
            PokeballAnim.BeginAnimation(Canvas.TopProperty, animY);

            await Task.Delay(duration);

            // 🔥 disparition Pokémon
            await DisparaitrePokemon(EnemyPokemon);

            // ⏱ durée des shakes
            int waitTime = nombre switch
            {
                1 => 1800,
                2 => 2800,
                _ => 100
            };

            await Task.Delay(waitTime);

            // ❌ ÉCHEC → réapparition stylée
            if (!reussi)
            {
                await ApparaitrePokemon(EnemyPokemon);
            }

            // 🧼 clean
            PokeballAnim.Visibility = Visibility.Collapsed;
        }

        private void RestartGif(Image img, string path)
    {
        ImageBehavior.SetAnimatedSource(img, null); // reset
        ImageBehavior.SetAnimatedSource(img, new BitmapImage(new Uri(path, UriKind.Relative)));
    }
        private async Task DisparaitrePokemon(Image pokemon)
        {
            // 🔥 Fade (opacité)
            DoubleAnimation fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));

            // 🔥 Scale (réduction)
            ScaleTransform scale = new ScaleTransform(1, 1);
            pokemon.RenderTransform = scale;
            pokemon.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation shrinkX = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
            DoubleAnimation shrinkY = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));

            // lancer animations
            pokemon.BeginAnimation(UIElement.OpacityProperty, fade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, shrinkX);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkY);

            await Task.Delay(500);

            // reset propre
            pokemon.Visibility = Visibility.Hidden;
            pokemon.Opacity = 1;
            pokemon.RenderTransform = null;
        }
        private async Task ApparaitrePokemon(Image pokemon)
        {
            pokemon.Visibility = Visibility.Visible;

            pokemon.Opacity = 0;

            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            pokemon.BeginAnimation(UIElement.OpacityProperty, fade);

            await Task.Delay(300);

            pokemon.Opacity = 1;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await YouTubePlayer.EnsureCoreWebView2Async();

            string videoId = "tdD57sinnT0";

            string html = $@"
    <html>
    <body style='margin:0;background:black;overflow:hidden;'>
        <iframe width='100%' height='100%'
        src='https://www.youtube.com/embed/{videoId}?autoplay=1&controls=0'
        frameborder='0'
        allow='autoplay'
        allowfullscreen>
        </iframe>
    </body>
    </html>";

            YouTubePlayer.NavigateToString(html);
        }
        void UpdateBagUI()
        {
            SetButtonContent(
                BtnPokeBall,
                "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/poke-ball.png",
                $"Poké Ball x{nbPokeBall}"
            );

            SetButtonContent(
                BtnSuperBall,
                "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/great-ball.png",
                $"Super Ball x{nbSuperBall}"
            );

            SetButtonContent(
                BtnHyperBall,
                "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/ultra-ball.png",
                $"Hyper Ball x{nbHyperBall}"
            );

            SetButtonContent(
                BtnPotion,
                "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/potion.png",
                $"Potion x{nbPotion}"
            );

            SetButtonContent(
                BtnSuperPotion,
                "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/super-potion.png",
                $"Super Potion x{nbSuperPotion}"
            );

            SetButtonContent(
                BtnHyperPotion,
                "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/hyper-potion.png",
                $"Hyper Potion x{nbHyperPotion}"
            );
        }
        void SetButtonContent(Button btn, string imageUrl, string text)
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            Image img = new Image
            {
                Source = new BitmapImage(new Uri(imageUrl)),
                Width = 24,
                Height = 20,
                Margin = new Thickness(4)
            };

            TextBlock txt = new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily("Consolas"),
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };

            panel.Children.Add(img);
            panel.Children.Add(txt);

            btn.Content = panel;
        }
    }

}