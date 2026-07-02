namespace StartingWeapons.Utilities;

/// <summary>
///     Provides <see cref="Player"/> extension methods.
/// </summary>
public static class PlayerExtensions
{
    public static bool HasAltFunctionUse(this Player player)
    {
        return player.altFunctionUse == ItemAlternativeFunctionID.ActivatedAndUsed;
    }
}