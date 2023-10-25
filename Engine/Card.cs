namespace BLM16.Games.Euchre.Engine;

/// <summary>
/// The possible suits a card can be.
/// </summary>
public enum Suit
{
	Hearts,
	Diamonds,
	Spades,
	Clubs
}

/// <summary>
/// The possible ranks a card can be.
/// </summary>
public enum Rank
{
	Nine,
	Ten,
	Jack,
	Queen,
	King,
	Ace
}

/// <summary>
/// Represents a playing card.
/// </summary>
public record Card(Rank Rank, Suit Suit)
{
	/// <summary>
	/// Is the card a trump card?
	/// </summary>
	/// <param name="trump">The trump suit</param>
	/// <returns>is card of the trump suit or the left bower</returns>
	public bool IsTrump(Suit trump)
		=> Suit == trump
		|| (Rank == Rank.Jack && Helpers.IsSameColor(Suit, trump));

	/// <summary>
	/// Compares this card to another card accounting for trump.
	/// </summary>
	/// <param name="other">The other card to compare to</param>
	/// <param name="trump">The trump suit</param>
	/// <returns><see langword="this"/> &gt; <paramref name="other"/> accounting for trump where <paramref name="other"/> is used as the basis for following suit</returns>
	public bool IsHigherThan(Card other, Suit trump)
	{
		if (this.IsTrump(trump) && other.IsTrump(trump))
		{
			// Right bower
			if (this.Rank == Rank.Jack && this.Suit == trump)
			{
				return true;
			}
			if (other.Rank == Rank.Jack && other.Suit == trump)
			{
				return false;
			}

			// Left bower
			if (this.Rank == Rank.Jack && Helpers.IsSameColor(this.Suit, trump))
			{
				return true;
			}
			if (other.Rank == Rank.Jack && Helpers.IsSameColor(other.Suit, trump))
			{
				return false;
			}

			// Bowers are discounted so rank is all that matters now
			return this.Rank > other.Rank;
		}
		
		if (this.IsTrump(trump))
		{
			return true;
		}
		
		if (other.IsTrump(trump))
		{
			return false;
		}

		// other.Suit used as the basis for what suit to follow
		// If we don't match the suit or play trump, it isn't higher
		if (this.Suit != other.Suit)
		{
			return false;
		}

		// Trump are discounted and suit is the same
		// We only care about the rank of the card now
		return this.Rank > other.Rank;
	}
}
