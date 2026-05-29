using System.Windows;
using System.Media;

namespace cyberg_guard_assistant_chat_app
{
    public partial class WelcomeChat : Window
    {
        public WelcomeChat()
        {
            InitializeComponent();
            PlayGreeting(); // play sound when WelcomeChat opens
        }

        private void PlayGreeting()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("hello.wav");
                player.Load();
                player.Play();
            }
            catch
            {
                MessageBox.Show("⚠️ Could not play greeting.wav. Make sure the file is in the project folder.");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userName = NameInput.Text.Trim();

            if (!string.IsNullOrEmpty(userName))
            {
                CSChat chatWindow = new CSChat(userName);
                chatWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter your name before starting the chat.");
            }
        }
    }
}
