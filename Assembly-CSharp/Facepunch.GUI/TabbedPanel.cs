using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facepunch.GUI;

internal class TabbedPanel
{
	public struct Tab
	{
		public string name;

		public Action drawFunc;
	}

	private int selectedTabID;

	private List<Tab> tabs = new List<Tab>();

	public Tab selectedTab => tabs[selectedTabID];

	public void Add(Tab tab)
	{
		tabs.Add(tab);
	}

	internal void DrawVertical(float width)
	{
		GUILayout.BeginVertical(GUILayout.Width(width), GUILayout.ExpandHeight(expand: true));
		for (int i = 0; i < tabs.Count; i++)
		{
			if (GUILayout.Toggle(selectedTabID == i, tabs[i].name, new GUIStyle("devtab")))
			{
				selectedTabID = i;
			}
		}
		if (GUILayout.Toggle(false, "", new GUIStyle("devtab"), GUILayout.ExpandHeight(expand: true)))
		{
			selectedTabID = -1;
		}
		GUILayout.EndVertical();
	}

	internal void DrawContents()
	{
		if (selectedTabID >= 0)
		{
			Tab tab = selectedTab;
			GUILayout.BeginVertical(new GUIStyle("devtabcontents"), GUILayout.ExpandHeight(expand: true), GUILayout.ExpandWidth(expand: true));
			if (tab.drawFunc != null)
			{
				tab.drawFunc();
			}
			GUILayout.EndVertical();
		}
	}
}
