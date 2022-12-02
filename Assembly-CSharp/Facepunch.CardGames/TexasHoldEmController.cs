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

	public enum PokerRoundResult
	{
		Loss = 0,
		PrimaryWinner = 1,
		SecondaryWinner = 2
	}

	public List<PlayingCard> communityCards = new List<PlayingCard>();

	public const int SMALL_BLIND = 5;

	public const int BIG_BLIND = 10;

	public const string WON_HAND_STAT = "won_hand_texas_holdem";

	private int dealerIndex;

	private StackOfCards deck = new StackOfCards(1);

	public override int MinPlayers => 2;

	public override int MinBuyIn => 100;

	public override int MaxBuyIn => 1000;

	public override int MinToPlay => 10;

	public PokerInputOption LastAction { get; private set; }

	public ulong LastActionTarget { get; private set; }

	public int LastActionValue { get; private set; }

	public int BiggestRaiseThisTurn { get; private set; }

	public TexasHoldEmController(BaseCardGameEntity owner)
		: base(owner)
	{
	}

	public int GetCurrentBet()
	{
		int num = 0;
		foreach (CardPlayerData item in PlayersInRound())
		{
			num = Mathf.Max(num, item.betThisTurn);
		}
		return num;
	}

	public bool TryGetDealer(out CardPlayerData dealer)
	{
		return ToCardPlayerData(dealerIndex, includeOutOfRound: true, out dealer);
	}

	public bool TryGetSmallBlind(out CardPlayerData smallBlind)
	{
		int relIndex = ((NumPlayersInGame() < 3) ? dealerIndex : (dealerIndex + 1));
		return ToCardPlayerData(relIndex, includeOutOfRound: true, out smallBlind);
	}

	public bool TryGetBigBlind(out CardPlayerData bigBlind)
	{
		int relIndex = ((NumPlayersInGame() < 3) ? (dealerIndex + 1) : (dealerIndex + 2));
		return ToCardPlayerData(relIndex, includeOutOfRound: true, out bigBlind);
	}

	protected override int GetFirstPlayerRelIndex(bool startOfRound)
	{
		int num = NumPlayersInGame();
		if (startOfRound && num == 2)
		{
			return dealerIndex;
		}
		return (dealerIndex + 1) % num;
	}

	public static ushort EvaluatePokerHand(List<PlayingCard> cards)
	{
		ushort result = 0;
		int[] array = new int[cards.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = cards[i].GetPokerEvaluationValue();
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

	public void InputsToList(int availableInputs, List<PokerInputOption> result)
	{
		PokerInputOption[] array = (PokerInputOption[])Enum.GetValues(typeof(PokerInputOption));
		foreach (PokerInputOption pokerInputOption in array)
		{
			if (pokerInputOption != 0 && ((uint)availableInputs & (uint)pokerInputOption) == (uint)pokerInputOption)
			{
				result.Add(pokerInputOption);
			}
		}
	}

	protected override CardPlayerData GetNewCardPlayerData(int mountIndex)
	{
		if (base.IsServer)
		{
			return new CardPlayerData(base.ScrapItemID, base.Owner.GetPlayerStorage, mountIndex, base.IsServer);
		}
		return new CardPlayerData(mountIndex, base.IsServer);
	}

	public override void Save(CardGame syncData)
	{
		base.Save(syncData);
		syncData.texasHoldEm = Pool.Get<CardGame.TexasHoldEm>();
		syncData.texasHoldEm.dealerIndex = dealerIndex;
		syncData.texasHoldEm.communityCards = Pool.GetList<int>();
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
		deck = new StackOfCards(1);
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
		int num = 0;
		List<CardPlayerData> obj = Pool.GetList<CardPlayerData>();
		CardPlayerData[] playerData = base.PlayerData;
		foreach (CardPlayerData cardPlayerData in playerData)
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
			base.Owner.GetPot().inventory.Clear();
			return;
		}
		bool flag = num > 1;
		int num2 = GetScrapInPot();
		playerData = base.PlayerData;
		foreach (CardPlayerData cardPlayerData2 in playerData)
		{
			if (cardPlayerData2.HasUserInGame)
			{
				num2 -= cardPlayerData2.betThisRound;
			}
		}
		bool flag2 = true;
		playerData = base.PlayerData;
		foreach (CardPlayerData obj2 in playerData)
		{
			obj2.remainingToPayOut = obj2.betThisRound;
		}
		while (obj.Count > 1)
		{
			int num3 = int.MaxValue;
			int num4 = 0;
			playerData = base.PlayerData;
			foreach (CardPlayerData cardPlayerData3 in playerData)
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
			foreach (CardPlayerData item2 in PlayersInRound())
			{
				if (item2.finalScore < num6)
				{
					num6 = item2.finalScore;
				}
			}
			if (flag2)
			{
				base.resultInfo.winningScore = num6;
			}
			int num7 = 0;
			foreach (CardPlayerData item3 in PlayersInRound())
			{
				if (item3.finalScore == num6)
				{
					num7++;
				}
			}
			int num8 = Mathf.CeilToInt((float)(num5 + num2) / (float)num7);
			num2 = 0;
			foreach (CardPlayerData item4 in PlayersInRound())
			{
				if (item4.finalScore == num6)
				{
					if (flag)
					{
						item4.EnableSendingCards();
					}
					PayOutFromPot(item4, num8);
					PokerRoundResult resultCode = (flag2 ? PokerRoundResult.PrimaryWinner : PokerRoundResult.SecondaryWinner);
					AddRoundResult(item4, num8, (int)resultCode);
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
			PayOutFromPot(obj[0], num10);
			PokerRoundResult resultCode2 = ((base.resultInfo.results.Count == 0) ? PokerRoundResult.PrimaryWinner : PokerRoundResult.SecondaryWinner);
			AddRoundResult(obj[0], num10, (int)resultCode2);
		}
		base.Owner.ClientRPC(null, "OnResultsDeclared", base.resultInfo);
		StorageContainer pot = base.Owner.GetPot();
		int amount = pot.inventory.GetAmount(base.ScrapItemID, onlyUsableAmounts: true);
		if (amount > 0)
		{
			Debug.LogError($"{GetType().Name}: Something went wrong in the winner calculation. Pot still has {amount} scrap left over after payouts. Expected 0. Clearing it.");
			pot.inventory.Clear();
		}
		Pool.FreeList(ref obj);
	}

	protected override void AddRoundResult(CardPlayerData pData, int winnings, int winState)
	{
		base.AddRoundResult(pData, winnings, winState);
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
			foreach (CardPlayerData item in PlayersInRound())
			{
				if (deck.TryTakeCard(out var card))
				{
					item.Cards.Add(card);
				}
				else
				{
					Debug.LogError(GetType().Name + ": No more cards in the deck to deal!");
				}
			}
		}
		SyncAllLocalPlayerCards();
	}

	private bool DealCommunityCards()
	{
		if (!base.HasActiveRound)
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

	protected override void OnTurnTimeout(CardPlayerData pData)
	{
		if (TryGetActivePlayer(out var activePlayer) && activePlayer == pData)
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
		if (!base.HasActiveRound)
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
				int num = (LastActionValue = TryAddBet(playerData, currentBet - playerData.betThisTurn));
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
				int num = TryAddBet(playerData, value);
				BiggestRaiseThisTurn = Mathf.Max(BiggestRaiseThisTurn, num - currentBet);
				LastActionValue = num;
				break;
			}
			case 4:
			{
				int currentBet = GetCurrentBet();
				int num = GoAllIn(playerData);
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
				playerData.SetHasCompletedTurn(hasActed: true);
			}
			LastActionTarget = playerData.UserID;
			LastAction = (PokerInputOption)input;
			if (flag && NumPlayersInCurrentRound() == 1)
			{
				EndRoundWithDelay();
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
			CardPlayerData newActivePlayer;
			if (ShouldEndCycle())
			{
				EndCycle();
			}
			else if (TryMoveToNextPlayerWithInputs(startIndex, out newActivePlayer))
			{
				StartTurnTimer(newActivePlayer, MaxTurnTime);
				base.Owner.SendNetworkUpdate();
			}
			else
			{
				EndCycle();
			}
		}
	}

	protected override void StartNextCycle()
	{
		base.StartNextCycle();
		int num = GetFirstPlayerRelIndex(startOfRound: false);
		int num2 = NumPlayersInGame();
		int num3 = 0;
		CardPlayerData result;
		while (!ToCardPlayerData(num, includeOutOfRound: true, out result) || !result.HasUserInCurrentRound)
		{
			num = ++num % num2;
			num3++;
			if (num3 > num2)
			{
				Debug.LogError(GetType().Name + ": This should never happen. Ended turn with no players in game?.");
				EndRoundWithDelay();
				return;
			}
		}
		int num4 = GameToRoundIndex(num);
		if (num4 < 0 || num4 > NumPlayersInCurrentRound())
		{
			Debug.LogError($"StartNextCycle NewActiveIndex is out of range: {num4}. Clamping it to between 0 and {NumPlayersInCurrentRound()}.");
			num4 = Mathf.Clamp(num4, 0, NumPlayersInCurrentRound());
		}
		int startIndex = num4;
		CardPlayerData newActivePlayer;
		if (ShouldEndCycle())
		{
			EndCycle();
		}
		else if (TryMoveToNextPlayerWithInputs(startIndex, out newActivePlayer))
		{
			StartTurnTimer(newActivePlayer, MaxTurnTime);
			UpdateAllAvailableInputs();
			base.Owner.SendNetworkUpdate();
		}
		else
		{
			EndCycle();
		}
	}

	protected override bool ShouldEndCycle()
	{
		int num = 0;
		foreach (CardPlayerData item in PlayersInRound())
		{
			if (item.GetScrapAmount() > 0)
			{
				num++;
			}
		}
		if (num == 1)
		{
			return true;
		}
		foreach (CardPlayerData item2 in PlayersInRound())
		{
			if (item2.GetScrapAmount() > 0 && (item2.betThisTurn != GetCurrentBet() || !item2.hasCompletedTurn))
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
		BiggestRaiseThisTurn = 0;
		if (DealCommunityCards())
		{
			QueueNextCycleInvoke();
			return;
		}
		foreach (CardPlayerData item in PlayersInRound())
		{
			List<PlayingCard> obj = Pool.GetList<PlayingCard>();
			obj.AddRange(item.Cards);
			obj.AddRange(communityCards);
			ushort finalScore = EvaluatePokerHand(obj);
			Pool.FreeList(ref obj);
			item.finalScore = finalScore;
		}
		EndRoundWithDelay();
	}

	protected override int GetAvailableInputsForPlayer(CardPlayerData playerData)
	{
		PokerInputOption pokerInputOption = PokerInputOption.None;
		if (playerData == null || isWaitingBetweenTurns)
		{
			return (int)pokerInputOption;
		}
		if (!base.HasActiveRound)
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

	protected override void HandlePlayerLeavingDuringTheirTurn(CardPlayerData pData)
	{
		ReceivedInputFromPlayer(pData, 1, countAsAction: true, 0, playerInitiated: false);
	}
}
