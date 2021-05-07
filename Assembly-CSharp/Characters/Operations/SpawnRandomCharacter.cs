using Characters.AI;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public sealed class SpawnRandomCharacter : CharacterOperation
	{
		[SerializeField]
		private Character[] _characterPrefabs;

		[SerializeField]
		private Transform _position;

		[SerializeField]
		private bool _setPlayerAsTarget;

		[SerializeField]
		private bool _containInWave;

		public override void Run(Character owner)
		{
			if (_characterPrefabs.Length != 0)
			{
				Character original = _characterPrefabs.Random();
				Character character;
				if (_containInWave)
				{
					character = ((!(_position != null)) ? Object.Instantiate(original) : Object.Instantiate(original, _position.position, Quaternion.identity));
					Map.Instance.waveContainer.Attach(character);
				}
				else
				{
					character = Object.Instantiate(original, _position.position, Quaternion.identity);
					character.transform.parent = Map.Instance.transform;
				}
				if (_setPlayerAsTarget)
				{
					AIController componentInChildren = character.GetComponentInChildren<AIController>();
					componentInChildren.target = Singleton<Service>.Instance.levelManager.player;
					componentInChildren.character.ForceToLookAt(componentInChildren.target.transform.position.x);
				}
			}
		}
	}
}
