using System;
using System.Collections.Generic;
using System.Linq;
using Dictionary.Models;

namespace Dictionary.DataStructures
{
    public class BSTStorage : IStorageEngine
    {
        private class AVLNode
        {
            public DictionaryEntry Entry { get; set; }
            public AVLNode Left { get; set; }
            public AVLNode Right { get; set; }
            public int Height { get; set; } = 1;

            public AVLNode(DictionaryEntry entry)
            {
                Entry = entry;
            }
        }

        private AVLNode _root;
        private int _count;

        public int Count => _count;

        public void Insert(DictionaryEntry entry)
        {
            if (entry != null)
            {
                bool isNew = false;
                _root = InsertNode(_root, entry, ref isNew);
                if (isNew) _count++;
            }
        }

        private AVLNode InsertNode(AVLNode node, DictionaryEntry entry, ref bool isNew)
        {
            if (node == null)
            {
                isNew = true;
                return new AVLNode(entry);
            }

            int cmp = string.Compare(entry.Word, node.Entry.Word, StringComparison.OrdinalIgnoreCase);
            if (cmp < 0)
            {
                node.Left = InsertNode(node.Left, entry, ref isNew);
            }
            else if (cmp > 0)
            {
                node.Right = InsertNode(node.Right, entry, ref isNew);
            }
            else
            {
                node.Entry = entry;
                return node;
            }

            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

            node = Balance(node);
            return node;
        }

        public DictionaryEntry Search(string word)
        {
            var node = SearchNode(_root, word);
            return node?.Entry;
        }

        private AVLNode SearchNode(AVLNode node, string word)
        {
            if (node == null)
                return null;

            int cmp = string.Compare(word, node.Entry.Word, StringComparison.OrdinalIgnoreCase);
            if (cmp < 0)
                return SearchNode(node.Left, word);
            else if (cmp > 0)
                return SearchNode(node.Right, word);
            else
                return node;
        }

        public List<DictionaryEntry> SearchPrefix(string prefix)
        {
            var results = new List<DictionaryEntry>();
            SearchPrefixNode(_root, prefix, results);
            return results.OrderBy(x => x.Word, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private void SearchPrefixNode(AVLNode node, string prefix, List<DictionaryEntry> results)
        {
            if (node == null)
                return;

            if (node.Entry.Word.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(node.Entry);
            }

            int cmp = string.Compare(prefix, node.Entry.Word, StringComparison.OrdinalIgnoreCase);
            if (cmp <= 0)
                SearchPrefixNode(node.Left, prefix, results);
            if (cmp >= 0)
                SearchPrefixNode(node.Right, prefix, results);
        }

        public void Delete(string word)
        {
            int countBefore = _count;
            _root = DeleteNode(_root, word);
            if (_count < countBefore)
                _count--;
        }

        private AVLNode DeleteNode(AVLNode node, string word)
        {
            if (node == null)
                return null;

            int cmp = string.Compare(word, node.Entry.Word, StringComparison.OrdinalIgnoreCase);
            if (cmp < 0)
            {
                node.Left = DeleteNode(node.Left, word);
            }
            else if (cmp > 0)
            {
                node.Right = DeleteNode(node.Right, word);
            }
            else
            {
                _count--;

                if (node.Left == null)
                    return node.Right;
                if (node.Right == null)
                    return node.Left;

                AVLNode minNode = FindMin(node.Right);
                node.Entry = minNode.Entry;
                node.Right = DeleteNode(node.Right, minNode.Entry.Word);
            }

            if (node == null)
                return null;

            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

            return Balance(node);
        }

        private AVLNode FindMin(AVLNode node)
        {
            while (node.Left != null)
                node = node.Left;
            return node;
        }

        private int GetHeight(AVLNode node)
        {
            return node?.Height ?? 0;
        }

        private int GetBalance(AVLNode node)
        {
            if (node == null)
                return 0;
            return GetHeight(node.Left) - GetHeight(node.Right);
        }

        private AVLNode Balance(AVLNode node)
        {
            if (node == null)
                return null;

            int balance = GetBalance(node);

            if (balance > 1)
            {
                if (GetBalance(node.Left) < 0)
                    node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            if (balance < -1)
            {
                if (GetBalance(node.Right) > 0)
                    node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            return node;
        }

        private AVLNode RotateRight(AVLNode y)
        {
            AVLNode x = y.Left;
            AVLNode T2 = x.Right;

            x.Right = y;
            y.Left = T2;

            y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));
            x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));

            return x;
        }

        private AVLNode RotateLeft(AVLNode x)
        {
            AVLNode y = x.Right;
            AVLNode T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));
            y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));

            return y;
        }

        public void Clear()
        {
            _root = null;
            _count = 0;
        }

        public IEnumerable<DictionaryEntry> GetAll()
        {
            var results = new List<DictionaryEntry>();
            InOrder(_root, results);
            return results;
        }

        private void InOrder(AVLNode node, List<DictionaryEntry> results)
        {
            if (node == null)
                return;

            InOrder(node.Left, results);
            results.Add(node.Entry);
            InOrder(node.Right, results);
        }
    }
}
