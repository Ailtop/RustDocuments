using UnityEngine;

namespace Characters.AI
{
	public class MagicianBallAssigner : MonoBehaviour
	{
		[SerializeField]
		private float _radius;

		private void Awake()
		{
			Assign();
		}

		private void Assign()
		{
			if (base.transform.childCount <= 0)
			{
				Debug.LogError(base.name + " has no child");
				return;
			}
			int num = 360 / base.transform.childCount;
			Vector3 vector = Vector2.up * _radius;
			for (int i = 0; i < base.transform.childCount; i++)
			{
				base.transform.GetChild(i).position = base.transform.position + vector;
				base.transform.GetChild(i).rotation = Quaternion.Euler(0f, 0f, num * i + 90);
				vector = Quaternion.Euler(0f, 0f, num) * vector;
			}
		}
	}
}
