using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace BTRReportProcesser.Lib
{
    static class Overrides
    {
        public static string DefaultIfNull(this ComboBox s, string defaultVal)
        {
            if (s.SelectedValue is null) return defaultVal;

            return s.SelectedValue.ToString();
        }
    }
}
