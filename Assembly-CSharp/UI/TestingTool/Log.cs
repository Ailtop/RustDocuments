using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TestingTool
{
	public class Log : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private Button _copy;

		[SerializeField]
		private Button _clear;

		private readonly StringBuilder _sb = new StringBuilder();

		public void StartLog()
		{
			Application.logMessageReceived += ApplicationLogMessageReceived;
		}

		private void Awake()
		{
			_copy.onClick.AddListener(Copy);
			_clear.onClick.AddListener(Clear);
		}

		private void OnEnable()
		{
			_text.text = _sb.ToString();
		}

		private void Copy()
		{
			GUIUtility.systemCopyBuffer = _text.text;
		}

		private void Clear()
		{
			_sb.Clear();
			_text.text = string.Empty;
		}

		private void ApplicationLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			if (type != LogType.Log && type != LogType.Warning && _sb.Length <= 10000)
			{
				_sb.AppendFormat("[{0}] {1}\n{2}\n\n", type, condition, stackTrace);
			}
		}
	}
}
