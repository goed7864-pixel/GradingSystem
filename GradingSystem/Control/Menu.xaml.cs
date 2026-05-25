using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GradingSystem.Services;

namespace GradingSystem.Control
{
    public partial class Menu : UserControl
    {
        public event NavigationRequestedEventHandler NavigationRequested;
        public delegate void NavigationRequestedEventHandler(string pageName);

        private readonly TokenService _tokenService;
        private string _userRole;
        private Border _activeButton;

        public Menu()
        {
            InitializeComponent();
            _tokenService = ServiceLocator.Instance.TokenService;
            Loaded += Menu_Loaded;
        }

        private void Menu_Loaded(object sender, RoutedEventArgs e)
        {
            // Получаем роль пользователя
            _userRole = _tokenService.GetCurrentUserRole();

            // Адаптируем меню под роль
            AdaptMenuForRole();

            // Устанавливаем главную страницу как активную по умолчанию
            SetActiveButton(HomeButtonBorder);
        }

        private void AdaptMenuForRole()
        {
            if (_userRole == "student")
            {
                // Для студентов скрываем курсы и группы
                CoursesButtonBorder.Visibility = Visibility.Collapsed;
                GroupsButtonBorder.Visibility = Visibility.Collapsed;
            }
            else if (_userRole == "teacher")
            {
                // Для преподавателей показываем все пункты меню
                CoursesButtonBorder.Visibility = Visibility.Visible;
                GroupsButtonBorder.Visibility = Visibility.Visible;
            }
        }

        private void SetActiveButton(Border button)
        {
            // Сбрасываем стиль предыдущей активной кнопки
            if (_activeButton != null)
            {
                _activeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                if (_activeButton.Child is TextBlock prevText)
                {
                    prevText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7D7D88"));
                    prevText.FontWeight = FontWeights.Normal;
                }
            }

            // Устанавливаем стиль для новой активной кнопки
            _activeButton = button;
            _activeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEF1FF"));
            if (_activeButton.Child is TextBlock activeText)
            {
                activeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B2B2B"));
                activeText.FontWeight = FontWeights.SemiBold;
            }
        }

        private void HomeButton_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveButton(HomeButtonBorder);
            NavigationRequested?.Invoke("HomePage");
        }

        private void CoursesButton_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveButton(CoursesButtonBorder);
            NavigationRequested?.Invoke("Lessons");
        }

        private void GroupsButton_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveButton(GroupsButtonBorder);
            NavigationRequested?.Invoke("Groups");
        }

        private void AssignmentsButton_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveButton(AssignmentsButtonBorder);
            NavigationRequested?.Invoke("Assignments");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем токен при выходе
            var authService = ServiceLocator.Instance.AuthService;
            authService.Logout();

            // Закрываем текущее окно и открываем окно входа
            var loginWindow = new login();
            loginWindow.Show();

            Window.GetWindow(this)?.Close();
        }
    }
}