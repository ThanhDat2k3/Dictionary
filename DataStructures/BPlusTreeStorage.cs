using Dictionary.Models;

namespace Dictionary.DataStructures
{
    public class BPlusTreeStorage : IStorageEngine
    {
        private const int M = 64;
        private Node _root;
        private int _count;
        private Node _firstLeaf;

        public int Count => _count;

        private class Node
        {
            public bool IsLeaf;
            public int KeyCount;
            public string[] Keys;


            public Node[] Children;


            public DictionaryEntry[] Entries;
            public Node Next;

            public Node(bool isLeaf)
            {
                IsLeaf = isLeaf;

                Keys = new string[M];

                if (isLeaf)
                {
                    Entries = new DictionaryEntry[M];
                }
                else
                {
                    Children = new Node[M + 1];
                }
            }
        }

        public BPlusTreeStorage()
        {
            Clear();
        }

        public void Clear()
        {
            _root = new Node(true);
            _firstLeaf = _root;
            _count = 0;
        }


        public DictionaryEntry Search(string word)
        {
            if (string.IsNullOrEmpty(word)) return null;


            string key = word.ToLower();
            Node leaf = FindLeaf(_root, key);

            int pos = BinarySearchExact(leaf, key);
            if (pos >= 0) return leaf.Entries[pos];

            return null;
        }

        private Node FindLeaf(Node node, string key)
        {
            while (!node.IsLeaf)
            {

                int left = 0, right = node.KeyCount - 1;
                int i = 0;

                while (left <= right)
                {
                    int mid = left + (right - left) / 2;
                    int cmp = string.Compare(key, node.Keys[mid], StringComparison.Ordinal);
                    if (cmp >= 0)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }
                i = left;
                node = node.Children[i];
            }
            return node;
        }

        private int BinarySearchExact(Node leaf, string key)
        {
            int left = 0, right = leaf.KeyCount - 1;
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                int cmp = string.Compare(key, leaf.Keys[mid], StringComparison.Ordinal);

                if (cmp == 0) return mid;
                if (cmp < 0) right = mid - 1;
                else left = mid + 1;
            }
            return -1;
        }

        public List<DictionaryEntry> SearchPrefix(string prefix)
        {
            var results = new List<DictionaryEntry>();
            if (string.IsNullOrEmpty(prefix)) return results;

            string lowerPrefix = prefix.ToLower();
            int prefixLen = lowerPrefix.Length;


            Node current = FindLeaf(_root, lowerPrefix);
            if (current == null) return results;


            while (current != null)
            {
                bool foundAny = false;

                for (int i = 0; i < current.KeyCount; i++)
                {
                    string key = current.Keys[i];


                    if (key.Length >= prefixLen &&
                        string.Compare(key, 0, lowerPrefix, 0, prefixLen, StringComparison.Ordinal) == 0)
                    {
                        results.Add(current.Entries[i]);
                        foundAny = true;
                    }
                    else if (foundAny)
                    {

                        return results;
                    }
                }

                current = current.Next;
            }
            return results;
        }
        public void Insert(DictionaryEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.Word)) return;
            string key = entry.Word.ToLower();

            bool isNew = false;
            Node newNode;
            string upKey;

            InsertInternal(_root, key, entry, out upKey, out newNode, ref isNew);

            if (newNode != null)
            {
                Node newRoot = new Node(false);
                newRoot.Keys[0] = upKey;
                newRoot.Children[0] = _root;
                newRoot.Children[1] = newNode;
                newRoot.KeyCount = 1;
                _root = newRoot;
            }

            if (isNew) _count++;
        }

        private void InsertInternal(Node node, string key, DictionaryEntry entry, out string upKey, out Node newNode, ref bool isNew)
        {
            upKey = null;
            newNode = null;

            if (node.IsLeaf)
            {
                int pos = 0;
                while (pos < node.KeyCount && string.Compare(key, node.Keys[pos], StringComparison.Ordinal) > 0)
                    pos++;


                if (pos < node.KeyCount && node.Keys[pos] == key)
                {
                    node.Entries[pos] = entry;
                    return;
                }

                isNew = true;

                for (int i = node.KeyCount; i > pos; i--)
                {
                    node.Keys[i] = node.Keys[i - 1];
                    node.Entries[i] = node.Entries[i - 1];
                }

                node.Keys[pos] = key;
                node.Entries[pos] = entry;
                node.KeyCount++;


                if (node.KeyCount == M)
                    SplitLeaf(node, out upKey, out newNode);
            }
            else
            {
                int pos = 0;
                while (pos < node.KeyCount && string.Compare(key, node.Keys[pos], StringComparison.Ordinal) >= 0)
                    pos++;

                string childUpKey;
                Node childNewNode;
                InsertInternal(node.Children[pos], key, entry, out childUpKey, out childNewNode, ref isNew);


                if (childNewNode != null)
                {
                    int i = node.KeyCount;
                    while (i > pos)
                    {
                        node.Keys[i] = node.Keys[i - 1];
                        node.Children[i + 1] = node.Children[i];
                        i--;
                    }
                    node.Keys[pos] = childUpKey;
                    node.Children[pos + 1] = childNewNode;
                    node.KeyCount++;

                    // Nếu nút nội bộ đầy, tiến hành Split
                    if (node.KeyCount == M)
                        SplitInternal(node, out upKey, out newNode);
                }
            }
        }

        private void SplitLeaf(Node leaf, out string upKey, out Node newLeaf)
        {
            newLeaf = new Node(true);
            int mid = M / 2;

            newLeaf.KeyCount = M - mid;
            for (int i = 0; i < newLeaf.KeyCount; i++)
            {
                newLeaf.Keys[i] = leaf.Keys[mid + i];
                newLeaf.Entries[i] = leaf.Entries[mid + i];

                leaf.Keys[mid + i] = null; // Xóa tham chiếu
                leaf.Entries[mid + i] = null;
            }
            leaf.KeyCount = mid;

            // Nối danh sách liên kết
            newLeaf.Next = leaf.Next;
            leaf.Next = newLeaf;

            upKey = newLeaf.Keys[0];
        }

        private void SplitInternal(Node node, out string upKey, out Node newNode)
        {
            newNode = new Node(false);
            int mid = M / 2;
            upKey = node.Keys[mid];

            newNode.KeyCount = M - mid - 1;
            for (int i = 0; i < newNode.KeyCount; i++)
            {
                newNode.Keys[i] = node.Keys[mid + 1 + i];
                newNode.Children[i] = node.Children[mid + 1 + i];
            }
            newNode.Children[newNode.KeyCount] = node.Children[M];

            node.KeyCount = mid;
        }

        public void Delete(string word)
        {
            if (string.IsNullOrEmpty(word)) return;
            string key = word.ToLower();
            Node leaf = FindLeaf(_root, key);

            int pos = BinarySearchExact(leaf, key);
            if (pos >= 0)
            {
                for (int i = pos; i < leaf.KeyCount - 1; i++)
                {
                    leaf.Keys[i] = leaf.Keys[i + 1];
                    leaf.Entries[i] = leaf.Entries[i + 1];
                }

                leaf.Keys[leaf.KeyCount - 1] = null;
                leaf.Entries[leaf.KeyCount - 1] = null;
                leaf.KeyCount--;
                _count--;
            }
        }

        // ==========================================
        // 5. LẤY TOÀN BỘ DỮ LIỆU (O(n))
        // ==========================================
        public IEnumerable<DictionaryEntry> GetAll()
        {
            Node current = _firstLeaf;
            while (current != null)
            {
                for (int i = 0; i < current.KeyCount; i++)
                {
                    yield return current.Entries[i];
                }
                current = current.Next;
            }
        }
    }
}