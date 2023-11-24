using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace RCTelegramBot
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            //Properties.Settings.Default.Reset();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Properties.Settings.Default.OnSystemStartup)
            {
                SetStartup(true);
            }

            if (Properties.Settings.Default.FirstLaunch || string.IsNullOrEmpty(Properties.Settings.Default.BotToken))
            {
                Application.Run(new SetupBotForm());
            }
            else
            {
                MainForm mainForm = new MainForm();

                // При закрытии формы сворачиваем в трей
                mainForm.FormClosing += (sender, args) =>
                {
                    if (Properties.Settings.Default.Collapsed)
                    {
                        mainForm.Hide();
                        mainForm.ShowInTaskbar = false;
                        mainForm.notifyIcon.Visible = true;
                        args.Cancel = true;
                    }
                    Properties.Settings.Default.Save();
                };

                Application.Run(mainForm);
            }

        }

        private static void SetStartup(bool runAtStartup)
        {
            string appName = "RCTelegramBot";
            string executablePath = Application.ExecutablePath;

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (runAtStartup)
            {
                registryKey.SetValue(appName, executablePath);
            }
            else
            {
                registryKey.DeleteValue(appName, false);
            }
        }
    }
}
