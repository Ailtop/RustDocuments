using UnityEngine;
using UnityEngine.Serialization;

namespace Characters
{
	public class WitchSettings : ScriptableObject
	{
		private static WitchSettings _instance;

		[Header("두개골")]
		[SerializeField]
		public int[] 골수이식_비용;

		[SerializeField]
		public int 골수이식_마법공격력p;

		[SerializeField]
		public int[] 신속한탈골_비용;

		[SerializeField]
		public float 신속한탈골_교대대기시간가속;

		[Space]
		[SerializeField]
		[FormerlySerializedAs("칼슘투여_비용")]
		public int[] 영양공급_비용;

		[SerializeField]
		public float 영양공급_스킬쿨다운p;

		[Space]
		[SerializeField]
		public int[] 외골격강화_비용;

		[SerializeField]
		public float 외골격강화_보호막;

		[SerializeField]
		[Information("고정", InformationAttribute.InformationType.Info, false)]
		public float 외골격강화_보호막지속시간;

		[SerializeField]
		[Information("고정", InformationAttribute.InformationType.Info, false)]
		public float 외골격강화_교대대기시간감소;

		[Header("뼈")]
		[SerializeField]
		public int[] 통뼈_비용;

		[SerializeField]
		public int 통뼈_물리공격력p;

		[Space]
		[SerializeField]
		public int[] 골절상면역_비용;

		[SerializeField]
		public int 골절상면역_체력증가;

		[Space]
		[SerializeField]
		[FormerlySerializedAs("유연한척추_비용")]
		public int[] 육중한뼈대_비용;

		[SerializeField]
		public float 육중한뼈대_받는피해;

		[Space]
		[SerializeField]
		public int[] 재조립_비용;

		[SerializeField]
		public int 재조립_체력회복p;

		[Header("영혼")]
		[Space]
		[SerializeField]
		public int[] 영혼가속_비용;

		[SerializeField]
		public float 영혼가속_치명타확률p;

		[Space]
		[SerializeField]
		public int[] 선조의의지_비용;

		[SerializeField]
		public int 선조의의지_정수쿨다운가속p;

		[Space]
		[SerializeField]
		public int[] 날카로운정신_비용;

		[SerializeField]
		public int 날카로운정신_공격속도p;

		[SerializeField]
		public int 날카로운정신_이동속도p;

		[Space]
		[SerializeField]
		public int[] 고대연금술_비용;

		[Header("아이템")]
		[SerializeField]
		public int 고대연금술_골드량_커먼;

		[SerializeField]
		public int 고대연금술_골드량_레어;

		[SerializeField]
		public int 고대연금술_골드량_유니크;

		[SerializeField]
		public int 고대연금술_골드량_레전더리;

		[Header("정수")]
		[SerializeField]
		public int 고대연금술_골드량_정수_커먼;

		[SerializeField]
		public int 고대연금술_골드량_정수_레어;

		[SerializeField]
		public int 고대연금술_골드량_정수_유니크;

		[SerializeField]
		public int 고대연금술_골드량_정수_레전더리;

		public static WitchSettings instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Resources.Load<WitchSettings>("WitchSettings");
					_instance.Initialize();
				}
				return _instance;
			}
		}

		private void Initialize()
		{
		}
	}
}
