using System;
using System.Windows;
using System.Windows.Controls;

namespace Life.Pages
{
    public partial class pgFirst : Page
    {
        public pgFirst()
        {
            InitializeComponent();
            DataContext = Model.fieldParam;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            Model.Navigator.Navigate(new Uri("Pages\\pgGame.xaml", UriKind.Relative));
        }
    }
}
