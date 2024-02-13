using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTRReportProcesser.Lib
{
    internal class HeaderData
    {
        public string commentTag { get; set; }
        public string countryTag { get; set; }
        public string practitionerTag { get; set; }
        public string patientTag { get; set; }
        public string siteTag { get; set; }
        public int rowNumber { get; set; }

        public HeaderData(string ct, string cot, string pt, string pat, string st, int rn) 
        {
            commentTag = ct;
            countryTag = cot;
            practitionerTag = pt;
            patientTag = pat;
            siteTag = st;
            rowNumber = rn;
        }

        public HeaderData() { }




    }
}
