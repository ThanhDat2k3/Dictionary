using System;

namespace Dictionary.Models
{
    public static class DataStructureInfo
    {
        public enum StorageEngineType
        {
            BPlusTree = 0,
            AVLTree = 1,
            HashTable = 2
        }

        public static class Performance
        {
            public const int LARGE_DATASET_SIZE = 368000;

            public static readonly double[] AvgSearchTime = new double[]
            {
                2.5,
                3.0,
                1.0
            };

            public static readonly double[] AvgInsertTime = new double[]
            {
                2.8,
                3.2,
                1.5
            };

            public static readonly int[] MaxDepth = new int[]
            {
                4,
                19,
                1
            };
        }

        public static string GetInfo(StorageEngineType type)
        {
            return type switch
            {
                StorageEngineType.BPlusTree => "B+ Tree (Balanced Multi-way Tree)\n" +
                    "- Tốt cho tìm kiếm theo khoảng và tiền tố\n" +
                    "- Độ sâu: O(log_k n) - đủ cân bằng\n" +
                    "- Thích hợp cho 368000+ từ\n" +
                    "- Performance: Ổn định",

                StorageEngineType.AVLTree => "AVL Tree (Self-Balancing BST)\n" +
                    "- Tự động cân bằng sau mỗi insert/delete\n" +
                    "- Độ sâu: O(log n) - Luôn cân bằng\n" +
                    "- Tốn O(log n) cho rebalancing\n" +
                    "- Thích hợp cho 368000+ từ (Cải tiến)\n" +
                    "- Performance: Tốt",

                StorageEngineType.HashTable => "Hash Table (Bảng băm)\n" +
                    "- Tìm kiếm chính xác: O(1) - Cực nhanh\n" +
                    "- Prefix search: O(n) - Chậm\n" +
                    "- Memory overhead: ~30%\n" +
                    "- Performance: Cực tốt cho tìm kiếm chính xác",

                _ => "Unknown"
            };
        }

        public static string GetRecommendation()
        {
            return "Với 368000 từ:\n" +
                "1. Tìm kiếm chính xác → Hash Table (Nhanh nhất)\n" +
                "2. Tìm kiếm tiền tố → B+ Tree (Cân bằng tốt)\n" +
                "3. Thử nghiệm → AVL Tree (Cân bằng động)";
        }
    }
}
