using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equity_Order_Book
{
    public class Trade
    {
        public int TradeId { get; set; }
        public required string Ticker { get; set; }
        public required string Name { get; set; }
        public int Notional { get; set; }
        public decimal Price { get; set; }
    }

    public class Trades
    {
        public List<Trade> GetSampleTrades()
        {
            List<Trade> trades = new List<Trade>
            {
                new Trade { TradeId = 1, Ticker = "AAPL", Name = "Apple",  Notional = 1000, Price = 150.15M },
                new Trade { TradeId = 2, Ticker = "GOOG", Name = "Alphabet Inc",  Notional = 2000, Price = 2700.67M },
                new Trade { TradeId = 3, Ticker = "MSFT", Name = "Microsoft Inc", Notional = 1500, Price = 289.50M },
                new Trade { TradeId = 4, Ticker = "AMZN", Name = "Amazon.com Inc", Notional = 500, Price = 3300.44M },
                new Trade { TradeId = 5, Ticker = "TSLA", Name = "Tesla", Notional = 750, Price = 649.87M },
                new Trade { TradeId = 6, Ticker = "FB", Name = "Meta", Notional = 850, Price = 350.12M },
                new Trade { TradeId = 7, Ticker = "NFLX", Name = "Netflix",  Notional = 450, Price = 520.25M },
                new Trade { TradeId = 8, Ticker = "NVDA", Name =  "Nvidia", Notional = 650, Price = 189.97M },
                new Trade { TradeId = 9, Ticker = "JPM", Name = "JP Morgan Chase", Notional = 1000, Price = 140.55M },
                new Trade { TradeId = 10, Ticker = "V", Name = "Visa Inc.", Notional = 900, Price = 209.90M },
                new Trade { TradeId = 11, Ticker = "MA", Name = "Mastercard Inc.", Notional = 700, Price = 344.89M },
                new Trade { TradeId = 12, Ticker = "PG", Name = "Proctor & Gamble", Notional = 650, Price = 145.32M },
                new Trade { TradeId = 13, Ticker = "UNH", Name = "UnitedHealth Group Inc", Notional = 600, Price = 389.78M },
                new Trade { TradeId = 14, Ticker = "GOOG", Name = "Alphabet Inc", Notional = 2100, Price = 2701.45M },
                new Trade { TradeId = 15, Ticker = "GOOG", Name = "Alphabet Inc", Notional = 1950, Price = 2699.33M },
                new Trade { TradeId = 16, Ticker = "MSFT", Name = "Microsoft Inc", Notional = 1520, Price = 290.10M },
                new Trade { TradeId = 17, Ticker = "MSFT", Name = "Microsoft Inc", Notional = 1480, Price = 289.30M },
                new Trade { TradeId = 18, Ticker = "MSFT", Name = "Microsoft Inc",Notional = 1510, Price = 288.90M },
                new Trade { TradeId = 19, Ticker = "AMZN", Name = "Amazon.com Inc", Notional = 510, Price = 3301.10M },
                new Trade { TradeId = 20, Ticker = "AMZN", Name = "Amazon.com Inc", Notional = 490, Price = 3299.80M },
            };
            return trades;
        }

    }
}