using RCTelegramBot.Commands;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace RCTelegramBot.Bot
{
    public class KeyboardCreator
    {
        public static InlineKeyboardMarkup CreateVolumeKeyboard()
        {
            List<List<InlineKeyboardButton>> keyboardButtons = new List<List<InlineKeyboardButton>>();

            List<InlineKeyboardButton> row1 = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_0} 0%", callbackData: "volume_0"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_25} 5%", callbackData: "volume_5"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_25} 10%", callbackData: "volume_10"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_50} 20%", callbackData: "volume_20")
            };

            List<InlineKeyboardButton> row2 = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_50} 30%", callbackData: "volume_30"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_50} 40%", callbackData: "volume_40"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_50} 50%", callbackData: "volume_50"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_50} 60%", callbackData: "volume_60")
            };

            List<InlineKeyboardButton> row3 = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_50} 70%", callbackData: "volume_70"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_100} 80%", callbackData: "volume_80"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_100} 90%", callbackData: "volume_90"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.SOUND_100} 100%", callbackData: "volume_100")
            };

            List<InlineKeyboardButton> row4 = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.ARROW_UP} Volume+", callbackData: "volume_+"),
                //InlineKeyboardButton.WithCallbackData(text: $"{Smile.MEDIA} Media Control", callbackData: "mediarc"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.ARROW_DOW} Volume-", callbackData: "volume_-"),
            };


            keyboardButtons.Add(row1);
            keyboardButtons.Add(row2);
            keyboardButtons.Add(row3);
            keyboardButtons.Add(row4);


            return new InlineKeyboardMarkup(keyboardButtons);
        }

        public static ReplyKeyboardMarkup CreateFileManagerKeyboard()
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new []
                {
                    new KeyboardButton($"RootFolder {Smile.HOME}"),
                    new KeyboardButton($"../ {Smile.UP}"),
                },
                new []
                {
                    new KeyboardButton($"Выйти {Smile.BACK}"),
                    new KeyboardButton($"Помощь {Smile.HELP}")
                }
            });

            replyKeyboard.ResizeKeyboard = true;
            replyKeyboard.OneTimeKeyboard = false;

            return replyKeyboard;
        }

        public static InlineKeyboardMarkup CreateFolderKeyboard()
        {
            List<List<InlineKeyboardButton>> keyboardButtons = new List<List<InlineKeyboardButton>>();

            List<InlineKeyboardButton> row1 = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.FIRST_LIST}", callbackData: "folderlist_first"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.PREV_LIST}", callbackData: "folderlist_prev"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.NEXT_LIST}", callbackData: "folderlist_next"),
                InlineKeyboardButton.WithCallbackData(text: $"{Smile.LAST_LIST}", callbackData: "folderlist_last")
            };

            keyboardButtons.Add(row1);

            return new InlineKeyboardMarkup(keyboardButtons);
        }

        public static ReplyKeyboardMarkup CreateDufaultKeyboard()
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new []
                {
                    new KeyboardButton($"Screens {Smile.DISPLAY}"),
                    new KeyboardButton($"Cameras {Smile.CAMERA}"),
                    new KeyboardButton($"FileExplorer {Smile.FOLDER}"),
                },
                new []
                {
                    new KeyboardButton($"Volume {Smile.SOUND_50}"),
                    new KeyboardButton($"Reboot {Smile.ARROW_REFRESH}"),
                    new KeyboardButton($"Shutdown {Smile.DOT_RED}")
                }
            });

            // Устанавливаем свойства клавиатуры
            replyKeyboard.ResizeKeyboard = true;
            replyKeyboard.OneTimeKeyboard = false;
            return replyKeyboard;
        }


        public static ReplyKeyboardMarkup CreateFileKeyboard()
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new []
                {
                    new KeyboardButton($"Запустить Файл {Smile.OPEN_FILE}"),
                    new KeyboardButton($"Скачать файл {Smile.DOWNLOAD}"),
                },
                new []
                {
                    new KeyboardButton($"RootFolder {Smile.HOME}"),
                    new KeyboardButton($"../ {Smile.UP}"),
                },
                new []
                {
                    new KeyboardButton($"Помощь {Smile.HELP}"),
                    new KeyboardButton($"Выйти {Smile.BACK}"),
                }
            });

            replyKeyboard.ResizeKeyboard = true;
            replyKeyboard.OneTimeKeyboard = false;

            return replyKeyboard;
        }
    }
}
