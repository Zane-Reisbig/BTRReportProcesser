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


namespace BTRReportProcesser
{
    public sealed partial class MainPage : Page
    {
        public Size WINDOW_SIZE = new Size(800, 600);
        private bool isDebug = true;

        public DebugConsole I_Console;
        public StorageFile MyExcelFile;

        //public DispatcherTimer I_Timer;

        public MainPage()
        {
            this.InitializeComponent();

            // Force 800x600 so I don't have to style it correctly
            ApplicationView.PreferredLaunchViewSize = WINDOW_SIZE;
            Window.Current.CoreWindow.SizeChanged += (s, e) => { ApplicationView.GetForCurrentView().TryResizeView(WINDOW_SIZE); };

            if (isDebug)
            {
                this.I_Console = new DebugConsole(ConsoleOutput);
                this.I_Console.maxCharsTillClear = 50;
            }


            Async_Init();
            this.Loaded += Page_Loaded;
        }

        private async void Async_Init()
        {
            if (isDebug)
            {

                StorageFolder installedLocation = Package.Current.InstalledLocation;
                MyExcelFile = await installedLocation.GetFileAsync(@"Assets\dataset1-USAOnly.xlsx");
                this.Frame.Navigate(typeof(WorkPage), MyExcelFile);

            }



        }


        private void Page_Loaded(object sender, object e)
        {

           // //  DispatcherTimer setup
           //I_Timer = new DispatcherTimer();
           //I_Timer.Tick += I_Timer_Tick;
           //I_Timer.Interval = TimeSpan.FromSeconds(1);
           //I_Timer.Start();

        }

        //public void I_Timer_Tick(object sender, object e)
        //{
        //    I_Console.Println("Hello World!");   
        //}

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

            MyExcelFile = (StorageFile) items[0];
            ContinueButton.IsEnabled = true;
            GotFileConf.Text = "Got A Good File!";
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WorkPage), MyExcelFile);
        }

    }

    public class DebugConsole
    {
        private TextBlock target { get; set; }
        public int? maxCharsTillClear { get; set; }

        private object _lastCommand;
        public object LastCommand
        {
            get => _lastCommand; 
            set
            { 
                _lastCommand = value.ToString()
                                    .Contains("\n") 
                                    ? 
                                    value.ToString() 
                                    : 
                                    value.ToString() + "\n";
            } 
        }

        private void AutoClear() {
            if (target.Text.Length <= maxCharsTillClear) return;

            Clear();
            target.Text += "...\n";
            target.Text += LastCommand;
        }

        public DebugConsole(TextBlock target)
        {
            this.target = target;
            Clear();
            LastCommand = "Console Initilized\n";
        }

        public void Println(object da)
        {
            LastCommand = da;

            target.Text += da;
            target.Text += "\n";

            if (maxCharsTillClear != null) AutoClear();
        }

        public void Print(object da)
        {
            LastCommand = da;

            target.Text += da;

            if (maxCharsTillClear != null) AutoClear();
        }

        public void Clear()
        {
            target.Text = "";
        }

    }

}
