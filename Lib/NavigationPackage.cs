using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTRReportProcesser.Lib
{
    internal static class NavigationPackage
    {
        public static Dictionary<string, object> Objects = new Dictionary<string, object>();

        public static void AddOrUpdate(string key, object value)
        {
            if (!Objects.ContainsKey(key)) { Objects.Add(key, value); return; }

            Objects[key] = value;
        }
    }
}
