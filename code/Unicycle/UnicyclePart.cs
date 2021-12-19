using System;
using System.Collections.Generic;
using System.Linq;

public class UnicyclePart
{

	public int Id { get; set; }
	public string Name { get; set; }
	public string Model { get; set; }
	public PartType Type { get; set; }
	public bool IsDefault { get; set; }

	// todo: to json, from json, tool to create and modify parts
	// also need to increment id in the tool so it's always unique
	public static List<UnicyclePart> All = new()
	{
		//
		new() { Id = 0, Type = PartType.Wheel, Name = "Dev Wheel", Model = "models/parts/wheels/dev_wheel.vmdl", IsDefault = true },
		new() { Id = 2, Type = PartType.Wheel, Name = "Mini Dev Wheel", Model = "models/parts/wheels/dev_wheel_mini.vmdl" },

		//
		new() { Id = 3, Type = PartType.Frame, Name = "Dev Frame", Model = "models/parts/frames/dev_frame.vmdl", IsDefault = true },

		//
		new() { Id = 4, Type = PartType.Seat, Name = "Dev Seat", Model = "models/parts/seats/dev_seat.vmdl", IsDefault = true },

		//
		new() { Id = 5, Type = PartType.Pedal, Name = "Dev Pedal", Model = "models/parts/pedals/dev_pedal.vmdl", IsDefault = true },

		//
		new() { Id = 6, Type = PartType.Trail, Name = "Default", Model = "particles/trails/default_trail.vpcf", IsDefault = true },
		
		//
		new() { Id = 7, Type = PartType.Trail, Name = "Rainbow", Model = "particles/trails/rainbow_trail.vpcf", IsDefault = false },
		
		//
		new() { Id = 8, Type = PartType.Trail, Name = "Fire", Model = "particles/trails/fire_trail.vpcf", IsDefault = false },

		//
		new() { Id = 9, Type = PartType.Trail, Name = "Electric", Model = "particles/trails/electric_trail.vpcf", IsDefault = false },

		//
		new() { Id = 10, Type = PartType.Trail, Name = "Dot", Model = "particles/trails/dot_trail.vpcf", IsDefault = false },

		//
		new() { Id = 11, Type = PartType.Trail, Name = "Pixel", Model = "particles/trails/pixel_trail.vpcf", IsDefault = false },

		//
		new() { Id = 12, Type = PartType.Trail, Name = "Pixel Fire", Model = "particles/trails/fire_pixel_trail.vpcf", IsDefault = false },

		//
		new() { Id = 13, Type = PartType.Trail, Name = "Water", Model = "particles/trails/water_trail.vpcf", IsDefault = false },

		//
		new() { Id = 14, Type = PartType.Trail, Name = "Shapes", Model = "particles/trails/Shapes_trail.vpcf", IsDefault = false },

		//
		new() { Id = 15, Type = PartType.Trail, Name = "Binary", Model = "particles/trails/binary_trail.vpcf", IsDefault = false },

		//
		new() { Id = 16, Type = PartType.Trail, Name = "Coin", Model = "particles/trails/coins_trail.vpcf", IsDefault = false },

		//
		new() { Id = 17, Type = PartType.Trail, Name = "Trash", Model = "particles/trails/Trash_trail.vpcf", IsDefault = false },

		//
		new() { Id = 18, Type = PartType.Trail, Name = "Miami", Model = "particles/trails/Miami_trail.vpcf", IsDefault = false },

		//
		new() { Id = 19, Type = PartType.Trail, Name = "Clouds", Model = "particles/trails/cloud_trail.vpcf", IsDefault = false },

		//
		new() { Id = 20, Type = PartType.Trail, Name = "Musical", Model = "particles/trails/music_trail.vpcf", IsDefault = false },

		//
		new() { Id = 21, Type = PartType.Trail, Name = "Black Hole", Model = "particles/trails/blackhole_trail.vpcf", IsDefault = false }

	};

}

public enum PartType
{
	Frame,
	Wheel,
	Seat,
	Pedal,
	Trail
}

