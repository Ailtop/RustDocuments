using System;
using Facepunch.CardGames;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class CardTableUI : UIDialog
{
	[Serializable]
	public class PlayingCardImage
	{
		public Rank rank;

		public Suit suit;

		public Sprite image;

		public Sprite imageSmall;

		public Sprite imageTransparent;
	}

	[Serializable]
	public class InfoTextUI
	{
		public enum Attitude
		{
			Neutral,
			Good,
			Bad
		}

		public GameObject gameObj;

		public RustText rustText;

		public Image background;
	}

	public interface ICardGameSubUI
	{
		int DynamicBetAmount { get; }

		void UpdateInGameUI(CardTableUI ui, CardGameController game);

		string GetSecondaryInfo(CardTableUI ui, CardGameController game, out InfoTextUI.Attitude attitude);

		void UpdateInGameUI_NoPlayer(CardTableUI ui);
	}

	[Header("Card Table")]
	[SerializeField]
	private InfoTextUI primaryInfo;

	[SerializeField]
	private InfoTextUI secondaryInfo;

	[SerializeField]
	private InfoTextUI playerLeaveInfo;

	[SerializeField]
	private GameObject playingUI;

	[SerializeField]
	private GameObject availableInputsUI;

	[SerializeField]
	private PlayingCardImage[] cardImages;

	[SerializeField]
	private CardInputWidget[] inputWidgets;

	[SerializeField]
	private RustSlider dismountProgressSlider;

	[SerializeField]
	private Translate.Phrase phraseLoading;

	[SerializeField]
	private Translate.Phrase phraseWaitingForNextRound;

	[SerializeField]
	private Translate.Phrase phraseNotEnoughPlayers;

	[SerializeField]
	private Translate.Phrase phraseYourTurn;

	[SerializeField]
	private Translate.Phrase phrasePlayerLeftGame;

	[SerializeField]
	private Color colourNeutralUI;

	[SerializeField]
	private Color colourGoodUI;

	[SerializeField]
	private Color colourBadUI;

	[SerializeField]
	private CanvasGroup timerCanvas;

	[SerializeField]
	private RustSlider timerSlider;

	[SerializeField]
	private UIChat chat;

	[SerializeField]
	private HudElement Hunger;

	[SerializeField]
	private HudElement Thirst;

	[SerializeField]
	private HudElement Health;

	[SerializeField]
	private HudElement PendingHealth;

	public Sprite cardNone;

	public Sprite cardBackLarge;

	public Sprite cardBackSmall;

	private static Sprite cardBackLargeStatic;

	private static Sprite cardBackSmallStatic;

	[SerializeField]
	private TexasHoldEmUI texasHoldEmUI;
}
