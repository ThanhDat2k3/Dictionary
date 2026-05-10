using System;
using System.Collections.Generic;
using System.Linq;
using Dictionary.Models;

namespace Dictionary.DataStructures
{
    public class HashTableStorage : IStorageEngine
    {
        private Dictionary<string, DictionaryEntry> _table;

        public int Count => _table.Count;

        public HashTableStorage()
        {
            _table = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public void Insert(DictionaryEntry entry)
        {
            if (entry != null)
            {
                _table[entry.Word] = entry;
            }
        }

        public DictionaryEntry Search(string word)
        {
            return _table.ContainsKey(word) ? _table[word] : null;
        }

        public List<DictionaryEntry> SearchPrefix(string prefix)
        {
            return _table
                .Where(x => x.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Value)
                .OrderBy(x => x.Word, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public void Delete(string word)
        {
            _table.Remove(word);
        }

        public void Clear()
        {
            _table.Clear();
        }

        public IEnumerable<DictionaryEntry> GetAll()
        {
            return _table.Values;
        }
    }
}
