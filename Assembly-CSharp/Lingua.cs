using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Data;
using UnityEngine;

public static class Lingua
{
	public enum Language
	{
		Korean,
		English,
		Japanese,
		Chinese_Simplifed,
		Chinese_Traditional,
		German,
		Spanish,
		Portguese,
		Russian,
		Polish,
		French
	}

	public sealed class Key : IEquatable<Key>
	{
		public static readonly Key languageCode = new Key("language/code");

		public static readonly Key languageSystem = new Key("language/system");

		public static readonly Key languageNative = new Key("language/native");

		public static readonly Key languageNumber = new Key("language/number");

		public static readonly Key languageDisplayOrder = new Key("language/displayOrder");

		public static readonly Key titleFont = new Key("font/title");

		public static readonly Key bodyFont = new Key("font/body");

		public static readonly Key colorClose = new Key("cc");

		public static readonly Key colorOpenGold = new Key("cogold");

		public static readonly Key colorOpenDarkQuartz = new Key("codark");

		public static readonly Key colorOpenBone = new Key("cobone");

		public readonly string key;

		public readonly int hashcode;

		private Key(string key)
		{
			this.key = key;
			hashcode = StringComparer.OrdinalIgnoreCase.GetHashCode(key);
		}

		public override int GetHashCode()
		{
			return hashcode;
		}

		public bool Equals(Key other)
		{
			return hashcode.Equals(other.hashcode);
		}
	}

	public const string language = "language";

	public const string label = "label";

	public const string name = "name";

	public const string desc = "desc";

	public const string flavor = "flavor";

	public const string active = "active";

	public const string skill = "skill";

	public static readonly Dictionary<int, List<string>> _localizedStrings;

	public static readonly EnumArray<Language, int> languangeNumberToIndex;

	public static readonly EnumArray<Language, int> displayOrderToIndex;

	public static ReadOnlyCollection<string> nativeNames { get; private set; }

	private static int _current
	{
		get
		{
			return GameData.Settings.language;
		}
		set
		{
			GameData.Settings.language = value;
		}
	}

	public static event Action OnChange;

	static Lingua()
	{
		languangeNumberToIndex = new EnumArray<Language, int>();
		displayOrderToIndex = new EnumArray<Language, int>();
		_localizedStrings = new Dictionary<int, List<string>>();
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		using (StreamReader rdr = new StreamReader(new MemoryStream(Resources.Load<TextAsset>("Strings").bytes)))
		{
			CsvReader csvReader = new CsvReader(rdr, ",");
			csvReader.Read();
			while (csvReader.Read())
			{
				if (!string.IsNullOrWhiteSpace(csvReader[0]) && !csvReader[0].StartsWith("#"))
				{
					List<string> list = new List<string>();
					for (int i = 2; i < csvReader.FieldsCount; i++)
					{
						list.Add(Regex.Unescape(csvReader[i]).Trim());
					}
					string text = csvReader[0];
					int hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(text);
					if (_localizedStrings.ContainsKey(hashCode))
					{
						Debug.Log("Lingua : An item with the same key has already been added. Key: " + text);
						continue;
					}
					_localizedStrings.Add(hashCode, list);
					dictionary.Add(text, list);
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder(255);
		foreach (KeyValuePair<string, List<string>> item in dictionary)
		{
			int num = item.Key.LastIndexOf('/');
			string text2 = ((num != -1) ? item.Key.Substring(0, num) : item.Key);
			for (int j = 0; j < item.Value.Count; j++)
			{
				int num2 = item.Value[j].LastIndexOf(']');
				if (num2 == -1)
				{
					continue;
				}
				int num3 = -1;
				stringBuilder.Clear();
				stringBuilder.Append(item.Value[j]);
				do
				{
					num3 = stringBuilder.LastIndexOf('[', num2 - 1);
					if (num3 == -1)
					{
						Debug.LogError($"{item.Key} : There is no '[' pair in {item.Key}[{j}]");
						break;
					}
					string text3 = stringBuilder.ToString(num3 + 1, num2 - num3 - 1);
					List<string> value;
					if (dictionary.TryGetValue(text2 + "/" + text3, out value) || dictionary.TryGetValue(text3, out value))
					{
						stringBuilder.Remove(num3, num2 - num3 + 1);
						string value2 = value[j];
						if (string.IsNullOrWhiteSpace(value2))
						{
							value2 = value[0];
						}
						stringBuilder.Insert(num3, value2);
						num2 = stringBuilder.LastIndexOf(']', num3 - 1);
						continue;
					}
					Debug.LogWarning(item.Key + " : There is no key: " + text2 + "/" + text3 + " or " + text3);
					break;
				}
				while (num2 != -1);
				item.Value[j] = stringBuilder.ToString();
			}
		}
		nativeNames = _localizedStrings[Key.languageNative.hashcode].AsReadOnly();
		List<string> list2 = _localizedStrings[Key.languageNumber.hashcode];
		for (int k = 0; k < list2.Count; k++)
		{
			int result;
			if (int.TryParse(list2[k], out result))
			{
				languangeNumberToIndex.Array[result] = k;
			}
		}
		List<string> list3 = _localizedStrings[Key.languageSystem.hashcode];
		string text4 = Application.systemLanguage.ToString();
		_current = GameData.Settings.language;
		if (_current >= 0 && _current < list3.Count)
		{
			return;
		}
		_current = Convert.ToInt32(Language.English);
		for (int l = 0; l < list3.Count; l++)
		{
			if (list3[l].Equals(text4, StringComparison.OrdinalIgnoreCase))
			{
				Debug.Log("System language is automatically detected : " + text4);
				_current = l;
				break;
			}
		}
	}

	public static bool TryGetLocalizedString(Key key, out string @string)
	{
		return TryGetLocalizedStringAt(key.hashcode, _current, out @string);
	}

	public static bool TryGetLocalizedString(string key, out string @string)
	{
		return TryGetLocalizedStringAt(StringComparer.OrdinalIgnoreCase.GetHashCode(key), _current, out @string);
	}

	public static string GetLocalizedString(Key key)
	{
		return GetLocalizedStringAt(key.hashcode, _current);
	}

	public static bool TryGetLocalizedStringArray(string key, out string[] strings)
	{
		List<string> list = new List<string>();
		string @string;
		if (TryGetLocalizedString(key, out @string))
		{
			strings = new string[1] { @string };
			return true;
		}
		int i;
		for (i = 0; TryGetLocalizedString($"{key}/{i}", out @string); i++)
		{
			list.Add(@string);
		}
		if (i == 0)
		{
			strings = null;
			return false;
		}
		strings = list.ToArray();
		return true;
	}

	public static string[] GetLocalizedStringArray(string key)
	{
		List<string> list = new List<string>();
		string @string;
		if (TryGetLocalizedString(key, out @string))
		{
			return new string[1] { @string };
		}
		for (int i = 0; TryGetLocalizedString($"{key}/{i}", out @string); i++)
		{
			list.Add(@string);
		}
		return list.ToArray();
	}

	public static string[][] GetLocalizedStringArrays(string key)
	{
		List<string[]> list = new List<string[]>();
		string[] strings;
		for (int i = 0; TryGetLocalizedStringArray($"{key}/{i}", out strings); i++)
		{
			list.Add(strings);
		}
		return list.ToArray();
	}

	public static string[] GetLocalizedStrings(params Key[] keys)
	{
		string[] array = new string[keys.Length];
		for (int i = 0; i < keys.Length; i++)
		{
			array[i] = GetLocalizedStringAt(keys[i].hashcode, _current);
		}
		return array;
	}

	public static string[] GetLocalizedStrings(params string[] keys)
	{
		string[] array = new string[keys.Length];
		for (int i = 0; i < keys.Length; i++)
		{
			array[i] = GetLocalizedStringAt(StringComparer.OrdinalIgnoreCase.GetHashCode(keys[i]), _current);
		}
		return array;
	}

	public static string GetLocalizedString(string key)
	{
		return GetLocalizedStringAt(StringComparer.OrdinalIgnoreCase.GetHashCode(key), _current);
	}

	private static string GetLocalizedStringAt(int key, int index)
	{
		string @string;
		TryGetLocalizedStringAt(key, index, out @string);
		return @string;
	}

	private static bool TryGetLocalizedStringAt(int key, int number, out string @string)
	{
		List<string> value;
		if (_localizedStrings.TryGetValue(key, out value))
		{
			int index = languangeNumberToIndex.Array[number];
			@string = value[index];
			if (string.IsNullOrWhiteSpace(@string))
			{
				@string = value[0];
			}
			return true;
		}
		@string = string.Empty;
		return false;
	}

	public static void Change(int number)
	{
		_current = languangeNumberToIndex.Array[number];
		Lingua.OnChange?.Invoke();
	}

	public static void Change(Language language)
	{
		Change(Convert.ToInt32(language));
	}
}
