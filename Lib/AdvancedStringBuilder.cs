using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTRReportProcesser.Lib
{
    class AdvancedStringBuilder
    {
        public bool DoStringMutation;
        string hold;
        List<string> content;

        string _prepend; 
        public String OnAddPrepend
        {
            get { return _prepend; }
            set { _prepend = value == null ? "" : value; }
        }

        string _append;
        public String OnAddAppend
        {
            get { return _append; }
            set { _append = value == null ? "" : value; }
        }


        public AdvancedStringBuilder(string append, string prepend)
        {
            OnAddAppend = append;
            OnAddPrepend = prepend;
            _Init();
        }

        public AdvancedStringBuilder()
        {
            OnAddAppend = "";
            OnAddPrepend = "";
            _Init();
        }

        private void _Init()
        {
            DoStringMutation = true;
            content = new List<string>();
            hold = string.Empty;
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
            hold = string.Format(content, args);
            if (DoStringMutation) OnAdd();
            this.content.Add(hold);
        }

        public void AppendLineFormat(string content, params object[] args)
        {
            AppendFormat(content + "\n", args);
        }

        private void OnAdd()
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

        public override string ToString()
        {
            return string.Join("", content);
        }

    }

}
