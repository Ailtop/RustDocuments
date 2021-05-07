using UnityEngine;

namespace Level
{
	public class RestoreMapLight : MonoBehaviour
	{
		[Information("레이어를 Interaction으로 설정하고 트리거 콜라이더를 넣어주세요.", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private float _changingTime = 1f;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			Map.Instance.RestoreLight(_changingTime);
		}
	}
}
