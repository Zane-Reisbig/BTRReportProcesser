using ExcelDataReader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using BTRReportProcesser.Lib;
using BTRReportProcesser.Assets;
using Windows.ApplicationModel;
using System.Runtime.CompilerServices;
using Windows.Storage.AccessCache;
using System.Buffers.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BTRReportProcesser
{

    public sealed partial class WorkPage : Page
    {
        private const string OUT_FOLDER = "outputFolder";

        ExcelDataset dataset;
        FileSystem fs;
        private const bool isDebug = false;

        public WorkPage()
        {
            dataset = null;
            this.InitializeComponent();

        }

        // Surgery Last Tuesday
        // Wendsday 7th a 4:15 PM


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await CreateDataset((StorageFile)e.Parameter);
            fs = await FileSystem.CreateAsync();
            InitControls();

            if (StorageApplicationPermissions.FutureAccessList.ContainsItem(OUT_FOLDER))
            {
                FileOutPath.PlaceholderText = (await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(OUT_FOLDER)).Path;
            }

            if (isDebug)
            {
                ExportData_Button_Click(null, null);
            }
        }
        
        public async Task CreateDataset(StorageFile file)
        {
            dataset = await ExcelDataset.CreateAsync(file);
        }

        private void ComboSelectionChanged_ComboBox(object sender, PointerRoutedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex == 0) return;
            if ((sender as ComboBox).SelectedValue as string == "") return;

            InitControls();
        }
        private void Refresh_Controls_Button_Click(object sender, RoutedEventArgs e)
        {
            InitControls();
        }

        public void InitControls()
        {
            string CountrBoxValue = Country_ComboBox.SelectedValue     as string;
            string PractiBoxValue = Practitoner_ComboBox.SelectedValue as string;
            string PNumbeBoxValue = PNumber_ComboBox.SelectedValue     as string;
            string SiteIdBoxValue = SiteId_ComboBox.SelectedValue      as string;

            var sortedData = dataset.EnumerableDataLines.Where(
            (row) =>
            {
                if (row["Country"]        == (CountrBoxValue == null ? row["Country"]        : CountrBoxValue) &&
                    row["PI Name"]        == (PractiBoxValue == null ? row["PI Name"]        : PractiBoxValue) &&
                    row["Patient Number"] == (PNumbeBoxValue == null ? row["Patient Number"] : PNumbeBoxValue) &&
                    row["Site ID"]        == (SiteIdBoxValue == null ? row["Site ID"]        : SiteIdBoxValue))
                {
                    return true;
                }

                return false;
            });

            Country_ComboBox.ItemsSource       = sortedData.Select(row => row["Country"]       .ToString()).Distinct().ToList();
            Practitoner_ComboBox.ItemsSource   = sortedData.Select(row => row["PI Name"]       .ToString()).Distinct().ToList();
            PNumber_ComboBox.ItemsSource       = sortedData.Select(row => row["Patient Number"].ToString()).Distinct().ToList();
            SiteId_ComboBox.ItemsSource        = sortedData.Select(row => row["Site ID"]       .ToString()).Distinct().ToList();

            Country_ComboBox.SelectedValue     = CountrBoxValue;
            Practitoner_ComboBox.SelectedValue = PractiBoxValue;
            PNumber_ComboBox.SelectedValue     = PNumbeBoxValue;
            SiteId_ComboBox.SelectedValue      = SiteIdBoxValue;
        }

        
        private void Reset_ComboBoxes(object sender, RoutedEventArgs e)
        {
            ComboBox sender_parent = this.FindName((sender as Button).Tag as string) as ComboBox;
            sender_parent.SelectedValue = null;
            sender_parent.SelectedIndex = -1;
        }

        private async void ExportData_Button_Click(object sender, RoutedEventArgs e)
        {

            fs = await FileSystem.CreateAsync();
            Stopwatch time = new Stopwatch();
            time.Start();

            if (ProcessComments_Toggle.IsChecked == null) return;

            SortedDictionary mappedComments = null;
            SortedDictionary countedComments = null;
            AdvancedStringBuilder CSVOut = new AdvancedStringBuilder(";", null);

            string CountrBoxValue = Country_ComboBox.SelectedValue     as string;
            string PractiBoxValue = Practitoner_ComboBox.SelectedValue as string;
            string PNumbeBoxValue = PNumber_ComboBox.SelectedValue     as string;
            string SiteIdBoxValue = SiteId_ComboBox.SelectedValue      as string;

            var sortedData = dataset.EnumerableDataLines.Where(
            (row) =>
            {
                if (row["Country"]        == (CountrBoxValue == null ? row["Country"]        : CountrBoxValue) &&
                    row["PI Name"]        == (PractiBoxValue == null ? row["PI Name"]        : PractiBoxValue) &&
                    row["Patient Number"] == (PNumbeBoxValue == null ? row["Patient Number"] : PNumbeBoxValue) &&
                    row["Site ID"]        == (SiteIdBoxValue == null ? row["Site ID"]        : SiteIdBoxValue))
                {
                    return true;
                }

                return false;
            });

            CommentManager commentManager = new CommentManager(
                CommentLookupTable.Table,
                sortedData.Select(row => row["Overread Selection Reason"].ToString()).ToList()
            );

            CSVOut.DoStringMutation = false;
            CSVOut.AppendLine("sep=;");
            CSVOut.DoStringMutation = true;

            // Header Info
            CSVOut.AppendLine("Clario | ReisbigLLC | V0.1");
            CSVOut.AppendLineFormat("Created;{0}", DateTime.Now);

            // Filter Info
            CSVOut.AppendLineFormat("Country;{0}",        Country_ComboBox.DefaultIfNull("All")    );
            CSVOut.AppendLineFormat("Practitoner;{0}",    Practitoner_ComboBox.DefaultIfNull("All"));
            CSVOut.AppendLineFormat("Patient Number;{0}", PNumber_ComboBox.DefaultIfNull("All")    );
            CSVOut.AppendLineFormat("Site Id;{0}",        SiteId_ComboBox.DefaultIfNull("All")     );
            CSVOut.AppendLine();

            // Association Data
            CSVOut.AppendLine("-- Association Data --");
            if ((bool)ProcessComments_Toggle.IsChecked)
            {
                mappedComments = new SortedDictionary(commentManager.MappedComments);
                foreach (var c in mappedComments)
                {
                    CSVOut.AppendLine(c.key + ";" + c.value);
                }
                CSVOut.AppendLine();
            }

            // Line Item Counts
            CSVOut.AppendLine("-- Line Item Totals --");
            countedComments = new SortedDictionary(commentManager.CommentCounts);
            foreach (var c in countedComments)
            {
                CSVOut.AppendLine(c.key + ";" + c.value);
            }

            // Footer
            time.Stop();
            CSVOut.AppendLine();
            CSVOut.AppendLine();
            CSVOut.AppendLineFormat("Total Runtime; {0}ms", time.Elapsed);
            CSVOut.AppendLineFormat("Checksum; {0}", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(CSVOut.ToString())));

            if((bool) !FileOutType_Toggle.IsChecked)
            {
                DataPackage dp = new DataPackage();
                dp.RequestedOperation = DataPackageOperation.Copy;
                dp.SetText(CSVOut.ToString());
                Clipboard.SetContent(dp);
                return;
            }


            //StorageFolder outFolder;
            //StorageFolder installedLocation = Package.Current.InstalledLocation;
            //StorageFolder outFolder = await installedLocation.CreateFolderAsync("Datasets", CreationCollisionOption.OpenIfExists);
            string fileName = "";
            fileName += "Report_";
            fileName += Country_ComboBox    .DefaultIfNull("All").Replace(",", "").Replace(" ", "") + "_";
            fileName += Practitoner_ComboBox.DefaultIfNull("All").Replace(",", "").Replace(" ", "") + "_";
            fileName += PNumber_ComboBox    .DefaultIfNull("All").Replace(",", "").Replace(" ", "") + "_";
            fileName += SiteId_ComboBox     .DefaultIfNull("All").Replace(",", "").Replace(" ", "") + "_";
            fileName += DateTime.UtcNow.Hour + "h" + DateTime.UtcNow.Minute + "m";
            fileName += ".csv";

            //var myFile = await outFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            //StreamWriter streamWriter = new StreamWriter(await myFile.OpenStreamForWriteAsync());
            //streamWriter.Write(CSVOut.ToString());
            //streamWriter.Close();

            await fs.WriteFile(
                fileName,
                CreationCollisionOption.ReplaceExisting,
                CSVOut.ToString(),
                OUT_FOLDER
            );


        }

        private void FileOutType_Toggle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Storyboard sb = ((Grid)sender).Resources["EnterStoryboard"] as Storyboard;
            sb.Begin();
        }

        private void FileOutType_Toggle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Storyboard sb = ((Grid)sender).Resources["ExitStoryboard"] as Storyboard;
            sb.Begin();
        }

        private void FileOutType_Toggle_Click(object sender, RoutedEventArgs e)
        {

            if ((bool) (sender as AppBarToggleButton).IsChecked)
            {
                Toggle_Text.Text = "Exporting to New File";
                FileOutType_Toggle.Label = "Exporting";
                FileOutType_Toggle.Icon = new SymbolIcon(Symbol.OpenFile);
                return;
            }

            Toggle_Text.Text = "Copying to Clipboard";
            FileOutType_Toggle.Label = "Copying";
            FileOutType_Toggle.Icon = new SymbolIcon(Symbol.Copy);

        }

        private async void FileOutPath_DoubleTapped(object sender, RoutedEventArgs e)
        {
            FileOutPath.PlaceholderText = "";
            FileOutPath.PlaceholderText = (await fs.RequestFolderAccess(OUT_FOLDER, FutureItemListOptions.ReplaceFutureItemIfExists)).Path;
        }
    }

}
