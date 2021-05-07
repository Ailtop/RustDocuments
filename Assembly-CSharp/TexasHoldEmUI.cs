using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class TexasHoldEmUI : MonoBehaviour
{
	[SerializeField]
	private Image[] holeCardImages;

	[SerializeField]
	private Image[] flopCardImages;

	[SerializeField]
	private RustText potText;

	[SerializeField]
	private TexasHoldEmPlayerWidget[] playerWidgets;

	[SerializeField]
	private GameObject raiseRoot;

	[SerializeField]
	private Translate.Phrase phraseNotEnoughBuyIn;

	[SerializeField]
	private Translate.Phrase phraseTooMuchBuyIn;

	[SerializeField]
	private Translate.Phrase phraseYouWinTheRound;

	[SerializeField]
	private Translate.Phrase phraseRoundWinner;

	[SerializeField]
	private Translate.Phrase phraseRoundWinners;

	[SerializeField]
	private Translate.Phrase phraseScrapWon;

	[SerializeField]
	private Translate.Phrase phraseScrapReturned;

	[SerializeField]
	private Translate.Phrase phraseRoyalFlush;

	[SerializeField]
	private Translate.Phrase phraseStraightFlush;

	[SerializeField]
	private Translate.Phrase phraseFourOfAKind;

	[SerializeField]
	private Translate.Phrase phraseFullHouse;

	[SerializeField]
	private Translate.Phrase phraseFlush;

	[SerializeField]
	private Translate.Phrase phraseStraight;

	[SerializeField]
	private Translate.Phrase phraseThreeOfAKind;

	[SerializeField]
	private Translate.Phrase phraseTwoPair;

	[SerializeField]
	private Translate.Phrase phrasePair;

	[SerializeField]
	private Translate.Phrase phraseHighCard;
}
