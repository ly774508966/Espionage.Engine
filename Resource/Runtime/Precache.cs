﻿namespace Espionage.Engine.Resources
{
	/// <summary>
	/// Precache is used for making resources persistant.
	/// Meaning it won't be unloaded when it goes out of scope.
	/// </summary>
	public static class Precache
	{
		public static void Add( Resource model )
		{
			model.Persistant = true;
		}
		
		public static void Add( params Resource[] models )
		{
			foreach ( var model in models )
			{
				model.Persistant = true;
			}
		}
	}
}
