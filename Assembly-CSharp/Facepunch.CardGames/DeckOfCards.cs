using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Facepunch.CardGames;

public class DeckOfCards
{
	private List<PlayingCard> deck;

	public DeckOfCards()
	{
		deck = new List<PlayingCard>(52);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 13; j++)
			{
				deck.Add(PlayingCard.GetCard(i, j));
			}
		}
		ShuffleDeck();
	}

	public bool TryTakeCard(out PlayingCard card)
	{
		if (deck.Count == 0)
		{
			card = null;
			return false;
		}
		card = deck[deck.Count - 1];
		deck.RemoveAt(deck.Count - 1);
		return true;
	}

	public void AddCard(PlayingCard card)
	{
		deck.Insert(0, card);
	}

	public void ShuffleDeck()
	{
		int num = deck.Count;
		while (num > 1)
		{
			num--;
			int index = Random.Range(0, num);
			PlayingCard value = deck[index];
			deck[index] = deck[num];
			deck[num] = value;
		}
	}

	public void Print()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Cards in the deck: ");
		foreach (PlayingCard item in deck)
		{
			stringBuilder.AppendLine(string.Concat(item.Rank, " of ", item.Suit));
		}
		Debug.Log(stringBuilder.ToString());
	}
}
