using System;
using System.Collections.Generic;
using System.Linq;
using Dictionary.Models;

namespace Dictionary.DataStructures
{
    public interface IStorageEngine
    {
        void Insert(DictionaryEntry entry);
        DictionaryEntry Search(string word);
        List<DictionaryEntry> SearchPrefix(string prefix);
        void Delete(string word);
        void Clear();
        int Count { get; }
        IEnumerable<DictionaryEntry> GetAll();
    }
}
