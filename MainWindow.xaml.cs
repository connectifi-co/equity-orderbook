using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using ConnectifiDesktopAgent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Finos.Fdc3;
using Finos.Fdc3.Context;
using ConnectifiDesktopAgent.Bridge;

namespace Equity_Order_Book
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        DesktopAgent desktopAgent;
        public ObservableCollection<Trade> AllTrades { get; set; } = new ObservableCollection<Trade>();
        public ObservableCollection<Trade> DisplayedTrades { get; set; } = new ObservableCollection<Trade>();
        private ObservableCollection<ColorInfo> colorList;
        private string _currentIntent { get; set; } = "";
        private string _currentTicker { get; set; } = "";
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _initialized = false;
        private AppSelectionWPF _resolverDialog;

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

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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


        private void DisplayedTrades_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

        private bool AreTradesEqual(ObservableCollection<Trade> allTrades, ObservableCollection<Trade> displayedTrades)
        {
            // Assuming that trade equality is based on their IDs
            return !allTrades.Except(displayedTrades, new TradeComparer()).Any();
        }

        // Implementing IEqualityComparer to compare trades
        public class TradeComparer : IEqualityComparer<Trade>
        {
            public bool Equals(Trade x, Trade y)
            {
                return x.TradeId == y.TradeId; // Assuming trade equality is based on their IDs
            }

            public int GetHashCode(Trade obj)
            {
                return obj.TradeId.GetHashCode();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var agentControl = new DesktopAgentWPF();
            (this.Content as Grid).Children.Add(agentControl);
            desktopAgent = await agentControl.CreateAgent("https://dev.connectifi-interop.com", "equityOrderBook@DemoSecure");

            desktopAgent.OnHandleIntentResolution += (_, evt) =>
            {
                _resolverDialog = new AppSelectionWPF(this);
                CurrentIntent = _currentIntent;
                CurrentTicker = _currentTicker;
                _resolverDialog.ShowAppSelectionAsync(evt.HandleIntentResolution);
            };
            desktopAgent.OnConnectifiEvent += OnConnectifiEvent;
            return;
        }

        private async void OnConnectifiEvent(object? sender, ConnectifiEventArgs e)
        {
            if (e.ConnectifiEvent is OnFDC3ReadyConnectifiEvent)
            {
                AllTradesButton.IsEnabled = true;
                FilterButton.IsEnabled = true;
                OrderBookGrid.IsEnabled = true;
                var userChannels = await desktopAgent.GetUserChannels();
                foreach (var channel in userChannels)
                {
                    if (channel.DisplayMetadata != null && channel.DisplayMetadata.Color != null && channel.DisplayMetadata.Name != null)
                    {
                        colorList.Add(new ColorInfo { HexCode = channel.DisplayMetadata.Color, Name = channel.DisplayMetadata.Name, Id = channel.Id });
                    }
                }
                _initialized = true;

                addContextListener();
                addIntentListener();
            }
        }

        private async void addIntentListener()
        {
            var intent = "ViewOrders";

            IntentHandler<Instrument> intentHandler = (context, contextMetadata) =>
            {
                var matchingTrades = AllTrades.Where(trade => trade.Ticker.ToUpper().Equals(context.ID.Ticker)).ToList();
                TradesFilter.Text = context.ID.Ticker;

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

            IListener intentListener = await desktopAgent.AddIntentListener(intent, intentHandler);
        }

        private async void addContextListener()
        {

            var contextType = "fdc3.instrument";

            ContextHandler<Instrument> contextHandler = (context, contextMetadata) =>
            {
                var matchingTrades = AllTrades.Where(trade => trade.Ticker.ToUpper().Equals(context.ID.Ticker)).ToList();
                TradesFilter.Text = context.ID.Ticker;

                // Update DisplayedTrades based on matching criteria
                DisplayedTrades.Clear();
                foreach (var trade in matchingTrades)
                {
                    DisplayedTrades.Add(trade);
                }
            };

            IListener contextListener = await desktopAgent.AddContextListener(contextType, contextHandler);
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
                IIntentResolution intentResolution = await desktopAgent.RaiseIntent(intent, context);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No Applications Found For {intent}");
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


            IContext context = new Context(contextType, new { Ticker = trade.Ticker }, trade.Name);
            try
            {
                IIntentResolution intentResolution = await desktopAgent.RaiseIntent(intent, context);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No Applications Found For {intent}");
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
            await desktopAgent.Broadcast(selectedContext);

        }

        private void channelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (channelComboBox.SelectedItem != null)
            {
                ColorInfo selectedColor = (ColorInfo)channelComboBox.SelectedItem;
                string colorId = selectedColor.Id;
                desktopAgent.JoinUserChannel(colorId);
                SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom(selectedColor.HexCode);
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