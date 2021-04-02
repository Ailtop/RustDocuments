using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class TexasHoldEmPlayerWidget : MonoBehaviour
{
	[SerializeField]
	private RawImage avatar;

	[SerializeField]
	private RustText playerName;

	[SerializeField]
	private RustText scrapTotal;

	[SerializeField]
	private RustText betTotal;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Color inactiveBackground;

	[SerializeField]
	private Color activeBackground;

	[SerializeField]
	private Color foldedBackground;

	[SerializeField]
	private Color winnerBackground;

	[SerializeField]
	private Animation actionShowAnimation;

	[SerializeField]
	private RustText actionText;

	[SerializeField]
	private Sprite dealerChip;

	[SerializeField]
	private Sprite smallBlindChip;

	[SerializeField]
	private Sprite bigBlindChip;

	[SerializeField]
	private Sprite noChip;

	[SerializeField]
	private Image chip;

	[SerializeField]
	private Image[] cardsDisplay;

	[SerializeField]
	private Translate.Phrase allInPhrase;

	[SerializeField]
	private Translate.Phrase foldPhrase;

	[SerializeField]
	private Translate.Phrase raisePhrase;

	[SerializeField]
	private Translate.Phrase betPhrase;

	[SerializeField]
	private Translate.Phrase checkPhrase;

	[SerializeField]
	private Translate.Phrase callPhrase;
}
