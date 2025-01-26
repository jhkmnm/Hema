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
    }
} 
