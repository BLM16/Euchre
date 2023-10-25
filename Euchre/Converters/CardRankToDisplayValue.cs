namespace BLM16.Games.Euchre.Converters;

using System.Diagnostics;
using System.Globalization;

public class CardRankToDisplayValue : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Engine.Rank rank)
        {
            throw new ArgumentException($"{nameof(Convert)} only works on type {typeof(Engine.Rank)}");
        }

        return rank switch
        {
            Engine.Rank.Nine => '9',
            Engine.Rank.Ten => 'T',
            Engine.Rank.Jack => 'J',
            Engine.Rank.Queen => 'Q',
            Engine.Rank.King => 'K',
            Engine.Rank.Ace => 'A',

            // All the suits are accounted for
            _ => throw new UnreachableException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not char rank)
        {
            throw new ArgumentException($"{nameof(ConvertBack)} only works on type char");
        }

        return rank switch
        {
            '9' => Engine.Rank.Nine,
            'T' => Engine.Rank.Ten,
            'J' => Engine.Rank.Jack,
            'Q' => Engine.Rank.Queen,
            'K' => Engine.Rank.King,
            'A' => Engine.Rank.Ace,

            // Not a valid rank display
            _ => throw new ArgumentException($"{nameof(ConvertBack)} only maps values 9, T, J, Q, K, A")
        };
    }
}
