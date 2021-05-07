using System.Collections.Generic;
using System.Linq;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace Level
{
	[RequireComponent(typeof(Collider2D))]
	public class EnterZoneAbilityAttacher : MonoBehaviour
	{
		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		[SerializeField]
		private Character.Type[] _types;

		private ICollection<Character> _enteredCharacters;

		private void Awake()
		{
			_abilityComponent.Initialize();
			_enteredCharacters = new List<Character>();
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			Character component = collision.GetComponent<Character>();
			if (!(component == null) && (_types == null || _types.Contains(component.type)))
			{
				AttachTo(component);
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			Character component = collision.GetComponent<Character>();
			if (!(component == null) && (_types == null || _types.Contains(component.type)))
			{
				DetachFrom(component);
			}
		}

		private void AttachTo(Character who)
		{
			who.ability.Add(_abilityComponent.ability);
			_enteredCharacters.Add(who);
		}

		private void DetachFrom(Character who)
		{
			who.ability.Remove(_abilityComponent.ability);
			_enteredCharacters.Remove(who);
		}

		private void DetachAll()
		{
			foreach (Character enteredCharacter in _enteredCharacters)
			{
				DetachFrom(enteredCharacter);
			}
		}

		private void OnDestroy()
		{
			DetachAll();
		}

		private void OnDisable()
		{
			DetachAll();
		}
	}
}
