using System;
using System.Windows;

namespace Dictionary.UI
{
    public partial class ImportLimitWindow : Window
    {
        public int LimitCount { get; private set; }

        public ImportLimitWindow()
        {
            InitializeComponent();
            txtLimit.Text = "0";
            txtLimit.Focus();
            txtLimit.SelectAll();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtLimit.Text, out int limit) && limit >= 0)
            {
                LimitCount = limit;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Vui lòng nhập một số dương hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
