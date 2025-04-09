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
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CKPEConfig.ViewModels;
using CKPEConfig.Views;

namespace CKPEConfig;

public class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			DisableAvaloniaDataAnnotationValidation();
			try
			{
				MainWindowViewModel vm = new();
				desktop.MainWindow = new MainWindow(vm);
			}
			catch (Exception e)
			{
				desktop.MainWindow = new ErrorWindow(e);
			}
		}

		base.OnFrameworkInitializationCompleted();
	}

	private static void DisableAvaloniaDataAnnotationValidation()
	{
		DataAnnotationsValidationPlugin[] dataValidationPluginsToRemove =
			BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

		foreach (DataAnnotationsValidationPlugin plugin in dataValidationPluginsToRemove)
			BindingPlugins.DataValidators.Remove(plugin);
	}
}
