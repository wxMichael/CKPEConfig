/*
 *
 * CKPE Config
 * Copyright (C) 2025  wxMichael
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using CKPEConfig.Models;
using CKPEConfig.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace CKPEConfig.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	private const string IniFileName = "CreationKitPlatformExtended.ini";

	private readonly string[] _settingBlacklist = ["bUIClassicTheme", "bUIDarkTheme", "uUIDarkThemeId", "nCharset"];

	private readonly Dictionary<string, Dictionary<int, string>> _useComboBox = new()
	{
		{
			"nGenerationVersion", new Dictionary<int, string>
			{
				{ 0, "(0) 64-bit Havok Little Endian [PC or XB1]" },
				{ 1, "(1) 64-bit Havok Big Endian [PS4, Non-CKPE Default]" },
				{ 2, "(2) 32-bit Havok Little Endian [PC or XB1]" },
			}
		},
		{
			"uTintMaskResolution", new Dictionary<int, string>
			{
				{ 256, "256" },
				{ 512, "512" },
				{ 1024, "1024" },
				{ 2048, "2048" },
				{ 4096, "4096" },
			}
		},
	};

	public MainWindowViewModel()
	{
		AppVersion = FileVersionInfo.GetVersionInfo(Environment.ProcessPath!).FileVersion!.Split()[0];
		SystemInfo = new SystemInfo();

		string iniFilePath;
		if (File.Exists(IniFileName))
		{
			iniFilePath = IniFileName;
		}
		else
		{
			using RegistryKey? key =
				Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Bethesda Softworks\Fallout4");
			string? gamePath = key?.GetValue("Installed Path")?.ToString();
			if (gamePath != null)
			{
				string iniPath = Path.Combine(gamePath, IniFileName);
				if (File.Exists(iniPath))
					iniFilePath = iniPath;
				else
					throw new FileNotFoundException(IniFileName);
			}
			else
			{
				throw new FileNotFoundException(IniFileName);
			}
		}

		string parentPath = Path.GetDirectoryName(iniFilePath)!;
		string ckPath = Path.Combine(parentPath, "CreationKit.exe");
		string ckpePath = Path.Combine(parentPath, "winhttp.dll");

		if (!File.Exists(ckPath)) CkVersion = "NOT INSTALLED";
		else CkVersion = FileVersionInfo.GetVersionInfo(ckPath).FileVersion ?? "UNKNOWN VERSION";

		if (!File.Exists(ckpePath))
		{
			CkpeVersion = "NOT INSTALLED";
		}
		else
		{
			FileVersionInfo ckpeVersionInfo = FileVersionInfo.GetVersionInfo(ckpePath);
			string? fileCompany = ckpeVersionInfo.CompanyName;
			if (fileCompany == null || !fileCompany.Contains("perchik", StringComparison.OrdinalIgnoreCase))
				CkpeVersion = "NOT INSTALLED";
			else
				CkpeVersion = ckpeVersionInfo.FileVersion ?? "UNKNOWN VERSION";
		}

		Config = new IniFile(iniFilePath);

		string? userValue = Config.Data["CreationKit"]["nCharset"].Value;
		if (string.IsNullOrEmpty(userValue))
			userValue = "1";
		int charset = int.Parse(userValue);
		SelectedCharsetValue = Charsets[charset];
	}

	public string AppVersion { get; }
	public string CkVersion { get; }
	public string CkpeVersion { get; }
	public SystemInfo SystemInfo { get; }

	[ObservableProperty] public partial string SelectedCharsetValue { get; set; }

	public Dictionary<int, string> Charsets { get; } = new()
	{
		{ 1, "(1) Default" },
		{ 0, "(0) ANSI" },
		{ 2, "(2) Symbol" },
		{ 128, "(128) Shift JIS" },
		{ 129, "(129) Hangul" },
		{ 134, "(134) GB 2312" },
		{ 136, "(136) Chinese Big5" },
		{ 255, "(255) OEM" },
		{ 130, "(130) Johab" },
		{ 177, "(177) Hebrew" },
		{ 178, "(178) Arabic" },
		{ 161, "(161) Greek" },
		{ 162, "(163) Turkish" },
		{ 163, "(163) Vietnamese" },
		{ 222, "(222) Thai" },
		{ 238, "(238) East Europe" },
		{ 204, "(204) Russian" },
		{ 77, "(77) Mac" },
		{ 186, "(186) Baltic" },
	};

	public bool InCreationKitSection => CurrentSection == "CreationKit";

	public int SettingsContainerRowSpan => CurrentSection == "CreationKit" ? 1 : 2;
	public int SettingsContainerHeight => CurrentSection == "CreationKit" ? 120 : 500;

	[NotifyPropertyChangedFor(nameof(InCreationKitSection),
		nameof(SettingsContainerRowSpan),
		nameof(SettingsContainerHeight))]
	[ObservableProperty]
	public partial string? CurrentSection { get; private set; }

	[ObservableProperty] public partial bool TipExperimental { get; private set; }
	[ObservableProperty] public partial string? TipKey { get; private set; }
	[ObservableProperty] public partial string? Tip { get; private set; }
	public IniFile Config { get; }

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
	public partial bool IsNotSaving { get; set; } = true;

	[ObservableProperty] public partial ObservableCollection<object> Items { get; set; } = [];

	[RelayCommand(CanExecute = nameof(IsNotSaving))]
	private void SaveConfig()
	{
		IsNotSaving = false;
		Config.Save();
		IsNotSaving = true;
	}

	public void OnWindowClose(object? sender, EventArgs e)
	{
		SaveConfig();
	}

	public void SectionChanged(string sectionName)
	{
		CurrentSection = sectionName;
		Items.Clear();

		foreach (KeyValuePair<string, IniLine> lineData in Config.Data[sectionName])
			try
			{
				if (string.IsNullOrEmpty(lineData.Value.Key) || lineData.Value.Value == null ||
				    _settingBlacklist.Contains(lineData.Key))
					continue;

				char keyPrefix = lineData.Key[0];
				switch (keyPrefix)
				{
					case 'b':
						AddCheckBox(lineData);
						break;
					case 'u' or 'i' or 'n' or 'f':
						if (_useComboBox.TryGetValue(lineData.Key, out Dictionary<int, string>? items))
							AddComboBox(lineData, items);
						else
							AddNumericUpDown(lineData, keyPrefix);
						break;
					case 's' or 'S' or 'c':
						if (lineData.Value.Key.Contains("font", StringComparison.OrdinalIgnoreCase))
							AddFontSelector(lineData);
						else
							AddTextBox(lineData, keyPrefix);
						break;
					default:
						if (lineData.Key.StartsWith("HKFunc_"))
							AddHotKeyBox(lineData);
						else
							Items.Add(new TextBlock
							{
								Text = $"{lineData.Key}={lineData.Value.Value}",
								VerticalAlignment = VerticalAlignment.Center,
							});
						break;
				}
			}
			catch (Exception exception)
			{
				ErrorWindow errorWindow = new(exception);
				errorWindow.Show();
			}
	}

	private void AddCheckBox(KeyValuePair<string, IniLine> lineData)
	{
		if (lineData.Value.Value != "true" && lineData.Value.Value != "false")
			lineData.Value.Value = "false";

		CheckBox checkBox = new()
		{
			Name = lineData.Key,
			Content = GetFriendlyName(lineData),
			IsChecked = lineData.Value.Value == "true",
			Command = new RelayCommand<Tuple<IniLine, Control>>(UpdateValue),
		};
		checkBox.CommandParameter = new Tuple<IniLine, Control>(lineData.Value, checkBox);
		checkBox.PointerEntered += UpdateTipEvent;
		checkBox.PointerExited += ClearTipEvent;
		Items.Add(checkBox);
	}

	private void AddTextBox(KeyValuePair<string, IniLine> lineData, char keyPrefix)
	{
		DockPanel panel = new() { HorizontalSpacing = 5 };
		panel.Children.Add(new TextBlock
		{
			[DockPanel.DockProperty] = Dock.Left,
			Text = GetFriendlyName(lineData),
			VerticalAlignment = VerticalAlignment.Center,
		});

		TextBox textBox = new()
		{
			Name = lineData.Key,
			Classes = { "Small" },
			Text = lineData.Value.Value!,
			Width = 150,
			HorizontalAlignment = HorizontalAlignment.Right,
			BorderBrush = Brushes.Black,
			BorderThickness = new Thickness(1),
		};
		if (keyPrefix == 'c')
		{
			textBox.MaxLength = 1;
			textBox.Width = 40;
		}

		textBox.TextChanged += UpdateValueEvent;
		panel.PointerEntered += UpdateTipEvent;
		panel.PointerExited += ClearTipEvent;
		panel.Children.Add(textBox);
		Items.Add(panel);
	}

	private void AddNumericUpDown(KeyValuePair<string, IniLine> lineData, char keyPrefix)
	{
		DockPanel panel = new() { HorizontalSpacing = 5 };
		panel.Children.Add(new TextBlock
		{
			[DockPanel.DockProperty] = Dock.Left,
			Text = GetFriendlyName(lineData),
			VerticalAlignment = VerticalAlignment.Center,
		});
		NumericUpDown control = new()
		{
			Name = lineData.Key,
			Classes = { "Small" },
			Width = 90,
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
			BorderBrush = Brushes.Black,
			BorderThickness = new Thickness(1),
		};

		// TODO: Better default?
		if (string.IsNullOrEmpty(lineData.Value.Value))
			lineData.Value.Value = "0";

		switch (keyPrefix)
		{
			case 'u':
				control.FormatString = "0";
				control.Minimum = 0;
				control.Value = uint.Parse(lineData.Value.Value);
				break;
			case 'i' or 'n':
				control.FormatString = "0";
				control.Value = int.Parse(lineData.Value.Value);
				break;
			case 'f':
				control.FormatString = "0.00";
				control.Increment = 0.05m;
				try
				{
					control.Value = decimal.Parse(lineData.Value.Value);
				}
				catch (Exception e)
				{
					control.Value = 0;
					ErrorWindow errorWindow = new(e);
					errorWindow.Show();
				}

				break;
		}

		if (lineData.Value.Comment != null)
		{
			Match match = ValueRangeRegex().Match(lineData.Value.Comment);
			if (match.Success)
			{
				decimal min = decimal.Parse(match.Groups["Minimum"].Value);
				decimal max = decimal.Parse(match.Groups["Maximum"].Value);
				control.Minimum = decimal.Min(min, max);
				control.Maximum = decimal.Max(min, max);
			}
		}

		if (control.Value != null)
		{
			control.Value = decimal.Clamp(control.Value.Value, control.Minimum, control.Maximum);
			lineData.Value.Value = control.Value.ToString();
		}

		control.ValueChanged += UpdateValueEvent;
		panel.PointerEntered += UpdateTipEvent;
		panel.PointerExited += ClearTipEvent;
		panel.Children.Add(control);
		Items.Add(panel);
	}

	private void AddComboBox(KeyValuePair<string, IniLine> lineData, Dictionary<int, string> items)
	{
		DockPanel panel = new() { HorizontalSpacing = 5 };
		panel.Children.Add(new TextBlock
		{
			[DockPanel.DockProperty] = Dock.Left,
			Text = GetFriendlyName(lineData),
			VerticalAlignment = VerticalAlignment.Center,
		});

		// TODO: Use setting default? Requires hardcoded default or downloading repo INI
		if (string.IsNullOrEmpty(lineData.Value.Value) ||
		    !int.TryParse(lineData.Value.Value, out int value) || !items.ContainsKey(value))
			lineData.Value.Value = items.Keys.First().ToString();

		ComboBox comboBox = new()
		{
			Name = lineData.Key,
			Classes = { "Small" },
			VerticalAlignment = VerticalAlignment.Center,
			ItemsSource = items.Values,
			SelectedValue = items[int.Parse(lineData.Value.Value)],
			BorderBrush = Brushes.Black,
			BorderThickness = new Thickness(1),
		};

		comboBox.SelectionChanged += UpdateValueEvent;
		panel.PointerEntered += UpdateTipEvent;
		panel.PointerExited += ClearTipEvent;
		panel.Children.Add(comboBox);
		Items.Add(panel);
	}

	private static string GetFriendlyName(KeyValuePair<string, IniLine> lineData)
	{
		int offset = lineData.Key.StartsWith("HKFunc_", StringComparison.OrdinalIgnoreCase) ? 7 : 1;
		return FriendlyNameRegex().Replace(lineData.Key[offset..], "${A} ${B}");
	}

	private void AddFontSelector(KeyValuePair<string, IniLine> lineData)
	{
		DockPanel panel = new() { HorizontalSpacing = 5 };
		panel.Children.Add(new TextBlock
		{
			[DockPanel.DockProperty] = Dock.Left,
			Text = GetFriendlyName(lineData),
			VerticalAlignment = VerticalAlignment.Center,
		});

		if (string.IsNullOrEmpty(lineData.Value.Value))
			lineData.Value.Value = "Consolas";

		string fallbackFont = lineData.Value.Value == "Consolas" ? "Courier New" : "Consolas";
		FontFamily userFont = FontManager.Current.SystemFonts
		                                 .Where(x => x.Name == lineData.Value.Value)
		                                 .FirstOrDefault(FontFamily.Parse(fallbackFont));
		lineData.Value.Value = userFont.Name;

		FontSelector fontSelector = new()
		{
			HorizontalAlignment = HorizontalAlignment.Right,
		};
		ComboBox comboBox = fontSelector.Content as ComboBox ?? throw new NullReferenceException();
		comboBox.Name = lineData.Key;
		comboBox.HorizontalAlignment = HorizontalAlignment.Right;
		comboBox.ItemsSource = FontManager.Current.SystemFonts;
		comboBox.BorderBrush = Brushes.Black;
		comboBox.BorderThickness = new Thickness(1);
		comboBox.SelectedItem = userFont;
		comboBox.SelectionChanged += UpdateValueEvent;
		panel.PointerEntered += UpdateTipEvent;
		panel.PointerExited += ClearTipEvent;
		panel.Children.Add(fontSelector);
		Items.Add(panel);
	}

	private static bool IsValidKey(Key key)
	{
		return key is >= Key.D0 and <= Key.Z
			or >= Key.NumPad0 and <= Key.F12
			or >= Key.LeftShift and <= Key.RightAlt
			or Key.OemComma or Key.OemPlus or Key.OemMinus or Key.OemPeriod or Key.OemTilde
			or Key.OemOpenBrackets or Key.OemCloseBrackets or Key.OemBackslash // or Key.Back
			or Key.Enter or Key.Space or Key.Tab or Key.OemQuestion or Key.OemPipe;
	}

	private static void OnKeyDownHandler(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.None || sender is not TextBox textBox) return;
		e.Handled = true;

		if (!IsValidKey(e.Key)) return;

		StringBuilder combo = new();
		if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
			combo.Append("SHIFT+");
		if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
			combo.Append("CTRL+");
		if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
			combo.Append("ALT+");

		if (e.Key is < Key.LeftShift or > Key.RightAlt)
			combo.Append(e.Key == Key.Enter
				? "ENTER"
				: (string.IsNullOrWhiteSpace(e.KeySymbol) ? e.Key.ToString() : e.KeySymbol).ToUpper());

		textBox.Text = combo.ToString();
		textBox.CaretIndex = textBox.Text.Length;
	}

	private void AddHotKeyBox(KeyValuePair<string, IniLine> lineData)
	{
		DockPanel panel = new() { HorizontalSpacing = 5 };
		panel.Children.Add(new TextBlock
		{
			[DockPanel.DockProperty] = Dock.Left,
			Text = GetFriendlyName(lineData),
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			TextAlignment = TextAlignment.Right,
		});

		TextBox textBox = new()
		{
			Name = lineData.Key,
			Classes = { "Small" },
			Text = lineData.Value.Value,
			Width = 150,
			HorizontalAlignment = HorizontalAlignment.Right,
			BorderBrush = Brushes.Black,
			BorderThickness = new Thickness(1),
		};
		textBox.AddHandler(Control.ContextRequestedEvent, ClearHotKey, RoutingStrategies.Tunnel);
		textBox.AddHandler(InputElement.KeyDownEvent, OnKeyDownHandler, RoutingStrategies.Tunnel);
		textBox.TextChanged += UpdateValueEvent;
		panel.PointerEntered += UpdateTipEvent;
		panel.PointerExited += ClearTipEvent;
		panel.Children.Add(textBox);
		Items.Add(panel);
	}

	private static void ClearHotKey(object? sender, ContextRequestedEventArgs e)
	{
		if (sender is not TextBox textBox) return;
		e.Handled = true;
		textBox.Text = string.Empty;
	}

	internal void ClearTipEvent(object? sender, RoutedEventArgs args)
	{
		TipExperimental = false;
		TipKey = string.Empty;
		Tip = string.Empty;
		args.Handled = true;
	}

	internal void UpdateTipEvent(object? sender, RoutedEventArgs args)
	{
		if (CurrentSection == null) return;
		if (sender is not Control control) return;
		if (control is DockPanel dockPanel)
			control = dockPanel.Children.LastOrDefault() ?? throw new NullReferenceException();
		if (control is StackPanel stackPanel)
			control = stackPanel.Children.LastOrDefault() ?? throw new NullReferenceException();
		if (control is FontSelector fontSelector)
			control = fontSelector.Content as ComboBox ?? throw new InvalidOperationException();

		IniLine setting = Config.Data[CurrentSection][control.Name ?? throw new InvalidOperationException()];
		if (setting.Comment == null)
		{
			TipExperimental = false;
			TipKey = string.Empty;
			Tip = string.Empty;
		}
		else
		{
			TipExperimental = setting.Comment.StartsWith("[Experimental]");
			TipKey = setting.Key;
			Tip = TipExperimental ? setting.Comment[14..].TrimStart() : setting.Comment;
		}

		args.Handled = true;
	}

	private void UpdateValueEvent(object? sender, RoutedEventArgs args)
	{
		if (CurrentSection == null) return;
		if (sender is not Control control) return;
		if (control is DockPanel panel)
			control = panel.Children.LastOrDefault() ?? throw new NullReferenceException();
		if (control is FontSelector fontSelector)
			control = fontSelector.Content as ComboBox ?? throw new InvalidOperationException();

		IniLine setting = Config.Data[CurrentSection][control.Name!];
		UpdateValue(new Tuple<IniLine, Control>(setting, control));
		args.Handled = true;
	}

	private void UpdateValue(Tuple<IniLine, Control>? item)
	{
		if (CurrentSection == null) return;
		if (item == null) return;
		string newValue;
		switch (item.Item2)
		{
			case CheckBox checkBox:
				newValue = checkBox.IsChecked == true ? "true" : "false";
				break;
			case ComboBox comboBox:
				string setting = comboBox.Name ?? throw new NullReferenceException();
				if (setting.Contains("font", StringComparison.OrdinalIgnoreCase))
				{
					FontFamily selected = (FontFamily?)comboBox.SelectedItem ?? throw new NullReferenceException();
					newValue = selected.Name;
				}
				else
				{
					string selected = (string?)comboBox.SelectedItem ?? throw new NullReferenceException();
					if (!_useComboBox[setting].TryGetKeyByValue(selected, out int value))
						throw new NullReferenceException();
					newValue = value.ToString();
				}

				break;
			case NumericUpDown numericUpDown:
				newValue = ((int?)numericUpDown.Value)?.ToString() ?? "0";
				break;
			case TextBox textBox:
				newValue = textBox.Text ?? string.Empty;
				break;
			default:
				throw new NotImplementedException("Unknown control type");
		}

		item.Item1.Value = newValue;
	}

	public void SetCharset(string charset)
	{
		if (Charsets.TryGetKeyByValue(charset, out int k))
			Config.Data["CreationKit"]["nCharset"].Value = k.ToString();
	}

	public void SetTheme(bool isClassic, bool isDark, int darkId)
	{
		Config.Data["CreationKit"]["bUIClassicTheme"].Value = isClassic ? "true" : "false";
		Config.Data["CreationKit"]["bUIDarkTheme"].Value = isDark ? "true" : "false";
		Config.Data["CreationKit"]["uUIDarkThemeId"].Value = darkId.ToString();
	}

	[GeneratedRegex(@"(?:(?<A>[A-Z0-9])(?<B>[A-Z][a-z])|(?<A>[a-z])(?<B>[A-Z0-9]))")]
	private static partial Regex FriendlyNameRegex();

	// [-3.0 : 3.0]
	[GeneratedRegex(@"\[\s*(?<Minimum>[-.0-9]+)\s*:\s*(?<Maximum>[-.0-9]+)\s*\]")]
	private static partial Regex ValueRangeRegex();
}
