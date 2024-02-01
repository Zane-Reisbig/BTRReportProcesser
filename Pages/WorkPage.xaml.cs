using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BTRReportProcesser
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WorkPage : Page
    {
        const int HEADER_ROW = 10;

        DataSet sheet;
        StorageFile myFile;
        DebugConsole I_Console;

        public WorkPage()
        {
            sheet = null;
            myFile = null;

            this.InitializeComponent();
            I_Console = new DebugConsole(ConsoleOutput);


        }

        // Surgery Last Tuesday
        // Wendsday 7th a 4:15 PM


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            myFile = (StorageFile)e.Parameter;

            // Call Main so we can get async function
            Main();

        }

        public async void Main()
        {

            // Make sure to await async. This function sets -
            // critical variable "sheet" and allows us to actually -
            // do some work in this thing.

            await ReadInFile(null, null);
            InitControls();
        }
        private void MouseLeave_ComboBox(object sender, PointerRoutedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex == -1) return;
            if ((sender as ComboBox).SelectedValue as String == "") return;


            InitControls();
        }

        public void InitControls()
        {
            if (sheet == null) return;

            String hold1 = Country_ComboBox.SelectedValue as String;
            String hold2 = Practitoner_ComboBox.SelectedValue as String;
            String hold3 = PNumber_ComboBox.SelectedValue as String;
            String hold4 = SiteId_ComboBox.SelectedValue as String;



            var sortedItems = sheet.Tables[0].AsEnumerable()
            .Where((row) =>
            {
                var x = row["Country"];
                
                if (row["Country"]        == (hold1 == null || hold1 == "" ? row["Country"]        : hold1) &&
                    row["PI Name"]        == (hold2 == null || hold2 == "" ? row["PI Name"]        : hold2) &&
                    row["Patient Number"] == (hold3 == null || hold3 == "" ? row["Patient Number"] : hold3) &&
                    row["Site ID"]        == (hold4 == null || hold4 == "" ? row["Site ID"]        : hold4)) return true;

                return false;
            });

            Country_ComboBox.ItemsSource     = sortedItems.Select(row => row["Country"]       .ToString()).Distinct().ToList();
            Practitoner_ComboBox.ItemsSource = sortedItems.Select(row => row["PI Name"]       .ToString()).Distinct().ToList();
            PNumber_ComboBox.ItemsSource     = sortedItems.Select(row => row["Patient Number"].ToString()).Distinct().ToList();
            SiteId_ComboBox.ItemsSource      = sortedItems.Select(row => row["Site ID"]       .ToString()).Distinct().ToList();

            Country_ComboBox.SelectedValue     = hold1;
            Practitoner_ComboBox.SelectedValue = hold2;
            PNumber_ComboBox.SelectedValue     = hold3;
            SiteId_ComboBox.SelectedValue      = hold4;
        }

        public async Task ReadInFile(object s, object e)
        {
            var c_stream = await myFile.OpenStreamForReadAsync();

            using (c_stream)
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateOpenXmlReader(c_stream))
                {
                    this.sheet = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                            ReadHeaderRow = (rowReader) =>
                            {
                                int i = 0;
                                do
                                {
                                    rowReader.Read();
                                    i++;
                                } while (i < HEADER_ROW);
                            }
                        }
                    });
                }
            }

        }

        private void SiteId_Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            SiteId_ComboBox.SelectedValue = "";
            SiteId_ComboBox.SelectedIndex = -1;
            InitControls();
        }

        private void PNumber_Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            PNumber_ComboBox.SelectedValue = "";
            PNumber_ComboBox.SelectedIndex = -1;
            InitControls();
        }

        private void Practitoner_Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            Practitoner_ComboBox.SelectedValue = "";
            Practitoner_ComboBox.SelectedIndex = -1;
            InitControls();
        }

        private void Country_Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            Country_ComboBox.SelectedValue = "";
            Country_ComboBox.SelectedIndex = -1;
            InitControls();
        }

        private void Refresh_Controls_Button_Click(object sender, RoutedEventArgs e)
        {

            InitControls();

        }
    }
}
