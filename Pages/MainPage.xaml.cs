using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Printers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel;
using Windows.UI.ViewManagement;
using Windows.System;
using Windows.Management.Deployment;
using System.Threading.Tasks;
using System.Security.Principal;
using System.ComponentModel;
using System.Threading;


namespace BTRReportProcesser
{

    public sealed partial class MainPage : Page
    {
        public Size WINDOW_SIZE = new Size(800, 600);
        private bool isDebug = false;

        public StorageFile MyExcelFile;
        private UserInformation UserInfo;

        //public DispatcherTimer I_Timer;

        public MainPage()
        {
            this.InitializeComponent();

            // Force 800x600 so I don't have to style it correctly
            ApplicationView.PreferredLaunchViewSize = WINDOW_SIZE;
            Window.Current.CoreWindow.SizeChanged += (s, e) => { ApplicationView.GetForCurrentView().TryResizeView(WINDOW_SIZE); };



            // Good lord make sure the frame isnt null before acting on it
            // not a problem in x32 but in x64 somereason its borked
            // So now we init the stuff on page loaded instead of page init
            this.Loaded += Async_Init;

        }

        private async Task GetCurrentUser()
        {
            IReadOnlyList<User> users = await User.FindAllAsync();

            var current = users.Where(p => p.AuthenticationStatus == UserAuthenticationStatus.LocallyAuthenticated &&
                                        p.Type == UserType.LocalUser).FirstOrDefault();

            // user may have username
            var data = await current.GetPropertyAsync(KnownUserProperties.AccountName);
            String displayUserName = (string)data;

            // user might not have this either
            String first = (string)await current.GetPropertyAsync(KnownUserProperties.FirstName);
            String last = (string)await current.GetPropertyAsync(KnownUserProperties.LastName);

            UserInfo = new UserInformation(displayUserName, first, last);


        }

        public class UserInformation
        {
            public String DisplayUserName;
            public String First;
            public String Last;
            public User CurrentUser;

            public UserInformation(string DisplayUserName, string First, string Last)
            {
                this.DisplayUserName = DisplayUserName;
                this.First = First;
                this.Last = Last;

                this.CurrentUser = null;
                var x = GetUserAsync();

                //while (!x.IsCompleted)
                //{
                //    Thread.Sleep(1);
                //}
            }

            private async Task GetUserAsync()
            {
                var hold = await User.FindAllAsync();
                this.CurrentUser = hold[0];
            }
        }

        private async void Async_Init(object sender, object e)
        {
            if (isDebug)
            {

                StorageFolder installedLocation = Package.Current.InstalledLocation;
                MyExcelFile = await installedLocation.GetFileAsync(@"Assets\dataset1-USAOnly.xlsx");
                this.Frame.Navigate(typeof(WorkPage), MyExcelFile);

            }

            await GetCurrentUser();

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

//#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//            // Just do this in the background, shouldn't take more than a second

//            var x = ApplicationData.GetForUserAsync(User.GetFromId(WindowsIdentity.GetCurrent()));
//            //MyExcelFile.CopyAsync(, "last.xlsx");
//            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WorkPage), MyExcelFile);
        }

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //MyExcelFile = await StorageFile.GetFileFromPathAsync(ApplicationData.Current.LocalFolder + @"/last.xlsx");
        }
    }

}
