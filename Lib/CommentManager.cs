using BTRReportProcesser.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
namespace BTRReportProcesser.Lib
{
    class CommentManager
    {
        List<Comment> processedComments;
        public Dictionary<string, int> CommentCounts;
        public Dictionary<string, int> MappedComments;
        public Dictionary<string, int> NoAssociations;
        Dictionary<string, string> LookupTable;
        public CommentManager(Dictionary<string, string> associationsTable, List<string> comments)
        {
            processedComments = new List<Comment>();
            AddCommentList(comments);

            LookupTable = associationsTable;
            MappedComments = new Dictionary<string, int>();
            CommentCounts = new Dictionary<string, int>();
            NoAssociations = new Dictionary<string, int>();


            MapAndCountComments();
        }

        public void AddComment(string comment)
        {
            this.processedComments.Add(new Comment(comment));
        }

        public void AddCommentList(List<string> comments)
        {
            foreach (string c in comments)
            {
                AddComment(c);
            }
        }

        public List<string> GetAllSplitComments()
        {
            List<string> accumulator = new List<string>();

            foreach (Comment c in processedComments)
            {
                accumulator.AddRange(c.SplitComments);
            }

            return accumulator;
        }
        private void MapAndCountComments()
        {
            foreach (string c in this.GetAllSplitComments())
            {
                if (c is null) continue;
                if (c == "")   continue;
                if (c == "\"") continue;

                if (!LookupTable.ContainsKey(c))
                {
                    SetDefaultDoesntExist(c, this.NoAssociations);
                    continue;
                }

                SetDefaultDoesntExist(c, this.CommentCounts);
                SetDefaultDoesntExist(LookupTable[c], this.MappedComments);
            }

        }

        private void SetDefaultDoesntExist(string key, Dictionary<string, int> target)
        {


            if (target.ContainsKey(key))
            {
                target[key] += 1;
                return;
            }

            target.Add(key, 1);
        }

    }

    public class Comment
    {
        public List<String> SplitComments;
        public string SanitizedComments;
        public string RawString;

        public Comment(string commentText)
        {

            this.RawString = commentText;
            this.SanitizedComments = String.Empty;

            this.SplitComments = commentText
                .Split(";")
                .Select(item => item.Trim())
                .ToList();

            SanitizeComments();
        }

        public void SanitizeComments()
        {
            List<String> newComments = new List<String>();

            string hold = String.Empty;
            bool isHold = false;
            foreach (string item in SplitComments)
            {
                if (item == "") continue;

                if (isHold)
                {
                    newComments.Add(hold + " " + item);
                    isHold = false;
                    hold = String.Empty;
                    continue;
                }

                if (CommentLookupTable.Table.ContainsKey(item))
                {
                    newComments.Add(item);
                    continue;
                }

                hold = item;
                isHold = true;
            };

            SplitComments = newComments;
        }
    }
}
