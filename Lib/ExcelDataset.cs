using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BTRReportProcesser.Lib
{
    internal class ExcelDataset
    {
        int HEADER_ROW = 10;
        public DataSet parsedFile;
        public EnumerableRowCollection<DataRow> EnumerableDataLines;

        private ExcelDataset() { parsedFile = null; EnumerableDataLines = null; }

        private async Task<ExcelDataset> Init(StorageFile target)
        {
            var c_stream = await target.OpenStreamForReadAsync();

            using (c_stream)
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateOpenXmlReader(c_stream))
                {
                    this.parsedFile = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                            ReadHeaderRow = (rowReader) =>
                            {
                                int i = 0;
                                while (i < HEADER_ROW)
                                {
                                    rowReader.Read();
                                    i++;
                                }
                            }
                        }
                    });

                }
            }

            EnumerableDataLines = parsedFile.Tables[0].AsEnumerable();
            return this;
        }

        public static Task<ExcelDataset> CreateAsync(StorageFile target) {
            var ret = new ExcelDataset();
            return ret.Init(target);
        }

    }

    //public static Task<List<string>> GetColData(string colName, bool distinct = true)
    //{
    //    if(distinct) return EnumerableDataLines.Select(row => row[colName].ToString()).Distinct().ToList();

    //    return EnumerableDataLines.Select(row => row[colName].ToString()).Distinct().ToList();
    //}
}
