#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Math;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class SaveRestore : SingletonComponent<SaveRestore>
{
	public static bool IsSaving = false;

	public bool timedSave = true;

	public int timedSavePause;

	public static DateTime SaveCreatedTime;

	private static MemoryStream SaveBuffer = new MemoryStream(33554432);

	internal static void ClearMapEntities()
	{
		BaseEntity[] array = UnityEngine.Object.FindObjectsOfType<BaseEntity>();
		if (array.Length == 0)
		{
			return;
		}
		DebugEx.Log("Destroying " + array.Length + " old entities");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kill();
			if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
			{
				stopwatch.Reset();
				stopwatch.Start();
				DebugEx.Log("\t" + (i + 1) + " / " + array.Length);
			}
		}
		ItemManager.Heartbeat();
		DebugEx.Log("\tdone.");
	}

	public static bool Load(string strFilename = "", bool allowOutOfDateSaves = false)
	{
		SaveCreatedTime = DateTime.UtcNow;
		try
		{
			if (strFilename == "")
			{
				strFilename = World.SaveFolderName + "/" + World.SaveFileName;
			}
			if (!File.Exists(strFilename))
			{
				Interface.CallHook("OnNewSave", strFilename);
				if (!File.Exists("TestSaves/" + strFilename))
				{
					UnityEngine.Debug.LogWarning("Couldn't load " + strFilename + " - file doesn't exist");
					return false;
				}
				strFilename = "TestSaves/" + strFilename;
			}
			Dictionary<BaseEntity, ProtoBuf.Entity> dictionary = new Dictionary<BaseEntity, ProtoBuf.Entity>();
			using (FileStream fileStream = File.OpenRead(strFilename))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					SaveCreatedTime = File.GetCreationTime(strFilename);
					if (binaryReader.ReadSByte() != 83 || binaryReader.ReadSByte() != 65 || binaryReader.ReadSByte() != 86 || binaryReader.ReadSByte() != 82)
					{
						UnityEngine.Debug.LogWarning("Invalid save (missing header)");
						return false;
					}
					if (binaryReader.PeekChar() == 68)
					{
						binaryReader.ReadChar();
						SaveCreatedTime = Epoch.ToDateTime(binaryReader.ReadInt32());
					}
					if (binaryReader.ReadUInt32() != 213)
					{
						if (allowOutOfDateSaves)
						{
							UnityEngine.Debug.LogWarning("This save is from an older (possibly incompatible) version!");
						}
						else
						{
							UnityEngine.Debug.LogWarning("This save is from an older version. It might not load properly.");
						}
					}
					ClearMapEntities();
					Assert.IsTrue(BaseEntity.saveList.Count == 0, "BaseEntity.saveList isn't empty!");
					Network.Net.sv.Reset();
					Rust.Application.isLoadingSave = true;
					HashSet<uint> hashSet = new HashSet<uint>();
					while (fileStream.Position < fileStream.Length)
					{
						RCon.Update();
						uint num = binaryReader.ReadUInt32();
						long position = fileStream.Position;
						ProtoBuf.Entity entData = null;
						try
						{
							entData = ProtoBuf.Entity.DeserializeLength(fileStream, (int)num);
						}
						catch (Exception exception)
						{
							UnityEngine.Debug.LogWarning("Skipping entity since it could not be deserialized - stream position: " + position + " size: " + num);
							UnityEngine.Debug.LogException(exception);
							fileStream.Position = position + num;
							continue;
						}
						if (entData.basePlayer != null && dictionary.Any((KeyValuePair<BaseEntity, ProtoBuf.Entity> x) => x.Value.basePlayer != null && x.Value.basePlayer.userid == entData.basePlayer.userid))
						{
							UnityEngine.Debug.LogWarning("Skipping entity " + entData.baseNetworkable.uid + " - it's a player " + entData.basePlayer.userid + " who is in the save multiple times");
							continue;
						}
						if (entData.baseNetworkable.uid != 0 && hashSet.Contains(entData.baseNetworkable.uid))
						{
							UnityEngine.Debug.LogWarning("Skipping entity " + entData.baseNetworkable.uid + " " + StringPool.Get(entData.baseNetworkable.prefabID) + " - uid is used multiple times");
							continue;
						}
						if (entData.baseNetworkable.uid != 0)
						{
							hashSet.Add(entData.baseNetworkable.uid);
						}
						BaseEntity baseEntity = GameManager.server.CreateEntity(StringPool.Get(entData.baseNetworkable.prefabID), entData.baseEntity.pos, Quaternion.Euler(entData.baseEntity.rot));
						if ((bool)baseEntity)
						{
							baseEntity.InitLoad(entData.baseNetworkable.uid);
							dictionary.Add(baseEntity, entData);
						}
					}
				}
			}
			DebugEx.Log("Spawning " + dictionary.Count + " entities");
			object obj = Interface.CallHook("OnSaveLoad", dictionary);
			if (obj is bool)
			{
				return (bool)obj;
			}
			BaseNetworkable.LoadInfo info = default(BaseNetworkable.LoadInfo);
			info.fromDisk = true;
			Stopwatch stopwatch = Stopwatch.StartNew();
			int num2 = 0;
			foreach (KeyValuePair<BaseEntity, ProtoBuf.Entity> item in dictionary)
			{
				BaseEntity key = item.Key;
				if (key == null)
				{
					continue;
				}
				RCon.Update();
				info.msg = item.Value;
				key.Spawn();
				key.Load(info);
				if (BaseEntityEx.IsValid(key))
				{
					num2++;
					if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
					{
						stopwatch.Reset();
						stopwatch.Start();
						DebugEx.Log("\t" + num2 + " / " + dictionary.Count);
					}
				}
			}
			foreach (KeyValuePair<BaseEntity, ProtoBuf.Entity> item2 in dictionary)
			{
				BaseEntity key2 = item2.Key;
				if (!(key2 == null))
				{
					RCon.Update();
					if (BaseEntityEx.IsValid(key2))
					{
						key2.PostServerLoad();
					}
				}
			}
			DebugEx.Log("\tdone.");
			if ((bool)SingletonComponent<SpawnHandler>.Instance)
			{
				DebugEx.Log("Enforcing SpawnPopulation Limits");
				SingletonComponent<SpawnHandler>.Instance.EnforceLimits();
				DebugEx.Log("\tdone.");
			}
			Rust.Application.isLoadingSave = false;
			return true;
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogWarning("Error loading save (" + strFilename + ")");
			UnityEngine.Debug.LogException(exception2);
			return false;
		}
	}

	public static void GetSaveCache()
	{
		BaseEntity[] array = BaseEntity.saveList.ToArray();
		if (array.Length == 0)
		{
			return;
		}
		DebugEx.Log("Initializing " + array.Length + " entity save caches");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < array.Length; i++)
		{
			BaseEntity baseEntity = array[i];
			if (BaseEntityEx.IsValid(baseEntity))
			{
				baseEntity.GetSaveCache();
				if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
				{
					stopwatch.Reset();
					stopwatch.Start();
					DebugEx.Log("\t" + (i + 1) + " / " + array.Length);
				}
			}
		}
		DebugEx.Log("\tdone.");
	}

	public static void InitializeEntityLinks()
	{
		BaseEntity[] array = (from x in BaseNetworkable.serverEntities
			where x is BaseEntity
			select x as BaseEntity).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		DebugEx.Log("Initializing " + array.Length + " entity links");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < array.Length; i++)
		{
			RCon.Update();
			array[i].RefreshEntityLinks();
			if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
			{
				stopwatch.Reset();
				stopwatch.Start();
				DebugEx.Log("\t" + (i + 1) + " / " + array.Length);
			}
		}
		DebugEx.Log("\tdone.");
	}

	public static void InitializeEntitySupports()
	{
		if (!ConVar.Server.stability)
		{
			return;
		}
		StabilityEntity[] array = (from x in BaseNetworkable.serverEntities
			where x is StabilityEntity
			select x as StabilityEntity).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		DebugEx.Log("Initializing " + array.Length + " stability supports");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < array.Length; i++)
		{
			RCon.Update();
			array[i].InitializeSupports();
			if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
			{
				stopwatch.Reset();
				stopwatch.Start();
				DebugEx.Log("\t" + (i + 1) + " / " + array.Length);
			}
		}
		DebugEx.Log("\tdone.");
	}

	public static void InitializeEntityConditionals()
	{
		BuildingBlock[] array = (from x in BaseNetworkable.serverEntities
			where x is BuildingBlock
			select x as BuildingBlock).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		DebugEx.Log("Initializing " + array.Length + " conditional models");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < array.Length; i++)
		{
			RCon.Update();
			array[i].UpdateSkin(true);
			if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
			{
				stopwatch.Reset();
				stopwatch.Start();
				DebugEx.Log("\t" + (i + 1) + " / " + array.Length);
			}
		}
		DebugEx.Log("\tdone.");
	}

	public static IEnumerator Save(string strFilename, bool AndWait = false)
	{
		if (Rust.Application.isQuitting)
		{
			yield break;
		}
		Stopwatch timerCache = new Stopwatch();
		Stopwatch timerWrite = new Stopwatch();
		Stopwatch timerDisk = new Stopwatch();
		int iEnts = 0;
		timerCache.Start();
		using (TimeWarning.New("SaveCache", 100))
		{
			Stopwatch sw = Stopwatch.StartNew();
			BaseEntity[] array = BaseEntity.saveList.ToArray();
			foreach (BaseEntity baseEntity in array)
			{
				if (baseEntity == null || !BaseEntityEx.IsValid(baseEntity))
				{
					continue;
				}
				try
				{
					baseEntity.GetSaveCache();
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				if (sw.Elapsed.TotalMilliseconds > 5.0)
				{
					if (!AndWait)
					{
						yield return CoroutineEx.waitForEndOfFrame;
					}
					sw.Reset();
					sw.Start();
				}
			}
		}
		timerCache.Stop();
		SaveBuffer.Position = 0L;
		SaveBuffer.SetLength(0L);
		timerWrite.Start();
		using (TimeWarning.New("SaveWrite", 100))
		{
			BinaryWriter writer = new BinaryWriter(SaveBuffer);
			writer.Write((sbyte)83);
			writer.Write((sbyte)65);
			writer.Write((sbyte)86);
			writer.Write((sbyte)82);
			writer.Write((sbyte)68);
			writer.Write(Epoch.FromDateTime(SaveCreatedTime));
			writer.Write(213u);
			BaseNetworkable.SaveInfo saveInfo = default(BaseNetworkable.SaveInfo);
			saveInfo.forDisk = true;
			if (!AndWait)
			{
				yield return CoroutineEx.waitForEndOfFrame;
			}
			foreach (BaseEntity save in BaseEntity.saveList)
			{
				if (save == null || save.IsDestroyed)
				{
					UnityEngine.Debug.LogWarning("Entity is NULL but is still in saveList - not destroyed properly? " + save, save);
					continue;
				}
				MemoryStream memoryStream = null;
				try
				{
					memoryStream = save.GetSaveCache();
				}
				catch (Exception exception2)
				{
					UnityEngine.Debug.LogException(exception2);
				}
				if (memoryStream == null || memoryStream.Length <= 0)
				{
					UnityEngine.Debug.LogWarningFormat("Skipping saving entity {0} - because {1}", save, (memoryStream == null) ? "savecache is null" : "savecache is 0");
				}
				else
				{
					writer.Write((uint)memoryStream.Length);
					writer.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
					iEnts++;
				}
			}
		}
		timerWrite.Stop();
		if (!AndWait)
		{
			yield return CoroutineEx.waitForEndOfFrame;
		}
		timerDisk.Start();
		using (TimeWarning.New("SaveBackup", 100))
		{
			ShiftSaveBackups(strFilename);
		}
		using (TimeWarning.New("SaveDisk", 100))
		{
			try
			{
				string text = strFilename + ".new";
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				try
				{
					using (FileStream destination = File.OpenWrite(text))
					{
						SaveBuffer.Position = 0L;
						SaveBuffer.CopyTo(destination);
					}
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError("Couldn't write save file! We got an exception: " + ex);
					if (File.Exists(text))
					{
						File.Delete(text);
					}
					yield break;
				}
				File.Copy(text, strFilename, true);
				File.Delete(text);
			}
			catch (Exception ex2)
			{
				UnityEngine.Debug.LogError("Error when saving to disk: " + ex2);
				yield break;
			}
		}
		timerDisk.Stop();
		UnityEngine.Debug.LogFormat("Saved {0} ents, cache({1}), write({2}), disk({3}).", iEnts.ToString("N0"), timerCache.Elapsed.TotalSeconds.ToString("0.00"), timerWrite.Elapsed.TotalSeconds.ToString("0.00"), timerDisk.Elapsed.TotalSeconds.ToString("0.00"));
	}

	private static void ShiftSaveBackups(string fileName)
	{
		_003C_003Ec__DisplayClass12_0 _003C_003Ec__DisplayClass12_ = default(_003C_003Ec__DisplayClass12_0);
		_003C_003Ec__DisplayClass12_.fileName = fileName;
		int num = Mathf.Max(ConVar.Server.saveBackupCount, 2);
		if (!File.Exists(_003C_003Ec__DisplayClass12_.fileName))
		{
			return;
		}
		try
		{
			int num2 = 0;
			for (int i = 1; i <= num && File.Exists(_003C_003Ec__DisplayClass12_.fileName + "." + i); i++)
			{
				num2++;
			}
			string text = _003CShiftSaveBackups_003Eg__GetBackupName_007C12_0(num2 + 1, ref _003C_003Ec__DisplayClass12_);
			for (int num3 = num2; num3 > 0; num3--)
			{
				string text2 = _003CShiftSaveBackups_003Eg__GetBackupName_007C12_0(num3, ref _003C_003Ec__DisplayClass12_);
				if (num3 == num)
				{
					File.Delete(text2);
				}
				else if (File.Exists(text2))
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
					File.Move(text2, text);
				}
				text = text2;
			}
			File.Copy(_003C_003Ec__DisplayClass12_.fileName, text, true);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Error while backing up old saves: " + ex.Message);
			UnityEngine.Debug.LogException(ex);
			throw;
		}
	}

	private void Start()
	{
		StartCoroutine(SaveRegularly());
	}

	private IEnumerator SaveRegularly()
	{
		while (true)
		{
			yield return CoroutineEx.waitForSeconds(ConVar.Server.saveinterval);
			if (timedSave && timedSavePause <= 0)
			{
				yield return StartCoroutine(DoAutomatedSave());
			}
		}
	}

	[IteratorStateMachine(typeof(_003CDoAutomatedSave_003Ed__15))]
	private IEnumerator DoAutomatedSave(bool AndWait = false)
	{
		Interface.CallHook("OnServerSave");
		return new _003CDoAutomatedSave_003Ed__15(0)
		{
			_003C_003E4__this = this,
			AndWait = AndWait
		};
	}

	public static bool Save(bool AndWait)
	{
		if (SingletonComponent<SaveRestore>.Instance == null)
		{
			return false;
		}
		if (IsSaving)
		{
			return false;
		}
		IEnumerator enumerator = SingletonComponent<SaveRestore>.Instance.DoAutomatedSave(true);
		while (enumerator.MoveNext())
		{
		}
		return true;
	}
}
