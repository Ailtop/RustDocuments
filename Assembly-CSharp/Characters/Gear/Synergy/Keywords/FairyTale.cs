using System.Collections;
using System.Linq;
using System.Text;
using Characters.Gear.Synergy.Keywords.FairyTaleSummon;
using Services;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class FairyTale : Keyword
	{
		[SerializeField]
		private Spirit _spirit;

		[SerializeField]
		private Oberon _oberon;

		[SerializeField]
		private int[] _spiritAttackCooldownsByLevel;

		[SerializeField]
		private RuntimeAnimatorController[] _spiritImages;

		private int[] _values;

		public override Key key => Key.FairyTale;

		protected override IList valuesByLevel => _values;

		private void Awake()
		{
			_values = _spiritAttackCooldownsByLevel.Append(0).ToArray();
			_spirit.gameObject.SetActive(false);
			_oberon.gameObject.SetActive(false);
			_spirit.transform.parent = null;
			_oberon.transform.parent = null;
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Object.Destroy(_spirit.gameObject);
				Object.Destroy(_oberon.gameObject);
			}
		}

		protected override void Initialize()
		{
			_spirit.Initialize(base.character);
			_oberon.Initialize(base.character);
			_oberon.transform.position = base.character.transform.position;
		}

		protected override void UpdateBonus()
		{
			if (base.level != 0)
			{
				if (base.level == base.maxLevel)
				{
					_spirit.gameObject.SetActive(false);
					_oberon.gameObject.SetActive(true);
				}
				else
				{
					_spirit.Set(_spiritAttackCooldownsByLevel[base.level], _spiritImages[base.level - 1]);
					_spirit.gameObject.SetActive(true);
					_oberon.gameObject.SetActive(false);
				}
			}
		}

		protected override void OnAttach()
		{
		}

		protected override void OnDetach()
		{
			_spirit.gameObject.SetActive(false);
			_oberon.gameObject.SetActive(false);
		}

		public override string GetDescription(Key key, int level)
		{
			if (level == base.maxLevel)
			{
				return Lingua.GetLocalizedString($"synergy/key/{key}/desc/oberon");
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<color=#B2977B>");
			for (int i = 1; i < _spiritAttackCooldownsByLevel.Length; i++)
			{
				if (level == i)
				{
					stringBuilder.Append("<color=#755754>");
				}
				stringBuilder.Append(_spiritAttackCooldownsByLevel[i]);
				if (level == i)
				{
					stringBuilder.Append("</color>");
				}
				if (i < _spiritAttackCooldownsByLevel.Length - 1)
				{
					stringBuilder.Append('/');
				}
			}
			stringBuilder.Append("</color>");
			return string.Format(Lingua.GetLocalizedString($"synergy/key/{key}/desc"), stringBuilder.ToString());
		}
	}
}
