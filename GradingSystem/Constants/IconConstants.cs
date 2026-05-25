namespace GradingSystem.Constants
{
    /// <summary>
    /// Константы для названий иконок, используемых в приложении.
    /// Используйте эти названия для поиска иконок в библиотеках:
    /// - Lucide Icons: https://lucide.dev/
    /// - Heroicons: https://heroicons.com/
    /// - Feather Icons: https://feathericons.com/
    /// - Material Icons: https://fonts.google.com/icons
    /// </summary>
    public static class IconConstants
    {
        #region Навигация и действия

        /// <summary>
        /// Иконка обновления/перезагрузки данных
        /// Поиск: "refresh", "reload", "sync"
        /// </summary>
        public const string Refresh = "refresh";

        /// <summary>
        /// Иконка поиска
        /// Поиск: "search", "magnifying-glass", "find"
        /// </summary>
        public const string Search = "search";

        /// <summary>
        /// Иконка добавления
        /// Поиск: "plus", "add", "create"
        /// </summary>
        public const string Add = "plus";

        /// <summary>
        /// Иконка редактирования
        /// Поиск: "edit", "pencil", "pen"
        /// </summary>
        public const string Edit = "edit";

        /// <summary>
        /// Иконка удаления
        /// Поиск: "trash", "delete", "bin"
        /// </summary>
        public const string Delete = "trash";

        #endregion

        #region Статистика и основные сущности

        /// <summary>
        /// Иконка групп/пользователей
        /// Поиск: "users", "group", "people"
        /// </summary>
        public const string Groups = "users";

        /// <summary>
        /// Иконка студентов
        /// Поиск: "graduation-cap", "academic-cap", "student"
        /// </summary>
        public const string Students = "graduation-cap";

        /// <summary>
        /// Иконка заданий
        /// Поиск: "document", "file-text", "assignment"
        /// </summary>
        public const string Assignments = "file-text";

        /// <summary>
        /// Иконка курсов
        /// Поиск: "books", "book-open", "library"
        /// </summary>
        public const string Courses = "book-open";

        /// <summary>
        /// Иконка работ/списка
        /// Поиск: "clipboard", "list", "clipboard-list"
        /// </summary>
        public const string Submissions = "clipboard-list";

        /// <summary>
        /// Иконка оценки
        /// Поиск: "star", "award", "medal"
        /// </summary>
        public const string Grade = "star";

        /// <summary>
        /// Иконка пользователя
        /// Поиск: "user", "person", "account"
        /// </summary>
        public const string User = "user";

        /// <summary>
        /// Иконка календаря/дедлайна
        /// Поиск: "calendar", "date", "schedule"
        /// </summary>
        public const string Calendar = "calendar";

        /// <summary>
        /// Иконка статистики
        /// Поиск: "chart", "bar-chart", "analytics"
        /// </summary>
        public const string Statistics = "bar-chart";

        #endregion

        #region Статусы

        /// <summary>
        /// Иконка выполнено/успех
        /// Поиск: "check", "checkmark", "check-circle"
        /// </summary>
        public const string Completed = "check-circle";

        /// <summary>
        /// Иконка не выполнено/ошибка
        /// Поиск: "x", "close", "x-circle"
        /// </summary>
        public const string NotCompleted = "x-circle";

        /// <summary>
        /// Иконка предупреждения
        /// Поиск: "alert", "warning", "alert-triangle"
        /// </summary>
        public const string Warning = "alert-triangle";

        #endregion

        #region Типы курсов

        /// <summary>
        /// Иконка программирования
        /// Поиск: "code", "terminal", "brackets"
        /// </summary>
        public const string Programming = "code";

        /// <summary>
        /// Иконка базы данных
        /// Поиск: "database", "server", "storage"
        /// </summary>
        public const string Database = "database";

        /// <summary>
        /// Иконка веб-разработки
        /// Поиск: "globe", "world", "web"
        /// </summary>
        public const string Web = "globe";

        /// <summary>
        /// Иконка алгоритмов
        /// Поиск: "cpu", "processor", "chip"
        /// </summary>
        public const string Algorithms = "cpu";

        /// <summary>
        /// Иконка математики
        /// Поиск: "calculator", "math", "function"
        /// </summary>
        public const string Mathematics = "calculator";

        #endregion

        #region Типы активности

        /// <summary>
        /// Иконка комментария
        /// Поиск: "message", "comment", "chat"
        /// </summary>
        public const string Comment = "message-circle";

        /// <summary>
        /// Иконка по умолчанию
        /// Поиск: "pin", "bookmark", "flag"
        /// </summary>
        public const string Default = "bookmark";

        #endregion
    }

    /// <summary>
    /// Пути к файлам иконок в проекте
    /// </summary>
    public static class IconPaths
    {
        private const string IconsFolder = "/Icons/";

        #region Навигация и действия
        public static string Refresh => $"{IconsFolder}refresh.svg";
        public static string Search => $"{IconsFolder}search.svg";
        public static string Add => $"{IconsFolder}plus.svg";
        public static string Edit => $"{IconsFolder}edit.svg";
        public static string Delete => $"{IconsFolder}trash.svg";
        #endregion

        #region Статистика и основные сущности
        public static string Groups => $"{IconsFolder}users.svg";
        public static string Students => $"{IconsFolder}graduation-cap.svg";
        public static string Assignments => $"{IconsFolder}file-text.svg";
        public static string Courses => $"{IconsFolder}book-open.svg";
        public static string Submissions => $"{IconsFolder}clipboard-list.svg";
        public static string Grade => $"{IconsFolder}star.svg";
        public static string User => $"{IconsFolder}user.svg";
        public static string Calendar => $"{IconsFolder}calendar.svg";
        public static string Statistics => $"{IconsFolder}bar-chart.svg";
        #endregion

        #region Статусы
        public static string Completed => $"{IconsFolder}check-circle.svg";
        public static string NotCompleted => $"{IconsFolder}x-circle.svg";
        public static string Warning => $"{IconsFolder}alert-triangle.svg";
        #endregion

        #region Типы курсов
        public static string Programming => $"{IconsFolder}code.svg";
        public static string Database => $"{IconsFolder}database.svg";
        public static string Web => $"{IconsFolder}globe.svg";
        public static string Algorithms => $"{IconsFolder}cpu.svg";
        public static string Mathematics => $"{IconsFolder}calculator.svg";
        #endregion

        #region Типы активности
        public static string Comment => $"{IconsFolder}message-circle.svg";
        public static string Default => $"{IconsFolder}bookmark.svg";
        #endregion
    }
}
