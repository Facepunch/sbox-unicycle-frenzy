using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class CustomizationComponent : EntityComponent
{

	public static string EnsembleJson
	{
		get => Cookie.Get( "customization.ensemble", string.Empty );
		set => Cookie.Set( "customization.ensemble", value );
	}

	public List<CustomizationPart> Parts = new();

	protected override void OnActivate()
	{
		base.OnActivate();

		if ( Entity.IsClient )
		{
			Deserialize( EnsembleJson );
		}
	}

	public CustomizationPart GetEquippedPart( PartType type )
	{
		var result = Parts.FirstOrDefault( x => x.PartType == type );

		if ( result == null ) 
			result = CustomizationPart.FindDefaultPart( type );

		return result;
	}

	public void Equip( string resourceName ) => Equip( CustomizationPart.Find( resourceName ) );
	public void Equip( CustomizationPart part )
	{
		if ( part == null ) throw new Exception("Can't equip null");

		if ( Parts.Contains( part ) )
		{
			//throw new Exception( "Equipping a part that is already equipped" );
			return;
		}

		var partInSlot = GetEquippedPart( part.PartType );
		if ( partInSlot != null )
		{
			Unequip( partInSlot );
		}

		Parts.Add( part );

		if ( Host.IsClient )
		{
			EnsembleJson = Serialize();
			EquipPartOnServer( part.ResourceName );
		}
	}

	public void Unequip( string resourceName ) => Unequip( CustomizationPart.Find( resourceName ) );
	public void Unequip( CustomizationPart part )
	{
		if ( part == null ) throw new Exception( "Can't equip null" );

		if ( !Parts.Contains( part ) )
		{
			//throw new Exception( "Unequipping a part that isn't equipped" );
			return;
		}

		Parts.Remove( part );

		if ( Host.IsClient )
		{
			EnsembleJson = Serialize();
			UnequipPartOnServer( part.ResourceName );
		}
	}

	public bool IsEquipped( CustomizationPart part )
	{
		if ( part == null ) throw new Exception( "Can't equip null" );

		return Parts.Any( x => x.ResourceName == part.ResourceName );
	}

	public string Serialize()
	{
		return JsonSerializer.Serialize( Parts.Select( x => new Entry { ResourceName = x.ResourceName } ) );
	}

	public void Deserialize( string json )
	{
		Parts.Clear();

		if ( string.IsNullOrWhiteSpace( json ) )
			return;

		try
		{
			var entries = JsonSerializer.Deserialize<Entry[]>( json );

			foreach ( var entry in entries )
			{
				var item = CustomizationPart.Find( entry.ResourceName );
				if ( item == null ) continue;
				Equip( item );
			}
		}
		catch ( Exception e )
		{
			Log.Warning( e, "Error deserailizing clothing" );
		}
	}

	public int GetPartsHash()
	{
		int hash = 0;
		foreach ( var part in Parts )
		{
			hash = HashCode.Combine( hash, part.ResourceName );
		}
		return hash;
	}

	public struct Entry
	{
		public string ResourceName { get; set; }
	}

	[ConCmd.Server]
	public static void EquipPartOnServer( string resourceName )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		var cfg = caller.Components.Get<CustomizationComponent>();
		if ( cfg == null ) return;

		cfg.Equip( resourceName );
	}

	[ConCmd.Server]
	public static void UnequipPartOnServer( string resourceName )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		var cfg = caller.Components.Get<CustomizationComponent>();
		if ( cfg == null ) return;

		cfg.Unequip( resourceName );
	}

}

