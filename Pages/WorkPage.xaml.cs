using BTRReportProcesser.Assets;
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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BTRReportProcesser
{
    public sealed partial class WorkPage : Page
    {
        const int HEADER_ROW = 10;

        DataSet sheet;
        StorageFile myFile;

        EnumerableRowCollection<DataRow> QueryableLinqLines;

        public WorkPage()
        {
            sheet = null;
            myFile = null;

            this.InitializeComponent();
        }

        // Surgery Last Tuesday
        // Wendsday 7th a 4:15 PM


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            myFile = (StorageFile)e.Parameter;

            // Make sure to await async. This function sets -
            // critical variable "sheet" and allows us to actually -
            // do some work in this thing.
            await ReadInFile(null, null);
            InitControls();
        }
        private void ComboSelectionChanged_ComboBox(object sender, PointerRoutedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex == 0) return;
            if ((sender as ComboBox).SelectedValue as String == "") return;

            InitControls();
        }

        public void InitControls()
        {
            if (sheet == null) return;

            String CountrBoxValue = Country_ComboBox.SelectedValue as String;
            String PractiBoxValue = Practitoner_ComboBox.SelectedValue as String;
            String PNumbeBoxValue = PNumber_ComboBox.SelectedValue as String;
            String SiteIdBoxValue = SiteId_ComboBox.SelectedValue as String;

            QueryableLinqLines = sheet.Tables[0].AsEnumerable()
            .Where((row) =>
            {

                if (row["Country"]        == (CountrBoxValue == null ? row["Country"] : CountrBoxValue) &&
                    row["PI Name"]        == (PractiBoxValue == null ? row["PI Name"] : PractiBoxValue) &&
                    row["Patient Number"] == (PNumbeBoxValue == null ? row["Patient Number"] : PNumbeBoxValue) &&
                    row["Site ID"]        == (SiteIdBoxValue == null ? row["Site ID"] : SiteIdBoxValue))
                {
                    return true;
                }

                return false;
            });

            Country_ComboBox.ItemsSource = QueryableLinqLines.Select(row => row["Country"].ToString()).Distinct().ToList();
            Practitoner_ComboBox.ItemsSource = QueryableLinqLines.Select(row => row["PI Name"].ToString()).Distinct().ToList();
            PNumber_ComboBox.ItemsSource = QueryableLinqLines.Select(row => row["Patient Number"].ToString()).Distinct().ToList();
            SiteId_ComboBox.ItemsSource = QueryableLinqLines.Select(row => row["Site ID"].ToString()).Distinct().ToList();

            Country_ComboBox.SelectedValue = CountrBoxValue;
            Practitoner_ComboBox.SelectedValue = PractiBoxValue;
            PNumber_ComboBox.SelectedValue = PNumbeBoxValue;
            SiteId_ComboBox.SelectedValue = SiteIdBoxValue;
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
        
        private void Reset_ComboBoxes(object sender, RoutedEventArgs e)
        {
            ComboBox sender_parent = this.FindName((sender as Button).Tag as string) as ComboBox;
            sender_parent.SelectedValue = null;
            sender_parent.SelectedIndex = -1;
        }

        private void SiteId_Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            SiteId_ComboBox.SelectedValue = null;
            SiteId_ComboBox.SelectedIndex = -1;
            InitControls();
        }

        private void PNumber_Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            PNumber_ComboBox.SelectedValue = null;
            PNumber_ComboBox.SelectedIndex = -1;
            InitControls();
        }

        private void Practitoner_Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            Practitoner_ComboBox.SelectedValue = null;
            Practitoner_ComboBox.SelectedIndex = -1;
            InitControls();
        }

        private void Country_Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            Country_ComboBox.SelectedValue = null;
            Country_ComboBox.SelectedIndex = -1;
            InitControls();
        }

        private void Refresh_Controls_Button_Click(object sender, RoutedEventArgs e)
        {
            InitControls();
        }

        private void ExportData_Button_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch time = new Stopwatch();
            time.Start();


            if (ProcessComments_Toggle.IsChecked == null) return;

            SortedDictionary mappedComments = null;
            SortedDictionary countedComments = null;


            List<String[]> allRows = QueryableLinqLines.Select(row => (String[])row.ItemArray.Select(item => item.ToString()).ToArray()).ToList();

            AdvancedStringBuilder CSVOut = new AdvancedStringBuilder("", ";");
            time.Stop();

            // Header Info
            CSVOut.AppendLine("Clario | ReisbigLLC | V0.1");
            CSVOut.AppendLineFormat("Created: {0}", System.DateTime.Now);
            CSVOut.AppendLineFormat("Total Runtime: {0}ms", time.Elapsed);

            // Filter Info
            CSVOut.AppendLineFormat("Country: {0}", Country_ComboBox.SelectedValue == null ? "All" as String : Country_ComboBox.SelectedValue);
            CSVOut.AppendLineFormat("Practitoner: {0}", Practitoner_ComboBox.SelectedValue == null ? "All" as String : Practitoner_ComboBox.SelectedValue);
            CSVOut.AppendLineFormat("Patient Number: {0}", PNumber_ComboBox.SelectedValue == null ? "All" as String : PNumber_ComboBox.SelectedValue);
            CSVOut.AppendLineFormat("Site Id: {0}", SiteId_ComboBox.SelectedValue == null ? "All" as String : SiteId_ComboBox.SelectedValue);

            // Association Data
            CSVOut.AppendLine("Association Data");
            CommentManager commentManager = new CommentManager(
                CommentLookupTable.Table,
                QueryableLinqLines.Select(row => row["Overread Selection Reason"].ToString()).ToList()
            );

            countedComments = new SortedDictionary(commentManager.CommentCounts);

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
            CSVOut.AppendLine("Line Item Totals");

            foreach (var c in countedComments)
            {
                CSVOut.AppendLine(c.key + ";" + c.value);
            }

            CSVOut.AppendLine();
            DataPackage dp = new DataPackage();
            dp.RequestedOperation = DataPackageOperation.Copy;
            dp.SetText(CSVOut.ToString());
            Clipboard.SetContent(dp);
        }

        private void FileOutType_Toggle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Hiding_Rectangle.Fill.Opacity = 0;
        }

        private void Hiding_Rectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Hiding_Rectangle.Fill.Opacity = 100;
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
    }


    class AdvancedStringBuilder
    {
        List<String> content;
        string hold;
        String OnAddPrepend;
        String OnAddAppend;
        bool DoStringMutation;
        public AdvancedStringBuilder(String prepend, String append)
        {
            content = new List<String>();
            hold = String.Empty;
            OnAddAppend = append;
            OnAddPrepend = prepend;
            DoStringMutation = true;
        }

        public AdvancedStringBuilder()
        {
            content = new List<String>();
            hold = String.Empty;
            OnAddAppend = "";
            OnAddPrepend = "";
            DoStringMutation = true;
        }

        public void Append(string content)
        {
            hold = content;
            if (DoStringMutation) OnAdd();
            this.content.Add(hold);
        }

        public void AppendLine(string content)
        {
            Append(content + "\n");
        }
        public void AppendLine()
        {
            Append("\n");
        }

        public void AppendFormat(string content, params object[] args)
        {
            hold = content;
            hold = String.Format(content, args);
            if (DoStringMutation) OnAdd();
            this.content.Add(hold);
        }

        public void AppendLineFormat(string content, params object[] args)
        {
            AppendFormat(content + "\n", args);
        }

        public void OnAdd()
        {
            this.hold = this.hold.Insert(0, OnAddPrepend);

            if (hold.EndsWith("\n"))
            {
                this.hold = this.hold.Insert(this.hold.Length - 1, OnAddAppend);
            }
            else
            {
                this.hold += OnAddAppend;
            }
        }

        public override String ToString()
        {
            return String.Join("", content);
        }

    }

    struct PairTuple
    {
        public String key;
        public int value;
    }

    class SortedDictionary : IEnumerable<PairTuple>
    {
        List<PairTuple> contents;
        Dictionary<String, int> original;

        public SortedDictionary(Dictionary<string, int> target)
        {
            contents = new List<PairTuple>();
            original = target;
            StripDict();
            Sort();

        }

        private void Sort()
        {
            this.contents = contents.OrderBy(x => x.value).Reverse().ToList();
        }

        private void StripDict()
        {
            PairTuple dictItem = new PairTuple();

            foreach (var item in original)
            {
                dictItem.key = item.Key;
                dictItem.value = item.Value;
                contents.Add(dictItem);
            }

        }

        public IEnumerator<PairTuple> GetEnumerator()
        {
            return contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }

    class CommentManager
    {
        List<Comment> processedComments;
        public Dictionary<String, int> CommentCounts;
        public Dictionary<String, int> MappedComments;
        Dictionary<String, String> LookupTable;
        public CommentManager(Dictionary<String, String> associationsTable, List<String> comments)
        {
            processedComments = new List<Comment>();
            AddCommentList(comments);

            LookupTable = associationsTable;
            MappedComments = new Dictionary<String, int>();
            CommentCounts = new Dictionary<String, int>();


            MapAndCountComments();
        }

        public void AddComment(String comment)
        {
            this.processedComments.Add(new Comment(comment));
        }

        public void AddCommentList(List<String> comments)
        {
            foreach (String c in comments)
            {
                AddComment(c);
            }
        }

        public List<String> GetAllSplitComments()
        {
            List<String> accumulator = new List<String>();

            foreach (Comment c in processedComments)
            {
                accumulator.AddRange(c.SplitComments);
            }

            return accumulator;
        }
        private void MapAndCountComments()
        {
            foreach (String c in this.GetAllSplitComments())
            {
                SetDefaultDoesntExist(c, this.CommentCounts);
                SetDefaultDoesntExist(LookupTable[c], this.MappedComments);
            }

        }

        private void SetDefaultDoesntExist(String key, Dictionary<String, int> target)
        {
            if (target.ContainsKey(key))
            {
                target[key] += 1;
                return;
            }

            target[key] = 1;
        }

    }

    public class Comment
    {
        public String[] SplitComments;
        public String RawString;

        public Comment(String commentText)
        {
            this.RawString = commentText;
            this.SplitComments = commentText
                .Split(";")
                .Select(item => item.Trim())
                .ToArray();
        }
    }

}
