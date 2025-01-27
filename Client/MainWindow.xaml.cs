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
using System.Net.Http.Json;
using System.Net.Http;

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
        private PaginationRequest _paginationRequest = new() { PageSize = 10, PageIndex = 1 };
        private int _totalPages = 1;

        public MainWindow()
        {
            InitializeComponent();
            _configService = new AppConfigService();
            _installedSoftwareService = new InstalledSoftwareService(_configService);
            _searchService = new SoftwareSearchService();
            softwares = new ObservableCollection<Software>();
            _allSoftwares = new List<Software>();
            dataGrid.ItemsSource = softwares;

            // 初始化时加载数据
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 选中软件列表按钮
            SelectNavButton(btnSoftwareList);
            
            // 加载已安装软件信息
            var installedSoftware = _installedSoftwareService.GetInstalledSoftware();
            
            // 加载软件列表并比较版本
            await LoadSoftwareList(installedSoftware);
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

        private async void btnInstalledSoftware_Click(object sender, RoutedEventArgs e)
        {
            SelectNavButton(sender as Button);
            await LoadInstalledSoftwareList();
        }

        private async Task LoadInstalledSoftwareList()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // 获取已安装软件列表
                var installedSoftware = _installedSoftwareService.GetInstalledSoftware();

                // 获取软件库中的软件列表
                using var client = new HttpClient();
                var response = await client.GetAsync("http://localhost:5000/api/Application");
                List<Software> repositorySoftware = new();
                
                if (response.IsSuccessStatusCode)
                {
                    repositorySoftware = await response.Content.ReadFromJsonAsync<List<Software>>() ?? new();
                }

                // 更新已安装软件的信息
                foreach (var software in installedSoftware)
                {
                    var repoSoftware = repositorySoftware.FirstOrDefault(s => 
                        s.Name.Equals(software.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (repoSoftware != null)
                    {
                        software.ExistsInRepository = true;
                        software.Version = repoSoftware.Version;  // 软件库中的版本
                        software.SetupFileName = repoSoftware.SetupFileName;
                        software.OfficialUrl = repoSoftware.OfficialUrl;
                    }
                    else
                    {
                        software.ExistsInRepository = false;
                    }
                }

                foreach (var software in installedSoftware)
                {
                    software.IsInstalledList = true;  // 标记为已安装列表视图
                }

                UpdateSoftwareList(installedSoftware);
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

        private async void btnSoftwareList_Click(object sender, RoutedEventArgs e)
        {
            SelectNavButton(sender as Button);
            await LoadSoftwareList();
        }

        private async Task LoadSoftwareList(List<Software> installedSoftware = null)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                
                using var client = new HttpClient();
                var response = await client.GetAsync(
                    $"http://localhost:5000/api/Application/paged?pageSize={_paginationRequest.PageSize}&pageIndex={_paginationRequest.PageIndex}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PaginatedResult<Software>>();
                    if (result != null)
                    {
                        foreach (var software in result.Items)
                        {
                            software.IsInstalledList = false;  // 标记为软件列表视图
                            var installed = installedSoftware?.FirstOrDefault(s => 
                                s.Name.Equals(software.Name, StringComparison.OrdinalIgnoreCase));
                            
                            if (installed != null)
                            {
                                software.IsInstalled = true;
                                software.InstalledVersion = installed.Version;
                                software.InstallPath = installed.InstallPath;
                                software.UninstallString = installed.UninstallString;
                            }
                        }

                        UpdateSoftwareList(result.Items);
                        _totalPages = result.TotalPages;
                        UpdatePaginationControls();
                    }
                }
                else
                {
                    MessageBox.Show("获取软件列表失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载软件列表时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void UpdatePaginationControls()
        {
            txtCurrentPage.Text = _paginationRequest.PageIndex.ToString();
            txtTotalPages.Text = _totalPages.ToString();
            btnPrevPage.IsEnabled = _paginationRequest.PageIndex > 1;
            btnNextPage.IsEnabled = _paginationRequest.PageIndex < _totalPages;
        }

        private async void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_paginationRequest.PageIndex > 1)
            {
                _paginationRequest.PageIndex--;
                await LoadSoftwareList();
            }
        }

        private async void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_paginationRequest.PageIndex < _totalPages)
            {
                _paginationRequest.PageIndex++;
                await LoadSoftwareList();
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            SelectNavButton(sender as Button);
            // 其他处理逻辑
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var software = (Software)button.DataContext;

            try
            {
                if (!string.IsNullOrEmpty(software.InstallPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = software.InstallPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("无法找到软件安装路径", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动软件失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
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
                    software.InstalledVersion = software.Version;
                    MessageBox.Show("更新命令已执行。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                button.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }
    }
} 
