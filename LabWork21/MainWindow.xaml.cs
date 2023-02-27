using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace LabWork21
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void SelectFileButton_OnClickAsync(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();
            
            if (!result.HasValue || !result.Value)
                return;

            PathTextBox.Text = dialog.FileName;
        }

        private void AlgComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HashTextBlock is null)
                return;
            HashTextBlock.Text = "";
        }

        private async void CalculateButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(PathTextBox.Text))
                return;

            HashTextBlock.Text = AlgComboBox.SelectedIndex switch
            {
                0 => await GetMd5FileHashAsync(PathTextBox.Text),
                1 => await GetSha1FileHashAsync(PathTextBox.Text),
                _ => throw new ArgumentException()
            };
        }

        private async void Task3Button_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var directoryInfo = new DirectoryInfo(dialog.SelectedPath);

            if (!directoryInfo.Exists)
                return;
            
            var files = new List<object>();
            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                var hash = await GetSha1FileHashAsync(fileInfo.FullName);
                
                files.Add(new
                {
                    fileInfo.Name,
                    Hash = hash,
                    fileInfo.FullName,
                    fileInfo.Length,
                    fileInfo.LastWriteTime
                });
            }

            FilesListView.ItemsSource = files;
        }

        private async void Task4Button_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var directoryInfo = new DirectoryInfo(dialog.SelectedPath);

            if (!directoryInfo.Exists)
                return;
            
            var files = new List<dynamic>();
            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                var hash = await GetSha1FileHashAsync(fileInfo.FullName);
                
                files.Add(new
                {
                    fileInfo.Name,
                    fileInfo.FullName,
                    Hash = hash
                });
            }

            FilesListView.ItemsSource = files
                .GroupBy(file => file.Hash)
                .Where(grouping => grouping.Count() > 1)
                .SelectMany(grouping => grouping)
                .OrderBy(file => file.Hash)
                .Select(file => new
                {
                    file.Name,
                    file.FullName,
                    file.Hash
                })
                .ToArray();
        }
        
        private async Task<string> GetMd5FileHashAsync(string fileName)
        {
            var hash = MD5.Create();
            await using var fileStream = File.Open(fileName, FileMode.Open);
            var bytes = await hash.ComputeHashAsync(fileStream);
            return Convert.ToBase64String(bytes);
        }
        
        private async Task<string> GetSha1FileHashAsync(string fileName)
        {
            var hash = SHA1.Create();
            await using var fileStream = File.Open(fileName, FileMode.Open);
            var bytes = await hash.ComputeHashAsync(fileStream);
            return Convert.ToBase64String(bytes);
        }
    }
}