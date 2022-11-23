
using Sandbox;
using System;

internal static class FloatExtensions
{
	public static string FormattedTimeMsf( this float seconds )
	{
		return TimeSpan.FromSeconds( seconds ).ToString( @"m\:ss\.ff" );
	}
	public static string FormattedTimeMs( this float seconds )
	{
		return TimeSpan.FromSeconds( seconds ).ToString( @"m\:ss" );
	}
	public static string FormattedTimeMsf( this TimeSince ts )
	{
		return TimeSpan.FromSeconds( (float)ts ).ToString( @"m\:ss\.ff" );
	}
	public static string FormattedTimeMs( this TimeSince ts )
	{
		return TimeSpan.FromSeconds( (float)ts ).ToString( @"m\:ss" );
	}
}
