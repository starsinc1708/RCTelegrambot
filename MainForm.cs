using CsvHelper.Configuration;
using CsvHelper;
using Emgu.CV.Flann;
using Newtonsoft.Json.Linq;
using RCTelegramBot.Bot;
using RCTelegramBot.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Linq;
using Telegram.Bot.Types;

namespace RCTelegramBot
{
    public partial class MainForm : Form
    {

        private class BotInfo
        {
            public string BotName { get; set; }
            public string BotUsername { get; set; }
        }

        public NotifyIcon notifyIcon;
        private bool InTaskbar { get; set; }

        private TelegramBot telebot;

        private bool BotEnabled;

        private BotInfo botInfo;

        private StringCollection users = null;

        private BindingList<LogEntry> logEntries = new BindingList<LogEntry>();

        public MainForm()
        {
            InitializeComponent();

            InitializeNotifyIcon();
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

        private void InitializeNotifyIcon()
        {
            Console.WriteLine(InTaskbar);
            notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.Icon, // Замените на свою иконку
                Visible = false, // Не показывать иконку в трее сразу
                ContextMenu = new ContextMenu(new[] { new MenuItem("Открыть", OnOpenClick), new MenuItem("Выход", OnExitClick) })
            };

            // Обработка двойного клика по иконке
            notifyIcon.DoubleClick += (sender, e) => ShowForm();
        }

        private void OnOpenClick(object sender, EventArgs e) => ShowForm();

        private void MainForm_Load(object sender, EventArgs e)
        {
            label3.Text = "";
            linkLabel1.Text = "";
            StartupCheckBox.Checked = Properties.Settings.Default.OnSystemStartup;
            CollapsedCheckBox.Checked = Properties.Settings.Default.Collapsed;
            CollapsedCheckBox.Checked = Properties.Settings.Default.Collapsed;
            checkBox1.Checked = Properties.Settings.Default.Logging;

            if (BotEnabled)
            {
                button1.Text = $"{Smile.BOT}" + " Бот активирован";
            }
            else
            {
                button1.Text = $"{Smile.CLOCK}" + " Бот деактивирован";
            }

            users = Properties.Settings.Default.AllowedUsers;
            foreach (string user in users)
            {
                DataGridViewRow row = new DataGridViewRow();
                DataGridViewCell cell = new DataGridViewTextBoxCell();
                cell.Value = user;
                row.Cells.Add(cell);
                UserNameDataGrid.Rows.Add(row);
            }

            UserNameDataGrid.Refresh();
            UserNameDataGrid.AllowUserToAddRows = false;
            dataGridViewLog.AllowUserToAddRows = false;
            telebot = new TelegramBot(Properties.Settings.Default.BotToken, users);
            telebot.LogEntryReceived += async logEntry => await OnLogEntryReceived(logEntry);
            botInfo = GetBotInfo(Properties.Settings.Default.BotToken);
            Console.WriteLine(dataGridViewLog.Rows.Count);
        }

        private async Task OnLogEntryReceived(LogEntry logEntry)
        {

            if (dataGridViewLog.InvokeRequired)
            {
                dataGridViewLog.Invoke(new Action<LogEntry>(AddLogEntryToDataGridView), logEntry);
                return;
            }

            
        }

        private void AddLogEntryToDataGridView(LogEntry logEntry)
        {
            dataGridViewLog.Rows.Add(logEntry.Username, logEntry.ChatId, logEntry.Command, logEntry.Time);
            dataGridViewLog.Refresh();
        }



        private void ShowForm()
        {
            Show();
            InTaskbar = false;
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            if (notifyIcon.Visible)
            {
                Application.Exit();
                return;
            }
            InTaskbar = true;
            Hide();
            ShowInTaskbar = false;
            notifyIcon.Visible = true;

        }

        protected override void OnResize(EventArgs e)
        {
            if (InTaskbar && WindowState == FormWindowState.Minimized)
            {
                Hide();
                InTaskbar = true;
                notifyIcon.Visible = true;
            }
            else
            {
                base.OnResize(e);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine(users.Count);
            telebot.UpdateUsers(users);
            if (BotEnabled)
            {
                button1.Text = $"{Smile.CLOCK}" + " Бот дективирован";
                telebot.Stop();
                BotEnabled = false;
                label3.Text = "";
                linkLabel1.Text = "";
            }
            else
            {
                button1.Text = $"{Smile.BOT}" + " Бот активирован";
                telebot.Start();
                BotEnabled = true;
                label3.Text = botInfo.BotName;
                linkLabel1.Text = "@" + botInfo.BotUsername;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start($"https://t.me/{botInfo.BotUsername}");
        }

        private void AddUserButton_Click(object sender, EventArgs e)
        {
            string username = UsernameTextBox.Text;
            if (UsernameTextBox.Text.StartsWith("@"))
            { 
                username = username.Substring(1);
            }
            AddUser(username);
            UsernameTextBox.Text = string.Empty;
        }

        private void AddUser(string user)
        {
            DataGridViewRow row = new DataGridViewRow();
            DataGridViewCell cell = new DataGridViewTextBoxCell();
            cell.Value = user;
            row.Cells.Add(cell);
            UserNameDataGrid.Rows.Add(row);
            Properties.Settings.Default.AllowedUsers.Add(user);
            UserNameDataGrid.Refresh();
            telebot.UpdateUsers(users);
            Properties.Settings.Default.Save();
        }

        private void DeleteUserButton_Click(object sender, EventArgs e)
        {
            int selectedUser = UserNameDataGrid.CurrentCell.RowIndex;
            UserNameDataGrid.Rows.RemoveAt(selectedUser);
            Properties.Settings.Default.AllowedUsers.RemoveAt(selectedUser);
            UserNameDataGrid.Refresh();

            Properties.Settings.Default.Save();
            telebot.UpdateUsers(users);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string commandList = "start - Приветствие бота 🤖\r\nhelp - Текущая справка ⁉️\r\nwhoami - Информация о пользователе \U0001f935\r\nsysteminfo - Информация о системе 💻\r\nfileexplorer - Перейти в режим работы с файлами 🗂\r\ncd - Получить текущую директорию 📄\r\nscreens - Информация об экранах 🖥\r\ncameras - Вывести список web-камер 📷\r\nvolume - Управление громкостью 🔉\r\nreboot - Перезагрузить ПК 🔄\r\nshutdown - Выключить ПК \U0001f6d1";
            Clipboard.SetText(commandList);
        }

        private void SaveLaunchSettingsButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.OnSystemStartup = StartupCheckBox.Checked;
            Properties.Settings.Default.Collapsed = CollapsedCheckBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            Application.Restart();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Logging = checkBox1.Checked; 
            Properties.Settings.Default.Save();
        }

        private void buttonSaveLogs_Click(object sender, EventArgs e)
        {
            // Создаем список объектов для хранения данных
            List<LogEntry> logEntries = new List<LogEntry>();

            // Преобразуем данные из DataGridView в объекты CsvLogEntry
            foreach (DataGridViewRow row in dataGridViewLog.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null && row.Cells[2].Value != null && row.Cells[3].Value != null)
                {
                    LogEntry logEntry = new LogEntry
                    {
                        Username = row.Cells[0].Value.ToString(),
                        ChatId = row.Cells[1].Value.ToString(),
                        Command = new string((from c in row.Cells[2].Value.ToString()
                                              where char.IsWhiteSpace(c) || char.IsLetterOrDigit(c)
                                              select c).ToArray()),
                        Time = row.Cells[3].Value.ToString()
                    };
                    logEntries.Add(logEntry);
                }
            }

            // Создаем директорию для сохранения
            string directoryPath = Path.Combine(Application.StartupPath, "logs");
            Directory.CreateDirectory(directoryPath);

            // Генерируем имя файла на основе текущей даты и времени
            string fileName = $"log_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv";
            string filePath = Path.Combine(directoryPath, fileName);

            // Записываем данные в CSV файл
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(logEntries);
            }

            MessageBox.Show("Логи сохранены.");
        }
    }
}
