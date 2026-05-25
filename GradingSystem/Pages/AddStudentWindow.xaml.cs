using System.Windows;

namespace GradingSystem.Pages
{
    public partial class AddStudentWindow : Window
    {
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public int? GroupId { get; private set; }

        public AddStudentWindow()
        {
            InitializeComponent();
            LoadGroups();
        }

        private async void LoadGroups()
        {
            // TODO: Загрузить группы из API
            GroupComboBox.Items.Add(new { Id = 0, Name = "Без группы" });
            GroupComboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Введите полное имя студента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !EmailTextBox.Text.Contains("@"))
            {
                MessageBox.Show("Введите корректный email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password) || PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FullName = FullNameTextBox.Text.Trim();
            Email = EmailTextBox.Text.Trim();
            Password = PasswordBox.Password;

            if (GroupComboBox.SelectedIndex > 0)
            {
                GroupId = ((dynamic)GroupComboBox.SelectedItem).Id;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
