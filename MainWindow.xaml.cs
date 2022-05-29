using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ScreenshotMagnifier2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {            
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.CaptureClicked(null, null);

            this.LoadLastSettings();
            timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 300), DispatcherPriority.Normal, TimerTicked, this.Dispatcher);
            timer.Start();
        }

        private void LoadLastSettings()
        {
            string fileName = GetFileName();

            if (!File.Exists(fileName))
            {
                return;
            }

            string json = File.ReadAllText(fileName);

            var settings = System.Text.Json.JsonSerializer.Deserialize<CurrentSettings>(json);

            this.width = settings.Width;
            this.height = settings.Height;
            this.top = settings.Top;
            this.left = settings.Left;

            Action action = UpdateRect;
            Dispatcher.BeginInvoke(action);

            MagnifierTab.Focus();
        }

        private void SaveSettings()
        {
            CurrentSettings currentSettings = new CurrentSettings
            {
                Left = left,
                Top = top,
                Width = width,
                Height = height
            };

            string json = System.Text.Json.JsonSerializer.Serialize(currentSettings);

            string fileName = GetFileName();
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }

            System.IO.File.WriteAllText(fileName, json);
        }

        private string GetFileName()
        {
            string fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ScreenshotMagnifierSettings.json");
            return fileName;
        }

        private DispatcherTimer timer;

        private void TimerTicked(object sender, EventArgs args)
        {
            try
            {
                this.Capture();
            }
            catch
            {

            }
        }

        private void CaptureClicked(object sender, RoutedEventArgs e)
        {
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;
                       
            var bitmapSource = GetScreenshot(screenLeft, screenTop, screenWidth, screenHeight);
            ScreenshotImage.Source = bitmapSource;
        }

        private void Capture()
        {
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            int captureLeft = screenLeft + (int)(screenWidth * left);
            int captureTop = screenTop + (int)(screenHeight * top);
            int captureHeight = (int)(screenHeight * height) ;
            int captureWidth = (int)(screenWidth * width) ;


            var bitmapSource = GetScreenshot(captureLeft, captureTop, captureWidth, captureHeight);
            Output.Source = bitmapSource;
        }

        private BitmapSource GetScreenshot(int left, int top, int width, int height)
        {
            using (Bitmap bmp = new Bitmap(width, height))
            {
                // Draw the screenshot into our bitmap.
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(left, top, 0, 0, bmp.Size);
                }

                var result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmp.GetHbitmap(),
                   IntPtr.Zero,
                   System.Windows.Int32Rect.Empty,
                   BitmapSizeOptions.FromWidthAndHeight(width, height));

                return result;
            }
        }

        double left = 0;
        double top = 0;
        double width = 100;
        double height = 100;

        bool isSelecting = false;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(SelectionGrid);
            left = (pos.X -5.0) / SelectionGrid.ActualWidth;
            top = (pos.Y -5.0)/ SelectionGrid.ActualHeight;
            width = 0.01;
            height = 0.01;
                       

            isSelecting = true;

            UpdateRect();          
            
        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isSelecting)
            {
                return;
            }

            var pos = e.GetPosition(SelectionGrid);
            double currentLeft = pos.X / SelectionGrid.ActualWidth;
            double currentTop = pos.Y / SelectionGrid.ActualHeight;

            width = currentLeft - left;
            height = currentTop - top;

            //TextBlock.Text = $"top={top}\r\nleft={left}\r\nwidth={width}\r\nheight={height}";

            UpdateRect();
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isSelecting = false;

            SaveSettings();
        }

        private void UpdateRect()
        {
            SelectionRect.Margin = new Thickness(SelectionGrid.ActualWidth * left, SelectionGrid.ActualHeight* top, SelectionGrid.ActualWidth - SelectionGrid.ActualWidth*(left + width), SelectionGrid.ActualHeight - SelectionGrid.ActualHeight*(top + height));
        }
    }
}
