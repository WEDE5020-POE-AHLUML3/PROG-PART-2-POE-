using System.Windows;

namespace cyberg_guard_assistant_chat_app
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Button click handler to open WelcomeChat
        private void OpenWelcomeChat_Click(object sender, RoutedEventArgs e)
        {
            WelcomeChat welcomeWindow = new WelcomeChat();
            welcomeWindow.Show();
            this.Close(); // close MainWindow after opening WelcomeChat
        }
    }
}
