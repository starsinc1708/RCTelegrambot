using AudioSwitcher.AudioApi.CoreAudio;
using DirectShowLib;
using Emgu.CV.Structure;
using NAudio.Wave;
using RCTelegramBot.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace RCTelegramBot.Bot
{
    public class TelegramBot
    {

        public delegate void LogEntryEventHandler(LogEntry logEntry);

        public event LogEntryEventHandler LogEntryReceived;

        private string API_TOKEN;
        private StringCollection USER_NAMES;
        private FileManager fileManager = new FileManager(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".JPE", ".BMP", ".GIF", ".PNG" };
        public static readonly List<string> MusicExtensions = new List<string> { ".MP3", ".WAV" };

        public TelegramBotClient client;
        private DsDevice[] webcams = null;

        int fileExplorerMode = -1;

        private bool isWorking;

        public TelegramBot(string token, StringCollection users)
        {
            API_TOKEN = token;
            USER_NAMES = users;
            client = new TelegramBotClient(API_TOKEN);
        }

        public void Start()
        {
            client.StartReceiving(Update, Error);
            isWorking = true;
        }

        private async Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            if (!isWorking) { return; }
        }

        private async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (!isWorking) { return; }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && !USER_NAMES.Contains(update.Message.From.Username))
            {
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Вам Запрещено пользоваться ботом! Свяжитесь с его хозяином, чтобы запросить доступ!");
                return;
            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery && !USER_NAMES.Contains(update.CallbackQuery.From.Username))
            {
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Вам Запрещено пользоваться ботом! Свяжитесь с его хозяином, чтобы запросить доступ!");
                return;
            }
            if (fileExplorerMode < 0) // обычный режим бота
            {
                switch (update.Type)
                {
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                        var message = update.Message;
                        if (message == null) { return; }
                        await HandleCommandsAsync(message);
                        break;
                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                        var callbackQuery = update.CallbackQuery;
                        await HandleCallbackQueryAsync(callbackQuery);
                        break;
                }
            }
            else if (fileExplorerMode == 0) // режим работы с файлами
            {
                switch (update.Type)
                {
                    case Telegram.Bot.Types.Enums.UpdateType.Message: 
                        var message = update.Message;
                        if (message == null) { return; }
                        await HandleExplorerCommandsAsync(message);
                        break;
                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                        var callbackQuery = update.CallbackQuery;
                        await HandleExplorerCallbackQueryAsync(callbackQuery);
                        break;
                }
            }
            else if (fileExplorerMode == 1) // режим работы с файлом
            {
                switch (update.Type)
                {
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                        var message = update.Message;
                        if (message == null) { return; }
                        await HandleFileCommandsAsync(message);
                        break;
                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                        var callbackQuery = update.CallbackQuery;
                        await HandleExplorerCallbackQueryAsync(callbackQuery);
                        break;
                }
            }
            if (Properties.Settings.Default.Logging)
            {
                LogEntry logEntry = new LogEntry();
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message) 
                {

                    logEntry.Username = update.Message.From.Username;
                    logEntry.ChatId = update.Message.Chat.Id.ToString();
                    logEntry.Command = "Команда: " + update.Message.Text;
                    logEntry.Time = DateTime.Now.ToString();
                }
                else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {

                    logEntry.Username = update.CallbackQuery.From.Username;
                    logEntry.ChatId = update.CallbackQuery.ChatInstance;
                    logEntry.Command = "Нажата кпопка: " + update.CallbackQuery.Data;
                    logEntry.Time = DateTime.Now.ToString();

                }
                await SendLogEntry(logEntry);
            }

        }

        public async Task SendLogEntry(LogEntry logEntry)
        {
            LogEntryReceived?.Invoke(logEntry);
        }


        [BotCommand("/fileexplorer")]
        private async Task FileExplorerAsync(Message message)
        {
            fileExplorerMode = 0;

            fileManager.ResetFolderPath();

            await client.SendTextMessageAsync(message.Chat.Id, "Вы перешли в режим работы с файлами", replyMarkup: KeyboardCreator.CreateFileManagerKeyboard());
            await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
        }

        [BotCommand("/cd")]
        private async Task CDAsync(Message message)
        {
            string command = message.Text;

            string[] commandParts = command.Split(' ');

            if (fileExplorerMode < 0)
            {
                fileExplorerMode = 0;
                await client.SendTextMessageAsync(message.Chat.Id, "Вы перешли в режим работы с файлами", replyMarkup: KeyboardCreator.CreateFileManagerKeyboard());
                if (commandParts.Length > 1)
                {
                    fileManager.GoToPath(commandParts[1]);
                    await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
                }
                return;
            }

            if (commandParts.Length > 1)
            {
                fileManager.GoToPath(commandParts[1]);
                await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
            }

        }

        [BotCommand("/start")]
        private async Task StartCommandAsync(Message message)
        {
            await client.SendTextMessageAsync(message.Chat.Id, Messages.START, replyMarkup: KeyboardCreator.CreateDufaultKeyboard());
        }

        [BotCommand("/reboot")]
        private async Task RebootCommandAsync(Message message)
        {
            try
            {
                Process.Start("shutdown", "/r /t 0");  // Перезагрузка
                await client.SendTextMessageAsync(message.Chat.Id, "Компьютер будет перезагружен.");
            }
            catch (Exception ex)
            {
                await client.SendTextMessageAsync(message.Chat.Id, $"Произошла ошибка: {ex.Message}");
            }
        }

        [BotCommand("/shutdown")]
        private async Task ShutdownCommandAsync(Message message)
        {
            try
            {
                Process.Start("cmd", "/c shutdown -s -f -t 00");
                await client.SendTextMessageAsync(message.Chat.Id, "Компьютер будет выключен.");
            }
            catch (Exception ex)
            {
                await client.SendTextMessageAsync(message.Chat.Id, $"Произошла ошибка: {ex.Message}");
            }
        }

        [BotCommand("/help")]
        private async Task HelpCommandAsync(Message message)
        {
            await client.SendTextMessageAsync(message.Chat.Id, Messages.HELP, replyMarkup: KeyboardCreator.CreateDufaultKeyboard());
        }

        [BotCommand("/alert")]
        private async Task AlertCommandAsync(Message message)
        {
            string[] commandParts = message.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (commandParts.Length < 2)
            {
                await client.SendTextMessageAsync(message.Chat.Id, Messages.INVALID_ALERT);
                return;
            }

            string alertMessage = string.Join(" ", commandParts.Skip(1));

            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                MessageBox.Show(alertMessage, "Уведомление");
            }));

            // Устанавливаем апартности потока и запускаем его
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();


            await client.SendTextMessageAsync(message.Chat.Id, $"Отображаю окно...");

        }

        [BotCommand("/browse")]
        private async Task BrowseCommandAsync(Message message)
        {
            string[] commandParts = message.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (commandParts.Length != 2)
            {
                await client.SendTextMessageAsync(message.Chat.Id, Messages.INVALID_URL);
                return;
            }

            string url = commandParts[1];
            if (!commandParts[1].StartsWith("https://"))
            {
                url = "https://" + url;
            }

            try
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                await client.SendTextMessageAsync(message.Chat.Id, $"Перехожу на адрес: {url}", disableWebPagePreview: true);
            }
            catch (ArgumentNullException ex)
            {
                await client.SendTextMessageAsync(message.Chat.Id, $"Ошибка при открытии браузера: {ex.Message}");
            }
        }

        [BotCommand("/whoami")]
        private async Task WhoAmIAsync(Message message)
        {
            string userInfo = $"User ID: {message.From.Id}\n" +
                              $"First Name: {message.From.FirstName}\n" +
                              $"Last Name: {message.From.LastName}\n" +
                              $"Username: {message.From.Username}\n";

            string chatInfo = $"Chat ID: {message.Chat.Id}\n" +
                              $"Chat Type: {message.Chat.Type}\n" +
                              $"Chat Title: {message.Chat.Title}\n";

            string combinedInfo = $"{Smile.USER} User Info:\n" + userInfo + "\n" +
                                  $"{Smile.HOME} Chat Info:\n" + chatInfo;

            await client.SendTextMessageAsync(message.Chat.Id, combinedInfo);
        }

        [BotCommand("/exec")]
        private async Task ExecCommandAsync(Message message)
        {
            string[] commandParts = message.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (commandParts.Length < 2)
            {
                await client.SendTextMessageAsync(message.Chat.Id, Messages.INVALID_EXEC);
                return;
            }

            string cmd = string.Join(" ", commandParts.Skip(1));

            string result = ExecuteCommand(cmd);

            await client.SendTextMessageAsync(message.Chat.Id, $"Результат выполнения команды:\n{result}");
        }

        [BotCommand("/systeminfo")]
        private async Task SystemInfoAsync(Message message)
        {
            string cpuInfo = "";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    cpuInfo += $"\t\t{Smile.DIAMOND_BLUE} Имя: {obj["Name"]}\n";
                    cpuInfo += $"\t\t{Smile.DIAMOND_BLUE} Архитектура: {obj["Architecture"]}\n";
                    cpuInfo += $"\t\t{Smile.DIAMOND_BLUE} Производитель: {obj["Manufacturer"]}\n";
                    cpuInfo += $"\t\t{Smile.DIAMOND_BLUE} Максимальная тактовая частота: {obj["MaxClockSpeed"]} MHz\n";
                }
            }

            string gpuInfo = "";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    gpuInfo += $"\t\t{Smile.DIAMOND_BLUE}Имя: {obj["Caption"]}\n";
                    gpuInfo += $"\t\t{Smile.DIAMOND_BLUE}Производитель: {obj["AdapterCompatibility"]}\n";
                    gpuInfo += $"\t\t{Smile.DIAMOND_BLUE}Объем памяти: {Math.Round(Convert.ToDouble(obj["AdapterRAM"]) / (1024 * 1024 * 1024), 2)} GB\n";
                }
            }

            string ramInfo = "";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    ramInfo += Math.Round(Convert.ToDouble(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024), 2);
                }
            }

            string formattedMessage = $"{Smile.DIAMOND_ORANGE} Информация о компьютере:\n" +
                $"\t\t{Smile.DIAMOND_BLUE} Имя компьютера: {Environment.MachineName}\n" +
                $"\t\t{Smile.DIAMOND_BLUE} Версия ОС: {Environment.OSVersion}\n" +
                $"{Smile.DIAMOND_ORANGE} CPU:\n" +
                $"{cpuInfo}" +
                $"{Smile.DIAMOND_ORANGE}GPU:\n" +
                $"{gpuInfo}" +
                $"{Smile.DIAMOND_ORANGE} Оперативная память:\n" +
                $"\t\tОбщий объем оперативной памяти: {ramInfo} GB\n";

            await client.SendTextMessageAsync(message.Chat.Id, formattedMessage);
        }

        [BotCommand("/screens")]
        private async Task ScreensAsync(Message message)
        {
            string screenInfo = "Список подключенных мониторов:\n";

            List<List<InlineKeyboardButton>> keyboardButtons = new List<List<InlineKeyboardButton>>();

            Screen[] screens = Screen.AllScreens;

            for (int i = 0; i < screens.Length; i++)
            {
                screenInfo += $"Монитор {i + 1}: {screens[i].DeviceName}\n";
                screenInfo += $"Размер экрана: {screens[i].Bounds.Width}x{screens[i].Bounds.Height} pixels\n\n";

                // Добавляем кнопку для скриншота монитора i
                List<InlineKeyboardButton> row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(text: $"Сделать скриншот [{i}]", callbackData: $"screenshot_{i}")
                };

                keyboardButtons.Add(row);
            }

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardButtons);

            await client.SendTextMessageAsync(message.Chat.Id, screenInfo, replyMarkup: keyboard);
        }

        [BotCommand("/cameras")]
        private async Task CamerasAsync(Message message)
        {

            List<List<InlineKeyboardButton>> keyboardButtons = new List<List<InlineKeyboardButton>>();
            webcams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            int i = 0;
            var cameraInfo = $"Список подключенных камер ({i}):\n";

            foreach (var webcam in webcams)
            {
                cameraInfo += $"[cam {i}] [{webcam.Name}]\n";
                List<InlineKeyboardButton> row = new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData(text: $"Сделать фото [{i}]", callbackData: $"photo_{i}")
                    };
                i++;
                keyboardButtons.Add(row);
            }

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardButtons);
            await client.SendTextMessageAsync(message.Chat.Id, cameraInfo, replyMarkup: keyboard);

        }

        [BotCommand("/volume")]
        private async Task VolumeAsync(Message message)
        {
            int currentVolume;
            using (var sessionManager = new CoreAudioController())
            {
                var defaultPlaybackDevice = sessionManager.DefaultPlaybackDevice;
                currentVolume = (int)defaultPlaybackDevice.Volume;
            }
            await client.SendTextMessageAsync(message.Chat.Id, $"{Smile.SOUND_50} Volume {currentVolume}", replyMarkup: KeyboardCreator.CreateVolumeKeyboard());
        }


        private async Task HandleCommandsAsync(Message message)
        {
            string command = message.Text;

            if (!command.StartsWith("/"))
            {
                command = "/" + command;
            }

            string[] commandParts = command.Split(' ');

            if (commandParts.Length > 1)
            {
                command = commandParts[0].ToLower() + string.Join(" ", commandParts.Skip(1));
            }
            else
            {
                command = command.ToLower();
            }

            MethodInfo[] methods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<BotCommandAttribute>();
                if (attr != null && command.StartsWith(attr.Command))
                {
                    await (Task)method.Invoke(this, new object[] { message });
                    return;
                }
            }

            await client.SendTextMessageAsync(message.Chat.Id, Messages.UNKNOWN_COMMAND);

        }
        private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            string callbackData = callbackQuery.Data;

            if (callbackData.StartsWith("screenshot_"))
            {
                int screenIndex = int.Parse(callbackData.Substring("screenshot_".Length));
                await CaptureScreenAndSendAsync(screenIndex, callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
            }
            else if (callbackData.StartsWith("photo_"))
            {
                int cameraIndex = int.Parse(callbackData.Substring("photo_".Length));
                await CreatePhotoAndSendAsync(cameraIndex, callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

            }
            else if (callbackData.StartsWith("volume_"))
            {
                var volumeParam = Convert.ToString(callbackData.Substring("volume_".Length));
                await ChangeVolumeAsync(volumeParam, callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

            }
            /*else if (callbackData.StartsWith("mediarc"))
            {
                await  OpenMediaPanelAsync(callbackQuery.Message);
            }
            else if (callbackData.StartsWith("media_"))
            {
                var mediaControlParam = Convert.ToString(callbackData.Substring("media_".Length));
                await MediaControlAsync(mediaControlParam, callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
            }*/
        }

        private async Task HandleExplorerCommandsAsync(Message message)
        {
            if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                string[] commandParts = message.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    if (!commandParts[0].StartsWith("/") && !commandParts[0].StartsWith("."))
                    {
                        commandParts[0] = "/" + commandParts[0];
                    }
                    switch (commandParts[0])
                    {
                        case "/Выйти":
                            fileExplorerMode = -1;
                            await HelpCommandAsync(message);
                            break;
                        case "/Помощь":
                            await client.SendTextMessageAsync(message.Chat.Id, Messages.FILE_EXPLORER_INFO, replyMarkup: KeyboardCreator.CreateFileManagerKeyboard());
                            break;
                        case "/RootFolder":
                            fileManager.MoveToRootFolder();
                            await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
                            break;
                        case "../":
                            fileManager.MoveUp();
                            await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
                            break;
                        case var _ when commandParts[0].StartsWith("/cd", StringComparison.OrdinalIgnoreCase):
                            await CDAsync(message);
                            break;
                        case var _ when commandParts[0].StartsWith("/folder_", StringComparison.OrdinalIgnoreCase):
                            int folderNumber = Convert.ToInt32(commandParts[0].Substring("/folder_".Length));
                            fileManager.MovetoFolder(folderNumber);
                            await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
                            break;
                        case var _ when commandParts[0].StartsWith("/file_", StringComparison.OrdinalIgnoreCase):
                            fileExplorerMode = 1;
                            int fileNumber = Convert.ToInt32(commandParts[0].Substring("/file_".Length));
                            await client.SendTextMessageAsync(message.Chat.Id, fileManager.ShowFileInfo(fileNumber), replyMarkup: KeyboardCreator.CreateFileKeyboard());
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, ex.Message);
                }
            }
            if (message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
            {
                //await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(offset: 0), replyMarkup: CreateFolderKeyboard());
            }
        }
        private async Task HandleExplorerCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            string callbackData = callbackQuery.Data;

            if (callbackData.StartsWith("folderlist_"))
            {
                string listModifyType = Convert.ToString(callbackData.Substring("folderlist_".Length));
                await ChangeFolderListAsync(listModifyType, callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
            }
        }

        private async Task HandleFileCommandsAsync(Message message)
        {
            if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                string[] commandParts = message.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    switch (commandParts[0])
                    {
                        case "Запустить":
                            fileManager.OpenFile();
                            await client.SendTextMessageAsync(message.Chat.Id, "Файл Открывается...", replyMarkup: KeyboardCreator.CreateFileKeyboard());
                            break;
                        case "Скачать":
                            using (var stream = System.IO.File.OpenRead(fileManager.CurrentFile))
                            {
                                if (ImageExtensions.Contains(Path.GetExtension(fileManager.CurrentFile).ToUpperInvariant()))
                                {
                                    await client.SendPhotoAsync(message.Chat.Id, InputFile.FromStream(stream));
                                }
                                else if (MusicExtensions.Contains(Path.GetExtension(fileManager.CurrentFile).ToUpperInvariant()))
                                {
                                    await client.SendAudioAsync(message.Chat.Id, InputFile.FromStream(stream));
                                }
                                else
                                {
                                    await client.SendDocumentAsync(message.Chat.Id, InputFile.FromStream(stream));
                                }
                            }
                            break;
                        case "Помощь":
                            await client.SendTextMessageAsync(message.Chat.Id, Messages.FILE_EXPLORER_INFO, replyMarkup: KeyboardCreator.CreateFileKeyboard());
                            break;
                        case var _ when commandParts[0].StartsWith("/cd", StringComparison.OrdinalIgnoreCase):
                            fileExplorerMode = 0;
                            await CDAsync(message);
                            break;
                        case var _ when commandParts[0].StartsWith("/folder_", StringComparison.OrdinalIgnoreCase):
                            fileExplorerMode = 0;
                            int folderNumber = Convert.ToInt32(commandParts[0].Substring("/folder_".Length));
                            fileManager.MovetoFolder(folderNumber);
                            await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFileManagerKeyboard());
                            break;
                        case var _ when commandParts[0].StartsWith("/file_", StringComparison.OrdinalIgnoreCase):
                            fileExplorerMode = 1;
                            int fileNumber = Convert.ToInt32(commandParts[0].Substring("/file_".Length));
                            await client.SendTextMessageAsync(message.Chat.Id, fileManager.ShowFileInfo(fileNumber), replyMarkup: KeyboardCreator.CreateFileKeyboard());
                            break;
                        case "RootFolder":
                            fileExplorerMode = 0;
                            fileManager.MoveToRootFolder();
                            await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
                            break;
                        case "../":
                            fileExplorerMode = 0;
                            fileManager.MoveUp();
                            await client.SendTextMessageAsync(message.Chat.Id, fileManager.CurrentFolderContent(), replyMarkup: KeyboardCreator.CreateFileManagerKeyboard());
                            break;
                        case "Выйти":
                            fileExplorerMode = -1;
                            await HelpCommandAsync(message);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, ex.Message);
                }
            }
        }


        private string ExecuteCommand(string cmd)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                var processInfo = new ProcessStartInfo("cmd.exe", $"/c {cmd}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.GetEncoding(866)
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                        return "Невозможно начать процесс выполнения команды.";

                    string result = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return result;
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка при выполнении команды: {ex.Message}";
            }
        }

        private async Task ChangeFolderListAsync(string listModifyType, long chatId, int messageId)
        {
            try
            {
                int page = fileManager.Page;
                switch (listModifyType)
                {
                    case "first":
                        page = 1;
                        break;
                    case "prev":
                        page -= 1;
                        if (page < 1)
                            page = 1;
                        break;
                    case "next":
                        page += 1;
                        if (page > fileManager.MaxPages)
                            page = fileManager.MaxPages;
                        break;
                    case "last":
                        page = fileManager.MaxPages;
                        break;
                }
                await client.EditMessageTextAsync(chatId, messageId, fileManager.CurrentFolderContent(page), replyMarkup: KeyboardCreator.CreateFolderKeyboard());
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException)
            {

            }
            catch (IndexOutOfRangeException)
            {

            }

        }

        private async Task ChangeVolumeAsync(string volumeParam, long chatId, int messageId)
        {
            using (var sessionManager = new CoreAudioController())
            {
                var defaultPlaybackDevice = sessionManager.DefaultPlaybackDevice;
                int volumeLevel = (int)defaultPlaybackDevice.Volume;

                if (int.TryParse(volumeParam, out int temp))
                {
                    volumeLevel = int.Parse(volumeParam);
                    defaultPlaybackDevice.Volume = volumeLevel;
                    await client.EditMessageTextAsync(chatId, messageId, $"{Smile.SOUND_50} Volume {volumeLevel}", replyMarkup: KeyboardCreator.CreateVolumeKeyboard());
                    return;
                }

                if (volumeParam == "+" && volumeLevel < 100)
                {
                    volumeLevel += 5;
                    defaultPlaybackDevice.Volume = volumeLevel;
                    await client.EditMessageTextAsync(chatId, messageId, $"{Smile.SOUND_50} Volume {volumeLevel}", replyMarkup: KeyboardCreator.CreateVolumeKeyboard());
                    return;
                }

                if (volumeParam == "-" && volumeLevel > 0)
                {
                    volumeLevel -= 5;
                    defaultPlaybackDevice.Volume = volumeLevel;
                    await client.EditMessageTextAsync(chatId, messageId, $"{Smile.SOUND_50} Volume {volumeLevel}", replyMarkup: KeyboardCreator.CreateVolumeKeyboard());
                    return;
                }
                return;
            }
        }

        private async Task CaptureScreenAndSendAsync(int screenIndex, long chatId, int messageId)
        {
            Screen selectedScreen = Screen.AllScreens[screenIndex];
            Bitmap screenshot = new Bitmap(selectedScreen.Bounds.Width, selectedScreen.Bounds.Height);

            using (Graphics g = Graphics.FromImage(screenshot))
            {
                // Улучшаем качество сглаживания
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // Захватываем изображение экрана
                g.CopyFromScreen(selectedScreen.Bounds.X, selectedScreen.Bounds.Y, 0, 0, selectedScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            }


            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string screenshotFolder = Path.Combine(appPath, "ScreenShots");

            if (!Directory.Exists(screenshotFolder))
            {
                Directory.CreateDirectory(screenshotFolder);
            }

            string fileName = $"screenshot_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.jpg";
            string filePath = Path.Combine(screenshotFolder, fileName);
            screenshot.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

            screenshot.Dispose();

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await client.SendPhotoAsync(chatId, InputFileStream.FromStream(stream), replyToMessageId: messageId);
            }
        }

        private async Task CreatePhotoAndSendAsync(int cameraIndex, long chatId, int messageId)
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string photoFolder = Path.Combine(appPath, "CameraPhotos");
            if (!Directory.Exists(photoFolder))
            {
                Directory.CreateDirectory(photoFolder);
            }

            string fileName = $"camera{cameraIndex}_photo_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.jpg";
            string filePath = Path.Combine(photoFolder, fileName);

            using (var capture = new Emgu.CV.Capture(cameraIndex))
            {
                capture.Start();

                await Task.Delay(1000);

                using (var image = capture.QueryFrame().ToImage<Bgr, byte>().Bitmap)
                {
                    capture.Stop();

                    using (var bitmap = image.Clone() as Bitmap)
                    {
                        bitmap.Save(filePath);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        await client.SendPhotoAsync(chatId, InputFile.FromStream(stream), replyToMessageId: messageId);
                    }
                }
            }
        }

        internal void Stop()
        {
            isWorking = false;
        }

        internal void UpdateUsers(StringCollection users)
        {
            USER_NAMES = users;
        }
    }
}
