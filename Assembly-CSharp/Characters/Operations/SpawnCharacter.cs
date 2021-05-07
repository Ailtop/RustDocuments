using Characters.AI;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public class SpawnCharacter : CharacterOperation
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private Transform _position;

		[SerializeField]
		private bool _masterSlaveLink;

		[SerializeField]
		private bool _setPlayerAsTarget;

		[SerializeField]
		private bool _containInWave;

		public override void Run(Character owner)
		{
			Character character = ((!(_position != null)) ? Object.Instantiate(_character) : Object.Instantiate(_character, _position.position, Quaternion.identity, Map.Instance.transform));
			if (_containInWave)
			{
				Map.Instance.waveContainer.Attach(character);
			}
			if (_setPlayerAsTarget)
			{
				AIController componentInChildren = character.GetComponentInChildren<AIController>();
				componentInChildren.target = Singleton<Service>.Instance.levelManager.player;
				componentInChildren.character.ForceToLookAt(componentInChildren.target.transform.position.x);
			}
			if (_masterSlaveLink)
			{
				Master componentInChildren2 = owner.GetComponentInChildren<Master>();
				Slave componentInChildren3 = character.GetComponentInChildren<Slave>();
				componentInChildren2.AddSlave(componentInChildren3);
				componentInChildren3.Initialize(componentInChildren2);
			}
		}
	}
}
