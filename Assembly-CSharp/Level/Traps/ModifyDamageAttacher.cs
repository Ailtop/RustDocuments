using Characters;
using Characters.Abilities;
using UnityEngine;

namespace Level.Traps
{
	[RequireComponent(typeof(Character))]
	public class ModifyDamageAttacher : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		[AbilityAttacher.Subcomponent]
		private AbilityAttacher _abilityAttacher;

		private void Start()
		{
			_abilityAttacher.Initialize(_character);
			_abilityAttacher.StartAttach();
		}

		private void OnDestroy()
		{
			_abilityAttacher.StopAttach();
		}
	}
}
