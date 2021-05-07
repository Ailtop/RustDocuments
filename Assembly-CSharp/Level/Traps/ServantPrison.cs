using Characters;
using Characters.Abilities;
using UnityEngine;

namespace Level.Traps
{
	public class ServantPrison : MonoBehaviour
	{
		private readonly GetInvulnerable _getInvulnerable = new GetInvulnerable
		{
			duration = 0.5f
		};

		[SerializeField]
		private Prop _prop;

		[SerializeField]
		private Character[] _characters;

		private void Awake()
		{
			_prop.onDestroy += OnPropDestroy;
		}

		private void OnPropDestroy()
		{
			for (int i = 0; i < _characters.Length; i++)
			{
				Character character = _characters[i];
				Map.Instance.waveContainer.Attach(character);
				character.gameObject.SetActive(true);
				character.ability.Add(_getInvulnerable);
			}
		}
	}
}
