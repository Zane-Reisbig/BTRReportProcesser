using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace BTRReportProcesser.Lib
{
    struct PairTuple
    {
        public string key;
        public int value;
    }

    class SortedDictionary : IEnumerable<PairTuple>
    {
        List<PairTuple> contents;
        Dictionary<string, int> original;

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
}
