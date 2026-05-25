using System.Windows;
using System.Windows.Controls;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class MainWindow : Window
    {
        private readonly TokenService _tokenService;
        private string _userRole;

        public MainWindow()
        {
            InitializeComponent();
            _tokenService = ServiceLocator.Instance.TokenService;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Получаем роль пользователя
            _userRole = _tokenService.GetCurrentUserRole();
            var userName = _tokenService.GetCurrentUserName();

            // Обновляем заголовок окна с именем пользователя
            if (!string.IsNullOrEmpty(userName))
            {
                this.Title = $"Система оценивания - {userName} ({(_userRole == "teacher" ? "Преподаватель" : "Студент")})";
            }

            // Проверяем роль и перенаправляем на главную страницу
            if (_userRole == "teacher")
            {
                MainFrame.Navigate(new HomePage());
            }
            else if (_userRole == "student")
            {
                MainFrame.Navigate(new StudentHomePage());
            }
            else
            {
                MessageBox.Show("Неизвестная роль пользователя. Пожалуйста, войдите снова.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void Menu_NavigationRequested(string pageName)
        {
            switch (pageName)
            {
                case "HomePage":
                    // Перенаправляем на соответствующую главную страницу в зависимости от роли
                    if (_userRole == "student")
                        MainFrame.Navigate(new StudentHomePage());
                    else
                        MainFrame.Navigate(new HomePage());
                    break;
                case "Lessons":
                    MainFrame.Navigate(new LessonsPage());
                    break;
                case "Groups":
                    MainFrame.Navigate(new GroupsPage());
                    break;
                case "Assignments":
                    // Для студентов открываем StudentAssignmentsPage, для учителей - AssignmentsPage
                    if (_userRole == "student")
                        MainFrame.Navigate(new StudentAssignmentsPage());
                    else
                        MainFrame.Navigate(new AssignmentsPage());
                    break;
                default:
                    // По умолчанию также учитываем роль
                    if (_userRole == "student")
                        MainFrame.Navigate(new StudentHomePage());
                    else
                        MainFrame.Navigate(new HomePage());
                    break;
            }
        }
    }
}