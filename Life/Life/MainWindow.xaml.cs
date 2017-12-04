using System.Windows;

namespace Life
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Model.fieldParam = new Model.FieldParam(10, 10);
            Model.Navigator = MainFrame.NavigationService;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Model.InProgress = false;
        }
    }
}
