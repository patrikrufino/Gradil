using Gradil.Models;
using Gradil.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using System.Diagnostics;

namespace Gradil.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isBusy = false;
        private string _selectedFilePath;
        private int _rows = 1;
        private int _cols = 1;
        private string _statusMessage;
        private string _previewImagePath;
        private System.Collections.ObjectModel.ObservableCollection<string> _generatedFiles = new System.Collections.ObjectModel.ObservableCollection<string>();
        private string _selectedGeneratedFile;

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged(nameof(SelectedFilePath));
                // Notify commands that depend on SelectedFilePath
                (GenerateCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                // Update command availability when busy state changes
                (GenerateCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public int Rows
        {
            get => _rows;
            set { _rows = value; OnPropertyChanged(nameof(Rows)); }
        }

        public int Cols
        {
            get => _cols;
            set { _cols = value; OnPropertyChanged(nameof(Cols)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public string PreviewImagePath
        {
            get => _previewImagePath;
            set { _previewImagePath = value; OnPropertyChanged(nameof(PreviewImagePath)); }
        }

        public System.Collections.ObjectModel.ObservableCollection<string> GeneratedFiles
        {
            get => _generatedFiles;
            set { _generatedFiles = value; OnPropertyChanged(nameof(GeneratedFiles)); }
        }

        public string SelectedGeneratedFile
        {
            get => _selectedGeneratedFile;
            set
            {
                _selectedGeneratedFile = value;
                OnPropertyChanged(nameof(SelectedGeneratedFile));
                // Notify commands that depend on SelectedGeneratedFile
                (OpenSelectedFileCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand SelectFileCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand OpenFolderCommand { get; }

        private readonly PosterGenerator _posterGenerator;

        public MainViewModel(PosterGenerator posterGenerator)
        {
            _posterGenerator = posterGenerator ?? throw new ArgumentNullException(nameof(posterGenerator));
                // Ensure output directory exists and load existing generated files
                string outputDir = GetOutputDir();
                if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
                // Load existing PDFs (most recent creation date first)
                try
                {
                    var existing = Directory.GetFiles(outputDir, "*.pdf");
                    // order by creation time desc
                    Array.Sort(existing, (a, b) => File.GetCreationTimeUtc(b).CompareTo(File.GetCreationTimeUtc(a)));
                    foreach (var f in existing)
                    {
                        _generatedFiles.Add(f);
                    }
                }
                catch { /* ignore IO errors during load */ }

                SelectFileCommand = new RelayCommand(_ => SelectFile());
                GenerateCommand = new RelayCommand(async _ => await GenerateAsync(), _ => !string.IsNullOrWhiteSpace(SelectedFilePath) && !IsBusy);
                OpenFolderCommand = new RelayCommand(_ => OpenOutputFolder());
                OpenSelectedFileCommand = new RelayCommand(_ => OpenSelectedFile(), _ => !string.IsNullOrWhiteSpace(SelectedGeneratedFile));
        }

        public ICommand OpenSelectedFileCommand { get; }

        private void SelectFile()
        {
            var dlg = new OpenFileDialog { Filter = "PDF Files|*.pdf", Title = "Selecione um arquivo PDF" };
            if (dlg.ShowDialog() == true)
            {
                SelectedFilePath = dlg.FileName;
                StatusMessage = "Arquivo selecionado.";
            }
        }

        private async Task GenerateAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Gerando...";
                string outputDir = GetOutputDir();
                var options = new TiledPosterOptions
                {
                    SourceFile = SelectedFilePath,
                    Rows = Rows,
                    Cols = Cols,
                    OutputFolder = outputDir
                };

                string output = await _posterGenerator.GenerateAsync(options);

                StatusMessage = $"Gerado: {Path.GetFileName(output)}";

                // add to generated files list and select
                GeneratedFiles.Insert(0, output);
                SelectedGeneratedFile = output;

                // Open the generated file directly with the default associated application
                Process.Start(new ProcessStartInfo(output) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OpenSelectedFile()
        {
            if (string.IsNullOrWhiteSpace(SelectedGeneratedFile) || !File.Exists(SelectedGeneratedFile)) return;
            // Open the PDF file directly with the default associated application
            Process.Start(new ProcessStartInfo(SelectedGeneratedFile) { UseShellExecute = true });
        }

        

        private void OpenOutputFolder()
        {
            string outputDir = GetOutputDir();
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{outputDir}\"") { UseShellExecute = true });
        }

        private string GetOutputDir()
        {
            // Use per-user temp folder so files are created in the current user's profile
            // even if the executable is run from a network location. Note: temp folders
            // can be cleaned by the OS; if you need persistent user files use
            // Environment.SpecialFolder.LocalApplicationData or MyDocuments.
            string baseDir = System.IO.Path.GetTempPath();
            return Path.Combine(baseDir, "Gradil");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
