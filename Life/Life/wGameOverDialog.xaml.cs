using System;
using System.Windows;

namespace Life
{
    public partial class wGameOverDialog : Window
    {
        public wGameOverDialog()
        {
            InitializeComponent();
        }

        private void Restart(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void New(object sender, RoutedEventArgs e)
        {
            Model.Navigator.Navigate(new Uri("Pages\\pgFirst.xaml", UriKind.Relative));
            Close();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
