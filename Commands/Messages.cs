namespace RCTelegramBot.Commands
{
    public static class Messages
    {
        public static readonly string UNKNOWN_COMMAND = $"{Smile.BOT} Извините, не распознал команду. Попробуйте /help для получения справки.";
        public static readonly string START = $"{Smile.BOT} Вас приветствует бот для удалённого управления компьютером. Введите /help для получения справки.";

        public static readonly string INVALID_URL = $"{Smile.DOT_RED} Неверный формат команды.\nИспользуй: /browse url";
        public static readonly string INVALID_ALERT = $"{Smile.DOT_RED} Неверный формат комманды.\nИспользуй: /alert message";
        public static readonly string INVALID_EXEC = $"{Smile.DOT_RED} Неверный формат комманды.\nИспользуй: /exec cmd";

        public static readonly string FILE_EXPLORER_INFO = "Тут будут подсказки по работе с файлами!";

        public static readonly string HELP = "" +
            $"{Smile.DIAMOND_ORANGE} Список доступных команд \r\n\r\n" +
            $"{Smile.DIAMOND_BLUE} Общее\r\n" +
            "/start - Приветствие бота\r\n" +
            "/help - Текущая справка\r\n" +
            "/whoami - Информация о пользователе\r\n" +
            "/browse [url] - Открыть ссылку в браузере по умолчанию\r\n" +
            "/alert [message] - Отобразить уведомление\r\n\n" +
            $"{Smile.DIAMOND_BLUE} Система\r\n" +
            "/systeminfo - Информация о системе\r\n" +
            "/exec [cmd] - Выполнить команду\r\n\r\n" +
            $"{Smile.DIAMOND_BLUE} Файловая система\r\n" +
            "/fileexplorer - Перейти в режим работы с файлами\r\n" +
            "/cd - Получить текущую директорию\r\n" +
            "/cd [path] - Указать текущую директорию\r\n\n" +
            $"{Smile.DIAMOND_BLUE} Медиа\r\n" +
            "/screens - Информация об экранах\r\n" +
            "/cameras - Вывести список web-камер\r\n" +
            "/volume - Управление громкостью\r\n" +
            //$"/media - управление воспроизведением (не работает корректно пока)\r\n\n" +
            /*$"{Smile.DIAMOND_BLUE} Клавиатура\r\n" +
            "/keyboard - Показать клавиатуру и (некоторые) горячие клавиши\r\n" +
            "/key [code1] [code2] ... - Нажать кнопку или комбинацию кнопок\r\n" +
            "[code] - Код кнопки (ENTER, SPACE, BACK_SPACE, ...) или текстовая кнопка (A, B, C, 0, 1, ...)\r\n" +
            "/key__play - Воспроизведение / пауза\r\n" +
            "/key__stop - Остановить воспроизведение\r\n" +
            "/key__next - Следующий трек\r\n/key__prev - Предыдущий трек\r\n" +
            "/media - Клавиатура с медиа кнопками\r\n\r\n" +*/
            $"{Smile.DIAMOND_BLUE} Питание\r\n" +
            "/reboot - Перезагрузить ПК\r\n" +
            "/shutdown - Выключить ПК";
    }


}
