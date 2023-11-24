using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;

namespace RCTelegramBot
{
    public partial class SetupBotForm : Form
    {

        private class BotInfo
        {
            public string BotName { get; set; }
            public string BotUsername { get; set; }
        }

        bool checkCancel = true;

        public SetupBotForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, System.EventArgs e)
        {

        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            e.Cancel = checkCancel;
            checkCancel = true;
        }


        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://t.me/BotFather");
        }

        private BotInfo GetBotInfo(string token)
        {
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string apiUrl = $"https://api.telegram.org/bot{token}/getMe";
                    HttpResponseMessage response = httpClient.GetAsync(apiUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = response.Content.ReadAsStringAsync().Result;
                        JObject jsonResponse = JObject.Parse(responseBody);

                        BotInfo botInfo = new BotInfo
                        {
                            BotName = jsonResponse["result"]["first_name"].ToString(),
                            BotUsername = jsonResponse["result"]["username"].ToString()
                        };

                        return botInfo;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении информации о боте!");
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string token = TokenTextBox1.Text.Trim();
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                BotInfo botInfo = GetBotInfo(token);
                string messageText = $"\nBot Name: {botInfo.BotName}\nUsername: @{botInfo.BotUsername}\n";
                DialogResult result = MessageBox.Show(messageText + "\n\nЭто правильный токен бота?", "Подтверждение", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    checkCancel = false;
                    tabControl1.SelectedIndex = 2;
                    Properties.Settings.Default.BotToken = token;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    TokenTextBox1.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении информации о боте!");
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        private void button1_Click(object sender, EventArgs e)
        {
            checkCancel = false;
            tabControl1.SelectedIndex = 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            if (textBox1.Text.StartsWith("@"))
            {
                username = username.Substring(1);
            }
            Properties.Settings.Default.MainUser = username;
            Properties.Settings.Default.AllowedUsers = new System.Collections.Specialized.StringCollection();
            Properties.Settings.Default.AllowedUsers.Add(username);
            Properties.Settings.Default.FirstLaunch = false;
            Properties.Settings.Default.OnSystemStartup = checkBox1.Checked;
            Properties.Settings.Default.Collapsed = checkBox2.Checked;
            Properties.Settings.Default.FirstLaunch = false;
            Properties.Settings.Default.Save();
            Application.Restart();
            Environment.Exit(0);
        }
    }
}