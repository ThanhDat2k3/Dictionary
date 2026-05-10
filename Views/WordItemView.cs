using System.Windows;
using Dictionary.Models;

namespace Dictionary.Views
{
    public class WordItemView
    {
        public string Word { get; set; }
        public string Preview { get; set; }
        public DictionaryEntry Source { get; set; }

        public WordItemView(DictionaryEntry entry)
        {
            Source = entry;
            Word = entry.Word ?? "N/A";

            // Tạo preview 60 ký tự
            string definition = entry.Definition ?? "";
            if (definition.Length > 60)
            {
                Preview = definition.Substring(0, 60).Trim() + "...";
            }
            else
            {
                Preview = definition;
            }
        }

        public override string ToString() => Word;
    }
}
