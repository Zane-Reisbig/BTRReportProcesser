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


namespace BTRReportProcesser
{

    public sealed partial class MainPage : Page
    {
        FileSystem fs;
        public StorageFile MyExcelFile;
        private const bool isDebug = false;
        private const string OUT_FOLDER = "outputFolder";
        private const string LAST_FILE_NAME = "_last.xlsx";
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
                this.Frame.Navigate(typeof(WorkPage), MyExcelFile);
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

            MyExcelFile = (StorageFile)items[0];
            ContinueButton.IsEnabled = true;
            GotFileConf.Text = "Got A Good File!";

            // TODO: Copy last File Here
            var outFolder = await fs.RequestFolderAccess(OUT_FOLDER);
            var lastFile  = await outFolder.CreateFileAsync(LAST_FILE_NAME, CreationCollisionOption.ReplaceExisting);
            await MyExcelFile.CopyAndReplaceAsync(lastFile);

        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WorkPage), MyExcelFile);
        }

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (fs is null) return;

            if ((bool)((CheckBox)sender).IsChecked)
            {
                StorageFolder backFolder = (StorageFolder)await fs.RequestFolderAccess(OUT_FOLDER);
                try
                {
                    MyExcelFile = await backFolder.GetFileAsync(LAST_FILE_NAME);
                    ContinueButton.IsEnabled = true;
                    return;
                }
                catch
                {
                    return;
                }
            }

            MyExcelFile = null;
            ContinueButton.IsEnabled = false;
        }
    }

}
