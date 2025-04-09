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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CKPEConfig.ViewModels;

namespace CKPEConfig.Views;

public partial class SettingsPage : UserControl
{
	private Border? _selectedThemeBorder;

	public SettingsPage()
	{
		InitializeComponent();
	}

	private void ThemeGrid_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		if (sender is not Border border || border == _selectedThemeBorder) return;
		if (DataContext is not MainWindowViewModel viewModel) return;
		switch (border.Name)
		{
			case "Default":
				viewModel.SetTheme(false, false, 0);
				break;
			case "ThemeDark1":
				viewModel.SetTheme(false, true, 0);
				break;
			case "ThemeDark2":
				viewModel.SetTheme(false, true, 1);
				break;
			case "ThemeDark3":
				viewModel.SetTheme(false, true, 2);
				break;
			case "ThemeClassic":
				viewModel.SetTheme(true, false, 0);
				break;
			case "ThemeCustom":
				viewModel.SetTheme(false, true, 3);
				break;
			default:
				throw new NotImplementedException("Unknown theme");
		}

		_selectedThemeBorder?.Classes.Remove("Selected");
		_selectedThemeBorder = border;
		border.Classes.Add("Selected");
	}

	private void Settings_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
	{
		if (sender is not ScrollViewer scrollViewer || e.Delta.Y == 0) return;
		if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;
		scrollViewer.Offset -= new Vector(e.Delta.Y * 50, 0);
	}

	private void Charset_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (sender is not ComboBox comboBox) return;
		if (DataContext is not MainWindowViewModel viewModel) return;
		if (comboBox.SelectedItem is string selected)
			viewModel.SetCharset(selected);
	}

	private void Charset_OnPointerEntered(object? sender, PointerEventArgs e)
	{
		if (sender is not StackPanel stackPanel) return;
		if (DataContext is not MainWindowViewModel viewModel) return;
		viewModel.UpdateTipEvent(stackPanel, e);
	}

	private void Charset_OnPointerExited(object? sender, PointerEventArgs e)
	{
		if (sender is not StackPanel stackPanel) return;
		if (DataContext is not MainWindowViewModel viewModel) return;
		viewModel.ClearTipEvent(stackPanel, e);
	}
}
