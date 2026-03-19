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
using Pokemon.Models;
using Pokemon.Services;


namespace Pokemon
{
    /// <summary>
    /// Logique d'interaction pour chatbot.xaml
    /// </summary>
    public partial class chatbot : Window
    {
        private readonly GitHubAiService _ai = new GitHubAiService();

        public chatbot()
        {
            InitializeComponent();
            ChatBox.Text += "IA : Bonjour ! 👋\n";
            ChatBox.Text += "Je suis votre assistant. Posez-moi une question.\n\n";
        }
        private async Task TypeText(string text)
        {
            foreach (char c in text)
            {
                ChatBox.Text += c;
                await Task.Delay(20); // vitesse d'écriture
            }

            ChatBox.Text += "\n\n";
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            var userText = InputBox.Text;

            InputBox.Clear();

            ChatBox.Text += $"Vous : {userText}\n\n";
            ChatBox.Text += "IA : ";

            var messages = new List<ChatMessage>
    {
        new ChatMessage { Role="system", Content=@"
Tu es un assistant spécialisé UNIQUEMENT dans les pokemons.

Règles :
- Tu refuses toute question hors de ce domaine
- Tu refuses toute demande illégale ou dangereuse
- Tu réponds de façon claire et professionnelle
- Si la question est interdite, tu réponds : 'Je ne peux pas répondre à cette demande car je m'occupe que de pokemons.'
" },
        new ChatMessage { Role="user", Content=userText }
    };

            var reply = await _ai.ChatAsync(messages);

            await TypeText(reply);
        }
    }
}