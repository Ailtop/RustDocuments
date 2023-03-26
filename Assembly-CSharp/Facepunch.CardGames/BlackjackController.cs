using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Facepunch.CardGames;

public class BlackjackController : CardGameController
{
	[Flags]
	public enum BlackjackInputOption
	{
		None = 0,
		SubmitBet = 1,
		Hit = 2,
		Stand = 4,
		Split = 8,
		DoubleDown = 0x10,
		Insurance = 0x20,
		MaxBet = 0x40,
		Abandon = 0x80
	}

	public enum BlackjackRoundResult
	{
		None = 0,
		Bust = 1,
		Loss = 2,
		Standoff = 3,
		Win = 4,
		BlackjackWin = 5
	}

	public enum CardsValueMode
	{
		Low = 0,
		High = 1
	}

	private enum BetType
	{
		Main = 0,
		Split = 1,
		Insurance = 2
	}

	public List<PlayingCard> dealerCards = new List<PlayingCard>();

	public const float BLACKJACK_PAYOUT_RATIO = 1.5f;

	public const float INSURANCE_PAYOUT_RATIO = 2f;

	private const float DEALER_MOVE_TIME = 1f;

	private const int NUM_DECKS = 6;

	private StackOfCards cardStack = new StackOfCards(6);

	public override int MinPlayers => 1;

	public override int MinBuyIn => 5;

	public override int MaxBuyIn => int.MaxValue;

	public override int MinToPlay => MinBuyIn;

	public override int EndRoundDelay => 1;

	public override int TimeBetweenRounds => 4;

	public BlackjackInputOption LastAction { get; private set; }

	public ulong LastActionTarget { get; private set; }

	public int LastActionValue { get; private set; }

	public bool AllBetsPlaced
	{
		get
		{
			if (!base.HasRoundInProgressOrEnding)
			{
				return false;
			}
			foreach (CardPlayerData item in PlayersInRound())
			{
				if (item.betThisRound == 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	public BlackjackController(BaseCardGameEntity owner)
		: base(owner)
	{
	}

	protected override int GetFirstPlayerRelIndex(bool startOfRound)
	{
		return 0;
	}

	public override List<PlayingCard> GetTableCards()
	{
		return dealerCards;
	}

	public void InputsToList(int availableInputs, List<BlackjackInputOption> result)
	{
		BlackjackInputOption[] array = (BlackjackInputOption[])Enum.GetValues(typeof(BlackjackInputOption));
		foreach (BlackjackInputOption blackjackInputOption in array)
		{
			if (blackjackInputOption != 0 && ((uint)availableInputs & (uint)blackjackInputOption) == (uint)blackjackInputOption)
			{
				result.Add(blackjackInputOption);
			}
		}
	}

	public bool WaitingForOtherPlayers(CardPlayerData pData)
	{
		if (!pData.HasUserInCurrentRound)
		{
			return false;
		}
		if (base.State == CardGameState.InGameRound && !pData.HasAvailableInputs)
		{
			foreach (CardPlayerData item in PlayersInRound())
			{
				if (item != pData && item.HasAvailableInputs)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetCardsValue(List<PlayingCard> cards, CardsValueMode mode)
	{
		int num = 0;
		foreach (PlayingCard card in cards)
		{
			if (!card.IsUnknownCard)
			{
				num += GetCardValue(card, mode);
				if (card.Rank == Rank.Ace)
				{
					mode = CardsValueMode.Low;
				}
			}
		}
		return num;
	}

	public int GetOptimalCardsValue(List<PlayingCard> cards)
	{
		int cardsValue = GetCardsValue(cards, CardsValueMode.Low);
		int cardsValue2 = GetCardsValue(cards, CardsValueMode.High);
		if (cardsValue2 <= 21)
		{
			return cardsValue2;
		}
		return cardsValue;
	}

	public int GetCardValue(PlayingCard card, CardsValueMode mode)
	{
		int rank = (int)card.Rank;
		if (rank <= 8)
		{
			return rank + 2;
		}
		if (rank <= 11)
		{
			return 10;
		}
		if (mode != 0)
		{
			return 11;
		}
		return 1;
	}

	public bool Has21(List<PlayingCard> cards)
	{
		return GetOptimalCardsValue(cards) == 21;
	}

	public bool HasBlackjack(List<PlayingCard> cards)
	{
		if (GetCardsValue(cards, CardsValueMode.High) == 21)
		{
			return cards.Count == 2;
		}
		return false;
	}

	public bool HasBusted(List<PlayingCard> cards)
	{
		return GetCardsValue(cards, CardsValueMode.Low) > 21;
	}

	private bool CanSplit(CardPlayerDataBlackjack pData)
	{
		if (pData.Cards.Count != 2)
		{
			return false;
		}
		if (HasSplit(pData))
		{
			return false;
		}
		int betThisRound = pData.betThisRound;
		if (pData.GetScrapAmount() < betThisRound)
		{
			return false;
		}
		return GetCardValue(pData.Cards[0], CardsValueMode.Low) == GetCardValue(pData.Cards[1], CardsValueMode.Low);
	}

	private bool HasAnyAces(List<PlayingCard> cards)
	{
		foreach (PlayingCard card in cards)
		{
			if (card.Rank == Rank.Ace)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanDoubleDown(CardPlayerDataBlackjack pData)
	{
		if (pData.Cards.Count != 2)
		{
			return false;
		}
		if (HasAnyAces(pData.Cards))
		{
			return false;
		}
		int betThisRound = pData.betThisRound;
		return pData.GetScrapAmount() >= betThisRound;
	}

	private bool CanTakeInsurance(CardPlayerDataBlackjack pData)
	{
		if (dealerCards.Count != 2)
		{
			return false;
		}
		if (dealerCards[1].Rank != Rank.Ace)
		{
			return false;
		}
		if (pData.insuranceBetThisRound > 0)
		{
			return false;
		}
		int num = Mathf.FloorToInt((float)pData.betThisRound / 2f);
		return pData.GetScrapAmount() >= num;
	}

	private bool HasSplit(CardPlayerDataBlackjack pData)
	{
		return pData.SplitCards.Count > 0;
	}

	protected override CardPlayerData GetNewCardPlayerData(int mountIndex)
	{
		if (base.IsServer)
		{
			return new CardPlayerDataBlackjack(base.ScrapItemID, base.Owner.GetPlayerStorage, mountIndex, base.IsServer);
		}
		return new CardPlayerDataBlackjack(mountIndex, base.IsServer);
	}

	public bool TryGetCardPlayerDataBlackjack(int index, out CardPlayerDataBlackjack cpBlackjack)
	{
		CardPlayerData cardPlayer;
		bool result = TryGetCardPlayerData(index, out cardPlayer);
		cpBlackjack = (CardPlayerDataBlackjack)cardPlayer;
		return result;
	}

	public int ResultsToInt(BlackjackRoundResult mainResult, BlackjackRoundResult splitResult, int insurancePayout)
	{
		return (int)(mainResult + 10 * (int)splitResult + 100 * insurancePayout);
	}

	public void ResultsFromInt(int result, out BlackjackRoundResult mainResult, out BlackjackRoundResult splitResult, out int insurancePayout)
	{
		mainResult = (BlackjackRoundResult)(result % 10);
		splitResult = (BlackjackRoundResult)(result / 10 % 10);
		insurancePayout = (result - mainResult - splitResult) / 100;
	}

	public override void Save(CardGame syncData)
	{
		syncData.blackjack = Pool.Get<CardGame.Blackjack>();
		syncData.blackjack.dealerCards = Pool.GetList<int>();
		syncData.lastActionId = (int)LastAction;
		syncData.lastActionTarget = LastActionTarget;
		syncData.lastActionValue = LastActionValue;
		for (int i = 0; i < dealerCards.Count; i++)
		{
			PlayingCard playingCard = dealerCards[i];
			if (base.HasActiveRound && i == 0)
			{
				syncData.blackjack.dealerCards.Add(-1);
			}
			else
			{
				syncData.blackjack.dealerCards.Add(playingCard.GetIndex());
			}
		}
		base.Save(syncData);
		ClearLastAction();
	}

	private void EditorMakeRandomMove(CardPlayerDataBlackjack pdBlackjack)
	{
		List<BlackjackInputOption> obj = Pool.GetList<BlackjackInputOption>();
		InputsToList(pdBlackjack.availableInputs, obj);
		if (obj.Count == 0)
		{
			Debug.Log("No moves currently available.");
			Pool.FreeList(ref obj);
			return;
		}
		BlackjackInputOption blackjackInputOption = obj[UnityEngine.Random.Range(0, obj.Count)];
		if (AllBetsPlaced)
		{
			if (GetOptimalCardsValue(pdBlackjack.Cards) < 17 && obj.Contains(BlackjackInputOption.Hit))
			{
				blackjackInputOption = BlackjackInputOption.Hit;
			}
			else if (obj.Contains(BlackjackInputOption.Stand))
			{
				blackjackInputOption = BlackjackInputOption.Stand;
			}
		}
		else if (obj.Contains(BlackjackInputOption.SubmitBet))
		{
			blackjackInputOption = BlackjackInputOption.SubmitBet;
		}
		if (obj.Count > 0)
		{
			int num = 0;
			if (blackjackInputOption == BlackjackInputOption.SubmitBet)
			{
				num = MinBuyIn;
			}
			Debug.Log(string.Concat(pdBlackjack.UserID, " Taking random action: ", blackjackInputOption, " with value ", num));
			ReceivedInputFromPlayer(pdBlackjack, (int)blackjackInputOption, countAsAction: true, num);
		}
		else
		{
			Debug.LogWarning(GetType().Name + ": No input options are available for the current player.");
		}
		Pool.FreeList(ref obj);
	}

	protected override int GetAvailableInputsForPlayer(CardPlayerData pData)
	{
		BlackjackInputOption blackjackInputOption = BlackjackInputOption.None;
		CardPlayerDataBlackjack cardPlayerDataBlackjack = (CardPlayerDataBlackjack)pData;
		if (cardPlayerDataBlackjack == null || isWaitingBetweenTurns || cardPlayerDataBlackjack.hasCompletedTurn || !cardPlayerDataBlackjack.HasUserInCurrentRound)
		{
			return (int)blackjackInputOption;
		}
		if (!base.HasActiveRound)
		{
			return (int)blackjackInputOption;
		}
		if (AllBetsPlaced)
		{
			blackjackInputOption |= BlackjackInputOption.Stand;
			if (!Has21(cardPlayerDataBlackjack.Cards))
			{
				blackjackInputOption |= BlackjackInputOption.Hit;
			}
			if (CanSplit(cardPlayerDataBlackjack))
			{
				blackjackInputOption |= BlackjackInputOption.Split;
			}
			if (CanDoubleDown(cardPlayerDataBlackjack))
			{
				blackjackInputOption |= BlackjackInputOption.DoubleDown;
			}
			if (CanTakeInsurance(cardPlayerDataBlackjack))
			{
				blackjackInputOption |= BlackjackInputOption.Insurance;
			}
		}
		else
		{
			blackjackInputOption |= BlackjackInputOption.SubmitBet;
			blackjackInputOption |= BlackjackInputOption.MaxBet;
		}
		return (int)blackjackInputOption;
	}

	protected override void SubEndGameplay()
	{
		dealerCards.Clear();
	}

	protected override void SubEndRound()
	{
		int dealerCardsVal = GetOptimalCardsValue(dealerCards);
		if (dealerCardsVal > 21)
		{
			dealerCardsVal = 0;
		}
		base.resultInfo.winningScore = dealerCardsVal;
		if (NumPlayersInCurrentRound() == 0)
		{
			base.Owner.ClientRPC(null, "OnResultsDeclared", base.resultInfo);
			return;
		}
		bool dealerHasBlackjack = HasBlackjack(dealerCards);
		foreach (CardPlayerDataBlackjack item in PlayersInRound())
		{
			int num = 0;
			int winnings2;
			BlackjackRoundResult mainResult = CheckResult(item.Cards, item.betThisRound, out winnings2);
			num += winnings2;
			BlackjackRoundResult splitResult = CheckResult(item.SplitCards, item.splitBetThisRound, out winnings2);
			num += winnings2;
			int num2 = item.betThisRound + item.splitBetThisRound + item.insuranceBetThisRound;
			int insurancePayout = 0;
			if (dealerHasBlackjack && item.insuranceBetThisRound > 0)
			{
				int num3 = Mathf.FloorToInt((float)item.insuranceBetThisRound * 3f);
				num += num3;
				insurancePayout = num3;
			}
			int resultCode = ResultsToInt(mainResult, splitResult, insurancePayout);
			AddRoundResult(item, num - num2, resultCode);
			PayOut(item, num);
		}
		ClearPot();
		base.Owner.ClientRPC(null, "OnResultsDeclared", base.resultInfo);
		BlackjackRoundResult CheckResult(List<PlayingCard> cards, int betAmount, out int winnings)
		{
			if (cards.Count == 0)
			{
				winnings = 0;
				return BlackjackRoundResult.None;
			}
			int optimalCardsValue = GetOptimalCardsValue(cards);
			if (optimalCardsValue > 21)
			{
				winnings = 0;
				return BlackjackRoundResult.Bust;
			}
			if (optimalCardsValue > base.resultInfo.winningScore)
			{
				base.resultInfo.winningScore = optimalCardsValue;
			}
			BlackjackRoundResult blackjackRoundResult = BlackjackRoundResult.Loss;
			bool flag = HasBlackjack(cards);
			if (dealerHasBlackjack)
			{
				if (flag)
				{
					blackjackRoundResult = BlackjackRoundResult.Standoff;
				}
			}
			else if (optimalCardsValue > dealerCardsVal)
			{
				blackjackRoundResult = (flag ? BlackjackRoundResult.BlackjackWin : BlackjackRoundResult.Win);
			}
			else if (optimalCardsValue == dealerCardsVal)
			{
				blackjackRoundResult = ((!flag) ? BlackjackRoundResult.Standoff : BlackjackRoundResult.BlackjackWin);
			}
			switch (blackjackRoundResult)
			{
			case BlackjackRoundResult.BlackjackWin:
				winnings = Mathf.FloorToInt((float)betAmount * 2.5f);
				break;
			case BlackjackRoundResult.Win:
				winnings = Mathf.FloorToInt((float)betAmount * 2f);
				break;
			case BlackjackRoundResult.Standoff:
				winnings = betAmount;
				break;
			default:
				winnings = 0;
				break;
			}
			return blackjackRoundResult;
		}
	}

	private int PayOut(CardPlayerData pData, int winnings)
	{
		if (winnings == 0)
		{
			return 0;
		}
		StorageContainer storage = pData.GetStorage();
		if (storage == null)
		{
			return 0;
		}
		storage.inventory.AddItem(base.Owner.scrapItemDef, winnings, 0uL, ItemContainer.LimitStack.None);
		return winnings;
	}

	protected override void HandlePlayerLeavingDuringTheirTurn(CardPlayerData pData)
	{
		ReceivedInputFromPlayer(pData, 128, countAsAction: true, 0, playerInitiated: false);
	}

	protected override void SubReceivedInputFromPlayer(CardPlayerData pData, int input, int value, bool countAsAction)
	{
		if (!Enum.IsDefined(typeof(BlackjackInputOption), input))
		{
			return;
		}
		BlackjackInputOption selectedMove = (BlackjackInputOption)input;
		CardPlayerDataBlackjack pdBlackjack = (CardPlayerDataBlackjack)pData;
		if (!base.HasActiveRound)
		{
			LastActionTarget = pData.UserID;
			LastAction = selectedMove;
			LastActionValue = 0;
			return;
		}
		int selectedMoveValue = 0;
		if (AllBetsPlaced)
		{
			DoInRoundPlayerInput(pdBlackjack, ref selectedMove, ref selectedMoveValue);
		}
		else
		{
			DoBettingPhasePlayerInput(pdBlackjack, value, countAsAction, ref selectedMove, ref selectedMoveValue);
		}
		LastActionTarget = pData.UserID;
		LastAction = selectedMove;
		LastActionValue = selectedMoveValue;
		if (NumPlayersInCurrentRound() == 0)
		{
			EndGameplay();
			return;
		}
		if (ShouldEndCycle())
		{
			EndCycle();
			return;
		}
		StartTurnTimer(pData, MaxTurnTime);
		base.Owner.SendNetworkUpdate();
	}

	private void DoInRoundPlayerInput(CardPlayerDataBlackjack pdBlackjack, ref BlackjackInputOption selectedMove, ref int selectedMoveValue)
	{
		if (selectedMove != BlackjackInputOption.Abandon && ((uint)pdBlackjack.availableInputs & (uint)selectedMove) != (uint)selectedMove)
		{
			return;
		}
		switch (selectedMove)
		{
		case BlackjackInputOption.Hit:
		{
			cardStack.TryTakeCard(out var card3);
			pdBlackjack.Cards.Add(card3);
			break;
		}
		case BlackjackInputOption.Stand:
			if (!pdBlackjack.TrySwitchToSplitHand())
			{
				pdBlackjack.SetHasCompletedTurn(hasActed: true);
			}
			break;
		case BlackjackInputOption.Split:
		{
			PlayingCard playingCard = pdBlackjack.Cards[1];
			bool num = playingCard.Rank == Rank.Ace;
			pdBlackjack.SplitCards.Add(playingCard);
			pdBlackjack.Cards.Remove(playingCard);
			cardStack.TryTakeCard(out var card2);
			pdBlackjack.Cards.Add(card2);
			cardStack.TryTakeCard(out card2);
			pdBlackjack.SplitCards.Add(card2);
			selectedMoveValue = TryMakeBet(pdBlackjack, pdBlackjack.betThisRound, BetType.Split);
			if (num)
			{
				pdBlackjack.SetHasCompletedTurn(hasActed: true);
			}
			break;
		}
		case BlackjackInputOption.DoubleDown:
		{
			selectedMoveValue = TryMakeBet(pdBlackjack, pdBlackjack.betThisRound, BetType.Main);
			cardStack.TryTakeCard(out var card);
			pdBlackjack.Cards.Add(card);
			if (!pdBlackjack.TrySwitchToSplitHand())
			{
				pdBlackjack.SetHasCompletedTurn(hasActed: true);
			}
			break;
		}
		case BlackjackInputOption.Insurance:
		{
			int maxAmount = Mathf.FloorToInt((float)pdBlackjack.betThisRound / 2f);
			selectedMoveValue = TryMakeBet(pdBlackjack, maxAmount, BetType.Insurance);
			break;
		}
		case BlackjackInputOption.Abandon:
			pdBlackjack.LeaveCurrentRound(clearBets: false, leftRoundEarly: true);
			break;
		}
		if (HasBusted(pdBlackjack.Cards) && !pdBlackjack.TrySwitchToSplitHand())
		{
			pdBlackjack.SetHasCompletedTurn(hasActed: true);
		}
		if (Has21(pdBlackjack.Cards) && !CanTakeInsurance(pdBlackjack) && !CanDoubleDown(pdBlackjack) && !CanSplit(pdBlackjack) && !pdBlackjack.TrySwitchToSplitHand())
		{
			pdBlackjack.SetHasCompletedTurn(hasActed: true);
		}
	}

	private void DoBettingPhasePlayerInput(CardPlayerDataBlackjack pdBlackjack, int value, bool countAsAction, ref BlackjackInputOption selectedMove, ref int selectedMoveValue)
	{
		if (selectedMove != BlackjackInputOption.Abandon && ((uint)pdBlackjack.availableInputs & (uint)selectedMove) != (uint)selectedMove)
		{
			return;
		}
		if (selectedMove == BlackjackInputOption.SubmitBet)
		{
			selectedMoveValue = TryMakeBet(pdBlackjack, value, BetType.Main);
			if (countAsAction)
			{
				pdBlackjack.SetHasCompletedTurn(hasActed: true);
			}
		}
		else if (selectedMove == BlackjackInputOption.MaxBet)
		{
			selectedMoveValue = TryMakeBet(pdBlackjack, BlackjackMachine.maxbet, BetType.Main);
			if (countAsAction)
			{
				pdBlackjack.SetHasCompletedTurn(hasActed: true);
			}
		}
		else if (selectedMove == BlackjackInputOption.Abandon)
		{
			pdBlackjack.LeaveCurrentRound(clearBets: false, leftRoundEarly: true);
		}
	}

	private int TryMakeBet(CardPlayerDataBlackjack pdBlackjack, int maxAmount, BetType betType)
	{
		int num = TryMoveToPotStorage(pdBlackjack, maxAmount);
		switch (betType)
		{
		case BetType.Main:
			pdBlackjack.betThisTurn += num;
			pdBlackjack.betThisRound += num;
			break;
		case BetType.Split:
			pdBlackjack.splitBetThisRound += num;
			break;
		case BetType.Insurance:
			pdBlackjack.insuranceBetThisRound += num;
			break;
		}
		return num;
	}

	protected override void SubStartRound()
	{
		dealerCards.Clear();
		cardStack = new StackOfCards(6);
		ClearLastAction();
		ServerPlaySound(CardGameSounds.SoundType.Shuffle);
		foreach (CardPlayerDataBlackjack item in PlayersInRound())
		{
			item.EnableSendingCards();
			item.availableInputs = GetAvailableInputsForPlayer(item);
			StartTurnTimer(item, MaxTurnTime);
		}
	}

	protected override void OnTurnTimeout(CardPlayerData pData)
	{
		if (!pData.HasUserInCurrentRound || pData.hasCompletedTurn)
		{
			return;
		}
		BlackjackInputOption blackjackInputOption = BlackjackInputOption.Abandon;
		int value = 0;
		if (AllBetsPlaced)
		{
			if ((pData.availableInputs & 4) == 4)
			{
				blackjackInputOption = BlackjackInputOption.Stand;
				ReceivedInputFromPlayer(pData, 4, countAsAction: true, 0, playerInitiated: false);
			}
		}
		else if ((pData.availableInputs & 1) == 1 && pData.GetScrapAmount() >= MinBuyIn)
		{
			blackjackInputOption = BlackjackInputOption.SubmitBet;
			value = MinBuyIn;
		}
		if (blackjackInputOption != BlackjackInputOption.Abandon)
		{
			ReceivedInputFromPlayer(pData, (int)blackjackInputOption, countAsAction: true, value, playerInitiated: false);
			return;
		}
		blackjackInputOption = BlackjackInputOption.Abandon;
		ReceivedInputFromPlayer(pData, (int)blackjackInputOption, countAsAction: true, 0, playerInitiated: false);
		pData.ClearAllData();
		if (base.HasActiveRound && NumPlayersInCurrentRound() < MinPlayers)
		{
			BeginRoundEnd();
		}
		if (pData.HasUserInGame)
		{
			base.Owner.ClientRPC(null, "ClientOnPlayerLeft", pData.UserID);
		}
		base.Owner.SendNetworkUpdate();
	}

	protected override void StartNextCycle()
	{
		base.StartNextCycle();
		if (ShouldEndCycle())
		{
			EndCycle();
			return;
		}
		foreach (CardPlayerDataBlackjack item in PlayersInRound())
		{
			StartTurnTimer(item, MaxTurnTime);
		}
		UpdateAllAvailableInputs();
		base.Owner.SendNetworkUpdate();
	}

	protected override bool ShouldEndCycle()
	{
		foreach (CardPlayerData item in PlayersInRound())
		{
			if (!item.hasCompletedTurn)
			{
				return false;
			}
		}
		return true;
	}

	protected override void EndCycle()
	{
		CardPlayerData[] playerData = base.PlayerData;
		for (int i = 0; i < playerData.Length; i++)
		{
			playerData[i].SetHasCompletedTurn(hasActed: false);
		}
		if (dealerCards.Count == 0)
		{
			DealInitialCards();
			ServerPlaySound(CardGameSounds.SoundType.Draw);
			QueueNextCycleInvoke();
			return;
		}
		bool flag = true;
		bool flag2 = true;
		foreach (CardPlayerDataBlackjack item in PlayersInRound())
		{
			if (!HasBusted(item.Cards))
			{
				flag = false;
			}
			if (!HasBlackjack(item.Cards))
			{
				flag2 = false;
			}
			if (item.SplitCards.Count > 0)
			{
				if (!HasBusted(item.SplitCards))
				{
					flag = false;
				}
				if (!HasBlackjack(item.SplitCards))
				{
					flag2 = false;
				}
			}
			if (!flag && !flag2)
			{
				break;
			}
		}
		ServerPlaySound(CardGameSounds.SoundType.Draw);
		if (NumPlayersInCurrentRound() > 0 && !flag && !flag2)
		{
			base.Owner.Invoke(DealerPlayInvoke, 1f);
			BeginRoundEnd();
		}
		else
		{
			EndRoundWithDelay();
		}
	}

	private void DealerPlayInvoke()
	{
		int cardsValue = GetCardsValue(dealerCards, CardsValueMode.High);
		if (GetCardsValue(dealerCards, CardsValueMode.Low) < 17 && (cardsValue < 17 || cardsValue > 21))
		{
			cardStack.TryTakeCard(out var card);
			dealerCards.Add(card);
			ServerPlaySound(CardGameSounds.SoundType.Draw);
			base.Owner.Invoke(DealerPlayInvoke, 1f);
			base.Owner.SendNetworkUpdate();
		}
		else
		{
			EndRoundWithDelay();
		}
	}

	private void DealInitialCards()
	{
		if (!base.HasActiveRound)
		{
			return;
		}
		PlayingCard card;
		foreach (CardPlayerData item in PlayersInRound())
		{
			cardStack.TryTakeCard(out card);
			item.Cards.Add(card);
		}
		cardStack.TryTakeCard(out card);
		dealerCards.Add(card);
		foreach (CardPlayerData item2 in PlayersInRound())
		{
			cardStack.TryTakeCard(out card);
			item2.Cards.Add(card);
			if (HasBlackjack(item2.Cards))
			{
				item2.SetHasCompletedTurn(hasActed: true);
			}
		}
		cardStack.TryTakeCard(out card);
		dealerCards.Add(card);
	}

	private void ClearLastAction()
	{
		LastAction = BlackjackInputOption.None;
		LastActionTarget = 0uL;
		LastActionValue = 0;
	}
}
