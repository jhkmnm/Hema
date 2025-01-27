using System;

namespace Client.Models
{
    public class Software
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string InstallPath { get; set; }
        public string UninstallString { get; set; }  // 用于存储卸载命令
        public bool IsInstalled { get; set; }  // 是否已安装
        public string SetupFileName { get; set; }  // 安装文件名
        public string OfficialUrl { get; set; }  // 官方网站

        // 已安装的版本号
        public string InstalledVersion { get; set; }

        // 是否在软件库中存在
        public bool ExistsInRepository { get; set; }

        // 软件列表的按钮显示控制属性
        public bool ShowInstallButton => !IsInstalled && !IsInstalledList;
        public bool ShowOpenButton => IsInstalled && !IsInstalledList;
        public bool ShowUpdateButton => IsInstalled && !IsInstalledList && 
                                      CompareVersions(InstalledVersion, Version) < 0;

        // 已安装列表的按钮显示控制属性
        public bool ShowUninstallButton => IsInstalled && IsInstalledList;
        public bool ShowInstalledUpdateButton => IsInstalled && IsInstalledList && 
                                               ExistsInRepository && 
                                               CompareVersions(InstalledVersion, Version) < 0;

        // 标记当前是否在已安装列表视图
        public bool IsInstalledList { get; set; }

        // 版本号比较方法
        private int CompareVersions(string version1, string version2)
        {
            if (string.IsNullOrEmpty(version1)) return -1;
            if (string.IsNullOrEmpty(version2)) return 1;

            var v1Parts = version1.Split('.');
            var v2Parts = version2.Split('.');

            int length = Math.Max(v1Parts.Length, v2Parts.Length);

            for (int i = 0; i < length; i++)
            {
                int v1 = i < v1Parts.Length ? int.Parse(v1Parts[i]) : 0;
                int v2 = i < v2Parts.Length ? int.Parse(v2Parts[i]) : 0;

                if (v1 < v2) return -1;
                if (v1 > v2) return 1;
            }

            return 0;
        }
    }
} 
