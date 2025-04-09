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
using Avalonia.Controls;
using CKPEConfig.ViewModels;

namespace CKPEConfig.Views;

public partial class MainWindow : Window
{
	private MainWindowViewModel? _viewModel;

	public MainWindow()
	{
		DataContextChanged += OnDataContextChanged;
		InitializeComponent();
	}

	public MainWindow(MainWindowViewModel vm)
	{
		DataContextChanged += OnDataContextChanged;
		DataContext = vm;
		InitializeComponent();
	}

	private void OnDataContextChanged(object? sender, EventArgs e)
	{
		if (DataContext is not MainWindowViewModel vm) return;
		_viewModel = vm;
		Closing += vm.OnWindowClose;
	}

	private void SectionList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (_viewModel == null) return;
		if (sender is not ListBox { SelectedItem: string sectionName }) return;
		_viewModel.SectionChanged(sectionName);
		if (SettingsPageControl != null)
		{
			SettingsPageControl.IsEnabled = true;
			SettingsPageControl.IsVisible = true;
		}

		e.Handled = true;
	}
}
