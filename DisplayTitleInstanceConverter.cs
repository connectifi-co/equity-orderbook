using Connectifi.DesktopAgent.Fdc3;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Equity_Order_Book
{
    public class DisplayTitleInstanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var app = value as ConnectifiApp;
            if (app != null)
            {
                return app.Title != app.InstanceTitle ? $"{app.Title} - {app.InstanceTitle}" : app.InstanceTitle;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
