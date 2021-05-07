using System.Collections.Generic;
using Characters;

public class SummonWave : Wave
{
	private List<Character> _characters = new List<Character>();

	public List<Character> characters => _characters;

	public override void Initialize()
	{
	}

	public void Attach(Character character)
	{
		_characters.Add(character);
		character.health.onDiedTryCatch += delegate
		{
			Detach(character);
		};
		if (base.state != State.Spawned)
		{
			base.state = State.Spawned;
			_onSpawn?.Invoke();
		}
	}

	private void Detach(Character character)
	{
		_characters.Remove(character);
		if (_characters.Count == 0)
		{
			base.state = State.Cleared;
			_onClear?.Invoke();
		}
	}
}
