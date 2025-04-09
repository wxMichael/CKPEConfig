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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using SharpGen.Runtime;
using Vortice.DXGI;
using static Vortice.DXGI.DXGI;

namespace CKPEConfig.Models;

public partial class SystemInfo
{
	private readonly Dictionary<string, IDXGIAdapter1> _adapterList = new();
	private readonly Dictionary<string, DisplayDevice> _displayList = new();

	public SystemInfo()
	{
		DisplayDevice device = new()
		{
			cb = Marshal.SizeOf<DisplayDevice>(),
		};

		for (uint i = 0; EnumDisplayDevices(null, i, ref device, 0); i++)
		{
			if ((device.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == 0) continue;
			if (!_displayList.TryAdd(device.DeviceString, device)) continue;
			if ((device.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) == 0) continue;

			PrimaryDisplay = device.DeviceString.Trim();
			break;
		}

		Result result = CreateDXGIFactory1(out IDXGIFactory1? mFactory);
		if (result.Success && mFactory != null)
			for (uint i = 0;; i++)
			{
				result = mFactory.EnumAdapters1(i, out IDXGIAdapter1 adapter);
				if (result.Failure) break;
				if ((adapter.Description1.Flags & AdapterFlags.Software) != 0) continue;
				string desc = adapter.Description1.Description.Trim();
				if (!_adapterList.TryAdd(desc, adapter)) continue;

				if (desc == PrimaryDisplay)
					PrimaryAdapter = adapter;
			}

		Gpu = PrimaryAdapter?.Description1.Description ?? "Unknown GPU";

		using RegistryKey? cpuKey =
			Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
		string? cpu = cpuKey?.GetValue("ProcessorNameString")?.ToString();
		Cpu = cpu == null ? null : CpuRegex().Replace(cpu, string.Empty).Trim();

		using RegistryKey? motherboardKey =
			Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
		Motherboard = motherboardKey?.GetValue("SystemProductName")?.ToString()?.Trim();

		OperatingSystem = Environment.OSVersion.Version.Major switch
		{
			10 => Environment.OSVersion.Version.Build switch
			{
				>= 22000 => "Windows 11",
				>= 17763 => "Windows 10",
				_ => "Unknown OS",
			},
			6 => Environment.OSVersion.Version.Minor switch
			{
				3 => "Windows 8.1",
				2 => "Windows 8",
				1 => "Windows 7",
				0 => "Windows Vista",
				_ => "Unknown OS",
			},
			5 => "Windows XP",
			_ => "Unknown OS",
		};
	}

	private string? PrimaryDisplay { get; }
	private IDXGIAdapter1? PrimaryAdapter { get; }

	public string? Motherboard { get; }
	public string? Cpu { get; }

	public string Gpu { get; }

	public string OperatingSystem { get; }

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DisplayDevice lpDisplayDevice,
	                                              uint dwFlags);

	[GeneratedRegex(@"(?:\d+(?:th|rd|nd) Gen|Processor|CPU|\d*[- ]Core|\(TM\)|\(R\))")]
	private static partial Regex CpuRegex();
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DisplayDevice
{
	public int cb;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string DeviceName;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string DeviceString;

	public DisplayDeviceStateFlags StateFlags;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string DeviceID;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string DeviceKey;
}

[Flags]
public enum DisplayDeviceStateFlags
{
	Active = 0x1,
	MultiDriver = 0x2,
	PrimaryDevice = 0x4,
	MirroringDriver = 0x8,
	VgaCompatible = 0x10,
	Removable = 0x20,
	ModesPruned = 0x8000000,
	Remote = 0x4000000,
	Disconnect = 0x2000000,
	AttachedToDesktop = 0x1,
}
