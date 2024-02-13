using BTRReportProcesser.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace BTRReportProcesser.Pages
{

    public sealed partial class FormatWizard : Page
    {
        DatasetViewerFormBuilder builder;
        StorageFile originalFile;

        public FormatWizard()
        {
            this.InitializeComponent();
            builder = null;
            originalFile = null;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            originalFile = ((StorageFile)e.Parameter);
            builder = new DatasetViewerFormBuilder(ExcelPreview, await originalFile.OpenStreamForReadAsync(), ExpandedView_TextBox);
            builder.Render();
            
            InitControls();
        }

        public List<DependencyObject> GetAllControls()
        {
            var ret = new List<DependencyObject>();
            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(this); i++)
            {
                ret.Add(VisualTreeHelper.GetChild(this, i));
            }

            return ret;
        }

        public void InitControls()
        {

            TextBox coun =  ExcelPreview.FindName("top0") as TextBox;
            TextBox site =  ExcelPreview.FindName("top1") as TextBox;
            TextBox prac =  ExcelPreview.FindName("top2") as TextBox;
            TextBox pate =  ExcelPreview.FindName("top3") as TextBox;
            TextBox comm =  ExcelPreview.FindName("top4") as TextBox;

            if (coun == null || site == null || prac == null || pate == null || comm == null) return;

            SetPreviewText(Country_Preview, coun.Text);
            SetPreviewText(Site_Preview, site.Text);
            SetPreviewText(Sub_Preview, pate.Text);
            SetPreviewText(PI_Preview, prac.Text);
            SetPreviewText(Comment_Preview, comm.Text);
            SelectedRow_TextBox.Text  = builder.CurrentScrollIndex.ToString();
            
            // enable continue button if everything is filled out
            if( !Country_Preview.IsEnabled && !Site_Preview.IsEnabled && !Sub_Preview.IsEnabled &&
                !PI_Preview.IsEnabled && !Comment_Preview.IsEnabled)
            {
                ContinueButton.IsEnabled = true;
            }

            // Override the blowup view to the selected field
            Expand_Override();

        }

        private void SetPreviewText(TextBox target, string outText)
        {
            ToolTip tt;
            target.Text = !target.GetTagAsBoolean() ? target.Text : outText;
            tt = new ToolTip();
            tt.Content = !target.GetTagAsBoolean() ? target.Text : outText;
            ToolTipService.SetToolTip(target, tt);
        }

        private void PanRightEnd_Click(object sender, RoutedEventArgs e)
        {
            for(var i = 0; i < builder.MAX_FOUND_ROWS - 1; i++)
            {
                builder.PanRight();
            }
            InitControls();
        }
        private void PanRight_Click(object sender, RoutedEventArgs e)
        {
            builder.PanRight();
            InitControls();
        }

        private void PanLeftEnd_Click(object sender, RoutedEventArgs e)
        {
            while(builder.CurrentPanIndex > 1)
            {
                builder.PanLeft();
            }
            InitControls();
        }
        private void PanLeft_Click(object sender, RoutedEventArgs e)
        {
            builder.PanLeft();
            InitControls();
        }

        private void ScrollUpTop_Click(object sender, RoutedEventArgs e)
        {
            while(builder.CurrentScrollIndex != 0)
            {
                builder.ScrollDown();
            }
            InitControls();
        }
        private void ScrollUp_Click(object sender, RoutedEventArgs e)
        {
            if (builder is null) return;

            builder.ScrollDown();
            InitControls();

        }

        private void ScrollDownTen_Click(object sender, RoutedEventArgs e)
        {
            for(var i = 0; i < 10; i++)
            {
                if(builder.CurrentScrollIndex + i > builder.LINES_TO_READ) { return; }

                builder.ScrollUp();
            }
            InitControls();
        }
        private void ScrollDown_Click(object sender, RoutedEventArgs e)
        {
            if (builder is null) return;

            builder.ScrollUp();
            InitControls();
        }

        private void Lock_Click(object sender, RoutedEventArgs e) { Lock_Click(sender as Button, e); }
        private void Lock_Click(Button sender, RoutedEventArgs e)
        {
            TextBox parent = FindName(sender.Tag.ToString()) as TextBox;
            bool state = parent.GetTagAsBoolean();

            if (!state)
            {
                parent.IsEnabled = true;
                parent.Tag = "true";
                ScrollDisabled_Text.Text = "";
                ScrollUp.IsEnabled = true;
                ScrollDown.IsEnabled = true;
                MassScrollUp_Button.IsEnabled = true;
                MassScrollDown_Button.IsEnabled = true;
                return;
            }

            parent.IsEnabled = false;
            parent.Tag = "false";
            ScrollDisabled_Text.Text = "Row Selection Made";
            ScrollUp.IsEnabled = false;
            ScrollDown.IsEnabled = false;
            MassScrollUp_Button.IsEnabled = false;
            MassScrollDown_Button.IsEnabled = false;

            InitControls();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            int row = Int32.Parse(SelectedRow_TextBox.Text);
            HeaderData headerData = new HeaderData(Comment_Preview.Text, Country_Preview.Text, PI_Preview.Text, Sub_Preview.Text, Site_Preview.Text, row);
            NavigationPackage.AddOrUpdate("headers", headerData);
            NavigationPackage.AddOrUpdate("excelFile", originalFile);
            NavigationPackage.AddOrUpdate("source", typeof(FormatWizard));


            this.Frame.Navigate(typeof(WorkPage), headerData);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), e);
        }

        private void Expand_Clicked(object sender, RoutedEventArgs e){ Expand_Clicked(sender as RadioButton, e);  }
        private void Expand_Clicked(RadioButton sender, RoutedEventArgs e)
        {
            TextBox parent = FindName(sender.Tag as string) as TextBox;

            parent.TextChanged += (box, ev) => { ExpandedView_TextBox.Text = ((TextBox)box).Text; };

        }

        private void Expand_Override()
        {
            RadioButton ourRadio;
            ourRadio = null;

            if ((bool) CountryPreview_Radio.IsChecked)
            {
                ourRadio = CountryPreview_Radio;
            }
            else if ((bool) CommentExpand_Radio.IsChecked)
            {
                ourRadio = CommentExpand_Radio;
            }
            else if ((bool) PIPreview_Radio.IsChecked)
            {
                ourRadio = PIPreview_Radio;
            }
            else if ((bool) SitePreview_Radio.IsChecked)
            {
                ourRadio = SitePreview_Radio;
            }
            else if ((bool) SubjectExpand_Radio.IsChecked)
            {
                ourRadio = SubjectExpand_Radio;
            }

            if (ourRadio is null) { return; }

            ExpandedView_TextBox.Text = ((TextBox)FindName(ourRadio.Tag.ToString())).Text;

        }
    }
}
