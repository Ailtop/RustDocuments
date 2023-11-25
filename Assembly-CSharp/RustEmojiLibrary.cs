using System;
using System.Collections.Generic;
using System.IO;
using ConVar;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class RustEmojiLibrary : BaseScriptableObject
{
	public enum EmojiType
	{
		Core = 0,
		Item = 1,
		Server = 2
	}

	[Serializable]
	public struct EmojiSource
	{
		public string Name;

		public EmojiType Type;

		public EmojiResult[] Emoji;

		public SteamDLCItem RequiredDLC;

		public SteamInventoryItem RequiredItem;

		public uint ServerCrc;

		public FileStorage.Type ServerFileType;

		public bool HasSkinTone => Emoji.Length > 1;

		public EmojiResult GetEmojiIndex(int index)
		{
			return Emoji[Mathf.Clamp(index, 0, Emoji.Length - 1)];
		}

		public bool CanBeUsedBy(BasePlayer p)
		{
			if (RequiredDLC != null && !RequiredDLC.CanUse(p))
			{
				return false;
			}
			if (RequiredItem != null && !RequiredItem.HasUnlocked(p.userID))
			{
				return false;
			}
			return true;
		}

		public bool StringMatch(string input, out int index)
		{
			index = 0;
			if (Name.Equals(input, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
			for (int i = 0; i < Emoji.Length; i++)
			{
				if ($"{Name}+{i}".Equals(input, StringComparison.CurrentCultureIgnoreCase))
				{
					index = i;
					return true;
				}
			}
			return false;
		}
	}

	public struct ServerEmojiConfig
	{
		public uint CRC;

		public FileStorage.Type FileType;
	}

	public static NetworkableId EmojiStorageNetworkId = new NetworkableId(0uL);

	[HideInInspector]
	public RustEmojiConfig[] Configs;

	public RenderTextureDescriptor RenderTextureDesc = new RenderTextureDescriptor(256, 256, GraphicsFormat.R8G8B8A8_UNorm, 0);

	public int InitialPoolSize = 10;

	private List<EmojiSource> all = new List<EmojiSource>();

	private List<EmojiSource> conditionalAccessOnly = new List<EmojiSource>();

	public GameObjectRef VideoPlayerRef;

	private static RustEmojiLibrary _instance = null;

	private static bool hasPrewarmed = false;

	private const long MAX_FILE_SIZE_BYTES = 250000L;

	public const int MAX_TEX_SIZE_PIXELS = 256;

	public static Dictionary<string, ServerEmojiConfig> allServerEmoji = new Dictionary<string, ServerEmojiConfig>();

	private static bool hasLoaded = false;

	[NonSerialized]
	public static List<string> cachedServerList = new List<string>();

	public static RustEmojiLibrary Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<RustEmojiLibrary>("assets/content/ui/gameui/emoji/rustemojilibrary.asset");
				_instance.Prewarm();
			}
			return _instance;
		}
	}

	private void Prewarm()
	{
		if (hasPrewarmed)
		{
			return;
		}
		hasPrewarmed = true;
		all.Clear();
		conditionalAccessOnly.Clear();
		RustEmojiConfig[] configs = Configs;
		foreach (RustEmojiConfig rustEmojiConfig in configs)
		{
			if (!rustEmojiConfig.Hide)
			{
				all.Add(rustEmojiConfig.Source);
				if (rustEmojiConfig.Source.RequiredItem != null || rustEmojiConfig.Source.RequiredDLC != null)
				{
					conditionalAccessOnly.Add(rustEmojiConfig.Source);
				}
			}
		}
		foreach (ItemDefinition item in ItemManager.itemList)
		{
			if (!item.hidden && !(item.iconSprite == null))
			{
				all.Add(new EmojiSource
				{
					Name = item.shortname,
					Type = EmojiType.Item,
					Emoji = new EmojiResult[1]
					{
						new EmojiResult
						{
							Sprite = item.iconSprite
						}
					}
				});
			}
		}
	}

	public bool TryGetEmoji(string key, out EmojiSource er, out int skinVariantIndex, out int allIndex, bool serverSide = false)
	{
		er = default(EmojiSource);
		skinVariantIndex = 0;
		allIndex = 0;
		Prewarm();
		foreach (EmojiSource item in serverSide ? conditionalAccessOnly : all)
		{
			if (item.Type != EmojiType.Server || !Global.blockServerEmoji)
			{
				if (item.StringMatch(key, out skinVariantIndex))
				{
					er = item;
					return true;
				}
				allIndex++;
			}
		}
		return false;
	}

	public static void FindAllServerEmoji()
	{
		if (hasLoaded)
		{
			return;
		}
		hasLoaded = true;
		string serverFolder = Server.GetServerFolder("serveremoji");
		if (!Directory.Exists(serverFolder))
		{
			return;
		}
		foreach (string item in Directory.EnumerateFiles(serverFolder))
		{
			try
			{
				FileInfo fileInfo = new FileInfo(item);
				bool flag = fileInfo.Extension == ".png";
				bool flag2 = fileInfo.Extension == ".jpg";
				if (!CheckByteArray(fileInfo.Length))
				{
					Debug.Log($"{serverFolder} file size is too big for emoji, max file size is {250000L} bytes");
					continue;
				}
				byte[] data = File.ReadAllBytes(item);
				if (flag && !ImageProcessing.IsValidPNG(data, 256))
				{
					Debug.Log(item + " is an invalid png");
				}
				else if (flag2 && !ImageProcessing.IsValidJPG(data, 256))
				{
					Debug.Log(item + " is an invalid jpg");
				}
				else if (flag || flag2)
				{
					FileStorage.Type type = FileStorage.Type.jpg;
					if (flag)
					{
						type = FileStorage.Type.png;
					}
					uint cRC = FileStorage.server.Store(data, type, EmojiStorageNetworkId);
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
					if (!allServerEmoji.ContainsKey(fileNameWithoutExtension))
					{
						allServerEmoji.Add(fileNameWithoutExtension, new ServerEmojiConfig
						{
							CRC = cRC,
							FileType = type
						});
					}
				}
			}
			catch (Exception arg)
			{
				Debug.Log($"Exception loading {item} - {arg}");
			}
		}
		cachedServerList = new List<string>();
		foreach (KeyValuePair<string, ServerEmojiConfig> item2 in allServerEmoji)
		{
			cachedServerList.Add(item2.Key);
			cachedServerList.Add(item2.Value.CRC.ToString());
			List<string> list = cachedServerList;
			int fileType = (int)item2.Value.FileType;
			list.Add(fileType.ToString());
		}
	}

	public static void ResetServerEmoji()
	{
		hasLoaded = false;
		allServerEmoji.Clear();
		FindAllServerEmoji();
	}

	private static bool CheckByteArray(long arrayLength)
	{
		return arrayLength <= 250000;
	}

	public static bool CheckByteArray(int arrayLength)
	{
		return (long)arrayLength <= 250000L;
	}
}
