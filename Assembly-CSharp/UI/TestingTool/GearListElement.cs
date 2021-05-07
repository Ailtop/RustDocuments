using System.Collections;
using Characters.Gear;
using Level;
using Services;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TestingTool
{
	public class GearListElement : MonoBehaviour
	{
		private static readonly EnumArray<Rarity, Color> _rarityColorTable = new EnumArray<Rarity, Color>(Color.black, Color.blue, Color.magenta, Color.red);

		[SerializeField]
		private Button _button;

		[SerializeField]
		private Image _thumbnail;

		[SerializeField]
		private TMP_Text _text;

		public Gear.Type type { get; private set; }

		public Resource.GearReference gearReference { get; set; }

		public string text => _text.text;

		public void Set(Resource.GearReference gearReference)
		{
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			this.gearReference = gearReference;
			type = gearReference.type;
			_text.text = Lingua.GetLocalizedString(gearReference.displayNameKey);
			if (string.IsNullOrWhiteSpace(_text.text))
			{
				_text.text = gearReference.name;
			}
			if (gearReference.obtainable)
			{
				_text.color = _rarityColorTable[gearReference.rarity];
			}
			else
			{
				_text.color = Color.gray;
			}
			_thumbnail.sprite = gearReference.thumbnail;
			_button.onClick.AddListener(delegate
			{
				StartCoroutine(CDropGear(gearReference));
			});
		}

		private IEnumerator CDropGear(Resource.GearReference gearReference)
		{
			Resource.Request<Gear> request = gearReference.LoadAsync();
			while (!request.isDone)
			{
				yield return null;
			}
			LevelManager levelManager = Singleton<Service>.Instance.levelManager;
			levelManager.DropGear(request.asset, levelManager.player.transform.position);
		}
	}
}
