namespace BLM16.Games.Euchre.Engine;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Game : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private readonly Deck deck;
	private (int, int) score;

	private int dealer;
	private int turn;
	private int leader;

	private List<List<Card>> hands;
	private Card[] kitty;
	private Card?[] playedCards;
	private int tricksPlayed;
	private (int, int) trickCount;

	private Suit trump;
	private int caller;

	/// <summary>
	/// The game score.
	/// Score.0 = player team's score.
	/// Score.1 = opposite team's score.
	/// </summary>
	public (int PlayerTeam, int OtherTeam) Score
	{
		get => score;
		set
		{
			if (score == value) return;

			score = value;
			NotifyPropertyChanged();
		}
	}

	/// <summary>
	/// The cards in everyone's hands.
	/// </summary>
	public List<List<Card>> Hands
	{
		get => hands;
		private set
		{
			if (hands == value) return;

			hands = value;
			NotifyPropertyChanged();
			NotifyPropertyChanged(nameof(PlayerHand)); // PlayerHand indexes Hands
		}
	}

	/// <summary>
	/// The cards in the player's hand.
	/// </summary>
	public List<Card> PlayerHand => Hands[PlayerTurnNumber];

	/// <summary>
	/// The 4 card kitty that does not get delt to players.
	/// The first card in the kitty is turned up when making trump.
	/// </summary>
	public Card[] Kitty
	{
		get => kitty;
		private set
		{
			if (kitty == value) return;

			kitty = value;
			NotifyPropertyChanged();
			NotifyPropertyChanged(nameof(TurnedUp)); // TurnedUp indexes Kitty
		}
	}

	/// <summary>
	/// The card that was turned up to make trump.
	/// </summary>
	public Card TurnedUp => kitty[0];

	/// <summary>
	/// Represents whose deal it is.
	/// It is the player's deal when this is <see cref="PlayerTurnNumber"/>.
	/// </summary>
	public int Dealer
	{
		get => dealer;
		private set
		{
			if (dealer == value) return;

			dealer = value;
			NotifyPropertyChanged();
		}
	}

	/// <summary>
	/// The current turn number in the trick.
	/// Represents whose turn it is to play a card.
	/// </summary>
	public int Turn
	{
		get => turn;
		set
		{
			if (turn == value) return;

			turn = value % 4; // Constrain turn from 0-3 inclusive
			NotifyPropertyChanged();
		}
	}

	public int PlayerTurnNumber => Helpers.GetHandOffset(Dealer, 2);

	/// <summary>
	/// The played cards in the current trick.
	/// Starts with the player left of the dealer.
	/// </summary>
	public Card?[] PlayedCards
	{
		get => playedCards;
		private set
		{
			if (playedCards == value) return;

			playedCards = value;
			NotifyPropertyChanged();
		}
	}

	/// <summary>
	/// The number of tricks played in the current hand.
	/// </summary>
	public int TricksPlayed
	{
		get => tricksPlayed;
		set
		{
			if (tricksPlayed == value) return;

			tricksPlayed = value;
			NotifyPropertyChanged();
		}
	}

	public (int PlayerTeam, int OtherTeam) TrickCount
	{
		get => trickCount;
		set
		{
			if (trickCount == value) return;

			trickCount = value;
			NotifyPropertyChanged();
		}
	}

	/// <summary>
	/// The current trump suit for the trick.
	/// </summary>
	public Suit Trump
	{
		get => trump;
		set
		{
			if (trump == value) return;

			trump = value;
			NotifyPropertyChanged();
		}
	}

	/// <summary>
	/// Has trump been made for the trick.
	/// </summary>
	public bool IsTrumpMade { get; private set; }

	/// <summary>
	/// The player who called trump for the trick.
	/// </summary>
	public int Caller
	{ 
		get => caller;
		set
		{
			if (caller == value) return;

			caller = value;
			NotifyPropertyChanged();
		}
	}

	/// <summary>
	/// Creates a new game with a trick already prepared.
	/// <see cref="NewTrick"/> should not be called until
	/// the first trick has been played.
	/// </summary>
	public Game()
	{
		deck = new Deck();
		dealer = 0;
		turn = 0;
		leader = 0;
		(hands, kitty) = deck.DealHands();
		playedCards = new Card[4];
		tricksPlayed = 0;
		trickCount = (0, 0);
	}

	/// <summary>
	/// Clears the played cards and sets up for the next trick.
	/// </summary>
	public void NewTrick()
	{
		var trickWinner = UpdateTrickCountAndGetWinner();
		
		TricksPlayed++;

		// End of hand
		if (TricksPlayed == 5)
		{
			GivePointsToHandWinner();
			NewHand();
			return;
		}

		// End of game
		if (TrickCount.PlayerTeam >= 10 || TrickCount.OtherTeam >= 10)
		{
			NewGame();
			return;
		}

		Turn = trickWinner; // The winner leads
		leader = trickWinner; // Track who is leading the trick
		playedCards = new Card[4];

		NotifyPropertyChanged(nameof(PlayedCards));
	}

	/// <summary>
	/// Shuffles the deck, clears the played cards, shifts the dealer, and deals new hands.
	/// </summary>
	private void NewHand()
	{
		deck.Shuffle();
		Dealer = (Dealer + 1) % 4; // Cycles between 0 and 3 inclusive
		Turn = 0;
		leader = 0;
		(Hands, Kitty) = deck.DealHands();
		PlayedCards = new Card[4];
		TricksPlayed = 0;
		TrickCount = (0, 0);
		IsTrumpMade = false;
	}

	private void NewGame()
	{

		Dealer = (Dealer + 1) % 4; // Pass the dealer to the next person for the new game
		Turn = 0;
		leader = 0;
		(Hands, Kitty) = deck.DealHands();
		PlayedCards = new Card[4];
		TricksPlayed = 0;
		TrickCount = (0, 0);
		IsTrumpMade = false;
	}

	/// <summary>
	/// Orders up the turned up card on behalf of the computer.
	/// This method should always be called, however the computer may choose not to make it.
	/// In this case, <see cref="IsTrumpMade"/> will remain <see langword="false"/>.
	/// </summary>
	/// <param name="handOffset">The offset of the computer player from the current turn</param>
	public void PlayComputerOrderUp(int handOffset)
	{
		var computerHand = Hands[Turn + handOffset];
		var orderUp = ComputerPlayer.OrderUpTrump(computerHand, TurnedUp.Suit);
		
		if (orderUp)
		{
			Trump = TurnedUp.Suit;
			IsTrumpMade = true;
			Caller = Turn + handOffset;
		}
	}

	/// <summary>
	/// Selects the trump suit on behalf of the computer after everyone has passed.
	/// This method should always be called, however the computer may choose not to make it.
	/// In this case, <see cref="IsTrumpMade"/> will remain <see langword="false"/>.
	/// This method accounts for stick the dealer which must be followed.
	/// </summary>
	/// <param name="handOffset">The offset of the computer player from the current turn</param>
	public void PlayComputerBidOnTrump(int handOffset)
	{
		var computerHand = Hands[Turn + handOffset];
		var (orderUp, suit) = ComputerPlayer.BidOnTrump(computerHand, TurnedUp.Suit);
		
		// Computer to order it up
		// Stick the dealer
		if (orderUp || handOffset == 3)
		{
			Trump = suit;
			IsTrumpMade = true;
			Caller = Turn + handOffset;
		}
	}

	/// <summary>
	/// Orders up the turned up card on behalf of the player.
	/// If the player passes, do not call this method.
	/// </summary>
	public void PlayPlayerOrderUp()
	{
		Trump = TurnedUp.Suit;
		IsTrumpMade = true;
		Caller = PlayerTurnNumber;
	}

	/// <summary>
	/// Selects the trump suit on behalf of the player after everyone has passed.
	/// If the player passes again, do not call this method.
	/// Stick the dealer must be followed, so as last call, calling this method is required.
	/// </summary>
	/// <param name="trumpSuit">The suit the player selected as trump</param>
	public void PlayPlayerBidOnTrump(Suit trumpSuit)
	{
		Trump = trumpSuit;
		IsTrumpMade = true;
		Caller = PlayerTurnNumber;
	}

	/// <summary>
	/// Plays for the computer player for the turn <see cref="Turn"/>.
	/// </summary>
	public void PlayCurrentComputerTurn()
	{
		var computerChoice = ComputerPlayer.PickCard(Hands[Turn], Trump, PlayedCards, leader, Caller == Turn);

		// Move the card from the computer's hand into played cards
		PlayedCards[Turn] = computerChoice;
		Hands[Turn].Remove(computerChoice);

		NotifyPropertyChanged(nameof(PlayedCards));
		NotifyPropertyChanged(nameof(Hands));
	}

	/// <summary>
	/// Plays for the player's turn <see cref="PlayerTurnNumber"/>
	/// </summary>
	/// <param name="cardToPlay">The card the player selected to play</param>
	public void PlayPlayerTurn(Card cardToPlay)
	{
		// Move the card from the player's hand into played cards
		PlayedCards[Turn] = cardToPlay;
		Hands[Turn].Remove(cardToPlay);
		
		NotifyPropertyChanged(nameof(PlayedCards));
		NotifyPropertyChanged(nameof(Hands));
		NotifyPropertyChanged(nameof(PlayerHand));
	}

	/// <summary>
	/// Give the trick to the team who won it.
	/// </summary>
	/// <returns>The offset of the trick winner.</returns>
	private int UpdateTrickCountAndGetWinner()
	{
		var winningCard = PlayedCards[leader]!;
		foreach (var card in PlayedCards)
		{
			if (card!.IsHigherThan(winningCard, Trump))
			{
				winningCard = card;
			}
		}

		var winnerOffset = Array.IndexOf(PlayedCards, winningCard);
		switch (winnerOffset)
		{
			case 1 or 3 when Dealer % 2 == 0: // Player team deal meaning offset 0 is other team
			case 0 or 2 when Dealer % 2 == 1: // Other team deal meaning offset 0 is player team
				TrickCount = (TrickCount.PlayerTeam + 1, TrickCount.OtherTeam);
				break;

			default:
				TrickCount = (TrickCount.PlayerTeam, TrickCount.OtherTeam + 1);
				break;
		}

		return winnerOffset;
	}

	/// <summary>
	/// Give the correct number of points to the team who won the hand.
	/// </summary>
	private void GivePointsToHandWinner()
	{
		switch (TrickCount)
		{
			case (5, 0):
				Score = (Score.PlayerTeam + 2, Score.OtherTeam);
				break;

			case (0, 5):
				Score = (Score.PlayerTeam, Score.OtherTeam + 2);
				break;

			case ( >= 3, _):
				// Dealer is absolute index where 0, 2 are always the player team
				// If dealer and caller are equal mod 2, either:
				//	 The player team dealed and won
				//   The opposing team dealed and the caller offset is 1 or 3 (player team)
				// If player team called it is 1 point
				// Otherwise 2 points for a euchre
				Score = (Caller + 1) % 2 == Dealer % 2 ?
							(Score.PlayerTeam + 1, Score.OtherTeam) :
							(Score.PlayerTeam + 2, Score.OtherTeam);
				break;

			case (_, >= 3):
				// Dealer is absolute index where 1, 3 are always the opposing team
				// If dealer and caller are not equal mod 2, either:
				//	 The opposing team dealed and the caller offset is 0 or 2 (opposing team)
				//   The player team dealed and the caller offset is 1 or 3 (opposing team)
				// If opposing team called it is 1 point
				// Otherwise 2 points for a euchre
				Score = (Caller + 1) % 2 != Dealer % 2 ?
							(Score.PlayerTeam, Score.OtherTeam + 1) :
							(Score.PlayerTeam, Score.OtherTeam + 2);
				break;
		}
	}

	private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
