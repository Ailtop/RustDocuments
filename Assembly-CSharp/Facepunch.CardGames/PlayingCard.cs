using PokerEvaluator;

namespace Facepunch.CardGames;

public class PlayingCard
{
	public readonly bool IsUnknownCard;

	public readonly Suit Suit;

	public readonly Rank Rank;

	public static PlayingCard[] cards = GenerateAllCards();

	public static PlayingCard unknownCard = new PlayingCard();

	private PlayingCard(Suit suit, Rank rank)
	{
		IsUnknownCard = false;
		Suit = suit;
		Rank = rank;
	}

	private PlayingCard()
	{
		IsUnknownCard = true;
		Suit = Suit.Spades;
		Rank = Rank.Two;
	}

	public static PlayingCard GetCard(Suit suit, Rank rank)
	{
		return GetCard((int)suit, (int)rank);
	}

	public static PlayingCard GetCard(int suit, int rank)
	{
		return cards[suit * 13 + rank];
	}

	public static PlayingCard GetCard(int index)
	{
		if (index == -1)
		{
			return unknownCard;
		}
		return cards[index];
	}

	public int GetIndex()
	{
		if (IsUnknownCard)
		{
			return -1;
		}
		return GetIndex(Suit, Rank);
	}

	public static int GetIndex(Suit suit, Rank rank)
	{
		return (int)((int)suit * 13 + rank);
	}

	public int GetPokerEvaluationValue()
	{
		return Arrays.primes[(int)Rank] | ((int)Rank << 8) | GetPokerSuitCode() | (1 << (int)(16 + Rank));
	}

	private int GetPokerSuitCode()
	{
		return Suit switch
		{
			Suit.Spades => 4096, 
			Suit.Hearts => 8192, 
			Suit.Diamonds => 16384, 
			Suit.Clubs => 32768, 
			_ => 4096, 
		};
	}

	private static PlayingCard[] GenerateAllCards()
	{
		PlayingCard[] array = new PlayingCard[52];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 13; j++)
			{
				array[i * 13 + j] = new PlayingCard((Suit)i, (Rank)j);
			}
		}
		return array;
	}
}
