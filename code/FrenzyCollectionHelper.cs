
using System.Collections.Generic;

class FrenzyCollectionHelper
{

	private static List<FrenzyCollectible.FrenzyLetter> Collection;

	public enum Result
	{
		None,
		Added,
		Completed
	}

	public static Result TryAdd( FrenzyCollectible.FrenzyLetter letter )
	{
		var cookiename = $"{Game.Server.MapIdent}.frenzycollection1";
		Collection ??= Cookie.Get<List<FrenzyCollectible.FrenzyLetter>>( cookiename, new() );

		FrenzyCollectionHud.Current.Display();

		if ( Collection.Contains( letter ) )
			return Result.None;

		Collection.Add( letter );

		Cookie.Set( cookiename, Collection );

		if ( Collection.Count == 6 )
			return Result.Completed;

		return Result.Added;
	}

	public static bool Contains( FrenzyCollectible.FrenzyLetter letter )
	{
		var cookiename = $"{Game.Server.MapIdent}.frenzycollection1";
		Collection ??= Cookie.Get<List<FrenzyCollectible.FrenzyLetter>>( cookiename, new() );

		return Collection.Contains( letter );
	}

}
