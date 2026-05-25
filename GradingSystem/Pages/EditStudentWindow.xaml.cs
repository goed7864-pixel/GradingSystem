using System.Windows;

namespace GradingSystem.Pages
{
    public partial class EditStudentWindow : Window
    {
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public int? GroupId { get; private set; }

        public EditStudentWindow(string fullName, string email, int? groupId = null)
        {
            InitializeComponent();

            FullNameTextBox.Text = fullName;
            EmailTextBox.Text = email;

            LoadGroups(groupId);
        }

        private async void LoadGroups(int? selectedGroupId)
        {
            // TODO: Загрузить группы из API
            GroupComboBox.Items.Add(new { Id = 0, Name = "Без группы" });

            if (selectedGroupId.HasValue)
            {
                GroupComboBox.SelectedIndex = 0; // TODO: выбрать нужную группу
            }
            else
            {
                GroupComboBox.SelectedIndex = 0;
            }
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

            FullName = FullNameTextBox.Text.Trim();
            Email = EmailTextBox.Text.Trim();

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
