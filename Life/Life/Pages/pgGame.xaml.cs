using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Life.Pages
{
    public partial class pgGame : Page
    {
        private int mode = 0;

        public pgGame()
        {
            InitializeComponent();
            Model.onGameOver += OnGameOver;
            Model.Update += Update;
            DataContext = Model.fieldParam;
        }

        private void OnGameOver(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    wGameOverDialog w = new wGameOverDialog();
                    if (w.ShowDialog() == true)
                    {
                        Model.ResetField();
                        DrawField();
                        mode = 1;
                        btnNext.Content = "Запустить";
                        grdSettings.Visibility = Visibility.Hidden;
                        btnNext.IsEnabled = true;
                        btnStop.Visibility = Visibility.Hidden;
                    }
                });
            }
            catch { }
        }

        private void Update(object sender, EventArgs e)
        {
            DrawField();
        }

        private void DrawField()
        {
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Model.fieldParam.CWidth, Model.fieldParam.CHeight))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.FillRectangle(System.Drawing.Brushes.Gray, new System.Drawing.Rectangle(0, 0, Model.fieldParam.CWidth, Model.fieldParam.CHeight));
                    for (int i = 0; i < Model.fieldParam.Width; i++)
                        g.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Silver),
                                   new System.Drawing.Point(i * Model.CellSize, 0),
                                   new System.Drawing.Point(i * Model.CellSize, Model.fieldParam.CHeight));
                    for (int j = 0; j < Model.fieldParam.Height; j++)
                        g.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Silver),
                                   new System.Drawing.Point(0, j * Model.CellSize),
                                   new System.Drawing.Point(Model.fieldParam.CWidth, j * Model.CellSize));

                    foreach (Point? p in Model.cells)
                    {
                        if (p == null) continue;
                        g.FillRectangle(new System.Drawing.SolidBrush(Model.CellFill), new System.Drawing.Rectangle((int)p.Value.X * Model.CellSize, (int)p.Value.Y * Model.CellSize, Model.CellSize, Model.CellSize));
                        g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Silver), new System.Drawing.Rectangle((int)p.Value.X * Model.CellSize, (int)p.Value.Y * Model.CellSize, Model.CellSize, Model.CellSize));
                    }

                    BitmapImage bi = new BitmapImage();
                    using (MemoryStream memory = new MemoryStream())
                    {
                        bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);

                        memory.Position = 0;
                        bi.BeginInit();
                        bi.StreamSource = memory;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.EndInit();
                    }

                    bi.Freeze();
                    Dispatcher.Invoke(() =>
                    {
                        canvas.Source = bi;
                    });
                }
            }
        }

        private void Fill(object sender, RoutedEventArgs e)
        {
            Model.CreateField();
            Model.Fill();
            DrawField();
        }

        private void canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (rbAuto.IsChecked == true || Model.InProgress) return;
            Model.AddCell((int)(e.GetPosition(canvas).X / Model.CellSize), (int)(e.GetPosition(canvas).Y / Model.CellSize));
            DrawField();
            Model.fieldParam.Count++;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            switch (mode)
            {
                case 0:
                    if (Model.cells.Count == 0)
                    {
                        MessageBox.Show("Сперва заполните поле!");
                        return;
                    }
                    mode++;
                    spFillSettings.Visibility = Visibility.Collapsed;
                    grdSettings.Visibility = Visibility.Visible;
                    break;
                case 1:
                    btnStop.Visibility = Visibility.Visible;
                    mode++;
                    Model.StartGame();
                    btnNext.IsEnabled = false;
                    break;
            }
        }

        private void SettingsClose(object sender, RoutedEventArgs e)
        {
            grdSettings.Visibility = Visibility.Hidden;
            spFillSettings.Visibility = Visibility.Visible;
            mode = 0;
        }

        private void Cancel(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            grdSettings.Visibility = Visibility.Hidden;
            spFillSettings.Visibility = Visibility.Visible;
            mode = 0;
        }

        private void SettingsConfirm(object sender, RoutedEventArgs e)
        {
            btnNext.Content = "Запустить";
            grdSettings.Visibility = Visibility.Hidden;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Model.CreateField();
            Model.fieldParam.Count = 1;
            DrawField();
        }

        private void rbManual_Checked(object sender, RoutedEventArgs e)
        {
            Model.CreateField();
            Model.fieldParam.Count = 1;
            DrawField();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Model.InProgress = false;
        }
    }
}
