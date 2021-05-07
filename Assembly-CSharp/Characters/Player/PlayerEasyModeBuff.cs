using Characters.Abilities;
using Data;
using UnityEngine;

namespace Characters.Player
{
	public class PlayerEasyModeBuff : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private AbilityAttacher _abilityAttacher;

		private bool _attached;

		private void Awake()
		{
			_abilityAttacher.Initialize(_character);
		}

		private void Update()
		{
			if (GameData.Settings.easyMode)
			{
				if (!_attached)
				{
					_abilityAttacher.StartAttach();
					_attached = true;
				}
			}
			else if (_attached)
			{
				_abilityAttacher.StopAttach();
				_attached = false;
			}
		}
	}
}
