using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using CutScenes;
using Level;
using Level.Npc;
using Level.Npc.FieldNpcs;
using SkulStories;
using Steamworks;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UserInput;

namespace Data
{
	public static class GameData
	{
		public interface IEditorDrawer
		{
			void DrawEditor();
		}

		public class Generic : IEditorDrawer
		{
			public class Tutorial : IEditorDrawer
			{
				private BoolData _played;

				private bool _isPlaying;

				public Tutorial()
				{
					_played = new BoolData("Tutorial/_played", true);
				}

				public void Start()
				{
					_isPlaying = true;
				}

				public void Stop()
				{
					_isPlaying = false;
				}

				public void End()
				{
					_isPlaying = false;
					SetData(true);
				}

				public bool isPlayed()
				{
					return _played.value;
				}

				public bool isPlaying()
				{
					return _isPlaying;
				}

				internal void Save()
				{
					_played.Save();
				}

				internal void Reset()
				{
					_played.Reset();
				}

				private void SetData(bool value)
				{
					_played.value = value;
					Save();
				}

				public void DrawEditor()
				{
				}
			}

			private Tutorial _tutorial;

			private StringData _lastPlayedVersion;

			private BoolData _playedTutorialDuringEA;

			public static readonly Generic instance = new Generic();

			private const string _playedTutorialDuringEA_DataPath = "Generic/tutorialPlayed";

			public static Tutorial tutorial => instance._tutorial;

			public static string lastPlayedVersion
			{
				get
				{
					return instance._lastPlayedVersion.value;
				}
				set
				{
					instance._lastPlayedVersion.value = value;
				}
			}

			public static bool playedTutorialDuringEA
			{
				get
				{
					return instance._playedTutorialDuringEA.value;
				}
				set
				{
					instance._playedTutorialDuringEA.value = value;
				}
			}

			public void Initialize()
			{
				_lastPlayedVersion = new StringData("Generic/_lastPlayedVersion", Application.version, true);
				_tutorial = new Tutorial();
				_playedTutorialDuringEA = new BoolData("Generic/tutorialPlayed", true);
			}

			public void DrawEditor()
			{
			}

			public static void ResetAll()
			{
				instance._playedTutorialDuringEA.Reset();
				instance._tutorial.Reset();
				SaveAll();
			}

			public static void SaveAll()
			{
				instance._playedTutorialDuringEA.Save();
				instance._tutorial.Save();
			}
		}

		public class Settings : IEditorDrawer
		{
			public static readonly Settings instance = new Settings();

			private StringData _keyBindings;

			private BoolData _arrowDashEnabled;

			private BoolData _lightEnabled;

			private FloatData _masterVolume;

			private BoolData _musicEnabled;

			private FloatData _musicVolume;

			private BoolData _sfxEnabled;

			private FloatData _sfxVolume;

			private IntData _language;

			private FloatData _cameraShakeIntensity;

			private FloatData _vibrationIntensity;

			private IntData _particleQuality;

			private BoolData _easyMode;

			private BoolData _showTimer;

			public static string keyBindings
			{
				get
				{
					return instance._keyBindings.value;
				}
				set
				{
					instance._keyBindings.value = value;
				}
			}

			public static bool arrowDashEnabled
			{
				get
				{
					return instance._arrowDashEnabled.value;
				}
				set
				{
					instance._arrowDashEnabled.value = value;
				}
			}

			public static bool lightEnabled
			{
				get
				{
					return instance._lightEnabled.value;
				}
				set
				{
					instance._lightEnabled.value = value;
				}
			}

			public static float masterVolume
			{
				get
				{
					return instance._masterVolume.value;
				}
				set
				{
					instance._masterVolume.value = value;
				}
			}

			public static bool musicEnabled
			{
				get
				{
					return instance._musicEnabled.value;
				}
				set
				{
					instance._musicEnabled.value = value;
				}
			}

			public static float musicVolume
			{
				get
				{
					return instance._musicVolume.value;
				}
				set
				{
					instance._musicVolume.value = value;
				}
			}

			public static bool sfxEnabled
			{
				get
				{
					return instance._sfxEnabled.value;
				}
				set
				{
					instance._sfxEnabled.value = value;
				}
			}

			public static float sfxVolume
			{
				get
				{
					return instance._sfxVolume.value;
				}
				set
				{
					instance._sfxVolume.value = value;
				}
			}

			public static int language
			{
				get
				{
					return instance._language.value;
				}
				set
				{
					instance._language.value = value;
				}
			}

			public static float cameraShakeIntensity
			{
				get
				{
					return instance._cameraShakeIntensity.value;
				}
				set
				{
					instance._cameraShakeIntensity.value = value;
				}
			}

			public static float vibrationIntensity
			{
				get
				{
					return instance._vibrationIntensity.value;
				}
				set
				{
					instance._vibrationIntensity.value = value;
				}
			}

			public static int particleQuality
			{
				get
				{
					return instance._particleQuality.value;
				}
				set
				{
					instance._particleQuality.value = value;
				}
			}

			public static bool easyMode
			{
				get
				{
					return instance._easyMode.value;
				}
				set
				{
					instance._easyMode.value = value;
				}
			}

			public static bool showTimer
			{
				get
				{
					return instance._showTimer.value;
				}
				set
				{
					instance._showTimer.value = value;
				}
			}

			public static void Save()
			{
				instance._keyBindings.Save();
				instance._arrowDashEnabled.Save();
				instance._lightEnabled.Save();
				instance._masterVolume.Save();
				instance._musicEnabled.Save();
				instance._musicVolume.Save();
				instance._sfxEnabled.Save();
				instance._sfxVolume.Save();
				instance._language.Save();
				instance._cameraShakeIntensity.Save();
				instance._vibrationIntensity.Save();
				instance._particleQuality.Save();
				instance._easyMode.Save();
				instance._showTimer.Save();
				PlayerPrefs.Save();
				FileBasedPrefs.SaveManually();
			}

			public void Initialize()
			{
				_keyBindings = new StringData("Settings/keyBindings", string.Empty, true);
				KeyMapper.Bind(_keyBindings.value);
				_arrowDashEnabled = new BoolData("Settings/arrowDashEnabled", false, false);
				bool defaultValue = Application.isConsolePlatform || SystemInfo.systemMemorySize >= 4000 || SystemInfo.graphicsMemorySize >= 1000;
				_lightEnabled = new BoolData("Settings/lightEnabled", defaultValue, false);
				Light2D.lightEnabled = _lightEnabled.value;
				_masterVolume = new FloatData("Settings/masterVolume", 0.8f);
				_musicEnabled = new BoolData("Settings/musicEnabled", true, false);
				_musicVolume = new FloatData("Settings/musicVolume", 0.6f);
				_sfxEnabled = new BoolData("Settings/sfxEnabled", true, false);
				_sfxVolume = new FloatData("Settings/sfxVolume", 0.8f);
				_language = new IntData("Settings/language", -1);
				_cameraShakeIntensity = new FloatData("Settings/cameraShakeIntensity", 0.5f);
				_vibrationIntensity = new FloatData("Settings/vibrationIntensity", 0.5f);
				_particleQuality = new IntData("Settings/particleQuality", 3);
				_easyMode = new BoolData("Settings/easyMode", false, false);
				_showTimer = new BoolData("Settings/showTimer", false, false);
			}

			public void DrawEditor()
			{
			}
		}

		public class Currency : IEditorDrawer
		{
			public enum Type
			{
				Gold,
				DarkQuartz,
				Bone
			}

			public delegate void OnEarnDelegate(int amount);

			public static Currency gold;

			public static Currency darkQuartz;

			public static Currency bone;

			public static EnumArray<Type, Currency> currencies;

			public readonly Sum<double> multiplier = new Sum<double>(1.0);

			private readonly string _key;

			private readonly IntData _balance;

			private readonly IntData _income;

			public readonly string colorCode;

			private double _remainder;

			public int balance
			{
				get
				{
					return _balance.value;
				}
				set
				{
					_balance.value = value;
				}
			}

			public int income
			{
				get
				{
					return _income.value;
				}
				set
				{
					_income.value = value;
				}
			}

			public event OnEarnDelegate onEarn;

			public static void Initialize()
			{
				gold = new Currency("gold", "FFDE37");
				darkQuartz = new Currency("darkQuartz", "9159DB");
				bone = new Currency("bone", "959595");
				currencies = new EnumArray<Type, Currency>(gold, darkQuartz, bone);
			}

			public static void ResetAll()
			{
				gold.Reset();
				darkQuartz.Reset();
				bone.Reset();
				SaveAll();
			}

			public static void SaveAll()
			{
				gold.Save();
				darkQuartz.Save();
				bone.Save();
				PlayerPrefs.Save();
				FileBasedPrefs.SaveManually();
			}

			private Currency(string key, string colorCode)
			{
				_key = key;
				_balance = new IntData("Currency/" + key + "/balance");
				_income = new IntData("Currency/" + key + "/income");
				this.colorCode = colorCode;
			}

			public void DrawEditor()
			{
			}

			public void Save()
			{
				_balance.Save();
				_income.Save();
			}

			public void Reset()
			{
				balance = 0;
				income = 0;
			}

			public void Earn(double amount)
			{
				double num = multiplier.total * amount;
				int num2 = (int)num;
				_remainder += num - (double)num2;
				if (_remainder >= 1.0)
				{
					int num3 = (int)_remainder;
					_remainder -= num3;
					num2 += num3;
				}
				balance += num2;
				income += num2;
				this.onEarn?.Invoke(num2);
			}

			public void Earn(int amount)
			{
				Earn((double)amount);
			}

			public bool Has(int amount)
			{
				return balance >= amount;
			}

			public bool Consume(int amount)
			{
				if (!Has(amount))
				{
					return false;
				}
				balance -= amount;
				return true;
			}
		}

		public class Progress : IEditorDrawer
		{
			public class WitchMastery : IEditorDrawer
			{
				public class Bonuses : IEditorDrawer
				{
					public readonly string key;

					private bool _foldout = true;

					private IntData[] _datas = new IntData[4];

					public IntData this[int index] => _datas[index];

					public Bonuses(string key)
					{
						this.key = key;
						for (int i = 0; i < 4; i++)
						{
							_datas[i] = new IntData(string.Format("{0}/{1}/{2}/{3}", "Progress", "WitchMastery", key, i));
						}
					}

					public void Save()
					{
						IntData[] datas = _datas;
						for (int i = 0; i < datas.Length; i++)
						{
							datas[i].Save();
						}
					}

					public void Reset()
					{
						IntData[] datas = _datas;
						for (int i = 0; i < datas.Length; i++)
						{
							datas[i].Reset();
						}
					}

					public int GerFormerRefundAmount()
					{
						int num = 0;
						for (int i = 0; i < _datas.Length; i++)
						{
							num += WitchMasteryFormerPrice.GeRefundAmount(i, _datas[i].value);
						}
						return num;
					}

					public void DrawEditor()
					{
					}
				}

				private const int count = 4;

				public readonly Bonuses skull = new Bonuses("skull");

				public readonly Bonuses body = new Bonuses("body");

				public readonly Bonuses soul = new Bonuses("soul");

				private bool _foldout;

				public void Save()
				{
					skull.Save();
					body.Save();
					soul.Save();
				}

				public void Reset()
				{
					skull.Reset();
					body.Reset();
					soul.Reset();
				}

				public void RefundFormer()
				{
					int num = 0;
					num += skull.GerFormerRefundAmount();
					num += body.GerFormerRefundAmount();
					num += soul.GerFormerRefundAmount();
					if (num != 0)
					{
						Debug.Log($"Old witch mastery is refunded. Refunded dark quartz amount : {num}");
						Currency.darkQuartz.balance += num;
						Reset();
					}
				}

				public void DrawEditor()
				{
				}
			}

			public class BoolDataEnumArray<T> : IEditorDrawer, IEnumerable<KeyValuePair<T, BoolData>>, IEnumerable where T : Enum
			{
				private readonly Dictionary<T, BoolData> _dictionary = new Dictionary<T, BoolData>();

				private readonly string _foldoutLabel;

				private bool _foldout;

				public BoolDataEnumArray(string foldoutLabel)
				{
					_foldoutLabel = foldoutLabel;
					T[] values = EnumValues<T>.Values;
					foreach (T val in values)
					{
						if (_dictionary.ContainsKey(val))
						{
							Debug.LogError($"The key {val} is duplicated.");
						}
						else
						{
							_dictionary.Add(val, new BoolData(string.Format("{0}/{1}", "BoolDataEnumArray", val)));
						}
					}
				}

				public void SaveAll()
				{
					foreach (BoolData value in _dictionary.Values)
					{
						value.Save();
					}
				}

				public void ResetAll()
				{
					foreach (BoolData value in _dictionary.Values)
					{
						value.Reset();
					}
				}

				public bool GetData(T key)
				{
					return _dictionary[key].value;
				}

				public void SetData(T key, bool value)
				{
					_dictionary[key].value = value;
				}

				public void DrawEditor()
				{
				}

				public IEnumerator<KeyValuePair<T, BoolData>> GetEnumerator()
				{
					return _dictionary.GetEnumerator();
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					return _dictionary.GetEnumerator();
				}
			}

			public static readonly Progress instance = new Progress();

			private BoolDataEnumArray<Level.Npc.FieldNpcs.NpcType> _fieldNpcEncountered;

			private BoolDataEnumArray<SpecialMap.Type> _specialMapEncountered;

			private BoolDataEnumArray<CutScenes.Key> _cutscene;

			private BoolDataEnumArray<SkulStories.Key> _skulstory;

			private WitchMastery _witch;

			private IntData _playTime;

			private IntData _deaths;

			private IntData _kills;

			private IntData _eliteKills;

			private IntData _totalAdventurerKills;

			private IntData _gainedDarkcite;

			private IntData _housingPoint;

			private IntData _housingSeen;

			private BoolData _reassembleUsed;

			private BoolData _arachneTutorial;

			private BoolData _foxRescued;

			private BoolData _ogreRescued;

			private BoolData _druidRescued;

			private BoolData _deathknightRescued;

			private EnumArray<Level.Npc.NpcType, BoolData> _rescuedByNpcType;

			public static WitchMastery witch => instance._witch;

			public static BoolDataEnumArray<Level.Npc.FieldNpcs.NpcType> fieldNpcEncountered => instance._fieldNpcEncountered;

			public static BoolDataEnumArray<SpecialMap.Type> specialMapEncountered => instance._specialMapEncountered;

			public static BoolDataEnumArray<CutScenes.Key> cutscene => instance._cutscene;

			public static BoolDataEnumArray<SkulStories.Key> skulstory => instance._skulstory;

			public static int playTime
			{
				get
				{
					return instance._playTime.value;
				}
				set
				{
					instance._playTime.value = value;
				}
			}

			public static int deaths
			{
				get
				{
					return instance._deaths.value;
				}
				set
				{
					instance._deaths.value = value;
				}
			}

			public static int kills
			{
				get
				{
					return instance._kills.value;
				}
				set
				{
					instance._kills.value = value;
				}
			}

			public static int totalAdventurerKills
			{
				get
				{
					return instance._totalAdventurerKills.value;
				}
				set
				{
					instance._totalAdventurerKills.value = value;
				}
			}

			public static int eliteKills
			{
				get
				{
					return instance._eliteKills.value;
				}
				set
				{
					instance._eliteKills.value = value;
				}
			}

			public static int gainedDarkcite
			{
				get
				{
					return instance._gainedDarkcite.value;
				}
				set
				{
					instance._gainedDarkcite.value = value;
				}
			}

			public static int housingPoint
			{
				get
				{
					return instance._housingPoint.value;
				}
				set
				{
					instance._housingPoint.value = value;
				}
			}

			public static int housingSeen
			{
				get
				{
					return instance._housingSeen.value;
				}
				set
				{
					instance._housingSeen.value = value;
				}
			}

			public static bool reassembleUsed
			{
				get
				{
					return instance._reassembleUsed.value;
				}
				set
				{
					instance._reassembleUsed.value = value;
				}
			}

			public static bool arachneTutorial
			{
				get
				{
					return instance._arachneTutorial.value;
				}
				set
				{
					instance._arachneTutorial.value = value;
				}
			}

			public static bool foxRescued
			{
				get
				{
					return instance._foxRescued.value;
				}
				set
				{
					instance._foxRescued.value = value;
				}
			}

			public static bool ogreRescued
			{
				get
				{
					return instance._ogreRescued.value;
				}
				set
				{
					instance._ogreRescued.value = value;
				}
			}

			public static bool druidRescued
			{
				get
				{
					return instance._druidRescued.value;
				}
				set
				{
					instance._druidRescued.value = value;
				}
			}

			public static bool deathknightRescued
			{
				get
				{
					return instance._deathknightRescued.value;
				}
				set
				{
					instance._deathknightRescued.value = value;
				}
			}

			public static bool GetRescued(Level.Npc.NpcType npcType)
			{
				return instance._rescuedByNpcType[npcType].value;
			}

			public static void SetRescued(Level.Npc.NpcType npcType, bool value)
			{
				instance._rescuedByNpcType[npcType].value = value;
			}

			public static void ResetAll()
			{
				instance._witch.Reset();
				instance._fieldNpcEncountered.ResetAll();
				instance._specialMapEncountered.ResetAll();
				instance._cutscene.ResetAll();
				instance._skulstory.ResetAll();
				instance._playTime.Reset();
				instance._deaths.Reset();
				instance._kills.Reset();
				instance._eliteKills.Reset();
				instance._totalAdventurerKills.Reset();
				instance._gainedDarkcite.Reset();
				instance._housingPoint.Reset();
				instance._housingSeen.Reset();
				instance._reassembleUsed.Reset();
				instance._arachneTutorial.Reset();
				instance._foxRescued.Reset();
				instance._ogreRescued.Reset();
				instance._druidRescued.Reset();
				instance._deathknightRescued.Reset();
				SaveAll();
			}

			public static void ResetNonpermaAll()
			{
				playTime = 0;
				kills = 0;
				eliteKills = 0;
				gainedDarkcite = 0;
				reassembleUsed = false;
				fieldNpcEncountered.ResetAll();
				specialMapEncountered.ResetAll();
				SaveAll();
			}

			public static void SaveAll()
			{
				instance._playTime.Save();
				instance._deaths.Save();
				instance._kills.Save();
				instance._eliteKills.Save();
				instance._totalAdventurerKills.Save();
				instance._gainedDarkcite.Save();
				instance._housingPoint.Save();
				instance._housingSeen.Save();
				instance._reassembleUsed.Save();
				instance._arachneTutorial.Save();
				instance._foxRescued.Save();
				instance._ogreRescued.Save();
				instance._druidRescued.Save();
				instance._deathknightRescued.Save();
				witch.Save();
				fieldNpcEncountered.SaveAll();
				specialMapEncountered.SaveAll();
				cutscene.SaveAll();
				skulstory.SaveAll();
				PlayerPrefs.Save();
				FileBasedPrefs.SaveManually();
			}

			public void Initialize()
			{
				_witch = new WitchMastery();
				_fieldNpcEncountered = new BoolDataEnumArray<Level.Npc.FieldNpcs.NpcType>("fieldNpcEncountered");
				_specialMapEncountered = new BoolDataEnumArray<SpecialMap.Type>("specialMapEncountered");
				_cutscene = new BoolDataEnumArray<CutScenes.Key>("CutScenes");
				_skulstory = new BoolDataEnumArray<SkulStories.Key>("SkulStories");
				_playTime = new IntData("Progress/playTime");
				_deaths = new IntData("Progress/deaths");
				_kills = new IntData("Progress/kills");
				_eliteKills = new IntData("Progress/eliteKills");
				_totalAdventurerKills = new IntData("Progress/totalAdventurerKills");
				_gainedDarkcite = new IntData("Progress/gainedDarkcite");
				_housingPoint = new IntData("Progress/housingPoint");
				_housingSeen = new IntData("Progress/_housingSeen");
				_reassembleUsed = new BoolData("Progress/reassembleUsed");
				_arachneTutorial = new BoolData("Progress/arachneTutorial");
				_foxRescued = new BoolData("Progress/foxRescued");
				_ogreRescued = new BoolData("Progress/ogreRescued");
				_druidRescued = new BoolData("Progress/druidRescued");
				_deathknightRescued = new BoolData("Progress/deathknightRescued");
				_rescuedByNpcType = new EnumArray<Level.Npc.NpcType, BoolData>(null, _foxRescued, _ogreRescued, _druidRescued, _deathknightRescued);
				if (Application.isPlaying)
				{
					CoroutineProxy.instance.StartCoroutine(_003CInitialize_003Eg__CCountPlayTime_007C80_0());
				}
			}

			public void DrawEditor()
			{
			}
		}

		public static class Gear
		{
			private const string key = "gear";

			private const string unlockedKey = "gear/unlocked";

			public static bool IsUnlocked(string typeName, string name)
			{
				return FileBasedPrefs.GetBool("gear/unlocked/" + typeName + "/" + name);
			}

			public static void SetUnlocked(string typeName, string name, bool value)
			{
				FileBasedPrefs.SetBool("gear/unlocked/" + typeName + "/" + name, value);
				PlayerPrefs.Save();
				FileBasedPrefs.SaveManually();
			}

			public static void ResetAll()
			{
				FileBasedPrefs.DeleteKey((string key) => key.StartsWith(key));
			}
		}

		private const int _currentVersion = 6;

		private static StringData _steamID;

		private static IntData _version;

		public static int version
		{
			get
			{
				return _version.value;
			}
			set
			{
				_version.value = value;
			}
		}

		public static void Initialize()
		{
			string value = SteamUser.GetSteamID().m_SteamID.ToString();
			_version = new IntData("version", true);
			int value2 = _version.value;
			_steamID = new StringData("steamID", true);
			_version.value = 6;
			_steamID.value = value;
			Generic.instance.Initialize();
			Currency.Initialize();
			Progress.instance.Initialize();
			Settings.instance.Initialize();
			if (value2 > 0 && value2 <= 2)
			{
				Progress.witch.RefundFormer();
			}
			if (value2 > 0 && value2 <= 3)
			{
				Settings.particleQuality++;
			}
			if (value2 > 0 && value2 <= 4)
			{
				if (Settings.language == 4)
				{
					Settings.language = 5;
				}
				else if (Settings.language == 5)
				{
					Settings.language = 4;
				}
			}
			if (value2 == 5 && Application.systemLanguage == SystemLanguage.German)
			{
				Settings.language = 5;
			}
			FileBasedPrefs.SaveManually();
		}
	}
}
