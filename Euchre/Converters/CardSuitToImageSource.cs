namespace BLM16.Games.Euchre.Converters;

using System.Diagnostics;
using System.Globalization;

public class CardSuitToImageSource : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not Engine.Suit suit)
		{
			throw new ArgumentException($"{nameof(Convert)} only works on type {typeof(Engine.Suit)}");
		}

		return suit switch
		{
			Engine.Suit.Hearts => ImageSource.FromFile("suit_hearts.png"),
			Engine.Suit.Diamonds => ImageSource.FromFile("suit_diamonds.png"),
			Engine.Suit.Spades => ImageSource.FromFile("suit_spades.png"),
			Engine.Suit.Clubs => ImageSource.FromFile("suit_clubs.png"),

			// All the suits are accounted for
			_ => throw new UnreachableException(),
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
