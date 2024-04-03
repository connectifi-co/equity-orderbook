using System.Linq;
using System.Windows.Controls;
using System.Windows;
using Connectifi.DesktopAgent.Fdc3;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using Svg.Skia;
using System.Net.Http;
using System.IO;
using SkiaSharp;
using System;
using System.Threading.Tasks;
using Connectifi.DesktopAgent;
using System.Configuration;

namespace Equity_Order_Book
{
    public partial class AppSelectionControl : UserControl
    {
        private readonly HandleIntentResolution handleIntentResolution;
        public ConnectifiApp? SelectedApp { get; private set; }
        public ObservableCollection<ConnectifiApp> MyApps { get; set; }

        public AppSelectionControl(HandleIntentResolution handleIntentResolution, string currentTicker, string currentIntent)
        {
            this.handleIntentResolution = handleIntentResolution;
            InitializeComponent();

            this.DataContext = this;

            MyApps = new ObservableCollection<ConnectifiApp>(handleIntentResolution.Message.Data.SelectMany(x => x.Apps));

            var cvs = new CollectionViewSource();
            cvs.Source = MyApps;


            // Create and add the group description
            var pgd = new PropertyGroupDescription(".");
            pgd.Converter = new GroupingTitleInstanceConverter();
            cvs.GroupDescriptions.Add(pgd);


            appListBox.ItemsSource = cvs.View;
            appListBox.SelectedIndex = -1;
            appListBox.SelectionChanged += appSelected;


            IntentResolverTextBox.Text = currentIntent + " for " + currentTicker;
        }

        private void appSelected(object sender, RoutedEventArgs e)
        {
            SelectedApp = (ConnectifiApp)appListBox.SelectedItem;
            if (SelectedApp != null)
            {
                var selectedAppIntent = handleIntentResolution.Message.Data.First(x => x.Apps.Contains(SelectedApp));
                handleIntentResolution.Callback(SelectedApp, selectedAppIntent.Intent.Name);
                Window.GetWindow(this).Close(); // Close the dialog
            }
        }

        private async void OnImageLoaded(object sender, RoutedEventArgs e)
        {
            var img = sender as System.Windows.Controls.Image;
            if (img != null && img.DataContext is ConnectifiApp dataContext && !string.IsNullOrEmpty(dataContext.Browser))
            {
                string url = $"{AppConfig.connectifiHost}/{dataContext.Browser.ToLower()}.svg";

                // Temporary path to save the converted PNG
                string tempPngPath = $"{Path.GetTempPath()}{Path.GetRandomFileName()}.png";

                // Convert SVG to PNG
                await ConvertSvgUrlToPngAsync(url, tempPngPath);

                // Load the PNG into the image control
                await LoadImageAsync(img, tempPngPath);
            }
        }


        private async Task LoadImageAsync(System.Windows.Controls.Image img, string url)
        {
            try
            {
                await img.Dispatcher.InvokeAsync(() =>
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(url, UriKind.Absolute);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bi.EndInit();

                    img.Source = bi;
                });
            }
            catch (Exception ex)
            {
                // Handle the exception or log it
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task ConvertSvgUrlToPngAsync(string svgUrl, string outputPath)
        {
            // Fetch SVG content from URL
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; local-dotnet)");
            var svgContent = await httpClient.GetStringAsync(svgUrl);


            // Parse SVG content
            var svg = new SKSvg();
            svg.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent)));
            if (svg.Picture != null)
            {
                var svgWidth = (int)svg.Picture.CullRect.Width;
                var svgHeight = (int)svg.Picture.CullRect.Height;


                // Convert to PNG
                var bitmap = new SKBitmap(svgWidth, svgHeight);
                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.DrawPicture(svg.Picture);
                }

                using (var stream = File.OpenWrite(outputPath))
                {
                    bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                }
            }
        }
    }
}