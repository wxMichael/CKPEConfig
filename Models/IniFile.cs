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
using System.IO;

namespace CKPEConfig.Models;

public class IniFile
{
	// TODO: Support adding sections and keys that dont exist
	private readonly string _iniFileName;
	private readonly List<IniLine> _iniLines = [];

	public IniFile(string iniFileName)
	{
		if (string.IsNullOrWhiteSpace(iniFileName))
			throw new ArgumentException("Bad file name");
		if (!File.Exists(iniFileName))
			throw new FileNotFoundException(iniFileName);
		_iniFileName = iniFileName;

		string? currentSection = null;
		string iniContents = File.ReadAllText(_iniFileName);
		foreach (string iniLine in iniContents.SplitToLines())
		{
			IniLine line = new(iniLine);
			_iniLines.Add(line);
			if (line.Section != null)
			{
				currentSection = line.Section;
				Data[currentSection] = [];
			}

			if (line.Key != null)
			{
				if (currentSection == null)
					throw new ArgumentException("Key before sections");
				if (line.Value == null)
					throw new InvalidDataException(line.Key + " is missing value");
				Data[currentSection][line.Key] = line;
			}
			else
			{
				if (line.Value != null)
					throw new InvalidDataException("Value without key");
			}
		}
	}

	public Dictionary<string, Dictionary<string, IniLine>> Data { get; } = [];

	public void Save()
	{
		using StreamWriter writer = new(new FileStream(_iniFileName, FileMode.Create, FileAccess.Write));
		foreach (IniLine line in _iniLines)
		{
			if (line.Section != null)
			{
				writer.Write($"[{line.Section}]");
				if (line.Comment != null)
				{
					if (string.IsNullOrEmpty(line.Whitespace)) writer.Write('\t');
					else writer.Write(line.Whitespace);
					writer.WriteLine(line.Comment == string.Empty ? ";" : $"; {line.Comment}");
				}

				writer.WriteLine();
				continue;
			}

			if (line is { Key: not null, Value: not null })
			{
				writer.Write($"{line.Key}={line.Value}");
				if (line.Comment != null)
				{
					if (string.IsNullOrEmpty(line.Whitespace)) writer.Write(' ');
					else writer.Write(line.Whitespace);
					writer.Write($"; {line.Comment}");
				}

				writer.WriteLine();
				continue;
			}

			if (line.Comment != null) writer.WriteLine(line.Comment == string.Empty ? ";" : $"; {line.Comment}");
			else writer.WriteLine();
		}
	}
}
