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
using System.IO;

namespace CKPEConfig.Models;

public class IniLine
{
	public readonly string? Comment;
	public readonly string? Key;
	public readonly string? Section;
	public readonly string? Whitespace;
	public string? Value;

	public IniLine(string line)
	{
		string raw = line.Trim();
		if (raw == string.Empty) return;
		if (raw.StartsWith(';'))
		{
			Comment = raw[1..].TrimStart();
			return;
		}

		string[] parts = raw.Split(';', 2, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 2) Comment = parts[1].Trim();

		if (parts[0].StartsWith(']'))
			throw new InvalidDataException($"Invalid section: {parts[0].TrimEnd()}");

		if (parts[0].StartsWith('['))
		{
			string[] sectionParts = parts[0].Split(']', 2);

			Section = sectionParts[0][1..];
			if (sectionParts[1] != string.Empty)
				// TODO: Handle setting after section?
				Whitespace = sectionParts[1];
			return;
		}

		string[] settingParts = parts[0].Split('=', 2);
		if (string.IsNullOrWhiteSpace(settingParts[0]))
			throw new InvalidDataException($"Invalid setting: {parts[0].TrimEnd()}");
		Key = settingParts[0].TrimEnd();

		if (string.IsNullOrWhiteSpace(settingParts[1]))
		{
			Value = string.Empty;
			Whitespace = settingParts[1];
			return;
		}

		Value = settingParts[1].Trim();
		if (Value.Length != settingParts[1].Length)
			Whitespace = settingParts[1].TrimStart()[Value.Length..];
	}
}
