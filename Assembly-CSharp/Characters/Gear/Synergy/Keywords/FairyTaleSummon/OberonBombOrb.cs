using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords.FairyTaleSummon
{
	public sealed class OberonBombOrb : MonoBehaviour
	{
		[SerializeField]
		private GameObject _body;

		[SerializeField]
		private float _noise;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _activateInfo;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _deactivateInfo;

		private Vector2 _originPoistion;

		private void Awake()
		{
			_activateInfo.Initialize();
			_deactivateInfo.Initialize();
		}

		public void Activate(Character owner)
		{
			Show();
			MoveRandom();
			_activateInfo.gameObject.SetActive(true);
			_activateInfo.Run(owner);
		}

		public void Deactivate(Character owner)
		{
			Hide();
			_deactivateInfo.gameObject.SetActive(true);
			_deactivateInfo.Run(owner);
			Restore();
		}

		private void MoveRandom()
		{
			_originPoistion = base.transform.position;
			base.transform.Translate(Random.insideUnitSphere * _noise);
		}

		private void Restore()
		{
			base.transform.position = _originPoistion;
		}

		private void Show()
		{
			_body.SetActive(true);
		}

		private void Hide()
		{
			_body.SetActive(false);
		}
	}
}
