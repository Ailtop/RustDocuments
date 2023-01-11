using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.PostProcessing;

public sealed class PropertySheetFactory
{
	private readonly Dictionary<Shader, PropertySheet> m_Sheets;

	public PropertySheetFactory()
	{
		m_Sheets = new Dictionary<Shader, PropertySheet>();
	}

	[Obsolete("Use PropertySheet.Get(Shader) with a direct reference to the Shader instead.")]
	public PropertySheet Get(string shaderName)
	{
		Shader shader = Shader.Find(shaderName);
		if (shader == null)
		{
			throw new ArgumentException($"Invalid shader ({shaderName})");
		}
		return Get(shader);
	}

	public PropertySheet Get(Shader shader)
	{
		if (shader == null)
		{
			throw new ArgumentException($"Invalid shader ({shader})");
		}
		if (m_Sheets.TryGetValue(shader, out var value))
		{
			return value;
		}
		string name = shader.name;
		value = new PropertySheet(new Material(shader)
		{
			name = $"PostProcess - {name.Substring(name.LastIndexOf('/') + 1)}",
			hideFlags = HideFlags.DontSave
		});
		m_Sheets.Add(shader, value);
		return value;
	}

	public void Release()
	{
		foreach (PropertySheet value in m_Sheets.Values)
		{
			value.Release();
		}
		m_Sheets.Clear();
	}
}
