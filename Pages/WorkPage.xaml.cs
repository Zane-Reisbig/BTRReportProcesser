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
using Windows.Storage.AccessCache;
using BTRReportProcesser.Pages;
using Windows.ApplicationModel.Core;

namespace BTRReportProcesser
{

    // TODO: LAST FILE Export

    public sealed partial class WorkPage : Page
    {
        private const string OUT_FOLDER = "outputFolder";

        HeaderData hd;
        ExcelDataset dataset;
        FileSystem fs;
        private const bool isDebug = false;

        public WorkPage()
        {
            dataset = null;
            hd = null;
            this.InitializeComponent();
            this.Loaded += (s, e) => 
            {
                FileOutType_Toggle.PointerEntered += (se, ev) => { Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0); };
                FileOutType_Toggle.PointerExited += (sen, eve) => { Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0); };
            };

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Type source = NavigationPackage.Objects["source"] as Type;


            if (source == typeof(MainPage))
            {
                try
                {
                    // try basic navigation
                    hd = new HeaderData();
                    hd.countryTag = "Country";
                    hd.practitionerTag = "PI_NAME";
                    hd.patientTag = "Subject Number";
                    hd.siteTag = "Site Number";
                    hd.commentTag = "Overread Selection Reason";
                    hd.rowNumber = 10;
                    dataset = await ExcelDataset.CreateAsync((StorageFile) e.Parameter, hd.rowNumber);
                    InitControls();
                }
                catch
                {
                    Frame.Navigate(typeof(FormatWizard), (StorageFile) e.Parameter);
                }
            }

            else if (source == typeof(FormatWizard))
            {
                hd = ((HeaderData) e.Parameter);
                hd = (HeaderData) NavigationPackage.Objects["headers"];
                StorageFile file = (StorageFile)NavigationPackage.Objects["excelFile"];
                dataset = (await ExcelDataset.CreateAsync(file, hd.rowNumber));
            }


            if(dataset is null)
            {
                Modals.Error.ErrorOkay("Failed to create dataset");
                throw new Exception("Dataset is Null");
            }


            fs = await FileSystem.CreateAsync();
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem(OUT_FOLDER))
            {
                FileOutPath.PlaceholderText = (await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(OUT_FOLDER)).Path;
            }

            if (isDebug)
            {
                ExportData_Button_Click(null, null);
            }

            ExitStoryboard.Begin();
        }

        private void ComboSelectionChanged_ComboBox(object sender, PointerRoutedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex == 0) return;
            if ((sender as ComboBox).SelectedValue as string == "") return;

            InitControls();
        }
        private void Refresh_Controls_Button_Click(object sender, RoutedEventArgs e)
        {
            Reset_ComboBoxes(Country_Reset_Button, null);
            Reset_ComboBoxes(Practitoner_Reset_Button, null);
            Reset_ComboBoxes(PNumber_Reset_Button, null);
            Reset_ComboBoxes(SiteId_Reset_Button, null);
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
                if (row[hd.countryTag]      == (CountrBoxValue == null ? row[hd.countryTag]      : CountrBoxValue) &&
                    row[hd.practitionerTag] == (PractiBoxValue == null ? row[hd.practitionerTag] : PractiBoxValue) &&
                    row[hd.patientTag]      == (PNumbeBoxValue == null ? row[hd.patientTag]      : PNumbeBoxValue) &&
                    row[hd.siteTag]         == (SiteIdBoxValue == null ? row[hd.siteTag]         : SiteIdBoxValue))
                {
                    return true;
                }

                return false;
            });

            Country_ComboBox.ItemsSource       = sortedData.Select(row => row[hd.countryTag]     .ToString()).Distinct().ToList();
            Practitoner_ComboBox.ItemsSource   = sortedData.Select(row => row[hd.practitionerTag].ToString()).Distinct().ToList();
            PNumber_ComboBox.ItemsSource       = sortedData.Select(row => row[hd.patientTag]     .ToString()).Distinct().ToList();
            SiteId_ComboBox.ItemsSource        = sortedData.Select(row => row[hd.siteTag]        .ToString()).Distinct().ToList();

            Country_ComboBox.SelectedValue     = CountrBoxValue;
            Practitoner_ComboBox.SelectedValue = PractiBoxValue;
            PNumber_ComboBox.SelectedValue     = PNumbeBoxValue;
            SiteId_ComboBox.SelectedValue      = SiteIdBoxValue;
        }

        
        private void Reset_ComboBoxes(object sender, RoutedEventArgs e)
        {
            if (sender is null) return;

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
            SortedDictionary noAssociations = null;
            AdvancedStringBuilder CSVOut = new AdvancedStringBuilder(";", null);

            string CountrBoxValue = Country_ComboBox.SelectedValue     as string;
            string PractiBoxValue = Practitoner_ComboBox.SelectedValue as string;
            string PNumbeBoxValue = PNumber_ComboBox.SelectedValue     as string;
            string SiteIdBoxValue = SiteId_ComboBox.SelectedValue      as string;

            var sortedData = dataset.EnumerableDataLines.Where(
            (row) =>
            {
                if (row[hd.countryTag]      == (CountrBoxValue == null ? row[hd.countryTag]      : CountrBoxValue) &&
                    row[hd.practitionerTag] == (PractiBoxValue == null ? row[hd.practitionerTag] : PractiBoxValue) &&
                    row[hd.patientTag]      == (PNumbeBoxValue == null ? row[hd.patientTag]      : PNumbeBoxValue) &&
                    row[hd.siteTag]         == (SiteIdBoxValue == null ? row[hd.siteTag]         : SiteIdBoxValue))
                {
                    return true;
                }

                return false;
            });

            CommentManager commentManager = new CommentManager(
                CommentLookupTable.Table,
                sortedData.Select(row => row[hd.commentTag].ToString()).ToList()
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
            CSVOut.AppendLine();

            // No Associations
            CSVOut.AppendLine("-- No Associations --");
            noAssociations = new SortedDictionary(commentManager.NoAssociations);
            foreach (var c in noAssociations)
            {
                CSVOut.AppendLine(c.key + ";" + c.value);
            }
            CSVOut.AppendLine();
            
            // Footer
            CSVOut.AppendLine();
            CSVOut.AppendLine();
            CSVOut.AppendLineFormat("Checksum;; {0}", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(CSVOut.ToString())));
            time.Stop();
            CSVOut.AppendLineFormat("Total Runtime; {0}ms", time.Elapsed);

            if((bool) !FileOutType_Toggle.IsChecked)
            {
                DataPackage dp = new DataPackage();
                dp.RequestedOperation = DataPackageOperation.Copy;
                dp.SetText(CSVOut.ToString());
                Clipboard.SetContent(dp);
                return;
            }
            
            // File name is the selected filters
            string fileName = "";
            fileName += "Report_";
            fileName += Country_ComboBox    .DefaultIfNull("All").Replace(",", "").Replace(" ", "") + "_";
            fileName += Practitoner_ComboBox.DefaultIfNull("All").Replace(",", "").Replace(" ", "") + "_";
            fileName += PNumber_ComboBox    .DefaultIfNull("All").Replace(",", "").Replace(" ", "") + "_";
            fileName += SiteId_ComboBox     .DefaultIfNull("All").Replace(",", "").Replace(" ", "") + "_";
            fileName += DateTime.UtcNow.Hour + "h" + DateTime.UtcNow.Minute + "m" + DateTime.UtcNow.Second + "s";
            fileName += ".csv";

            await fs.WriteFile(
                fileName,
                CreationCollisionOption.ReplaceExisting,
                CSVOut.ToString(),
                OUT_FOLDER
            );

            try
            {
                // Set last file                                                           if it wasnt obvious
                await dataset.orignalFile.CopyAsync(await fs.RequestOrGetFolder(OUT_FOLDER), "_last.xlsx", NameCollisionOption.ReplaceExisting);
            }
            catch(Exception ex)
            {
                var _ = ex.Source;
            }

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
                FileOutType_Toggle.Icon = new SymbolIcon(Symbol.OpenFile);
                return;
            }

            Toggle_Text.Text = "Copying to Clipboard";
            FileOutType_Toggle.Icon = new SymbolIcon(Symbol.Copy);

        }

        private async void FileOutPath_DoubleTapped(object sender, RoutedEventArgs e)
        {
            FileOutPath.PlaceholderText = "";
            FileOutPath.PlaceholderText = (await fs.RequestOrGetFolder(OUT_FOLDER, FutureItemListOptions.ReplaceFutureItemIfExists)).Path;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), e);
        }

    }

}
