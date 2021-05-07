using UnityEngine;

namespace Level.Npc
{
	public class NpcLineText : MonoBehaviour
	{
		[SerializeField]
		private string _commonTextKey;

		[SerializeField]
		private string[] _levelTextKey;

		[SerializeField]
		[Range(0f, 100f)]
		private float _duration;

		[SerializeField]
		[MinMaxSlider(0f, 100f)]
		private Vector2 _preCoolTimeRange;

		[SerializeField]
		[MinMaxSlider(0f, 100f)]
		private Vector2 _coolTimeRange;

		[SerializeField]
		private LineText _lineText;

		private float _cooltime;

		private float _elapsed;

		private bool _canRun;

		private void Start()
		{
			if (_lineText == null)
			{
				_lineText = GetComponentInChildren<LineText>();
			}
			_cooltime = Random.Range(_preCoolTimeRange.x, _preCoolTimeRange.y);
		}

		public void Run()
		{
			string[] localizedStringArray = Lingua.GetLocalizedStringArray(_commonTextKey);
			if (localizedStringArray.Length < 0)
			{
				_elapsed = 0f;
				return;
			}
			string text = localizedStringArray.Random();
			_lineText.Display(text, _duration);
			_cooltime = Random.Range(_coolTimeRange.x, _coolTimeRange.y);
			_elapsed = 0f;
		}

		public void Run(string text)
		{
			_lineText.Display(text, _duration);
			_cooltime = Random.Range(_coolTimeRange.x, _coolTimeRange.y);
			_elapsed = 0f;
		}

		private void Update()
		{
			_elapsed += Chronometer.global.deltaTime;
			if (_elapsed > _cooltime)
			{
				_canRun = true;
			}
			else
			{
				_canRun = false;
			}
			if (_canRun)
			{
				Run();
			}
		}
	}
}
