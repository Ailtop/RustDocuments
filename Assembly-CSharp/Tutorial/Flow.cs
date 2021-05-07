using System.Collections;
using UnityEngine;

namespace Tutorial
{
	public class Flow : MonoBehaviour
	{
		private enum StartCondition
		{
			TimeOutAfterSpawn,
			RemainMonsters,
			EnterZone
		}

		[SerializeField]
		private Task[] _tasks;

		private StartCondition _startCondition;

		public void OnServerInitialized()
		{
		}

		private IEnumerator Process()
		{
			Task[] tasks = _tasks;
			foreach (Task task in tasks)
			{
				yield return task.Play();
			}
		}

		private IEnumerator Process1_1()
		{
			yield return null;
		}

		private IEnumerator Process1_2()
		{
			yield return null;
		}

		private IEnumerator Process1_3()
		{
			yield return null;
		}

		private IEnumerator Process1_4()
		{
			yield return null;
		}
	}
}
