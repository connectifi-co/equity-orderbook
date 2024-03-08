
using System.Windows;
using ConnectifiDesktopAgent.Fdc3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;


namespace Equity_Order_Book
{
    internal class AppSelectionWPF
    {
        private MainWindow _mainWindow;
        private Window? _window;
        private string? currentTicker;
        private string? currentIntent;

        public AppSelectionWPF(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _mainWindow.PropertyChanged += MainWindow_PropertyChanged;

        }

        private void MainWindow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentIntent")
            {
                // Handle changes to CurrentIntent
                currentIntent = _mainWindow.CurrentIntent;
            }
            else if (e.PropertyName == "CurrentTicker")
            {
                // Handle changes to CurrentTicker
                currentTicker = _mainWindow.CurrentTicker;
            }
        }

        public void Close()
        {
            _window?.Close();
        }

        public void ShowAppSelectionAsync(HandleIntentResolution handleIntentResolution)
        {
            try

            {
                // TaskCompletionSource<ConnectifiApp> userSelectedApp = new TaskCompletionSource<ConnectifiApp>();
                var appSelectionControl = new AppSelectionControl(handleIntentResolution, currentTicker, currentIntent);

                _window = new Window
                {
                    Content = appSelectionControl,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.None,
                    Owner = _mainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    AllowsTransparency = true,
                    Background = Brushes.Transparent

                };

                // Center the new window over the main window
                _window.Left = _mainWindow.Left + (_mainWindow.Width - _window.Width) / 2;
                _window.Top = _mainWindow.Top + (_mainWindow.Height - _window.Height) / 2;


                // Attach the event handler before showing the dialog
                _window.Closed += (sender, args) =>
                {
                    _mainWindow.PropertyChanged -= MainWindow_PropertyChanged;
                    _mainWindow.Effect = null;
                };

                _mainWindow.MouseDown += (_, evt) =>
                {
                    _window.Close();
                };

                _mainWindow.Effect = new BlurEffect();
                _window.Show();

                var selectedApp = appSelectionControl.SelectedApp;
                if (selectedApp != null)
                {
                    //TO-DO
                    // userSelectedApp.SetResult(appSelectionControl.SelectedApp);
                }

                // return userSelectedApp.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                //return Task.FromResult<ConnectifiAppMetadata>(null); // Handle this appropriately
            }

        }
    }
}
