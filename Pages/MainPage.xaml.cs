using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.ApplicationModel;
using Windows.UI.ViewManagement;
using Windows.System;
using System.Threading.Tasks;
using BTRReportProcesser.Lib;
using Windows.UI.Xaml.Navigation;
using BTRReportProcesser.Pages;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;


namespace BTRReportProcesser
{

    public sealed partial class MainPage : Page
    {
        FileSystem fs;
        public StorageFile MyExcelFile;
        private const bool isDebug = false;
        private const string OUT_FOLDER = "outputFolder";
        private const string LAST_FILE_NAME = "_last.xlsx";
        private readonly string[] GOOD_FILE_TYPES = { ".xlsx", ".xls", ".csv" };
        public Size WINDOW_SIZE = new Size(800, 600);


        //public DispatcherTimer I_Timer;

        public MainPage()
        {
            this.InitializeComponent();

            // Force 800x600 so I don't have to style it correctly
            ApplicationView.PreferredLaunchViewSize = WINDOW_SIZE;
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            Window.Current.CoreWindow.SizeChanged += (s, e) => { ApplicationView.GetForCurrentView().TryResizeView(WINDOW_SIZE); };

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await Async_Init();
        }

        private async Task Async_Init()
        {
            fs = await FileSystem.CreateAsync();

            if (isDebug)
            {
                StorageFolder installedLocation = Package.Current.InstalledLocation;
                MyExcelFile = await installedLocation.GetFileAsync(@"Datasets\dataset1-USAOnly.xlsx");
                this.Frame.Navigate(typeof(FormatWizard), MyExcelFile);
                //this.Frame.Navigate(typeof(WorkPage), MyExcelFile);
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                GotFileConf.Text = "Wrong File Type";

                return;
            }

            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count == 0)
            {
                GotFileConf.Text = "File Upload FAILED!";

                return;
            }
            var hold = (StorageFile)items[0];
                
            // Not a good file? - THEN GET OUTTA HERE!!!!
            if (!CheckGoodFileName(hold))
            {
                GotFileConf.Text = "Excel Files Only >:(\n.xlsx | .xls | .csv";
                return;
            }

            await HandleLastIfSelected(hold);

            MyExcelFile = hold;
            ContinueButton.IsEnabled = true;

        }

        private async void CheckBox_Checked(object sender, RoutedEventArgs e) { await CheckBox_Checked(sender as CheckBox, e); }
        private async Task CheckBox_Checked(CheckBox sender, RoutedEventArgs e)
        {
            if (fs is null) return;
            if (sender is null) return;

            if (!(bool)sender.IsChecked)
            {
                MyExcelFile = null;
                ContinueButton.IsEnabled = false;
                return;
            }

            if (!fs.HaveAccessTo(OUT_FOLDER))
            {
                sender.Content = "No History File!";
                sender.IsEnabled = false;
                
                return;
            }

            StorageFolder backFolder = await fs.RequestOrGetFolder(OUT_FOLDER);
            try
            {
                MyExcelFile = await backFolder.GetFileAsync(LAST_FILE_NAME);
                ContinueButton.IsEnabled = true;

                return;
            }
            catch
            {
                sender.Content = "Error Getting Last File";
                sender.IsEnabled = false;
                sender.IsChecked = false;
                
                return;
            }

        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool) OpenFormatWizard_Check.IsChecked)
            {
                this.Frame.Navigate(typeof(FormatWizard), MyExcelFile);
                return;
            }

            NavigationPackage.AddOrUpdate("source", typeof(MainPage));
            this.Frame.Navigate(typeof(WorkPage), MyExcelFile);
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Info_TextBlock.Visibility == Visibility.Visible)
            {
                Info_TextBlock.Visibility = Visibility.Collapsed;
                return;
            }

            Info_TextBlock.Visibility = Visibility.Visible;

        }

        private async void File_Picker_Button_Click(object sender, RoutedEventArgs e)
        {
            StorageFile hold = await fs.GetSingleFile(PickerLocationId.Desktop, GOOD_FILE_TYPES);
            if (hold == null) return;
            if (!CheckGoodFileName(hold)) return;

            await HandleLastIfSelected(hold);
            ContinueButton.IsEnabled = true;
        }

        private bool CheckGoodFileName(StorageFile da)
        {
            foreach (string item in GOOD_FILE_TYPES)
            {

                if (da.FileType == item)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task HandleLastIfSelected(StorageFile hold)
        {

            if (hold.DisplayName == "_last")
            {
                GotFileConf.Text = "Last File Selected";
                LastDocument_Check.IsChecked = true;
                await CheckBox_Checked(LastDocument_Check, null);
            }
            else
            {
                GotFileConf.Text = "Got A Good File!";
                FileName_TextBlock.Text = hold.Name;
                MyExcelFile = hold;
            }

        }
    }

}
