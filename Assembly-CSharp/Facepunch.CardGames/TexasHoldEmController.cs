using System;
using System.Collections.Generic;
using PokerEvaluator;
using ProtoBuf;
using Rust;
using UnityEngine;

namespace Facepunch.CardGames;

public class TexasHoldEmController : CardGameController
{
	[Flags]
	public enum PokerInputOption
	{
		None = 0,
		Fold = 1,
		Call = 2,
		AllIn = 4,
		Check = 8,
		Raise = 0x10,
		Bet = 0x20,
		RevealHand = 0x40
	}

	public enum Playability
	{
		OK = 0,
		NoPlayer = 1,
		NotEnoughBuyIn = 2,
		TooMuchBuyIn = 3,
		RanOutOfScrap = 4,
		Idle = 5
	}

	public List<PlayingCard> communityCards = new List<PlayingCard>();

	public const int SMALL_BLIND = 5;

	public const int BIG_BLIND = 10;

	public const string WON_HAND_STAT = "won_hand_texas_holdem";

	public const int RAISE_INCREMENTS = 5;

	private int dealerIndex;

	private int activePlayerIndex;

	private DeckOfCards deck = new DeckOfCards();

	private bool isWaitingBetweenTurns;

	public override int MinBuyIn => 100;

	public override int MaxBuyIn => 1000;

	public override int MinPlayers => 2;

	public PokerInputOption LastAction { get; private set; }

	public ulong LastActionTarget { get; private set; }

	public int LastActionValue { get; private set; }

	public int BiggestRaiseThisTurn { get; private set; }

	public TexasHoldEmController(CardTable owner)
		: base(owner)
	{
	}

	public Playability GetPlayabilityStatus(CardPlayerData cpd)
	{
		if (!cpd.HasUser)
		{
			return Playability.NoPlayer;
		}
		int scrapAmount = cpd.GetScrapAmount();
		if (cpd.HasUserInGame)
		{
			if (scrapAmount <= 0)
			{
				return Playability.RanOutOfScrap;
			}
		}
		else
		{
			if (scrapAmount < MinBuyIn)
			{
				return Playability.NotEnoughBuyIn;
			}
			if (scrapAmount > MaxBuyIn)
			{
				return Playability.TooMuchBuyIn;
			}
		}
		return Playability.OK;
	}

	public int GetCurrentBet()
	{
		int num = 0;
		CardPlayerData[] array = playerData;
		foreach (CardPlayerData cardPlayerData in array)
		{
			if (cardPlayerData.HasUserInCurrentRound)
			{
				num = Mathf.Max(num, cardPlayerData.betThisTurn);
			}
		}
		return num;
	}

	public override bool IsAllowedToPlay(CardPlayerData cpd)
	{
		return GetPlayabilityStatus(cpd) == Playability.OK;
	}

	public bool TryGetActivePlayer(out CardPlayerData activePlayer)
	{
		return ToCardPlayerData(activePlayerIndex, includeFolded: false, out activePlayer);
	}

	public bool TryGetDealer(out CardPlayerData dealer)
	{
		return ToCardPlayerData(dealerIndex, includeFolded: true, out dealer);
	}

	public bool TryGetSmallBlind(out CardPlayerData smallBlind)
	{
		int relIndex = ((NumPlayersInGame() < 3) ? dealerIndex : (dealerIndex + 1));
		return ToCardPlayerData(relIndex, includeFolded: true, out smallBlind);
	}

	public bool TryGetBigBlind(out CardPlayerData bigBlind)
	{
		int relIndex = ((NumPlayersInGame() < 3) ? (dealerIndex + 1) : (dealerIndex + 2));
		return ToCardPlayerData(relIndex, includeFolded: true, out bigBlind);
	}

	public int GetFirstPlayerRelIndex(bool startOfRound)
	{
		int num = NumPlayersInGame();
		if (startOfRound && num == 2)
		{
			return dealerIndex;
		}
		return (dealerIndex + 1) % num;
	}

	private bool ToCardPlayerData(int relIndex, bool includeFolded, out CardPlayerData result)
	{
		if (!base.HasRoundInProgress)
		{
			Debug.LogWarning(GetType().Name + ": Tried to call ToCardPlayerData while no round was in progress. Returning null.");
			result = null;
			return false;
		}
		int num = (includeFolded ? NumPlayersInGame() : NumPlayersInCurrentRound());
		int index = RelToAbsIndex(relIndex % num, includeFolded);
		return TryGetCardPlayerData(index, out result);
	}

	public static ushort EvaluatePokerHand(List<PlayingCard> cards)
	{
		ushort result = 0;
		int[] array = new int[cards.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = cards[i].GetEvaluationValue();
		}
		if (cards.Count == 5)
		{
			result = PokerLib.Eval5Hand(array);
		}
		else if (cards.Count == 7)
		{
			result = PokerLib.Eval7Hand(array);
		}
		else
		{
			Debug.LogError("Currently we can only evaluate five or seven card hands.");
		}
		return result;
	}

	public int GetCurrentMinRaise(CardPlayerData playerData)
	{
		return Mathf.Max(10, GetCurrentBet() - playerData.betThisTurn + BiggestRaiseThisTurn);
	}

	public override List<PlayingCard> GetTableCards()
	{
		return communityCards;
	}

	public override void Save(ProtoBuf.CardTable syncData)
	{
		base.Save(syncData);
		syncData.texasHoldEm = Pool.Get<ProtoBuf.CardTable.TexasHoldEm>();
		syncData.texasHoldEm.dealerIndex = dealerIndex;
		syncData.texasHoldEm.communityCards = Pool.GetList<int>();
		syncData.texasHoldEm.activePlayerIndex = activePlayerIndex;
		syncData.texasHoldEm.biggestRaiseThisTurn = BiggestRaiseThisTurn;
		syncData.lastActionId = (int)LastAction;
		syncData.lastActionTarget = LastActionTarget;
		syncData.lastActionValue = LastActionValue;
		foreach (PlayingCard communityCard in communityCards)
		{
			syncData.texasHoldEm.communityCards.Add(communityCard.GetIndex());
		}
		ClearLastAction();
	}

	protected override void SubStartRound()
	{
		communityCards.Clear();
		deck = new DeckOfCards();
		BiggestRaiseThisTurn = 0;
		ClearLastAction();
		IncrementDealer();
		DealHoleCards();
		activePlayerIndex = GetFirstPlayerRelIndex(startOfRound: true);
		ServerPlaySound(CardGameSounds.SoundType.Shuffle);
		TryGetActivePlayer(out var activePlayer);
		activePlayer.availableInputs = GetAvailableInputsForPlayer(activePlayer);
		if ((activePlayer.availableInputs & 0x20) == 32)
		{
			ReceivedInputFromPlayer(activePlayer, 32, countAsAction: false, 5, playerInitiated: false);
		}
		else
		{
			ReceivedInputFromPlayer(activePlayer, 4, countAsAction: false, 5, playerInitiated: false);
		}
		TryGetActivePlayer(out activePlayer);
		activePlayer.availableInputs = GetAvailableInputsForPlayer(activePlayer);
		if ((activePlayer.availableInputs & 0x10) == 16)
		{
			ReceivedInputFromPlayer(activePlayer, 16, countAsAction: false, 10, playerInitiated: false);
		}
		else
		{
			ReceivedInputFromPlayer(activePlayer, 4, countAsAction: false, 10, playerInitiated: false);
		}
	}

	protected override void SubEndRound()
	{
		CancelNextTurnInvoke();
		int num = 0;
		List<CardPlayerData> obj = Pool.GetList<CardPlayerData>();
		CardPlayerData[] array = playerData;
		foreach (CardPlayerData cardPlayerData in array)
		{
			if (cardPlayerData.betThisRound > 0)
			{
				obj.Add(cardPlayerData);
			}
			if (cardPlayerData.HasUserInCurrentRound)
			{
				num++;
			}
		}
		if (obj.Count == 0)
		{
			return;
		}
		ClearWinnerInfo();
		bool flag = num > 1;
		SingletonComponent<InvokeHandler>.Instance.CancelInvoke(TimeoutTurn);
		int num2 = GetScrapInPot();
		array = playerData;
		foreach (CardPlayerData cardPlayerData2 in array)
		{
			if (cardPlayerData2.HasUserInGame)
			{
				num2 -= cardPlayerData2.betThisRound;
			}
		}
		bool flag2 = true;
		array = playerData;
		foreach (CardPlayerData obj2 in array)
		{
			obj2.remainingToPayOut = obj2.betThisRound;
		}
		while (obj.Count > 1)
		{
			int num3 = int.MaxValue;
			int num4 = 0;
			array = playerData;
			foreach (CardPlayerData cardPlayerData3 in array)
			{
				if (cardPlayerData3.betThisRound > 0)
				{
					if (cardPlayerData3.betThisRound < num3)
					{
						num3 = cardPlayerData3.betThisRound;
					}
					num4++;
				}
			}
			int num5 = num3 * num4;
			foreach (CardPlayerData item in obj)
			{
				item.betThisRound -= num3;
			}
			int num6 = int.MaxValue;
			foreach (CardPlayerData item2 in obj)
			{
				if (item2.HasUserInCurrentRound && item2.finalScore < num6)
				{
					num6 = item2.finalScore;
				}
			}
			if (flag2)
			{
				base.winnerInfo.winningScore = num6;
			}
			int num7 = 0;
			foreach (CardPlayerData item3 in obj)
			{
				if (item3.HasUserInCurrentRound && item3.finalScore == num6)
				{
					num7++;
				}
			}
			int num8 = Mathf.CeilToInt((float)(num5 + num2) / (float)num7);
			num2 = 0;
			foreach (CardPlayerData item4 in obj)
			{
				if (item4.HasUserInCurrentRound && item4.finalScore == num6)
				{
					if (flag)
					{
						item4.EnableSendingCards();
					}
					PayOut(item4, num8);
					AddWinner(item4, num8, flag2);
				}
			}
			for (int num9 = obj.Count - 1; num9 >= 0; num9--)
			{
				if (obj[num9].betThisRound == 0)
				{
					obj.RemoveAt(num9);
				}
			}
			flag2 = false;
		}
		if (obj.Count == 1)
		{
			int num10 = obj[0].betThisRound + num2;
			num2 = 0;
			PayOut(obj[0], num10);
			bool primaryWinner2 = base.winnerInfo.winners.Count == 0;
			AddWinner(obj[0], num10, primaryWinner2);
		}
		base.Owner.ClientRPC(null, "OnWinnersDeclared", base.winnerInfo);
		StorageContainer pot = base.Owner.GetPot();
		if (pot != null)
		{
			int amount = pot.inventory.GetAmount(base.ScrapItemID, onlyUsableAmounts: true);
			if (amount > 0)
			{
				Debug.LogError($"{GetType().Name}: Something went wrong in the winner calculation. Pot still has {amount} scrap left over after payouts. Expected 0. Clearing it.");
				pot.inventory.Clear();
			}
		}
		Pool.FreeList(ref obj);
		void AddWinner(CardPlayerData pData, int winnings, bool primaryWinner)
		{
			foreach (ProtoBuf.CardTable.WinnerBreakdown.Winner winner2 in base.winnerInfo.winners)
			{
				if (winner2.ID == pData.UserID)
				{
					winner2.winnings += winnings;
					return;
				}
			}
			ProtoBuf.CardTable.WinnerBreakdown.Winner winner = Pool.Get<ProtoBuf.CardTable.WinnerBreakdown.Winner>();
			winner.ID = pData.UserID;
			winner.winnings = winnings;
			winner.primaryWinner = primaryWinner;
			base.winnerInfo.winners.Add(winner);
			if (global::Rust.GameInfo.HasAchievements)
			{
				BasePlayer basePlayer = base.Owner.IDToPlayer(pData.UserID);
				if (basePlayer != null)
				{
					basePlayer.stats.Add("won_hand_texas_holdem", 1);
					basePlayer.stats.Save(forceSteamSave: true);
				}
			}
		}
	}

	protected override void SubEndGameplay()
	{
		communityCards.Clear();
	}

	private void IncrementDealer()
	{
		int num = NumPlayersInGame();
		if (num == 0)
		{
			dealerIndex = 0;
			return;
		}
		dealerIndex = Mathf.Clamp(dealerIndex, 0, num - 1);
		dealerIndex = ++dealerIndex % num;
	}

	private void DealHoleCards()
	{
		for (int i = 0; i < 2; i++)
		{
			CardPlayerData[] array = playerData;
			foreach (CardPlayerData cardPlayerData in array)
			{
				if (cardPlayerData.HasUserInCurrentRound)
				{
					if (deck.TryTakeCard(out var card))
					{
						cardPlayerData.Cards.Add(card);
					}
					else
					{
						Debug.LogError(GetType().Name + ": No more cards in the deck to deal!");
					}
				}
			}
		}
		SyncAllLocalPlayerCards();
	}

	private bool DealCommunityCards()
	{
		if (!base.HasRoundInProgress)
		{
			return false;
		}
		if (communityCards.Count == 0)
		{
			for (int i = 0; i < 3; i++)
			{
				if (deck.TryTakeCard(out var card))
				{
					communityCards.Add(card);
				}
			}
			ServerPlaySound(CardGameSounds.SoundType.Draw);
			return true;
		}
		if (communityCards.Count == 3 || communityCards.Count == 4)
		{
			if (deck.TryTakeCard(out var card2))
			{
				communityCards.Add(card2);
			}
			ServerPlaySound(CardGameSounds.SoundType.Draw);
			return true;
		}
		return false;
	}

	private void ClearLastAction()
	{
		LastAction = PokerInputOption.None;
		LastActionTarget = 0uL;
		LastActionValue = 0;
	}

	protected override void TimeoutTurn()
	{
		if (TryGetActivePlayer(out var activePlayer))
		{
			ReceivedInputFromPlayer(activePlayer, 1, countAsAction: true, 0, playerInitiated: false);
		}
	}

	protected override void SubReceivedInputFromPlayer(CardPlayerData playerData, int input, int value, bool countAsAction)
	{
		if (!Enum.IsDefined(typeof(PokerInputOption), input))
		{
			return;
		}
		if (!base.HasRoundInProgress)
		{
			if (input == 64)
			{
				playerData.EnableSendingCards();
			}
			LastActionTarget = playerData.UserID;
			LastAction = (PokerInputOption)input;
			LastActionValue = 0;
		}
		else
		{
			if (!TryGetActivePlayer(out var activePlayer) || activePlayer != playerData)
			{
				return;
			}
			bool flag = false;
			if ((playerData.availableInputs & input) != input)
			{
				return;
			}
			switch (input)
			{
			case 1:
				playerData.LeaveCurrentRound(clearBets: false, leftRoundEarly: true);
				flag = true;
				LastActionValue = 0;
				break;
			case 2:
			{
				int currentBet = GetCurrentBet();
				int num = (LastActionValue = AddToPot(playerData, currentBet - playerData.betThisTurn));
				break;
			}
			case 16:
			case 32:
			{
				int currentBet = GetCurrentBet();
				int biggestRaiseThisTurn = BiggestRaiseThisTurn;
				if (playerData.betThisTurn + value < currentBet + biggestRaiseThisTurn)
				{
					value = currentBet + biggestRaiseThisTurn - playerData.betThisTurn;
				}
				int num = AddToPot(playerData, value);
				BiggestRaiseThisTurn = Mathf.Max(BiggestRaiseThisTurn, num - currentBet);
				LastActionValue = num;
				break;
			}
			case 4:
			{
				int currentBet = GetCurrentBet();
				int num = AddAllToPot(playerData);
				BiggestRaiseThisTurn = Mathf.Max(BiggestRaiseThisTurn, num - currentBet);
				LastActionValue = num;
				break;
			}
			case 8:
				LastActionValue = 0;
				break;
			}
			if (countAsAction && input != 0)
			{
				playerData.SetHasActedThisTurn(hasActed: true);
			}
			LastActionTarget = playerData.UserID;
			LastAction = (PokerInputOption)input;
			if (flag && NumPlayersInCurrentRound() == 1)
			{
				EndRound();
				return;
			}
			int startIndex = activePlayerIndex;
			if (flag)
			{
				if (activePlayerIndex > NumPlayersInCurrentRound() - 1)
				{
					startIndex = 0;
				}
			}
			else
			{
				startIndex = (activePlayerIndex + 1) % NumPlayersInCurrentRound();
			}
			if (ShouldEndTurn())
			{
				EndTurn();
				return;
			}
			MoveToNextPlayerWithInputs(startIndex);
			StartTurnTimer(MaxTurnTime);
			base.Owner.SendNetworkUpdate();
		}
	}

	private bool ShouldEndTurn()
	{
		CardPlayerData[] array = playerData;
		foreach (CardPlayerData cardPlayerData in array)
		{
			if (cardPlayerData.HasUserInCurrentRound && cardPlayerData.GetScrapAmount() > 0 && (cardPlayerData.betThisTurn != GetCurrentBet() || !cardPlayerData.hasActedThisTurn))
			{
				return false;
			}
		}
		return true;
	}

	private void EndTurn()
	{
		CardPlayerData[] array = playerData;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetHasActedThisTurn(hasActed: false);
		}
		BiggestRaiseThisTurn = 0;
		if (DealCommunityCards())
		{
			QueueNextTurnInvoke();
			return;
		}
		array = playerData;
		foreach (CardPlayerData cardPlayerData in array)
		{
			if (cardPlayerData.HasUserInCurrentRound)
			{
				List<PlayingCard> obj = Pool.GetList<PlayingCard>();
				obj.AddRange(cardPlayerData.Cards);
				obj.AddRange(communityCards);
				ushort finalScore = EvaluatePokerHand(obj);
				Pool.FreeList(ref obj);
				cardPlayerData.finalScore = finalScore;
			}
		}
		EndRound();
	}

	private void QueueNextTurnInvoke()
	{
		SingletonComponent<InvokeHandler>.Instance.CancelInvoke(StartNextTurn);
		SingletonComponent<InvokeHandler>.Instance.Invoke(StartNextTurn, 1f);
		isWaitingBetweenTurns = true;
		base.Owner.SendNetworkUpdate();
	}

	private void CancelNextTurnInvoke()
	{
		SingletonComponent<InvokeHandler>.Instance.CancelInvoke(StartNextTurn);
		isWaitingBetweenTurns = false;
	}

	private void StartNextTurn()
	{
		isWaitingBetweenTurns = false;
		int num = GetFirstPlayerRelIndex(startOfRound: false);
		int num2 = NumPlayersInGame();
		int num3 = 0;
		CardPlayerData result;
		while (!ToCardPlayerData(num, includeFolded: true, out result) || !result.HasUserInCurrentRound)
		{
			num = ++num % num2;
			num3++;
			if (num3 > num2)
			{
				Debug.LogError(GetType().Name + ": This should never happen. Ended turn with no players in game?.");
				EndRound();
				return;
			}
		}
		int num4 = GameToRoundIndex(num);
		if (num4 < 0 || num4 > NumPlayersInCurrentRound())
		{
			Debug.LogError($"EndTurn NewActiveIndex is out of range: {num4}. Clamping it to between 0 and {NumPlayersInCurrentRound()}.");
			num4 = Mathf.Clamp(num4, 0, NumPlayersInCurrentRound());
		}
		int startIndex = num4;
		int num5 = 0;
		CardPlayerData[] array = playerData;
		foreach (CardPlayerData cardPlayerData in array)
		{
			if (cardPlayerData.HasUserInCurrentRound && cardPlayerData.GetScrapAmount() > 0)
			{
				num5++;
			}
		}
		if (num5 == 1 || ShouldEndTurn())
		{
			EndTurn();
			return;
		}
		MoveToNextPlayerWithInputs(startIndex);
		StartTurnTimer(MaxTurnTime);
		UpdateAllAvailableInputs();
		base.Owner.SendNetworkUpdate();
	}

	protected override int GetAvailableInputsForPlayer(CardPlayerData playerData)
	{
		PokerInputOption pokerInputOption = PokerInputOption.None;
		if (playerData == null || isWaitingBetweenTurns)
		{
			return (int)pokerInputOption;
		}
		if (!base.HasRoundInProgress)
		{
			if (!playerData.LeftRoundEarly && playerData.Cards.Count > 0 && !playerData.SendCardDetails)
			{
				pokerInputOption |= PokerInputOption.RevealHand;
			}
			return (int)pokerInputOption;
		}
		if (!TryGetActivePlayer(out var activePlayer) || playerData != activePlayer)
		{
			return (int)pokerInputOption;
		}
		int scrapAmount = playerData.GetScrapAmount();
		if (scrapAmount > 0)
		{
			pokerInputOption |= PokerInputOption.AllIn;
			pokerInputOption |= PokerInputOption.Fold;
			int currentBet = GetCurrentBet();
			if (playerData.betThisTurn >= currentBet)
			{
				pokerInputOption |= PokerInputOption.Check;
			}
			if (currentBet > playerData.betThisTurn && scrapAmount >= currentBet - playerData.betThisTurn)
			{
				pokerInputOption |= PokerInputOption.Call;
			}
			if (scrapAmount >= GetCurrentMinRaise(playerData))
			{
				pokerInputOption = ((BiggestRaiseThisTurn != 0) ? (pokerInputOption | PokerInputOption.Raise) : (pokerInputOption | PokerInputOption.Bet));
			}
		}
		return (int)pokerInputOption;
	}

	public override void OnTableDestroyed()
	{
		base.OnTableDestroyed();
		if (!base.HasGameInProgress)
		{
			return;
		}
		int maxAmount = GetScrapInPot() / NumPlayersInGame();
		CardPlayerData[] array = playerData;
		foreach (CardPlayerData cardPlayerData in array)
		{
			if (cardPlayerData.HasUserInGame)
			{
				PayOut(cardPlayerData, maxAmount);
			}
			if (cardPlayerData.HasUser)
			{
				RemoveScrapFromStorage(cardPlayerData);
			}
		}
	}

	protected override void SubOnPlayerLeaving(CardPlayerData playerData)
	{
		if (base.HasRoundInProgress && TryGetActivePlayer(out var activePlayer))
		{
			if (playerData == activePlayer)
			{
				ReceivedInputFromPlayer(activePlayer, 1, countAsAction: true, 0, playerInitiated: false);
			}
			else if (playerData.HasUserInCurrentRound && playerData.mountIndex < activePlayer.mountIndex && activePlayerIndex > 0)
			{
				activePlayerIndex--;
			}
		}
	}

	private void MoveToNextPlayerWithInputs(int startIndex)
	{
		activePlayerIndex = startIndex;
		TryGetActivePlayer(out var activePlayer);
		int num = 0;
		while (GetAvailableInputsForPlayer(activePlayer) == 0 && num < NumPlayersInCurrentRound())
		{
			activePlayerIndex = (activePlayerIndex + 1) % NumPlayersInCurrentRound();
			TryGetActivePlayer(out activePlayer);
			num++;
		}
	}
}
