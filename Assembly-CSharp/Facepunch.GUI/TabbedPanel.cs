using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facepunch.GUI
{
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
			GUILayout.BeginVertical(GUILayout.Width(width), GUILayout.ExpandHeight(true));
			for (int i = 0; i < tabs.Count; i++)
			{
				if (GUILayout.Toggle(selectedTabID == i, tabs[i].name, new GUIStyle("devtab")))
				{
					selectedTabID = i;
				}
			}
			if (GUILayout.Toggle(false, "", new GUIStyle("devtab"), GUILayout.ExpandHeight(true)))
			{
				selectedTabID = -1;
			}
			GUILayout.EndVertical();
		}

		internal void DrawContents()
		{
			if (selectedTabID >= 0)
			{
				Tab selectedTab = this.selectedTab;
				GUILayout.BeginVertical(new GUIStyle("devtabcontents"), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
				if (selectedTab.drawFunc != null)
				{
					selectedTab.drawFunc();
				}
				GUILayout.EndVertical();
			}
		}
	}
}
