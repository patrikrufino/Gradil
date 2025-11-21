using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Linq;

// Note: reading Windows accent color from registry; we try a few likely keys.
// This isn't guaranteed across all Windows versions but works for typical desktop setups.

namespace Gradil;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		// Apply app theme first, then overwrite AccentBrush with system accent color.
		ApplyAccentColor();
		ApplyWindowsTheme();

		// Listen to user preference changes (includes theme changes) and re-apply
		SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
	}

	protected override void OnExit(ExitEventArgs e)
	{
		base.OnExit(e);
		try { SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged; } catch { }
	}

	private void SystemEvents_UserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
	{
		// Re-apply on UI thread
		Dispatcher.BeginInvoke((Action)(() =>
		{
			ApplyAccentColor();
			ApplyWindowsTheme();
		}), DispatcherPriority.ApplicationIdle);
	}

	private void ApplyAccentColor()
	{
		try
		{
			Color accent = GetWindowsAccentColor();
			var brush = new SolidColorBrush(accent);
			brush.Freeze();

			// Decide foreground (black or white) based on perceived luminance for contrast
			Color fgColor = GetContrastingForeground(accent);
			var fgBrush = new SolidColorBrush(fgColor);
			fgBrush.Freeze();

			if (Current != null)
			{
				if (Current.Resources.Contains("AccentBrush"))
					Current.Resources["AccentBrush"] = brush;
				else
					Current.Resources.Add("AccentBrush", brush);

				if (Current.Resources.Contains("AccentForegroundBrush"))
					Current.Resources["AccentForegroundBrush"] = fgBrush;
				else
					Current.Resources.Add("AccentForegroundBrush", fgBrush);

				System.Diagnostics.Debug.WriteLine($"[Diag] Applied AccentBrush: {accent}, AccentForeground: {fgColor}");
			}
		}
		catch
		{
			// ignore - Theme dictionaries provide a fallback AccentBrush
		}
	}

	private Color GetContrastingForeground(Color background)
	{
		// Perceived luminance (rec. 709) - range 0..1
		double l = (0.2126 * background.R + 0.7152 * background.G + 0.0722 * background.B) / 255.0;
		// If luminance is low (dark background) use white, otherwise black
		return l < 0.5 ? Colors.White : Colors.Black;
	}

	private void ApplyWindowsTheme()
	{
		bool isLight = IsLightThemeEnabled();

		// Theme dictionary sources
		var lightUri = new Uri("Resources/Theme.Light.xaml", UriKind.Relative);
		var darkUri = new Uri("Resources/Theme.Dark.xaml", UriKind.Relative);

		// Remove existing theme dictionaries if present
		var existing = Current.Resources.MergedDictionaries.Where(d => d.Source != null && (d.Source.OriginalString.EndsWith("Theme.Light.xaml") || d.Source.OriginalString.EndsWith("Theme.Dark.xaml"))).ToList();
		foreach (var d in existing) Current.Resources.MergedDictionaries.Remove(d);

		// Add the selected theme dictionary so its resources are available
		var themeDict = new ResourceDictionary { Source = isLight ? lightUri : darkUri };
		Current.Resources.MergedDictionaries.Add(themeDict);
	}

	private bool IsLightThemeEnabled()
	{
		try
		{
			using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
			if (key != null)
			{
				var val = key.GetValue("AppsUseLightTheme");
				if (val is int iv) return iv != 0;
				if (val is byte bv) return bv != 0;
			}
		}
		catch { }
		// default to light for safety
		return true;
	}

	private Color GetWindowsAccentColor()
	{
		// Try common locations for accent color values
		// 1) DWM ColorizationColor (DWORD ARGB)
		using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\DWM"))
		{
			if (key != null)
			{
				var val = key.GetValue("ColorizationColor");
				if (val is int icol)
				{
					uint ui = unchecked((uint)icol);
					byte a = (byte)((ui >> 24) & 0xFF);
					byte r = (byte)((ui >> 16) & 0xFF);
					byte g = (byte)((ui >> 8) & 0xFF);
					byte b = (byte)(ui & 0xFF);
					if (a == 0) a = 255;
					return Color.FromArgb(a, r, g, b);
				}
			}
		}

		// 2) Explorer AccentColor (DWORD) - different path
		using (var key2 = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Accent"))
		{
			if (key2 != null)
			{
				var val = key2.GetValue("AccentColor");
				if (val is int icol2)
				{
					uint ui = unchecked((uint)icol2);
					byte b = (byte)((ui >> 24) & 0xFF);
					byte g = (byte)((ui >> 16) & 0xFF);
					byte r = (byte)((ui >> 8) & 0xFF);
					byte a = (byte)(ui & 0xFF);
					if (a == 0) a = 255;
					return Color.FromArgb(a, r, g, b);
				}
			}
		}

		// fallback color
		return Color.FromRgb(0, 120, 215);
	}
}

