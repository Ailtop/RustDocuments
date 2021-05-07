using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class GearImageContainer : MonoBehaviour
	{
		[SerializeField]
		private Image _image;

		public Image image
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
			}
		}
	}
}
