using UnityEngine;

namespace Level.Npc.FieldNpcs
{
	[ExecuteAlways]
	public class Flip : MonoBehaviour
	{
		[SerializeField]
		private Transform _body;

		private void Awake()
		{
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				StartCoroutine(_003CAwake_003Eg__CRun_007C1_0());
			}
		}
	}
}
