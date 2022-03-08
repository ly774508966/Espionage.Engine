﻿using System;
using System.Collections.Generic;
using Espionage.Engine.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Espionage.Engine.Resources
{
	/// <summary> Load and Unload models at runtime. </summary>
	[Group( "Models" ), Path( "models", "assets://Models/" )]
	public sealed class Model : Resource
	{
		// Provider
		private IProvider<Model, GameObject> Provider { get; }
		public override string Identifier => Provider.Identifier;

		//
		// Constructors
		//

		/// <summary> Create a new Model. </summary>
		/// <param name="provider"> How should we be processing this model. </param>
		public Model( IProvider<Model, GameObject> provider )
		{
			Provider = provider;
		}

		/// <summary> Loads the Model at the Path. </summary>
		/// <returns> The new model that has been loaded. </returns>
		public static Model Load( string path )
		{
			path = Files.GetPath( path );

			if ( !Files.Exists( path ) )
			{
				return Load( "models://error.umdl" );
			}

			if ( Database[path] is Model databaseModel )
			{
				((IResource)databaseModel).Load();
				return databaseModel;
			}

			using var _ = Debugging.Stopwatch( $"Loading Model [{Files.GetPath( path )}]" );

			var model = new Model( Files.Load<IFile<Model, GameObject>>( path ).Provider() );
			((IResource)model).Load();
			return model;
		}

		//
		// Spawning
		//

		private List<GameObject> Spawned { get; } = new();

		/// <summary> Spawns the Model as a GameObject. </summary>
		/// <returns> The spawned Model. </returns>
		public GameObject Spawn( Transform container )
		{
			var go = Object.Instantiate( Provider.Output, container );
			go.transform.localPosition = Vector3.zero;
			Spawned.Add( go );
			go.name = "Model";

			return go;
		}

		//
		// Resource
		//

		public override bool IsLoading => Provider.IsLoading;

		/// <summary> Remove target GameObject from Spawned List. </summary>
		/// <param name="gameObject"> The Spawned Model to Destroy </param>
		/// <remarks> MAKE SURE YOU CALL THIS OR ELSE HAVE FUN WITH MEMORY LEAKS! </remarks>
		public void Remove( GameObject gameObject )
		{
			Spawned.Remove( gameObject );
			((IResource)this).Unload();

			Object.Destroy( gameObject );
		}

		// States

		protected override void OnLoad( Action onLoad )
		{
			Provider.Load( onLoad );
		}

		protected override void OnUnload( Action onUnload )
		{
			using var _ = Debugging.Stopwatch( $"Unloading Model [{Files.GetPath( Identifier )}]" );

			// Clear Spawned Models
			foreach ( var instance in Spawned )
			{
				Object.Destroy( instance );
			}

			Spawned.Clear();

			// Tell Provider to Unload
			Provider.Unload( onUnload );
		}
	}
}
