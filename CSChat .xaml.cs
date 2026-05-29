using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace cyberg_guard_assistant_chat_app
{
    // ── Manages all bot response logic (OOP: separated concern) ─────────────
    public class CyberGuardResponseEngine
    {
        private readonly Random _random = new Random();

        // ── Random response pools (Req 3: random responses) ─────────────────
        private readonly Dictionary<string, List<string>> _randomResponses = new Dictionary<string, List<string>>
        {
            ["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                "Always hover over links before clicking — the real URL often reveals a fake domain.",
                "Phishing emails often create urgency like 'Your account will be closed!' — pause and verify before acting.",
                "Check the sender's email address carefully. Attackers use addresses like 'support@paypa1.com' to trick you.",
                "When in doubt, go directly to the official website instead of clicking any link in an email."
            },
            ["password"] = new List<string>
            {
                "Make sure to use strong, unique passwords for each account. Avoid using personal details like birthdays or names.",
                "A strong password should be at least 12 characters with uppercase, lowercase, numbers, and symbols.",
                "Never reuse passwords across accounts — if one is stolen, all your accounts become vulnerable.",
                "Consider using a password manager like Bitwarden or 1Password to generate and store passwords safely.",
                "Enable Multi-Factor Authentication (MFA) on every account — it protects you even if your password is stolen."
            },
            ["scam"] = new List<string>
            {
                "Scams rely on urgency and panic. If someone pressures you to act fast — pause and verify first.",
                "Legitimate organisations will never ask for your PIN, password, or payment via gift cards.",
                "If something feels too good to be true, it almost always is. Trust your instincts.",
                "Romance scams and lottery scams are common. Never send money to someone you haven't met in person.",
                "Tech support scams are widespread — Microsoft and Apple will never call you unsolicited about your PC."
            },
            ["privacy"] = new List<string>
            {
                "Limit what personal information you share on social media — attackers harvest this data.",
                "Review app permissions regularly. Many apps collect far more data than they need.",
                "Use a VPN on public Wi-Fi to prevent eavesdropping on your data.",
                "Think carefully before posting anything online — once it's there, it's very hard to remove.",
                "Check your privacy settings on all platforms and restrict who can see your personal information."
            },
            ["malware"] = new List<string>
            {
                "Never download software from unknown or untrusted sources — it may contain hidden malware.",
                "Keep your antivirus software updated and run regular scans on your device.",
                "Ransomware can lock your files permanently. Back up your data regularly to an external drive or cloud.",
                "Malware often arrives through email attachments — never open attachments from unknown senders.",
                "Keep your operating system updated — patches fix known vulnerabilities that malware exploits."
            },
            ["wifi"] = new List<string>
            {
                "Always use a VPN on public Wi-Fi — attackers can intercept your data on unsecured networks.",
                "Avoid logging into banking or sensitive accounts on public Wi-Fi networks.",
                "Make sure your home router uses WPA3 or WPA2 encryption with a strong unique password.",
                "Disable auto-connect on your device so it doesn't automatically join unknown networks.",
                "Use your phone's mobile hotspot instead of public Wi-Fi for sensitive tasks."
            },
            ["tip"] = new List<string>
            {
                "Always keep your software and operating system updated — patches fix security vulnerabilities.",
                "Use two-factor authentication on all important accounts for an extra layer of security.",
                "Back up your data regularly so you can recover from ransomware or hardware failure.",
                "Be cautious of unexpected emails, even from people you know — attackers spoof addresses.",
                "Log out of accounts on shared or public devices — don't leave sessions open.",
                "Check haveibeenpwned.com to see if your email has been in a known data breach.",
                "Use a password manager to generate and store strong unique passwords for every account."
            }
        };

        // ── Sentiment keywords (Req 6) ────────────────────────────────────────
        private readonly Dictionary<string, string> _sentimentResponses = new Dictionary<string, string>
        {
            ["worried"] = "It's completely understandable to feel worried. Cyber threats are real, but awareness is your strongest defence. ",
            ["scared"] = "Don't be scared — you're already doing the right thing by learning about this. ",
            ["nervous"] = "It's okay to feel nervous. Let me help you feel more confident about staying safe online. ",
            ["anxious"] = "I understand the anxiety around cybersecurity. Let's break it down into simple steps. ",
            ["frustrated"] = "I'm sorry you're feeling frustrated. Let's slow down and work through this together. ",
            ["annoyed"] = "I hear you — let's take a step back and I'll explain this more clearly. ",
            ["confused"] = "No worries at all — let me explain this as simply as possible. ",
            ["curious"] = "Great curiosity! That's the first step to staying safe online. ",
            ["interested"] = "Excellent! Staying interested in cybersecurity is one of the best habits you can have. "
        };

        // ── Keyword to topic map (Req 2) ──────────────────────────────────────
        private readonly Dictionary<string, string> _keywordTopicMap = new Dictionary<string, string>
        {
            ["phishing"] = "phishing",
            ["password"] = "password",
            ["scam"] = "scam",
            ["fraud"] = "scam",
            ["privacy"] = "privacy",
            ["personal data"] = "privacy",
            ["malware"] = "malware",
            ["virus"] = "malware",
            ["ransomware"] = "malware",
            ["spyware"] = "malware",
            ["wifi"] = "wifi",
            ["wi-fi"] = "wifi",
            ["network"] = "wifi",
            ["tip"] = "tip",
            ["tips"] = "tip",
            ["advice"] = "tip"
        };

        // ── Context tracking (Req 4) ──────────────────────────────────────────
        public string LastTopic { get; private set; } = "";

        public string GetResponse(string input, string userName, string favoriteTopic)
        {
            string raw = input.ToLower().Trim();

            string sentimentPrefix = DetectSentiment(raw);

            // Follow-up (Req 4: conversation flow)
            if (IsFollowUp(raw))
            {
                if (!string.IsNullOrEmpty(LastTopic) && _randomResponses.ContainsKey(LastTopic))
                    return sentimentPrefix + GetRandom(LastTopic) + BuildSuffix(userName, favoriteTopic, LastTopic);
                return sentimentPrefix + GetRandom("tip") + BuildSuffix(userName, favoriteTopic, "tip");
            }

            // Keyword recognition (Req 2 & 3)
            foreach (var kvp in _keywordTopicMap)
            {
                if (raw.Contains(kvp.Key))
                {
                    LastTopic = kvp.Value;
                    return sentimentPrefix + GetRandom(kvp.Value) + BuildSuffix(userName, favoriteTopic, kvp.Value);
                }
            }

            // Personalization recall (Req 5)
            if (!string.IsNullOrEmpty(favoriteTopic))
                return $"As someone interested in {favoriteTopic}, {userName}, here's something to consider: " + GetRandom("tip");

            return GetDefaultResponse(userName);
        }

        private string DetectSentiment(string input)
        {
            foreach (var kvp in _sentimentResponses)
                if (input.Contains(kvp.Key)) return kvp.Value;
            return "";
        }

        private bool IsFollowUp(string input)
        {
            string[] followUps = { "tell me more", "give me another", "more tips", "another tip",
                                   "explain more", "go on", "continue", "what else", "give me more",
                                   "more info", "more details" };
            foreach (string f in followUps)
                if (input.Contains(f)) return true;
            return false;
        }

        private string GetRandom(string topic)
        {
            if (_randomResponses.ContainsKey(topic))
            {
                var list = _randomResponses[topic];
                return list[new Random().Next(list.Count)];
            }
            return GetDefaultResponse("");
        }

        private string BuildSuffix(string userName, string favoriteTopic, string currentTopic)
        {
            if (!string.IsNullOrEmpty(favoriteTopic) && favoriteTopic == currentTopic)
                return $" Since this is your favourite topic, {userName}, would you like to go even deeper?";
            if (!string.IsNullOrEmpty(userName))
                return $" Feel free to ask me more, {userName}!";
            return "";
        }

        private string GetDefaultResponse(string userName)
        {
            string[] defaults = {
                "I'm not sure I understand. Can you try rephrasing?",
                "I didn't quite catch that. Try asking about passwords, phishing, scams, privacy, or malware.",
                "Hmm, I'm not sure about that one. Could you give me a bit more detail?",
                "I want to help! Try asking something like 'Give me a phishing tip' or 'Tell me about password safety'."
            };
            string response = defaults[new Random().Next(defaults.Length)];
            return string.IsNullOrEmpty(userName) ? response : $"{response} I'm here for you, {userName}!";
        }
    }

    // ── Main window ──────────────────────────────────────────────────────────
    public partial class CSChat : Window
    {
        private string userName;
        private string favoriteTopic = "";
        private int frustrationCount = 0;
        private List<string> sessionHistory = new List<string>();
        private readonly CyberGuardResponseEngine _engine = new CyberGuardResponseEngine();

        private readonly string saveFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "CyberGuardChats"
        );

        public CSChat(string name = "")
        {
            InitializeComponent();
            userName = name;

            if (!string.IsNullOrEmpty(userName))
                AddMessage($"👋 Welcome, {userName}! I'm your cybersecurity assistant. Ask me about scams, privacy, password safety, phishing, or any topic you're curious about.", false);

            LoadSavedChats();
            SavedChatsList.SelectionChanged += SavedChatsList_SelectionChanged;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => ProcessUserMessage();

        private void ChatInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) { ProcessUserMessage(); e.Handled = true; }
        }

        private async void ProcessUserMessage()
        {
            string userMessage = ChatInput.Text.Trim();
            if (string.IsNullOrEmpty(userMessage)) return;

            // Name capture (Req 5)
            if (userMessage.ToLower().StartsWith("my name is"))
            {
                userName = userMessage.ToLower().Replace("my name is", "").Trim();
                AddMessage(userMessage, true);
                ChatInput.Clear();
                string reply = $"Great to meet you, {userName}! I'll remember your name throughout our conversation.";
                AddMessage(reply, false);
                sessionHistory.Add($"User: {userMessage}");
                sessionHistory.Add($"Bot: {reply}");
                return;
            }

            // Favorite topic capture (Req 5)
            if (userMessage.ToLower().Contains("interested in") || userMessage.ToLower().StartsWith("my favorite topic is"))
            {
                string topic = userMessage.ToLower()
                    .Replace("my favorite topic is", "")
                    .Replace("i'm interested in", "")
                    .Replace("i am interested in", "")
                    .Trim();
                favoriteTopic = topic;
                AddMessage(userMessage, true);
                ChatInput.Clear();
                string reply = $"Great! I'll remember that you're interested in {favoriteTopic}, {userName}. It's a crucial part of staying safe online. As someone interested in {favoriteTopic}, you might want to review the security settings on your accounts regularly.";
                AddMessage(reply, false);
                sessionHistory.Add($"User: {userMessage}");
                sessionHistory.Add($"Bot: {reply}");
                return;
            }

            // Frustration tracking (Req 6)
            if (userMessage.ToLower().Contains("frustrated") || userMessage.ToLower().Contains("annoyed"))
            {
                frustrationCount++;
                if (frustrationCount >= 2)
                {
                    AddMessage(userMessage, true);
                    ChatInput.Clear();
                    string reply = $"I hear you, {userName}. I'm sorry this has been frustrating. Let's slow right down — tell me exactly what's confusing you.";
                    AddMessage(reply, false);
                    sessionHistory.Add($"User: {userMessage}");
                    sessionHistory.Add($"Bot: {reply}");
                    return;
                }
            }

            AddMessage(userMessage, true);
            sessionHistory.Add($"User: {userMessage}");
            ChatInput.Clear();

            TypingIndicator.Visibility = Visibility.Visible;
            await Task.Delay(1200);
            TypingIndicator.Visibility = Visibility.Collapsed;

            string botResponse = _engine.GetResponse(userMessage, userName, favoriteTopic);
            AddMessage(botResponse, false);
            sessionHistory.Add($"Bot: {botResponse}");
        }

        private void AddMessage(string text, bool isUser)
        {
            Border bubble = new Border
            {
                Background = isUser ? Brushes.White : Brushes.SkyBlue,
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12),
                Margin = new Thickness(5),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 600
            };

            TextBlock txt = new TextBlock
            {
                Text = text,
                Foreground = isUser ? Brushes.SkyBlue : Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16
            };

            bubble.Child = txt;
            ChatPanel.Children.Add(bubble);
            ChatScrollViewer.ScrollToBottom();
        }

        private void SavedChatsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SavedChatsList.SelectedItem == null) return;
            string selectedDisplay = SavedChatsList.SelectedItem.ToString();
            if (selectedDisplay == "No saved chats yet" || selectedDisplay == "Could not load chats") return;

            try
            {
                string[] files = Directory.GetFiles(saveFolderPath, "*.txt");
                string matchedFile = null;
                foreach (string file in files)
                {
                    string displayName = Path.GetFileNameWithoutExtension(file)
                        .Replace("Chat_", "").Replace("_", " ");
                    if (displayName == selectedDisplay) { matchedFile = file; break; }
                }
                if (matchedFile == null) return;

                ChatPanel.Children.Clear();
                sessionHistory.Clear();
                string[] lines = File.ReadAllLines(matchedFile);

                foreach (string line in lines)
                {
                    if (line.StartsWith("User: "))
                    {
                        string content = line.Substring("User: ".Length);
                        if (content == userName) continue;
                        AddMessage(content, true);
                        sessionHistory.Add(line);
                    }
                    else if (line.StartsWith("Bot: "))
                    {
                        AddMessage(line.Substring("Bot: ".Length), false);
                        sessionHistory.Add(line);
                    }
                }
                SavedChatsList.SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load chat: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TopicButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null) { ChatInput.Text = btn.Tag.ToString(); ProcessUserMessage(); }
        }

        private void ToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            if (SideMenu.Visibility == Visibility.Collapsed)
            {
                SideMenu.Visibility = Visibility.Visible;
                var anim = new ThicknessAnimation
                {
                    From = new Thickness(-250, 0, 0, 0),
                    To = new Thickness(0),
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase()
                };
                SideMenu.BeginAnimation(MarginProperty, anim);
            }
            else
            {
                var anim = new ThicknessAnimation
                {
                    From = new Thickness(0),
                    To = new Thickness(-250, 0, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase()
                };
                anim.Completed += (s, args) => { SideMenu.Visibility = Visibility.Collapsed; SideMenu.BeginAnimation(MarginProperty, null); };
                SideMenu.BeginAnimation(MarginProperty, anim);
            }
        }

        private void NewChat_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentChatToFile();
            ChatPanel.Children.Clear();
            sessionHistory.Clear();
            favoriteTopic = "";
            frustrationCount = 0;
            WelcomeChat welcomeWindow = new WelcomeChat();
            welcomeWindow.Show();
            this.Close();
        }

        private void SaveCurrentChatToFile()
        {
            if (sessionHistory.Count == 0) return;
            try
            {
                if (!Directory.Exists(saveFolderPath)) Directory.CreateDirectory(saveFolderPath);
                string fileName = $"Chat_{userName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                var lines = new List<string>
                {
                    $"CyberGuard Chat — {DateTime.Now:dddd, dd MMMM yyyy HH:mm}",
                    $"User: {userName}",
                    new string('-', 40)
                };
                lines.AddRange(sessionHistory);
                File.WriteAllLines(Path.Combine(saveFolderPath, fileName), lines);
                LoadSavedChats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save chat: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadSavedChats()
        {
            SavedChatsList.Items.Clear();
            try
            {
                if (!Directory.Exists(saveFolderPath)) { SavedChatsList.Items.Add("No saved chats yet"); return; }
                string[] files = Directory.GetFiles(saveFolderPath, "*.txt");
                if (files.Length == 0) { SavedChatsList.Items.Add("No saved chats yet"); return; }
                Array.Sort(files); Array.Reverse(files);
                foreach (string file in files)
                    SavedChatsList.Items.Add(Path.GetFileNameWithoutExtension(file).Replace("Chat_", "").Replace("_", " "));
            }
            catch { SavedChatsList.Items.Add("Could not load chats"); }
        }
    }
}