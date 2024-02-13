using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace BTRReportProcesser.Lib
{
    internal class DatasetViewerFormBuilder
    {
        public int MAX_FOUND_ROWS;
        public int LINES_TO_READ;
        bool ALLOW_SCROLL;
        int COL_AMOUNT;
        int ROW_AMOUNT;

        Point Origin;
        Grid targetGrid;
        TextBox OutputHandler;
        Stack<string> PanStack;
        private List<List<string>> OutputData;
        readonly List<List<string>> OutputDataCopy;
        public List<List<TextBox>> OutputGrid;
        int ButtonWidth;
        int PaddingTop;
        int ButtonXOffset;

        private int _sIndex;
        public int CurrentScrollIndex
        {
            get { return _sIndex; }
            set
            {
                if (value < 0) { _sIndex = 0; return; }

                if (value + COL_AMOUNT >= LINES_TO_READ) { _sIndex = LINES_TO_READ - (COL_AMOUNT + 1); return; }

                _sIndex = value;
                return;
            }
        }
        private int _pIndex;
        public int CurrentPanIndex
        {
            get { return _pIndex; }
            set
            {
                if (value < 0) { _pIndex = 0; return; }

                if (value + ROW_AMOUNT >= MAX_FOUND_ROWS) { _pIndex = MAX_FOUND_ROWS - (ROW_AMOUNT + 1); return; }

                _pIndex = value;
                return;
            }
        }



        public DatasetViewerFormBuilder(Grid target, Stream ExcelFileStream, TextBox ExpandedView)
        {
            this.MAX_FOUND_ROWS = 0;
            this.LINES_TO_READ = 30;
            this.ALLOW_SCROLL  = true;
            this.Origin        = new Point(10, 10);
            this.PaddingTop    = 50;
            this.ButtonWidth   = 120;
            this.ButtonXOffset = ButtonWidth + 10;
            this.ROW_AMOUNT    = 5;
            this.COL_AMOUNT    = 5;
            this.targetGrid    = target;
            this.OutputData    = ReadInFileLines(ExcelFileStream);
            this.OutputDataCopy = new List<List<String>>(OutputData);
            this.OutputGrid    = GenerateGrid(true, OutputData.GetRange(0 , ROW_AMOUNT).ToList() ) ;
            this.CurrentScrollIndex = 0;
            this.CurrentPanIndex = 0;
            this.PanStack = new Stack<string>();
            this.OutputHandler = ExpandedView;
        }

        public List<List<TextBox>> GenerateGrid(bool markFirstRow, List<List<string>> lines)
        {
            //_________________________
            //|___|___|___|___|___|___|
            //|___|___|___|___|___|___|
            //|___|___|___|___|___|___|
            //|___|___|___|___|___|___|

            Point anchor = Origin;
            List<List<TextBox>> data = new List<List<TextBox>>();
            List<TextBox> row;
            for(int colNum = 0; colNum < COL_AMOUNT; colNum++)
            {
                row = new List<TextBox>();
                    
                for (int rowNum = 0; rowNum < ROW_AMOUNT; rowNum++)
                {
                    var tb = new TextBox();
                    tb.Width  = ButtonWidth;
                    tb.Margin = new Thickness(anchor.X, anchor.Y, 0, 0);

                    try
                    {
                        tb.Text = lines[colNum][rowNum];
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        tb.Text = "";
                    }

                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    tb.VerticalAlignment   = VerticalAlignment.Top;
                    tb.IsReadOnly = true;
                    tb.PointerEntered += (s, e) => { OutputHandler.Text = ((TextBox)s).Text; };

                    if(colNum == 0 && markFirstRow) { tb.Name = String.Format("top{0}", rowNum); }

                    row.Add(tb);

                    anchor.X += ButtonXOffset;
                }

                anchor.Y += PaddingTop;
                anchor.X = Origin.X;
                data.Add(row);
            }

            return data;
        }

        private List<List<string>> ReadInFileLines(Stream fileStream)
        {
            List<List<string>> ret = new List<List<string>>();
            List<string> row;

            using (fileStream)
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(fileStream))
                {
                    int line = 1;
                    while (reader.Read() && line < LINES_TO_READ)
                    {
                        row = new List<string>();
                        int curColWidth = reader.FieldCount;
                        
                        // Used for panning later
                        MAX_FOUND_ROWS = Math.Max(MAX_FOUND_ROWS, curColWidth);

                        for(int i = 0; i < curColWidth - 1; i++)
                        {
                            object hold = reader.GetValue(i);
                            if (hold is null) continue;
                            row.Add(hold.ToString());
                        }

                        ret.Add(row);
                        line++;
                    }

                    // The result of each spreadsheet is in result.Tables
                }
            }

            fileStream.Close();
            return ret;
        }


        public void Render(List<List<TextBox>> toRender)
        {
            foreach(List<TextBox> row in toRender)
            {
                foreach(TextBox item in row)
                {
                    targetGrid.Children.Add(item);
                }
            }
        }

        private void Scroll(int behavior)
        {
            if (!ALLOW_SCROLL) return;

            List<List<string>> scrollContent;

            switch (behavior)
            {
                case 0:
                    CurrentScrollIndex++;
                    break;
                case 1:
                    CurrentScrollIndex--;
                    break;
            }

            scrollContent = OutputData.GetRange(CurrentScrollIndex, COL_AMOUNT);
            SetGridContent(scrollContent);
        }

        private void Pan(int behavior)
        {
            List<List<string>> panContent = new List<List<string>>(OutputDataCopy.GetRange(CurrentScrollIndex, ROW_AMOUNT));

            switch (behavior)
            {
                case 0:
                    CurrentPanIndex++;
                    for(int i = 0; i < ROW_AMOUNT; i++)
                    {
                        string hold = OutputGrid.ToArray()[i][0].Text;
                        PanStack.Push(hold);
                    }

                    List<List<string>> culledData = new List<List<string>>();
                    List<string> copyHold;
                    foreach(List<string> item in panContent)
                    {
                        copyHold = new List<string>(item);
                        if(item.Count < CurrentPanIndex)
                        {
                            copyHold.RemoveRange(0, item.Count);
                        }
                        else
                        {
                            copyHold.RemoveRange(0, CurrentPanIndex);
                        }
                        culledData.Add(copyHold);
                    }

                    SetGridContent(culledData);

                    break;
                case 1:
                    if (CurrentPanIndex == 0) return;
                    CurrentPanIndex--;

                    for (int i = ROW_AMOUNT - 1; i >= 0; i--)
                    {
                        if (panContent[i].Count > 0)
                        {
                            var popped = PanStack.Pop();
                            panContent[i][0] = panContent[i][0] = popped;
                        }
                        else
                        {
                            PanStack.Pop();
                        }

                    }

                    SetGridContent(panContent);
                    break;

            }

            ALLOW_SCROLL = CurrentPanIndex == 0 ? true : false;
        }
        
        public void PanRight()
        {
            Pan(0);
        }

        public void PanLeft()
        {
            // This is so gross
            Pan(1);
            if(CurrentPanIndex < 1 ) { return; }
            Pan(1);
            Pan(0);
        }

        public void ScrollUp()
        {
            Scroll(0);
        }

        public void ScrollDown()
        {
            Scroll(1);
        }

        public void SetGridContent(List<List<String>> content)
        {
            for(int row = 0; row < OutputGrid.Count; row++)
            {
                for(int box = 0; box < OutputGrid[row].Count; box++)
                {
                    string c;
                    TextBox t = OutputGrid[row][box];
                    try
                    {
                        c = content[row][box];
                    }
                    catch
                    {
                        c = "";
                    }

                    t.Text = c;
                }
            }
        } 

        public void Render()
        {
            Render(OutputGrid);
        }
    }
}
