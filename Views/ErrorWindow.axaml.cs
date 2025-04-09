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
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace CKPEConfig.Views;

public partial class ErrorWindow : Window
{
	private readonly IClipboard? _clipboard;

	public ErrorWindow()
	{
		InitializeComponent();
		ErrorMessage.Text = "Unspecified Error Occurred";
		CopyButton.IsEnabled = false;
		CopyButton.IsVisible = false;
	}

	public ErrorWindow(Exception exception)
	{
		InitializeComponent();
		ErrorMessage.Text = exception.ToString();
		if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
		    desktop.MainWindow?.Clipboard is not { } provider)
		{
			CopyButton.IsEnabled = false;
			CopyButton.IsVisible = false;
		}
		else
		{
			_clipboard = provider;
		}
	}

	public async void CopyButton_OnClick(object? sender, RoutedEventArgs e)
	{
		try
		{
			if (_clipboard != null) await _clipboard.SetTextAsync(ErrorMessage.Text);
		}
		catch (Exception)
		{
			// Ignored
		}
	}
}
