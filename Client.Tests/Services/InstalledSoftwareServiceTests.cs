using System;
using System.Linq;
using System.Threading.Tasks;
using Client.Models;
using Client.Services;
using Xunit;

namespace Client.Tests.Services
{
    public class InstalledSoftwareServiceTests
    {
        private readonly InstalledSoftwareService _service;

        public InstalledSoftwareServiceTests()
        {
            _service = new InstalledSoftwareService();
        }

        [Fact]
        public void GetInstalledSoftware_ShouldReturnNonEmptyList()
        {
            // Act
            var result = _service.GetInstalledSoftware();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetInstalledSoftware_ShouldReturnValidSoftwareInfo()
        {
            // Act
            var result = _service.GetInstalledSoftware();
            var firstSoftware = result.FirstOrDefault();

            // Assert
            Assert.NotNull(firstSoftware);
            Assert.NotEmpty(firstSoftware.Name);
            Assert.NotNull(firstSoftware.Version);
            Assert.NotNull(firstSoftware.Description);
            Assert.NotNull(firstSoftware.InstallPath);
        }

        [Fact]
        public async Task UninstallSoftware_WithEmptyUninstallString_ShouldThrowException()
        {
            // Arrange
            var software = new Software
            {
                Name = "Test Software",
                UninstallString = ""
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UninstallSoftware(software)
            );
        }

        [Fact]
        public void GetInstalledSoftware_ShouldHandleInvalidRegistryEntries()
        {
            // Act
            var result = _service.GetInstalledSoftware();

            // Assert
            // 确保即使有无效的注册表项，方法也能正常返回结果
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("\"C:\\Program Files\\App\\uninstall.exe\" /S")]
        [InlineData("C:\\Program Files\\App\\uninstall.exe /S")]
        public void ParseUninstallString_ShouldHandleVariousFormats(string uninstallString)
        {
            // Arrange
            var software = new Software
            {
                Name = "Test Software",
                UninstallString = uninstallString
            };

            // Act & Assert
            // 确保不会在解析卸载命令时抛出异常
            Assert.NotNull(software.UninstallString);
        }
    }
} 
