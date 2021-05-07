using System.Collections;
using UnityEngine;

namespace Characters.AI
{
	public class ReviveScarecrowOnDie : MonoBehaviour
	{
		[SerializeField]
		private Character[] _target;

		[SerializeField]
		private Character _origin;

		private void Start()
		{
			Character[] target = _target;
			foreach (Character character in target)
			{
				character.health.onDied += delegate
				{
					StartCoroutine(Revive(character));
				};
			}
		}

		private IEnumerator Revive(Character target)
		{
			yield return Chronometer.global.WaitForSeconds(3f);
			Character spawned = Object.Instantiate(_origin, target.transform.position, Quaternion.identity, base.transform);
			ScareCrawAI componentInChildren = spawned.GetComponentInChildren<ScareCrawAI>();
			spawned.ForceToLookAt(target.lookingDirection);
			spawned.gameObject.SetActive(true);
			spawned.health.onDied += delegate
			{
				StartCoroutine(Revive(spawned));
			};
			componentInChildren.Appear();
			Object.Destroy(target.gameObject);
		}
	}
}
