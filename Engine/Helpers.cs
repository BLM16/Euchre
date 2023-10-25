namespace BLM16.Games.Euchre.Engine;

/// <summary>
/// A collection of helper methods for the game
/// </summary>
public static class Helpers
{
	/// <summary>
	/// Are the suits the same color?
	/// </summary>
	/// <returns>true if HH, DD, SS, CC, HD, DH, SC, CS else false</returns>
	public static bool IsSameColor(Suit s1, Suit s2)
		=> s1 == s2
		|| s1 == Suit.Hearts && s2 == Suit.Diamonds
		|| s1 == Suit.Diamonds && s2 == Suit.Hearts
		|| s1 == Suit.Spades && s2 == Suit.Clubs
		|| s1 == Suit.Clubs && s2 == Suit.Spades;

	/// <summary>
	/// Gets the index for the <paramref name="targetPlayer"/> hand given a specific dealer.
	/// </summary>
	/// <param name="dealer">The dealer's internal index</param>
	/// <param name="targetPlayer">The turn internal index of the target player</param>
	/// <returns></returns>
	public static int GetHandOffset(int dealer, int targetPlayer)
	{
		// Dealer deals to the left
		var playerWithIndex0 = (dealer + 1) % 4;
		
		var offsetFromPlayerWithIndexZero = targetPlayer - playerWithIndex0;
		if (offsetFromPlayerWithIndexZero < 0)
		{
			// Constrain the indicies between 0 and 3 (thus -1 becomes 3 which is the last index)
			offsetFromPlayerWithIndexZero = 4 + offsetFromPlayerWithIndexZero;
		}

		return offsetFromPlayerWithIndexZero;
	}

	/// <summary>
	/// Gets the target player for the player with the hand at <paramref name="offset"/>.
	/// </summary>
	/// <param name="dealer">The dealer's internal index</param>
	/// <param name="offset">The offset of the target player's hand</param>
	/// <returns></returns>
	public static int GetTargetPlayerFromHandOffset(int dealer, int offset)
	{
		// Dealer deals to the left
		var playerWithIndex0 = (dealer + 1) % 4;
		return (playerWithIndex0 + offset) % 4;
	}
}

