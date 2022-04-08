using UnityEngine;

namespace Facepunch.GUI;

public static class Controls
{
	public static float labelWidth = 100f;

	public static float FloatSlider(string strLabel, float value, float low, float high, string format = "0.00")
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(strLabel, GUILayout.Width(labelWidth));
		float value2 = float.Parse(GUILayout.TextField(value.ToString(format), GUILayout.ExpandWidth(expand: true)));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		float result = GUILayout.HorizontalSlider(value2, low, high);
		GUILayout.EndHorizontal();
		return result;
	}

	public static int IntSlider(string strLabel, int value, int low, int high, string format = "0")
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(strLabel, GUILayout.Width(labelWidth));
		int num = int.Parse(GUILayout.TextField(value.ToString(format), GUILayout.ExpandWidth(expand: true)));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		int result = (int)GUILayout.HorizontalSlider(num, low, high);
		GUILayout.EndHorizontal();
		return result;
	}

	public static string TextArea(string strName, string value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(strName, GUILayout.Width(labelWidth));
		string result = GUILayout.TextArea(value);
		GUILayout.EndHorizontal();
		return result;
	}

	public static bool Checkbox(string strName, bool value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(strName, GUILayout.Width(labelWidth));
		bool result = GUILayout.Toggle(value, "");
		GUILayout.EndHorizontal();
		return result;
	}

	public static bool Button(string strName)
	{
		GUILayout.BeginHorizontal();
		bool result = GUILayout.Button(strName);
		GUILayout.EndHorizontal();
		return result;
	}
}
