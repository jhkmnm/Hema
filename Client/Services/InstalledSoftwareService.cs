using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Client.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace Client.Services
{
    public class InstalledSoftwareService
    {
        private readonly string[] registryPaths = {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        private readonly AppConfigService _configService;

        public InstalledSoftwareService(AppConfigService configService)
        {
            _configService = configService;
        }

        public List<Software> GetInstalledSoftware()
        {
            var softwareList = new List<Software>();

            foreach (string registryPath in registryPaths)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        foreach (string subKeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey != null)
                                {
                                    try
                                    {
                                        string displayName = subKey.GetValue("DisplayName") as string;
                                        string displayVersion = subKey.GetValue("DisplayVersion") as string;
                                        string installLocation = subKey.GetValue("InstallLocation") as string;
                                        string uninstallString = subKey.GetValue("UninstallString") as string;
                                        string description = subKey.GetValue("Comments") as string;
                                        if (string.IsNullOrEmpty(description))
                                        {
                                            description = subKey.GetValue("DisplayDesc") as string;
                                        }

                                        if (!string.IsNullOrEmpty(displayName))
                                        {
                                            softwareList.Add(new Software
                                            {
                                                Name = displayName,
                                                Version = displayVersion ?? "未知版本",
                                                Description = description ?? "暂无描述",
                                                InstallPath = installLocation ?? "未知路径",
                                                UninstallString = uninstallString
                                            });
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // 忽略读取失败的项
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return softwareList;
        }

        public async Task<bool> UninstallSoftware(Software software)
        {
            if (string.IsNullOrEmpty(software.UninstallString))
            {
                throw new InvalidOperationException("找不到卸载命令");
            }

            try
            {
                // 处理卸载命令
                string uninstallCmd = software.UninstallString;
                if (uninstallCmd.StartsWith("\""))
                {
                    // 处理带引号的路径
                    var parts = uninstallCmd.Split('"', StringSplitOptions.RemoveEmptyEntries);
                    var exePath = parts[0];
                    var arguments = parts.Length > 1 ? parts[1].Trim() : "";
                    
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = arguments + " /quiet",  // 添加静默参数
                        UseShellExecute = true,
                        Verb = "runas"  // 请求管理员权限
                    };

                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        return process.ExitCode == 0;
                    }
                }
                else
                {
                    // 处理不带引号的命令
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = uninstallCmd,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        return process.ExitCode == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"卸载过程出错: {ex.Message}", ex);
            }

            return false;
        }

        public List<Software> CompareWithImported(List<Software> importedSoftware)
        {
            var installedSoftware = GetInstalledSoftware();
            var result = new List<Software>();

            // 添加未安装的软件（放在前面）
            foreach (var software in importedSoftware)
            {
                var isInstalled = installedSoftware.Any(s => 
                    s.Name.Equals(software.Name, StringComparison.OrdinalIgnoreCase));
                
                software.IsInstalled = isInstalled;
                if (!isInstalled)
                {
                    result.Add(software);
                }
            }

            // 添加已安装的软件
            foreach (var software in importedSoftware)
            {
                if (software.IsInstalled)
                {
                    result.Add(software);
                }
            }

            return result;
        }

        public async Task<bool> InstallSoftware(Software software)
        {
            var setupPath = Path.Combine(_configService.SetupFilesPath, software.SetupFileName);
            
            if (File.Exists(setupPath))
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = setupPath,
                        UseShellExecute = true,
                        Verb = "runas"  // 请求管理员权限
                    };

                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        return process.ExitCode == 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"安装过程出错: {ex.Message}", ex);
                }
            }
            else if (!string.IsNullOrEmpty(software.OfficialUrl))
            {
                // 如果没有安装文件但有官网地址，则打开浏览器
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = software.OfficialUrl,
                        UseShellExecute = true
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception($"打开浏览器失败: {ex.Message}", ex);
                }
            }
            else
            {
                throw new Exception("找不到安装文件且未提供官方网站地址");
            }

            return false;
        }
    }
} 
