#nullable enable

/// <summary>
/// Helpers for reading from / writing to <see cref="FileSystem.Data"/>.
/// </summary>
public static class DataHelper
{
	public static T? ReadJson<T>( string fileName )
	{
		if ( !FileSystem.Data.FileExists( fileName ) )
		{
			return default;
		}

		try
		{
			var text = FileSystem.Data.ReadAllText( fileName );
			return Json.Deserialize<T>( text );
		}
		catch ( Exception ex )
		{
			Log.Warning( ex );
			return default;
		}
	}

	public static void WriteJson<T>( string fileName, T value )
	{
		try
		{
			var json = Json.Serialize( value );
			FileSystem.Data.WriteAllText( fileName, json );
		}
		catch ( Exception ex )
		{
			Log.Error( ex );
		}
	}
}
