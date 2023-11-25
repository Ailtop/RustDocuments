using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class WorldConfig
{
	public string JsonString = string.Empty;

	public float PercentageTier0 = 0.3f;

	public float PercentageTier1 = 0.3f;

	public float PercentageTier2 = 0.4f;

	public float PercentageBiomeArid = 0.4f;

	public float PercentageBiomeTemperate = 0.15f;

	public float PercentageBiomeTundra = 0.15f;

	public float PercentageBiomeArctic = 0.3f;

	public bool MainRoads = true;

	public bool SideRoads = true;

	public bool Trails = true;

	public bool Rivers = true;

	public bool Powerlines = true;

	public bool AboveGroundRails = true;

	public bool BelowGroundRails = true;

	public bool UnderwaterLabs = true;

	public List<string> PrefabBlacklist = new List<string>();

	public List<string> PrefabWhitelist = new List<string>();

	public bool IsPrefabAllowed(string name)
	{
		if (PrefabBlacklist.Count > 0)
		{
			foreach (string item in PrefabBlacklist)
			{
				if (name.Contains(item))
				{
					return false;
				}
			}
		}
		if (PrefabWhitelist.Count > 0)
		{
			foreach (string item2 in PrefabWhitelist)
			{
				if (name.Contains(item2))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public void LoadFromJsonFile(string fileName)
	{
		try
		{
			LoadFromJsonString(File.ReadAllText(fileName));
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
		}
	}

	public void LoadFromJsonString(string data)
	{
		try
		{
			LoadFromWorldConfig(JsonConvert.DeserializeObject<WorldConfig>(JsonString = data));
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
		}
	}

	public void LoadFromWorldConfig(WorldConfig data)
	{
		float num = data.PercentageTier0 + data.PercentageTier1 + data.PercentageTier2;
		if (num > 0f)
		{
			PercentageTier0 = data.PercentageTier0 / num;
			PercentageTier1 = data.PercentageTier1 / num;
			PercentageTier2 = data.PercentageTier2 / num;
		}
		else
		{
			PercentageTier0 = 0f;
			PercentageTier1 = 1f;
			PercentageTier2 = 0f;
		}
		float num2 = data.PercentageBiomeArid + data.PercentageBiomeTemperate + data.PercentageBiomeTundra + data.PercentageBiomeArctic;
		if (num2 > 0f)
		{
			PercentageBiomeArid = data.PercentageBiomeArid / num2;
			PercentageBiomeTemperate = data.PercentageBiomeTemperate / num2;
			PercentageBiomeTundra = data.PercentageBiomeTundra / num2;
			PercentageBiomeArctic = data.PercentageBiomeArctic / num2;
		}
		else
		{
			PercentageBiomeArid = 0f;
			PercentageBiomeTemperate = 1f;
			PercentageBiomeTundra = 0f;
			PercentageBiomeArctic = 0f;
		}
		MainRoads = data.MainRoads;
		SideRoads = data.SideRoads;
		Trails = data.Trails;
		Rivers = data.Rivers;
		Powerlines = data.Powerlines;
		AboveGroundRails = data.AboveGroundRails;
		BelowGroundRails = data.BelowGroundRails;
		UnderwaterLabs = data.UnderwaterLabs;
		PrefabBlacklist.Clear();
		if (data.PrefabBlacklist != null && data.PrefabBlacklist.Count > 0)
		{
			PrefabBlacklist.AddRange(data.PrefabBlacklist);
		}
		PrefabWhitelist.Clear();
		if (data.PrefabWhitelist != null && data.PrefabWhitelist.Count > 0)
		{
			PrefabWhitelist.AddRange(data.PrefabWhitelist);
		}
	}
}
