using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Dictionary.Models;

namespace Dictionary.Services
{
    public class StarDictReader
    {
        private string _indexPath;
        private string _dataPath;

        public StarDictReader(string indexPath, string dataPath)
        {
            _indexPath = indexPath;
            _dataPath = dataPath;
        }

        public List<DictionaryEntry> ReadDictionary()
        {
            var entries = new List<DictionaryEntry>();

            try
            {
                if (!File.Exists(_indexPath))
                {
                    System.Windows.MessageBox.Show($"Không tìm thấy file idx: {_indexPath}");
                    return entries;
                }

                if (!File.Exists(_dataPath))
                {
                    System.Windows.MessageBox.Show($"Không tìm thấy file dict.dz: {_dataPath}");
                    return entries;
                }

                // Đọc file idx
                var indexEntries = ReadIndexFile(_indexPath);
                var dataBytes = ReadDataFile(_dataPath);

                // Parse dữ liệu từ index
                foreach (var indexEntry in indexEntries)
                {
                    try
                    {
                        string definition = ExtractDefinition(dataBytes, indexEntry.DataOffset, indexEntry.DataSize);
                        var entry = new DictionaryEntry(indexEntry.Word, definition);
                        entries.Add(entry);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi đọc từ điển: {ex.Message}");
            }

            return entries;
        }

        private struct IndexEntry
        {
            public string Word;
            public long DataOffset;
            public int DataSize;
        }

        private List<IndexEntry> ReadIndexFile(string path)
        {
            var entries = new List<IndexEntry>();

            try
            {
                byte[] indexData = File.ReadAllBytes(path);
                int offset = 0;

                while (offset < indexData.Length)
                {
                    // Tìm null terminator (end of word)
                    int nullIndex = Array.IndexOf(indexData, (byte)0, offset);
                    if (nullIndex == -1 || nullIndex - offset > 255)
                        break;

                    string word = Encoding.UTF8.GetString(indexData, offset, nullIndex - offset).ToLower();
                    offset = nullIndex + 1;

                    // Đọc offset (4 bytes, big-endian)
                    if (offset + 4 > indexData.Length) break;
                    long dataOffset = BitConverter.ToUInt32(ReverseBytes(indexData, offset, 4), 0);
                    offset += 4;

                    // Đọc size (4 bytes, big-endian)
                    if (offset + 4 > indexData.Length) break;
                    int dataSize = BitConverter.ToInt32(ReverseBytes(indexData, offset, 4), 0);
                    offset += 4;

                    entries.Add(new IndexEntry { Word = word, DataOffset = dataOffset, DataSize = dataSize });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi đọc index file: {ex.Message}");
            }

            return entries;
        }

        private byte[] ReadDataFile(string path)
        {
            try
            {
                if (path.EndsWith(".dz", StringComparison.OrdinalIgnoreCase))
                {
                    using (var fileStream = File.OpenRead(path))
                    using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                    {
                        using (var ms = new MemoryStream())
                        {
                            gzipStream.CopyTo(ms);
                            return ms.ToArray();
                        }
                    }
                }
                else
                {
                    return File.ReadAllBytes(path);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi đọc data file: {ex.Message}");
                return new byte[0];
            }
        }

        private string ExtractDefinition(byte[] data, long offset, int size)
        {
            if (offset < 0 || offset + size > data.Length)
                return string.Empty;

            return Encoding.UTF8.GetString(data, (int)offset, size);
        }

        private byte[] ReverseBytes(byte[] data, int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, offset, result, 0, length);
            Array.Reverse(result);
            return result;
        }
    }
}
