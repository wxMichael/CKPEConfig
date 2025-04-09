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

namespace CKPEConfig;

public static class Utilities
{
	public static IEnumerable<string> SplitToLines(this string input)
	{
		using StringReader reader = new(input);
		while (reader.ReadLine() is { } line) yield return line;
	}

	public static bool TryGetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value,
	                                                  out TKey key)
		where TKey : notnull where TValue : IComparable<TValue>
	{
		foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
			if (keyValuePair.Value.Equals(value))
			{
				key = keyValuePair.Key;
				return true;
			}

		key = default!;
		return false;
	}
}
