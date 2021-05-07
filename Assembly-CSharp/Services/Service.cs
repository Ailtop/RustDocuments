using Data;
using Level;
using Singletons;
using Steamworks;
using UI;
using UI.TestingTool;
using UnityEngine;

namespace Services
{
	public sealed class Service : Singleton<Service>
	{
		[SerializeField]
		[GetComponent]
		private ControllerVibration _controllerVibration;

		[SerializeField]
		[GetComponent]
		private LevelManager _levelManager;

		[SerializeField]
		[GetComponent]
		private GearManager _gearManager;

		[SerializeField]
		[GetComponent]
		private FloatingTextSpawner _floatingTextSpawner;

		[SerializeField]
		[GetComponent]
		private LineTextManager _lineTextManager;

		[SerializeField]
		[GetComponent]
		private FadeInOut _fadeInOut;

		[SerializeField]
		[GetComponent]
		private Steam _steam;

		[SerializeField]
		private DebugPanel _debugger;

		public static bool quitting { get; private set; }

		public ControllerVibration controllerVibation => _controllerVibration;

		public LevelManager levelManager => _levelManager;

		public GearManager gearManager => _gearManager;

		public FloatingTextSpawner floatingTextSpawner => _floatingTextSpawner;

		public LineTextManager lineTextManager => _lineTextManager;

		public FadeInOut fadeInOut => _fadeInOut;

		protected override void Awake()
		{
			base.Awake();
			Application.quitting += delegate
			{
				quitting = true;
			};
			_debugger.StartLog();
			_steam.Initialize();
			GameData.Initialize();
			Application.targetFrameRate = 80;
			Physics2D.autoSyncTransforms = false;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F11) && Panel.steamIDWhitelist.Contains(SteamUser.GetSteamID().m_SteamID))
			{
				_debugger.gameObject.SetActive(!_debugger.gameObject.activeSelf);
			}
			Physics2D.SyncTransforms();
		}

		private void LateUpdate()
		{
			Physics2D.SyncTransforms();
		}
	}
}
