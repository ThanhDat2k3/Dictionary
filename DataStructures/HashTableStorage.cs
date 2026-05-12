using Dictionary.Models;

namespace Dictionary.DataStructures
{
    public class HashTableStorage : IStorageEngine
    {
        private Dictionary<string, DictionaryEntry> Table;
        private long TotalNodeAccessCount = 0;

        public int Count => Table.Count;

        public HashTableStorage()
        {
            Table = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public void Insert(DictionaryEntry entry)
        {
            if (entry != null)
            {
                TotalNodeAccessCount++;
                Table[entry.Word] = entry;
            }
        }

        public DictionaryEntry Search(string word)
        {
            long currentAccess = 0;
            var result = SearchInternal(word, ref currentAccess);
            TotalNodeAccessCount += currentAccess;
            return result;
        }

        private DictionaryEntry SearchInternal(string word, ref long accessCount)
        {
            accessCount++;
            return Table.ContainsKey(word) ? Table[word] : null;
        }

        public List<DictionaryEntry> SearchPrefix(string prefix)
        {
            long currentAccess = 0;
            var results = SearchPrefixInternal(prefix, ref currentAccess);
            TotalNodeAccessCount += currentAccess;
            return results;
        }

        private List<DictionaryEntry> SearchPrefixInternal(string prefix, ref long accessCount)
        {
            accessCount++;
            return Table
                .Where(x => x.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Value)
                .OrderBy(x => x.Word, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public void Delete(string word)
        {
            TotalNodeAccessCount++;
            Table.Remove(word);
        }

        public void Clear()
        {
            Table.Clear();
            TotalNodeAccessCount = 0;
        }

        public IEnumerable<DictionaryEntry> GetAll()
        {
            return Table.Values;
        }
    }
}
