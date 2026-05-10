using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dictionary.DataStructures;
using Dictionary.Models;
using Dictionary.Views;
using Dictionary.Services;
using Microsoft.Win32;

namespace Dictionary.UI
{
    public partial class MainWindow : Window
    {
        private IStorageEngine _currentEngine;
        private BPlusTreeStorage _bplusTreeEngine;
        private BSTStorage _bstEngine;
        private HashTableStorage _hashTableEngine;
        private List<SearchHistory> _searchHistory;
        private Stopwatch _inputTimer;
        private Stopwatch _searchTimer;

        public MainWindow()
        {
            InitializeComponent();
            _searchHistory = new List<SearchHistory>();
            InitializeStorageEngine();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSavedData();
            LoadAllDictionaries();
        }

        private void LoadAllDictionaries()
        {
            _inputTimer = Stopwatch.StartNew();
            try
            {
                string indexPath = @"C:\Users\ASUS\Documents\CTDL&GT\AnhViet\stardict_en_vi\en_vi.idx";
                string dataPath = indexPath.Replace(".idx", ".dict.dz");

                var reader = new StarDictReader(indexPath, dataPath);
                var entries = reader.ReadDictionary();

                // Load vào cả 3 cấu trúc dữ liệu
                _bplusTreeEngine = new BPlusTreeStorage();
                _bstEngine = new BSTStorage();
                _hashTableEngine = new HashTableStorage();

                foreach (var entry in entries)
                {
                    _bplusTreeEngine.Insert(entry);
                    _bstEngine.Insert(entry);
                    _hashTableEngine.Insert(entry);
                }

                _inputTimer.Stop();
                ShowNotification($"📥 Đã load từ điển thành công!\n" +
                    $"Số từ: {entries.Count()}\n" +
                    $"Thời gian load: {_inputTimer.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _inputTimer.Stop();
                ShowNotification($"⚠️ Lỗi load từ điển: {ex.Message}\n" +
                    "Ứng dụng sẽ hoạt động với từ điển trống.");
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData();
        }

        private void LoadSavedData()
        {
            _inputTimer = Stopwatch.StartNew();
            try
            {
                var persistenceService = App.PersistenceService;
                if (persistenceService.HasSavedData())
                {
                    var entries = persistenceService.LoadData();
                    foreach (var entry in entries)
                    {
                        _currentEngine.Insert(entry);
                    }
                }
                _inputTimer.Stop();
            }
            catch (Exception ex)
            {
                _inputTimer.Stop();
                ShowNotification($"❌ Lỗi tải dữ liệu lưu trữ: {ex.Message}");
            }
        }

        public void SaveData()
        {
            try
            {
                var persistenceService = App.PersistenceService;
                var entries = _currentEngine.GetAll();
                persistenceService.SaveData(entries);
            }
            catch (Exception ex)
            {
                ShowNotification($"❌ Lỗi lưu dữ liệu: {ex.Message}");
            }
        }

        private void InitializeStorageEngine()
        {
            _currentEngine = new BPlusTreeStorage();
            _bplusTreeEngine = new BPlusTreeStorage();
            _bstEngine = new BSTStorage();
            _hashTableEngine = new HashTableStorage();

            cbStorageEngine.SelectionChanged -= CbStorageEngine_SelectionChanged;
            cbStorageEngine.SelectedIndex = 0;
            cbStorageEngine.SelectionChanged += CbStorageEngine_SelectionChanged;
        }

        private void CbStorageEngine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbStorageEngine.SelectedIndex < 0)
                return;

            IStorageEngine newEngine = null;
            switch (cbStorageEngine.SelectedIndex)
            {
                case 0:
                    newEngine = new BPlusTreeStorage();
                    break;
                case 1:
                    newEngine = new BSTStorage();
                    break;
                case 2:
                    newEngine = new HashTableStorage();
                    break;
            }

            if (newEngine != null)
            {
                _inputTimer = Stopwatch.StartNew();

                // Get all data from current engine before clearing
                var dataToTransfer = _currentEngine.GetAll().ToList();

                // Clear the old engine
                _currentEngine.Clear();

                // Insert all data to new engine
                foreach (var entry in dataToTransfer)
                {
                    newEngine.Insert(entry);
                }

                _inputTimer.Stop();
                _currentEngine = newEngine;

                ShowNotification($"Chuyển đổi cấu trúc dữ liệu thành công!\nThời gian: {_inputTimer.ElapsedMilliseconds}ms");
            }
        }

        private void LoadDictionary(string filePath = null, int limit = 0)
        {
            _inputTimer = Stopwatch.StartNew();

            try
            {
                string indexPath = filePath ?? @"C:\Users\ASUS\Documents\CTDL&GT\AnhViet\stardict_en_vi\en_vi.idx";
                string dataPath = indexPath.Replace(".idx", ".dict.dz");

                var reader = new StarDictReader(indexPath, dataPath);
                var entries = reader.ReadDictionary();

                int importCount = 0;
                foreach (var entry in entries)
                {
                    if (limit > 0 && importCount >= limit)
                        break;

                    _currentEngine.Insert(entry);
                    importCount++;
                }
                _currentEngine.Search("apple");
                _currentEngine.SearchPrefix("app");
                _inputTimer.Stop();
                ShowNotification($"📥 Đã nhập {importCount} từ từ điển\nThời gian nhập: {_inputTimer.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _inputTimer.Stop();
                ShowNotification($"❌ Lỗi: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                e.Handled = true;
                string searchTerm = txtSearch.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm))
                {
                    ClearDetails();
                    lstSuggestions.ItemsSource = null;
                    lblSearchTime.Text = "⏱️ 0ms";
                    return;
                }

                if (rbExactSearch.IsChecked == true)
                {
                    // Chế độ tìm chính xác
                    PerformExactSearch(searchTerm);
                }
                else
                {
                    // Chế độ tìm tiền tố
                    PerformPrefixSearch(searchTerm);
                }
            }
        }

        private void PerformPrefixSearch(string searchTerm)
        {
            _searchTimer = Stopwatch.StartNew();
            var suggestions = _currentEngine.SearchPrefix(searchTerm).ToList();
            _searchTimer.Stop();

            double elapsedMs = _searchTimer.Elapsed.TotalMilliseconds;

            // Đo thời gian trên 3 cấu trúc
            double bplusTreeTime = MeasureSearchTime(() => _bplusTreeEngine.SearchPrefix(searchTerm));
            double bstTime = MeasureSearchTime(() => _bstEngine.SearchPrefix(searchTerm));
            double hashTableTime = MeasureSearchTime(() => _hashTableEngine.SearchPrefix(searchTerm));

            lblSearchTime.Text = $"⏱️ B+Tree: {bplusTreeTime:F3}ms | BST: {bstTime:F3}ms | Hash: {hashTableTime:F3}ms";

            // Lấy toàn bộ dữ liệu bắt đầu bằng tiền tố
            var viewItems = suggestions.Select(s => new WordItemView(s)).ToList();
            lstSuggestions.ItemsSource = viewItems;

            if (viewItems.Count > 0)
            {
                lstSuggestions.SelectedIndex = 0;
                DisplayWordDetails(viewItems[0].Source);
                AddToHistory(viewItems[0].Source.Word, _searchTimer.Elapsed, "Tiền tố", bplusTreeTime, bstTime, hashTableTime);
                ShowNotification($"✅ Tìm thấy {viewItems.Count} từ bắt đầu bằng \"{searchTerm}\"");
            }
            else
            {
                ClearDetails();
                lstSuggestions.ItemsSource = null;
                ShowNotification($"❌ Không tìm thấy từ nào bắt đầu bằng \"{searchTerm}\"");
            }
        }

        private void PerformExactSearch(string searchTerm)
        {
            _searchTimer = Stopwatch.StartNew();
            var result = _currentEngine.Search(searchTerm);
            _searchTimer.Stop();

            double elapsedMs = _searchTimer.Elapsed.TotalMilliseconds;

            // Đo thời gian trên 3 cấu trúc
            double bplusTreeTime = MeasureSearchTime(() => _bplusTreeEngine.Search(searchTerm));
            double bstTime = MeasureSearchTime(() => _bstEngine.Search(searchTerm));
            double hashTableTime = MeasureSearchTime(() => _hashTableEngine.Search(searchTerm));

            lblSearchTime.Text = $"⏱️ B+Tree: {bplusTreeTime:F3}ms | BST: {bstTime:F3}ms | Hash: {hashTableTime:F3}ms";

            if (result != null)
            {
                DisplayWordDetails(result);
                lstSuggestions.ItemsSource = null;
                AddToHistory(result.Word, _searchTimer.Elapsed, "Chính xác", bplusTreeTime, bstTime, hashTableTime);
                ShowNotification($"✅ Tìm thấy từ: {result.Word}\nThời gian: {elapsedMs:F3}ms");
            }
            else
            {
                ClearDetails();
                lstSuggestions.ItemsSource = null;
                ShowNotification($"❌ Không tìm thấy từ \"{searchTerm}\"");
            }
        }

        private double MeasureSearchTime(Func<object> searchFunc)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                searchFunc();
            }
            catch { }
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        private void RbSearchMode_Changed(object sender, RoutedEventArgs e)
        {
            if (txtSearch == null || lblSearchTime == null || lstSuggestions == null || rbExactSearch == null)
                return;

            txtSearch.Clear();
            lblSearchTime.Text = "⏱️ 0ms";
            lstSuggestions.ItemsSource = null;
            ClearDetails();

            if (rbExactSearch.IsChecked == true)
            {
                txtSearch.Tag = "Nhập từ chính xác và nhấn Enter...";
            }
            else
            {
                txtSearch.Tag = "Nhập tiền tố từ và nhấn Enter...";
            }
        }

        private void LstSuggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstSuggestions.SelectedItem is WordItemView item)
            {
                DisplayWordDetails(item.Source);
            }
        }

        private void DisplayWordDetails(DictionaryEntry entry)
        {
            lblWord.Text = entry.Word;
            icMeanings.ItemsSource = entry.Meanings;
        }

        private void ClearDetails()
        {
            lblWord.Text = "Từ vựng";
            icMeanings.ItemsSource = null;
        }

        private void AddToHistory(string word, TimeSpan searchTime, string searchType, double bplusTreeTime, double bstTime, double hashTableTime)
        {
            _searchHistory.Add(new SearchHistory
            {
                Word = word,
                Time = DateTime.Now,
                SearchTime = searchTime,
                SearchType = searchType,
                BPlusTreeTimeMs = bplusTreeTime,
                BSTTimeMs = bstTime,
                HashTableTimeMs = hashTableTime
            });

            UpdateHistoryView();
        }

        private void UpdateHistoryView()
        {
            lvHistory.ItemsSource = _searchHistory.OrderByDescending(x => x.Time).ToList();
        }

        private void NavTraCuu_Click(object sender, RoutedEventArgs e)
        {
            GridTraCuu.Visibility = Visibility.Visible;
            GridQuanLy.Visibility = Visibility.Collapsed;
            GridLichSu.Visibility = Visibility.Collapsed;
            txtSearch.Focus();
        }

        private void NavQuanLy_Click(object sender, RoutedEventArgs e)
        {
            GridTraCuu.Visibility = Visibility.Collapsed;
            GridQuanLy.Visibility = Visibility.Visible;
            GridLichSu.Visibility = Visibility.Collapsed;
            RefreshDataGrid();
        }

        private void NavLichSu_Click(object sender, RoutedEventArgs e)
        {
            GridTraCuu.Visibility = Visibility.Collapsed;
            GridQuanLy.Visibility = Visibility.Collapsed;
            GridLichSu.Visibility = Visibility.Visible;
            UpdateHistoryView();
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Index files (*.idx)|*.idx|All files (*.*)|*.*";
            dialog.InitialDirectory = @"C:\Users\ASUS\Documents\CTDL&GT\AnhViet\stardict_en_vi";

            if (dialog.ShowDialog() == true)
            {
                var importWindow = new ImportLimitWindow();
                if (importWindow.ShowDialog() == true)
                {
                    _inputTimer = Stopwatch.StartNew();
                    LoadDictionary(dialog.FileName, importWindow.LimitCount);
                }
            }
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            // Xác nhận trước khi xóa
            MessageBoxResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tất cả {_currentEngine.Count} từ?\n\n" +
                "⚠️ CẢNH BÁO: Hành động này không thể hoàn tác!",
                "Xóa tất cả dữ liệu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _inputTimer = Stopwatch.StartNew();
                    int countBefore = _currentEngine.Count;

                    _currentEngine.Clear();

                    _inputTimer.Stop();

                    RefreshDataGrid();
                    ClearDetails();
                    txtSearch.Clear();
                    lstSuggestions.ItemsSource = null;

                    ShowNotification(
                        $"✅ Xóa thành công!\n" +
                        $"Đã xóa {countBefore} từ\n" +
                        $"Thời gian: {_inputTimer.ElapsedMilliseconds}ms"
                    );
                }
                catch (Exception ex)
                {
                    _inputTimer.Stop();
                    ShowNotification($"❌ Lỗi: {ex.Message}");
                }
            }
        }



        private void RefreshDataGrid()
        {
            var dataList = _currentEngine.GetAll().ToList();
            dgDictionary.ItemsSource = dataList;
        }

        private void ShowNotification(string message)
        {
            MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}