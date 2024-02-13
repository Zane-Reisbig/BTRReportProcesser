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
            return s.SelectedValue is null ? defaultVal : s.SelectedValue.ToString();
        }

        public static bool GetTagAsBoolean(this Control tb)
        {
            if (tb.Tag is null) return false;

            return Boolean.Parse(tb.Tag as String);
        }
    }
}
