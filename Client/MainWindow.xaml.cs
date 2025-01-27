using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using Client.Models;
using Client.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client
{
    public partial class MainWindow : Window
    {
        private readonly InstalledSoftwareService _installedSoftwareService;
        private readonly SoftwareSearchService _searchService;
        private readonly AppConfigService _configService;
        private ObservableCollection<Software> softwares;
        private List<Software> _allSoftwares;
        private Button _currentSelectedButton;

        public MainWindow()
        {
            InitializeComponent();
            _configService = new AppConfigService();
            _installedSoftwareService = new InstalledSoftwareService(_configService);
            _searchService = new SoftwareSearchService();
            softwares = new ObservableCollection<Software>();
            _allSoftwares = new List<Software>();
            dataGrid.ItemsSource = softwares;
        }

        private void SelectNavButton(Button button)
        {
            if (_currentSelectedButton != null)
            {
                _currentSelectedButton.Background = Brushes.Transparent;
                _currentSelectedButton.Foreground = (Brush)new BrushConverter().ConvertFrom("#666666");
            }

            _currentSelectedButton = button;
            _currentSelectedButton.Background = (Brush)new BrushConverter().ConvertFrom("#E3F2FD");
            _currentSelectedButton.Foreground = (Brush)new BrushConverter().ConvertFrom("#2B579A");
        }

        private void btnDataManagement_Click(object sender, RoutedEventArgs e)
        {
            SelectNavButton(sender as Button);
            // 数据管理按钮点击事件
        }

        private void btnLoadData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = File.ReadAllText(openFileDialog.FileName);
                    var loadedSoftwares = JsonSerializer.Deserialize<List<Software>>(jsonString);
                    var comparedList = _installedSoftwareService.CompareWithImported(loadedSoftwares);
                    
                    UpdateSoftwareList(comparedList);
                    MessageBox.Show("数据导入成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入数据时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSaveData_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = JsonSerializer.Serialize(softwares);
                    File.WriteAllText(saveFileDialog.FileName, jsonString);
                    MessageBox.Show("数据导出成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出数据时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnInstalledSoftware_Click(object sender, RoutedEventArgs e)
        {
            SelectNavButton(sender as Button);
            
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _allSoftwares = _installedSoftwareService.GetInstalledSoftware();
                _searchService.UpdateSearchIndex(_allSoftwares);
                UpdateSoftwareList(_allSoftwares);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载已安装软件列表时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var software = (Software)button.DataContext;

            var result = MessageBox.Show(
                $"确定要卸载 {software.Name} 吗？",
                "确认卸载",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    button.IsEnabled = false;

                    var success = await _installedSoftwareService.UninstallSoftware(software);
                    
                    if (success)
                    {
                        softwares.Remove(software);
                        MessageBox.Show("卸载命令已执行完成。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("卸载过程可能未完全成功，请检查软件状态。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"卸载失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    button.IsEnabled = true;
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var software = (Software)button.DataContext;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                button.IsEnabled = false;

                var success = await _installedSoftwareService.InstallSoftware(software);
                
                if (success)
                {
                    software.IsInstalled = true;
                    MessageBox.Show("安装命令已执行。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"安装失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                button.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text;
            var searchResults = _searchService.Search(searchText);
            UpdateSoftwareList(searchResults);
        }

        private void UpdateSoftwareList(IEnumerable<Software> softwareList)
        {
            softwares.Clear();
            foreach (var software in softwareList)
            {
                softwares.Add(software);
            }
        }

        private void btnSoftwareList_Click(object sender, RoutedEventArgs e)
        {
            SelectNavButton(sender as Button);
            // 其他处理逻辑
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            SelectNavButton(sender as Button);
            // 其他处理逻辑
        }
    }
} 
