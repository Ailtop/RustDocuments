using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace TinyJSON;

public sealed class Encoder
{
	private static readonly Type includeAttrType = typeof(Include);

	private static readonly Type excludeAttrType = typeof(Exclude);

	private static readonly Type typeHintAttrType = typeof(TypeHint);

	private readonly StringBuilder builder;

	private readonly EncodeOptions options;

	private int indent;

	private bool PrettyPrintEnabled => (options & EncodeOptions.PrettyPrint) == EncodeOptions.PrettyPrint;

	private bool TypeHintsEnabled => (options & EncodeOptions.NoTypeHints) != EncodeOptions.NoTypeHints;

	private bool IncludePublicPropertiesEnabled => (options & EncodeOptions.IncludePublicProperties) == EncodeOptions.IncludePublicProperties;

	private bool EnforceHierarchyOrderEnabled => (options & EncodeOptions.EnforceHierarchyOrder) == EncodeOptions.EnforceHierarchyOrder;

	private Encoder(EncodeOptions options)
	{
		this.options = options;
		builder = new StringBuilder();
		indent = 0;
	}

	public static string Encode(object obj)
	{
		return Encode(obj, EncodeOptions.None);
	}

	public static string Encode(object obj, EncodeOptions options)
	{
		Encoder encoder = new Encoder(options);
		encoder.EncodeValue(obj, forceTypeHint: false);
		return encoder.builder.ToString();
	}

	private void EncodeValue(object value, bool forceTypeHint)
	{
		if (value == null)
		{
			builder.Append("null");
		}
		else if (value is string)
		{
			EncodeString((string)value);
		}
		else if (value is ProxyString)
		{
			EncodeString(((ProxyString)value).ToString(CultureInfo.InvariantCulture));
		}
		else if (value is char)
		{
			EncodeString(value.ToString());
		}
		else if (value is bool)
		{
			builder.Append(((bool)value) ? "true" : "false");
		}
		else if (value is Enum)
		{
			EncodeString(value.ToString());
		}
		else if (value is Array)
		{
			EncodeArray((Array)value, forceTypeHint);
		}
		else if (value is IList)
		{
			EncodeList((IList)value, forceTypeHint);
		}
		else if (value is IDictionary)
		{
			EncodeDictionary((IDictionary)value, forceTypeHint);
		}
		else if (value is Guid)
		{
			EncodeString(value.ToString());
		}
		else if (value is ProxyArray)
		{
			EncodeProxyArray((ProxyArray)value);
		}
		else if (value is ProxyObject)
		{
			EncodeProxyObject((ProxyObject)value);
		}
		else if (value is float || value is double || value is int || value is uint || value is long || value is sbyte || value is byte || value is short || value is ushort || value is ulong || value is decimal || value is ProxyBoolean || value is ProxyNumber)
		{
			builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
		}
		else
		{
			EncodeObject(value, forceTypeHint);
		}
	}

	private IEnumerable<FieldInfo> GetFieldsForType(Type type)
	{
		if (EnforceHierarchyOrderEnabled)
		{
			Stack<Type> stack = new Stack<Type>();
			while (type != null)
			{
				stack.Push(type);
				type = type.BaseType;
			}
			List<FieldInfo> list = new List<FieldInfo>();
			while (stack.Count > 0)
			{
				list.AddRange(stack.Pop().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
			}
			return list;
		}
		return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	private IEnumerable<PropertyInfo> GetPropertiesForType(Type type)
	{
		if (EnforceHierarchyOrderEnabled)
		{
			Stack<Type> stack = new Stack<Type>();
			while (type != null)
			{
				stack.Push(type);
				type = type.BaseType;
			}
			List<PropertyInfo> list = new List<PropertyInfo>();
			while (stack.Count > 0)
			{
				list.AddRange(stack.Pop().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
			}
			return list;
		}
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	private void EncodeObject(object value, bool forceTypeHint)
	{
		Type type = value.GetType();
		AppendOpenBrace();
		forceTypeHint = forceTypeHint || TypeHintsEnabled;
		bool includePublicPropertiesEnabled = IncludePublicPropertiesEnabled;
		bool firstItem = !forceTypeHint;
		if (forceTypeHint)
		{
			if (PrettyPrintEnabled)
			{
				AppendIndent();
			}
			EncodeString("@type");
			AppendColon();
			EncodeString(type.FullName);
			firstItem = false;
		}
		foreach (FieldInfo item in GetFieldsForType(type))
		{
			bool forceTypeHint2 = false;
			bool flag = item.IsPublic;
			object[] customAttributes = item.GetCustomAttributes(inherit: true);
			foreach (object o in customAttributes)
			{
				if (excludeAttrType.IsInstanceOfType(o))
				{
					flag = false;
				}
				if (includeAttrType.IsInstanceOfType(o))
				{
					flag = true;
				}
				if (typeHintAttrType.IsInstanceOfType(o))
				{
					forceTypeHint2 = true;
				}
			}
			if (flag)
			{
				AppendComma(firstItem);
				EncodeString(item.Name);
				AppendColon();
				EncodeValue(item.GetValue(value), forceTypeHint2);
				firstItem = false;
			}
		}
		foreach (PropertyInfo item2 in GetPropertiesForType(type))
		{
			if (!item2.CanRead)
			{
				continue;
			}
			bool forceTypeHint3 = false;
			bool flag2 = includePublicPropertiesEnabled;
			object[] customAttributes = item2.GetCustomAttributes(inherit: true);
			foreach (object o2 in customAttributes)
			{
				if (excludeAttrType.IsInstanceOfType(o2))
				{
					flag2 = false;
				}
				if (includeAttrType.IsInstanceOfType(o2))
				{
					flag2 = true;
				}
				if (typeHintAttrType.IsInstanceOfType(o2))
				{
					forceTypeHint3 = true;
				}
			}
			if (flag2)
			{
				AppendComma(firstItem);
				EncodeString(item2.Name);
				AppendColon();
				EncodeValue(item2.GetValue(value, null), forceTypeHint3);
				firstItem = false;
			}
		}
		AppendCloseBrace();
	}

	private void EncodeProxyArray(ProxyArray value)
	{
		if (value.Count == 0)
		{
			builder.Append("[]");
			return;
		}
		AppendOpenBracket();
		bool firstItem = true;
		foreach (Variant item in (IEnumerable<Variant>)value)
		{
			AppendComma(firstItem);
			EncodeValue(item, forceTypeHint: false);
			firstItem = false;
		}
		AppendCloseBracket();
	}

	private void EncodeProxyObject(ProxyObject value)
	{
		if (value.Count == 0)
		{
			builder.Append("{}");
			return;
		}
		AppendOpenBrace();
		bool firstItem = true;
		foreach (string key in value.Keys)
		{
			AppendComma(firstItem);
			EncodeString(key);
			AppendColon();
			EncodeValue(value[key], forceTypeHint: false);
			firstItem = false;
		}
		AppendCloseBrace();
	}

	private void EncodeDictionary(IDictionary value, bool forceTypeHint)
	{
		if (value.Count == 0)
		{
			builder.Append("{}");
			return;
		}
		AppendOpenBrace();
		bool firstItem = true;
		foreach (object key in value.Keys)
		{
			AppendComma(firstItem);
			EncodeString(key.ToString());
			AppendColon();
			EncodeValue(value[key], forceTypeHint);
			firstItem = false;
		}
		AppendCloseBrace();
	}

	private void EncodeList(IList value, bool forceTypeHint)
	{
		if (value.Count == 0)
		{
			builder.Append("[]");
			return;
		}
		AppendOpenBracket();
		bool firstItem = true;
		foreach (object item in value)
		{
			AppendComma(firstItem);
			EncodeValue(item, forceTypeHint);
			firstItem = false;
		}
		AppendCloseBracket();
	}

	private void EncodeArray(Array value, bool forceTypeHint)
	{
		if (value.Rank == 1)
		{
			EncodeList(value, forceTypeHint);
			return;
		}
		int[] indices = new int[value.Rank];
		EncodeArrayRank(value, 0, indices, forceTypeHint);
	}

	private void EncodeArrayRank(Array value, int rank, int[] indices, bool forceTypeHint)
	{
		AppendOpenBracket();
		int lowerBound = value.GetLowerBound(rank);
		int upperBound = value.GetUpperBound(rank);
		if (rank == value.Rank - 1)
		{
			for (int i = lowerBound; i <= upperBound; i++)
			{
				indices[rank] = i;
				AppendComma(i == lowerBound);
				EncodeValue(value.GetValue(indices), forceTypeHint);
			}
		}
		else
		{
			for (int j = lowerBound; j <= upperBound; j++)
			{
				indices[rank] = j;
				AppendComma(j == lowerBound);
				EncodeArrayRank(value, rank + 1, indices, forceTypeHint);
			}
		}
		AppendCloseBracket();
	}

	private void EncodeString(string value)
	{
		builder.Append('"');
		char[] array = value.ToCharArray();
		foreach (char c in array)
		{
			switch (c)
			{
			case '"':
				builder.Append("\\\"");
				continue;
			case '\\':
				builder.Append("\\\\");
				continue;
			case '\b':
				builder.Append("\\b");
				continue;
			case '\f':
				builder.Append("\\f");
				continue;
			case '\n':
				builder.Append("\\n");
				continue;
			case '\r':
				builder.Append("\\r");
				continue;
			case '\t':
				builder.Append("\\t");
				continue;
			}
			int num = Convert.ToInt32(c);
			if (num >= 32 && num <= 126)
			{
				builder.Append(c);
			}
			else
			{
				builder.Append("\\u" + Convert.ToString(num, 16).PadLeft(4, '0'));
			}
		}
		builder.Append('"');
	}

	private void AppendIndent()
	{
		for (int i = 0; i < indent; i++)
		{
			builder.Append('\t');
		}
	}

	private void AppendOpenBrace()
	{
		builder.Append('{');
		if (PrettyPrintEnabled)
		{
			builder.Append('\n');
			indent++;
		}
	}

	private void AppendCloseBrace()
	{
		if (PrettyPrintEnabled)
		{
			builder.Append('\n');
			indent--;
			AppendIndent();
		}
		builder.Append('}');
	}

	private void AppendOpenBracket()
	{
		builder.Append('[');
		if (PrettyPrintEnabled)
		{
			builder.Append('\n');
			indent++;
		}
	}

	private void AppendCloseBracket()
	{
		if (PrettyPrintEnabled)
		{
			builder.Append('\n');
			indent--;
			AppendIndent();
		}
		builder.Append(']');
	}

	private void AppendComma(bool firstItem)
	{
		if (!firstItem)
		{
			builder.Append(',');
			if (PrettyPrintEnabled)
			{
				builder.Append('\n');
			}
		}
		if (PrettyPrintEnabled)
		{
			AppendIndent();
		}
	}

	private void AppendColon()
	{
		builder.Append(':');
		if (PrettyPrintEnabled)
		{
			builder.Append(' ');
		}
	}
}
