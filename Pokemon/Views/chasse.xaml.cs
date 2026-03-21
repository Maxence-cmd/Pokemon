using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.VisualBasic;
using WpfAnimatedGif;

namespace Pokemon.Views
{
    public partial class chasse : Window
    {
        bool left, right, up, down;
        double speed = 1.5;
        DispatcherTimer timer;
        bool interactionActive = false;
        bool enCombat = false;
        Rectangle panneauActuel = null;
        Random random = new Random();
        int pokemonId;
        int pokemonIdEn;
        public chasse()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += GameLoop;
            timer.Start();
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

            // Pokémon random (1 à 151)
           // pokemonIdEn = random.Next(1, 152);
            //pokemonId = random.Next(1, 152);
            pokemonId = 25;
            pokemonIdEn = 25;
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
            //LancerAnimationPokeball();

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
    }
}