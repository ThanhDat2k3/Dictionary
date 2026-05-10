using System.Windows;

namespace Dictionary.UI
{
    public partial class AddWordWindow : Window
    {
        public string Word { get; private set; }
        public string Definition { get; private set; }

        public AddWordWindow()
        {
            InitializeComponent();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtWord.Text))
            {
                MessageBox.Show("⚠️ Vui lòng nhập từ vựng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDefinition.Text))
            {
                MessageBox.Show("⚠️ Vui lòng nhập định nghĩa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Word = txtWord.Text.Trim();
            Definition = txtDefinition.Text.Trim();
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
