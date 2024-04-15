using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Connectifi.DesktopAgent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Finos.Fdc3;
using Finos.Fdc3.Context;
using Connectifi.DesktopAgent.Bridge;
using System.Diagnostics;

namespace Equity_Order_Book
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public DesktopAgent DesktopAgent { get; private set; }
        private DesktopAgentWPF? _desktopAgentWPF;
        public DesktopAgentWPF? DesktopAgentWPF
        {
            get
            {
                return _desktopAgentWPF;
            }
            set
            {
                if (_desktopAgentWPF != value)
                {
                    _desktopAgentWPF = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<Trade> AllTrades { get; set; } = new ObservableCollection<Trade>();
        public ObservableCollection<Trade> DisplayedTrades { get; set; } = new ObservableCollection<Trade>();
        private readonly ObservableCollection<ColorInfo> colorList;
        private string _currentIntent { get; set; } = "";
        private string _currentTicker { get; set; } = "";
        public event PropertyChangedEventHandler? PropertyChanged;
        private AppSelectionWPF? _resolverDialog;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            // Load hypothetical trades
            var tradeData = new Trades();
            foreach (var trade in tradeData.GetSampleTrades())
            {
                AllTrades.Add(trade);
            }
            foreach (var trade in AllTrades)
            {
                DisplayedTrades.Add(trade);
            }
            DisplayedTrades.CollectionChanged += DisplayedTrades_CollectionChanged;

            colorList = new ObservableCollection<ColorInfo>();
            channelComboBox.ItemsSource = colorList;
            channelComboBox.ItemTemplate = (DataTemplate)this.Resources["ColorItemTemplate"];


        }

        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string CurrentIntent
        {
            get => _currentIntent;
            set
            {
                _currentIntent = value;
                OnPropertyChanged();
            }
        }

        public string CurrentTicker
        {
            get => _currentTicker;
            set
            {
                _currentTicker = value;
                OnPropertyChanged();
            }
        }


        private void DisplayedTrades_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CheckTradesAndUpdateButton();
        }

        private void CheckTradesAndUpdateButton()
        {
            if (AreTradesEqual(AllTrades, DisplayedTrades))
            {
                // Hide the "All Trades" button or set its IsEnabled to false
                AllTradesButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Show the "All Trades" button or set its IsEnabled to true
                AllTradesButton.Visibility = Visibility.Visible;
            }
        }

        private static bool AreTradesEqual(ObservableCollection<Trade> allTrades, ObservableCollection<Trade> displayedTrades)
        {
            // Assuming that trade equality is based on their IDs
            return !allTrades.Except(displayedTrades, new TradeComparer()).Any();
        }

        // Implementing IEqualityComparer to compare trades
        public class TradeComparer : IEqualityComparer<Trade>
        {
            public bool Equals(Trade? x, Trade? y)
            {
                return x?.TradeId == y?.TradeId; // Assuming trade equality is based on their IDs
            }

            public int GetHashCode(Trade obj)
            {
                return obj.TradeId.GetHashCode();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DesktopAgentWPF agentControl = new DesktopAgentWPF();
            (this.Content as Grid)?.Children.Add(agentControl);
            var response = await agentControl.CreateAgent(AppConfig.connectifiHost, AppConfig.connectifiAppId);
            DesktopAgent = response.Agent;
            DesktopAgentWPF = response.AgentWPF;
            DesktopAgentWPF.AgentState.OnStateChanged += (sender, args) =>
            {
                switch (DesktopAgentWPF.AgentState.Type)
                {
                    case AgentStateType.LoggingIn:
                    case AgentStateType.SignedOut:
                        colorList.Clear();
                        break;
                }
            };
            if (response == null)
            {
                MessageBox.Show("Could not create Agent.  Shutting down...");
                Application.Current.Shutdown();
                return;
            }
            DesktopAgent.OnHandleIntentResolution += (_, evt) =>
            {
                _resolverDialog = new AppSelectionWPF(this);
                CurrentIntent = _currentIntent;
                CurrentTicker = _currentTicker;
                _resolverDialog.ShowAppSelectionAsync(evt.HandleIntentResolution);
            };
            DesktopAgent.OnConnectifiEvent += OnConnectifiEvent;
            DesktopAgent.OnAgentDebugEvent += DesktopAgent_OnAgentDebugEvent;
        }

        private void DesktopAgent_OnAgentDebugEvent(object? sender, ConnectifiAgentDebugEvent e)
        {
            Debug.WriteLine(e.Message);
        }

        private async void OnConnectifiEvent(object? sender, ConnectifiEventArgs e)
        {
            if (e.ConnectifiEvent is OnFDC3ReadyConnectifiEvent)
            {
                AllTradesButton.IsEnabled = true;
                FilterButton.IsEnabled = true;
                OrderBookGrid.IsEnabled = true;
                var userChannels = await DesktopAgent.GetUserChannels();
                foreach (var channel in userChannels)
                {
                    if (channel.DisplayMetadata != null && channel.DisplayMetadata.Color != null && channel.DisplayMetadata.Name != null)
                    {
                        colorList.Add(new ColorInfo { HexCode = channel.DisplayMetadata.Color, Name = channel.DisplayMetadata.Name, Id = channel.Id });
                    }
                }

                await AddContextListener();
                await AddIntentListener();
            }
        }

        private async Task AddIntentListener()
        {
            var intent = "ViewOrders";

            IntentHandler<Instrument> intentHandler = (context, contextMetadata) =>
            {
                var ticker = context.ID?.Ticker ?? throw new InvalidOperationException("context must have a non-null ID");
                var matchingTrades = AllTrades.Where(trade => trade.Ticker.ToUpper().Equals(ticker)).ToList();
                TradesFilter.Text = ticker;

                // Update DisplayedTrades based on matching criteria
                DisplayedTrades.Clear();
                foreach (var trade in matchingTrades)
                {
                    DisplayedTrades.Add(trade);
                }
                return Task.Run(() =>
                {
                    IIntentResult intentResult = context;
                    return intentResult;
                });
            };

            await DesktopAgent.AddIntentListener(intent, intentHandler);
        }

        private async Task AddContextListener()
        {

            var contextType = "fdc3.instrument";

            ContextHandler<Instrument> contextHandler = (context, contextMetadata) =>
            {
                var ticker = context.ID?.Ticker ?? throw new InvalidOperationException("context must have a non-null ID");
                var matchingTrades = AllTrades.Where(trade => trade.Ticker.ToUpper().Equals(ticker)).ToList();
                TradesFilter.Text = ticker;

                // Update DisplayedTrades based on matching criteria
                DisplayedTrades.Clear();
                foreach (var trade in matchingTrades)
                {
                    DisplayedTrades.Add(trade);
                }
            };

            await DesktopAgent.AddContextListener(contextType, contextHandler);
        }

        private async void ViewNews_Click(object sender, RoutedEventArgs e)
        {
            var trade = (sender as Button)?.DataContext as Trade;
            if (trade == null) return;

            var contextType = "fdc3.instrument";
            var intent = "ViewNews";

            var ticker = trade.Ticker;
            _currentIntent = intent;
            _currentTicker = ticker;

            IContext context = new Context(contextType, new { Ticker = trade.Ticker }, trade.Name);
            try
            {
                IIntentResolution intentResolution = await DesktopAgent.RaiseIntent(intent, context);
                Console.WriteLine($"News raised intent: {intentResolution.Intent}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No Applications Found For {intent}: {ex.Message}");
            }
        }

        private async void ViewChart_Click(object sender, RoutedEventArgs e)
        {
            var trade = (sender as Button)?.DataContext as Trade;
            if (trade == null) return;

            var contextType = "fdc3.instrument";
            var intent = "ViewChart";

            var ticker = trade.Ticker;
            _currentIntent = intent;
            _currentTicker = ticker;


            IContext context = new Context(contextType, new { trade.Ticker }, trade.Name);
            try
            {
                IIntentResolution intentResolution = await DesktopAgent.RaiseIntent(intent, context);
                Console.WriteLine($"Chart raised intent: {intentResolution.Intent}");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"No Applications Found For {intent}: {ex.Message}");
            }
        }

        private void AllTradesButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayedTrades.Clear();
            foreach (var trade in AllTrades)
            {
                DisplayedTrades.Add(trade);
            }
            TradesFilter.Text = "";
        }

        private async void Broadcast_Click(object sender, RoutedEventArgs e)
        {
            if (channelComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a specific Channel to broadcast to");
                return;
            }
            var trade = (sender as Button)?.DataContext as Trade;
            if (trade == null) return;

            var instrumentId = new InstrumentID { Ticker = trade.Ticker };
            var instrumentContext = new Instrument(instrumentId);

            IContext selectedContext = instrumentContext;
            await DesktopAgent.Broadcast(selectedContext);

        }

        private void ChannelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (channelComboBox.SelectedItem != null)
            {
                ColorInfo selectedColor = (ColorInfo)channelComboBox.SelectedItem;
                string colorId = selectedColor.Id;
                DesktopAgent.JoinUserChannel(colorId);
                var brush = new BrushConverter().ConvertFrom(selectedColor.HexCode) as SolidColorBrush;
                ChannelLabel.Foreground = brush;
            }
        }
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {

            if (TradesFilter.Text.Length != 0)
            {
                var matchingTrades = AllTrades.Where(trade => trade.Ticker.ToUpper().Equals(TradesFilter.Text)).ToList();
                DisplayedTrades.Clear();
                foreach (var trade in matchingTrades)
                {
                    DisplayedTrades.Add(trade);
                }
            }
            else
            {
                DisplayedTrades.Clear();
                foreach (var trade in AllTrades)
                {
                    DisplayedTrades.Add(trade);
                }
            }
            // Update DisplayedTrades based on matching criteria

        }
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TradesFilter.Text = TradesFilter.Text.ToUpper();
            // Move the caret to the end of the text so the user can continue typing seamlessly
            TradesFilter.CaretIndex = TradesFilter.Text.Length;
        }
    }
}