using System;
using System.Collections.Generic;
using System.Linq;

namespace Dictionary.Models
{
    public class DictionaryEntry
    {
        public string Word { get; set; }
        public string Definition { get; set; }
        public string Specialization { get; set; } = "N/A";
        public string Field { get; set; } = "N/A";
        public List<string> Meanings { get; set; } = new List<string>();

        public DictionaryEntry() { }

        public DictionaryEntry(string word, string definition)
        {
            Word = word;
            Definition = definition;
            Meanings = definition.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public override string ToString()
        {
            return Word ?? "N/A";
        }
    }

    public class SearchHistory
    {
        public string Word { get; set; }
        public DateTime Time { get; set; }
        public TimeSpan SearchTime { get; set; }
        public string SearchType { get; set; } // "Chính xác", "Tiền tố"
        public double? BPlusTreeTimeMs { get; set; }
        public double? BSTTimeMs { get; set; }
        public double? HashTableTimeMs { get; set; }
    }
}
