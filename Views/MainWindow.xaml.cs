using Gradil.Services;
using Gradil.ViewModels;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Input;
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
            // Diagnostic: log and force-resolve the TitleBar background brush
            try
            {
                var res = TryFindResource("TitleBarBackgroundBrush");
                if (res is SolidColorBrush scb)
                {
                    Debug.WriteLine($"[Diag] TitleBarBackgroundBrush resolved to: {scb.Color}");
                    // Apply explicitly to ensure the Border shows the resolved brush
                    TitleBar.Background = scb;
                }
                else
                {
                    Debug.WriteLine($"[Diag] TitleBarBackgroundBrush not found or not a SolidColorBrush: {res?.GetType().FullName ?? "null"}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Diag] Error resolving TitleBarBackgroundBrush: " + ex);
            }

            // Also check again after loaded (resources may be merged at startup)
            Loaded += (s, e) =>
            {
                try
                {
                    var res2 = TryFindResource("TitleBarBackgroundBrush");
                    if (res2 is SolidColorBrush scb2)
                    {
                        Debug.WriteLine($"[Diag] (Loaded) TitleBarBackgroundBrush resolved to: {scb2.Color}");
                        TitleBar.Background = scb2;
                    }
                    else
                    {
                        Debug.WriteLine($"[Diag] (Loaded) TitleBarBackgroundBrush not found: {res2?.GetType().FullName ?? "null"}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[Diag] (Loaded) Error resolving TitleBarBackgroundBrush: " + ex);
                }
            };
            // Wire up minimal dependencies. PosterGenerator uses PdfSharpCore only.
            var generator = new PosterGenerator();
            DataContext = new MainViewModel(generator);
            // no preview: UI shows only file list and actions
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // no-op for now; keep for XAML hookup and future initialization
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If the click originated from one of the caption buttons (or their content),
            // don't start a window drag — let the button handle the input instead.
            var src = e.OriginalSource as DependencyObject;
            while (src != null)
            {
                if (src is System.Windows.Controls.Button) return;
                src = System.Windows.Media.VisualTreeHelper.GetParent(src);
            }

            if (e.ClickCount == 2)
            {
                // double-click toggles maximize/restore
                ToggleWindowState();
            }
            else
            {
                try { DragMove(); } catch { }
            }
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleWindowState()
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }
    }
}
