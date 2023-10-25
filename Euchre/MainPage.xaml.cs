namespace BLM16.Games.Euchre;

using Engine;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

public partial class MainPage : ContentPage
{
	/// <summary>
	/// The current instance of the game.
	/// </summary>
	private readonly Game game;

	/// <summary>
	/// A random number generator to randomize the computer choice's delay.
	/// </summary>
	private readonly Random rng;

	/// <summary>
	/// Keeps track of whether the player has finished their turn or not.
	/// Since the game loop runs asynchronously and waits for the player's
	/// input before resuming, this indicates to the TaskScheduler that
	/// the player has made their choice and to resume the game loop.
	/// </summary>
	private TaskCompletionSource<bool> playerInputCompletionSource;

	/// <summary>
	/// Limits the number of parallel threads for the game loop to 1.
	/// This forces the async game loop to stay in the same instance
	/// and prevents multiple turns from running simultaneously.
	/// </summary>
	private readonly SemaphoreSlim gameLoopSemaphore;

	public MainPage()
	{
		game = new Game();
		rng = new Random();
		playerInputCompletionSource = new TaskCompletionSource<bool>();
		gameLoopSemaphore = new SemaphoreSlim(1, 1);

		InitializeComponent();
		BindingContext = this;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Subscribe to game updates and change the UI accordingly
		game.PropertyChanged += GameChangeHandler;

		// Set initial values for cards
		PlayerHand.ItemsSource = game.PlayerHand;
		UpdatePlayedCards();

		// Disable player selection until it is their turn
		PlayerHand.IsEnabled = false;

		// Can be set prior to subscribing to engine notifications
		// Initialize this to prevent that
		DealerIndicator.Text = $"Dealer: {GetPositionNameFromAbsoluteIndex(game.Dealer)}";

		// Start game loop in separate thread
		// Separates the UI thread from the game
		Task.Run(async () =>
		{
			while (true)
			{
				await gameLoopSemaphore.WaitAsync(); // Wait for current turn to finish
				await HandleNextTurn();
				gameLoopSemaphore.Release(); // Release the lock for the next turn
			}
		});
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		// Unsubscribe from game updates when the UI disappears
		game.PropertyChanged -= GameChangeHandler;
	}

	/// <summary>
	/// Updates the played cards in the UI.
	/// </summary>
	private void UpdatePlayedCards()
	{
		var topCard = game.PlayedCards.ElementAtOrDefault(Helpers.GetHandOffset(game.Dealer, 0));
		PlayedCardTopSlot.Content = new Templates.Card()
		{
			BindingContext = topCard,
			IsVisible = topCard is not null
		};

		var rightCard = game.PlayedCards.ElementAtOrDefault(Helpers.GetHandOffset(game.Dealer, 1));
		PlayedCardRightSlot.Content = new Templates.Card()
		{
			BindingContext = rightCard,
			IsVisible = rightCard is not null
		};

		var bottomCard = game.PlayedCards.ElementAtOrDefault(Helpers.GetHandOffset(game.Dealer, 2));
		PlayedCardBottomSlot.Content = new Templates.Card()
		{
			BindingContext = bottomCard,
			IsVisible = bottomCard is not null
		};

		var leftCard = game.PlayedCards.ElementAtOrDefault(Helpers.GetHandOffset(game.Dealer, 3));
		PlayedCardLeftSlot.Content = new Templates.Card()
		{
			BindingContext = leftCard,
			IsVisible = leftCard is not null
		};
	}

	/// <summary>
	/// Handles calling the appropriate methods for the next turn in the game.
	/// </summary>
	/// <returns><see langword="void"/> but as a <see cref="Task"/> for the semaphore slim</returns>
	private async Task HandleNextTurn()
	{
		// No turns have yet happened this trick meaning we need to bid on trump
		if (!game.IsTrumpMade)
		{
			await HandleMakingTrump();

			// Update on UI thread
			await Dispatcher.DispatchAsync(() =>
			{
				OrderUpView.IsVisible = false;
				TrumpBiddingView.IsVisible = false;
				MainContent.IsVisible = true;
			});
		}

		// Computer's turn
		if (game.Turn != game.PlayerTurnNumber)
		{
			Thread.Sleep(rng.Next(300, 900)); // Simulate computer thinking
			game.PlayCurrentComputerTurn();
		}
		else
		{
			// Update on UI thread
			await Dispatcher.DispatchAsync(() =>
			{
				PlayerHand.IsEnabled = true; // Enable player to play a card
			});
			
			// Wait for player to play a card
			await playerInputCompletionSource.Task;

			// Update on UI thread
			await Dispatcher.DispatchAsync(() =>
			{
				PlayerHand.SelectedItem = null; // Clear the old selected card
				PlayerHand.IsEnabled = false; // Disable the player's ability to play again
			});

			// Reset the completion source for the player's next input
			playerInputCompletionSource = new TaskCompletionSource<bool>();
		}

		game.Turn++;

		// End of trick
		// Post-increment compares with the previous value of Turn then increments it
		if (Array.TrueForAll(game.PlayedCards, c => c is not null))
		{
			await Task.Delay(3000); // Sleep 3 seconds so user can see the game state
			game.NewTrick();
		}
	}

	/// <summary>
	/// Handles calling the appropriate methods for bidding on trump.
	/// </summary>
	/// <returns><see langword="void"/> but as a <see cref="Task"/> for the semaphore slim</returns>
	private async Task HandleMakingTrump()
	{
		// Order up the turned up card
		for (var i = 0; i < 4; i++)
		{
			if (game.Turn + i != game.PlayerTurnNumber)
			{
				Thread.Sleep(rng.Next(300, 900)); // Simulate computer thinking
				game.PlayComputerOrderUp(i);
			}
			else
			{
				// Update on UI thread
				await Dispatcher.DispatchAsync(() =>
				{
					OrderUpView.IsVisible = true;
					//OrderUpCardBtn.Rank = game.TurnedUp.Rank;
					//OrderUpCardBtn.Suit = game.TurnedUp.Suit;
					BtnOrderUp.Source = (FileImageSource)new Converters.CardSuitToImageSource().Convert(game.TurnedUp.Suit, typeof(FileImageSource), null, CultureInfo.CurrentCulture);
					TrumpBiddingView.IsVisible = false;
					MainContent.IsVisible = false;
				});

				// Wait for player to pick whether or not to order it up
				await playerInputCompletionSource.Task;

				// Update on UI thread
				await Dispatcher.DispatchAsync(() =>
				{
					OrderUpView.IsVisible = false;
				});

				// Reset the completion source for the player's next input
				playerInputCompletionSource = new TaskCompletionSource<bool>();
			}

			// The computer or player made trump
			if (game.IsTrumpMade)
			{
				// Update on UI thread
				await Dispatcher.DispatchAsync(() =>
				{
					TrumpSuitIndicator.Source = (FileImageSource)new Converters.CardSuitToImageSource().Convert(game.Trump, typeof(FileImageSource), null, CultureInfo.CurrentCulture);
					CallerIndicator.Text = $"Caller: {GetPositionNameFromDealerOffset(game.Caller)}";
				});
				return;
			}
		}

		// Bidding on trump suit
		for (var i = 0; i < 4; i++)
		{
			if (game.Turn + i != game.PlayerTurnNumber)
			{
				Thread.Sleep(rng.Next(300, 900)); // Simulate computer thinking
				game.PlayComputerBidOnTrump(i);
			}
			else
			{
				// Update on UI thread
				await Dispatcher.DispatchAsync(() =>
				{
					OrderUpView.IsVisible = false;
					BiddingPass.IsVisible = true;
					TrumpBiddingView.IsVisible = true;
					MainContent.IsVisible = false;

					// Stick the dealer so player must make trump as last player
					if (i == 3)
					{
						BiddingPass.IsVisible = false;
					}
				});

				// Wait for player to bid on trump
				await playerInputCompletionSource.Task;

				// Update on UI thread
				await Dispatcher.DispatchAsync(() =>
				{
					TrumpBiddingView.IsVisible = false;
				});

				// Reset the completion source for the player's next input
				playerInputCompletionSource = new TaskCompletionSource<bool>();
			}

			// The computer or player made trump
			if (game.IsTrumpMade)
			{
				// Update on UI thread
				await Dispatcher.DispatchAsync(() =>
				{
					TrumpSuitIndicator.Source = (FileImageSource)new Converters.CardSuitToImageSource().Convert(game.Trump, typeof(FileImageSource), null, CultureInfo.CurrentCulture);
					CallerIndicator.Text = $"Caller: {GetPositionNameFromDealerOffset(game.Caller)}";
				});
				return;
			}
		}
	}

	/// <summary>
	/// Callback that runs whenever the player clicks a card in their hand.
	/// </summary>
	private void PlayerHand_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		// Ignore when SelectedItem is cleared (only handle an actual selection)
		if (e.CurrentSelection.Count == 0) return;

		// Play the selected card
		var cardToPlay = (Card)e.CurrentSelection[0];
		game.PlayPlayerTurn(cardToPlay);

		// Signal that the player has taken their turn
		playerInputCompletionSource.SetResult(true);
	}

	/// <summary>
	/// Callback that runs whenever the player orders up trump.
	/// </summary>
	private void OrderUp_Clicked(object sender, EventArgs e)
	{
		game.PlayPlayerOrderUp();
		playerInputCompletionSource.SetResult(true);
	}

	/// <summary>
	/// Callback that runs whenever the player passes on making trump.
	/// </summary>
	private void PassOnTrump_Clicked(object sender, EventArgs e)
	{
		// Nothing needs to be done in the engine
		// Signal the player has given an input
		playerInputCompletionSource.SetResult(true);
	}

	#region Trump bidding callbacks

	private void SuitHearts_Clicked(object sender, EventArgs e)
	{
		game.PlayPlayerBidOnTrump(Suit.Hearts);
		playerInputCompletionSource.SetResult(true);
	}

	private void SuitDiamonds_Clicked(object sender, EventArgs e)
	{
		game.PlayPlayerBidOnTrump(Suit.Diamonds);
		playerInputCompletionSource.SetResult(true);
	}

	private void SuitSpades_Clicked(object sender, EventArgs e)
	{
		game.PlayPlayerBidOnTrump(Suit.Spades);
		playerInputCompletionSource.SetResult(true);
	}

	private void SuitClubs_Clicked(object sender, EventArgs e)
	{
		game.PlayPlayerBidOnTrump(Suit.Clubs);
		playerInputCompletionSource.SetResult(true);
	}

	#endregion

	/// <summary>
	/// Gets the position name for a given absolute player index (0 being top, 1 right, etc).
	/// </summary>
	/// <param name="playerIndex">The absolute index of the player</param>
	/// <returns>The name of the position (top, right, you, left)</returns>
	private static string GetPositionNameFromAbsoluteIndex(int playerIndex)
	{
		return playerIndex switch
		{
			0 => "Top",
			1 => "Right",
			2 => "You",
			3 => "Left",

			_ => throw new UnreachableException()
		};
	}

	/// <summary>
	/// Gets the position name for a given player offset from the dealer.
	/// </summary>
	/// <param name="offset">The player's offset from the dealer</param>
	/// <returns>The name of the position (top, right, you, left)</returns>
	/// <seealso cref="GetPositionNameFromAbsoluteIndex"/>
	private string GetPositionNameFromDealerOffset(int offset)
		=> GetPositionNameFromAbsoluteIndex(Helpers.GetTargetPlayerFromHandOffset(game.Dealer, offset));

	/// <summary>
	/// Handles all the subscribed game events and performs the appropriate actions
	/// to update the UI for the given event.
	/// </summary>
	private async void GameChangeHandler(object sender, PropertyChangedEventArgs e)
	{
		await Dispatcher.DispatchAsync(() =>
		{
			// Update on UI thread
			switch (e.PropertyName)
			{
				case nameof(game.PlayerHand):
					PlayerHand.ItemsSource = null; // Required to force the UI to update (known .NET MAUI bug)
					PlayerHand.ItemsSource = game.PlayerHand;
					break;
				case nameof(game.PlayedCards):
					UpdatePlayedCards();
					break;
				case nameof(game.Score):
					txtPlayerScore.Text = game.Score.PlayerTeam.ToString();
					txtOpponentScore.Text = game.Score.OtherTeam.ToString();
					break;
				case nameof(game.TrickCount):
					txtPlayerTricks.Text = game.TrickCount.PlayerTeam.ToString();
					txtOpponentTricks.Text = game.TrickCount.OtherTeam.ToString();
					break;
				case nameof(game.Dealer):
					DealerIndicator.Text = $"Dealer: {GetPositionNameFromAbsoluteIndex(game.Dealer)}";
					break;
			}
		});
	}
}
