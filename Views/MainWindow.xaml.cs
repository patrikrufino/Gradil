using Gradil.Services;
using Gradil.ViewModels;
using System.Windows;
using System.ComponentModel;
using System.IO;
using System;
using System.Threading.Tasks;
 

namespace Gradil.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Wire up minimal dependencies. PosterGenerator uses PdfSharpCore only.
            var generator = new PosterGenerator();
            DataContext = new MainViewModel(generator);
            // no preview: UI shows only file list and actions
        }
    }
}
