using Characters;
using Services;
using Singletons;
using UnityEngine;

public class CharacterNeverDie : MonoBehaviour
{
	[SerializeField]
	private Character _character;

	private void Start()
	{
		if (_character == null)
		{
			_character = Singleton<Service>.Instance.levelManager.player;
		}
		_character.health.onDie += OnDie;
	}

	private void OnDie()
	{
		_character.health.Heal(1.0);
	}

	public void Remove()
	{
		_character.health.onDie -= OnDie;
	}
}
