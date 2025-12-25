using System.Windows;
using System.Windows.Input;

namespace Pract15.Windows
{
    public partial class SimpleEditWindow : Window
    {
        public string Value { get; private set; }

        // Событие для уведомления об успешном сохранении
        public event Action<string> ValueSaved;

        public SimpleEditWindow(string title, string label, string initialValue = "")
        {
            InitializeComponent();
            Title = title;
            LabelText.Text = label;
            ValueTextBox.Text = initialValue;
            ValueTextBox.Focus();
            ValueTextBox.SelectAll();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Value = ValueTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(Value))
            {
                MessageBox.Show("Поле не может быть пустым", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ValueTextBox.Focus();
                return;
            }

            // Вызываем событие сохранения
            ValueSaved?.Invoke(Value);

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Save_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                Cancel_Click(sender, e);
            }
        }

        // ДОБАВЬТЕ ЭТОТ МЕТОД для обработки события TextChanged
        private void ValueTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Для отладки: выводим текущий текст (можно удалить в продакшене)
            System.Diagnostics.Debug.WriteLine($"Текст в поле: '{ValueTextBox.Text}'");
        }
    }
}