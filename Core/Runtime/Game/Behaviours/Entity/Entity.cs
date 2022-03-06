﻿using System.Collections.Generic;
using Espionage.Engine.Components;

namespace Espionage.Engine
{
	/// <summary>
	/// An Entity is the Root of a MonoBehaviour tree.
	/// Entity will cache all Components that implement
	/// the <see cref="IComponent{Entity}"/> interface or
	/// inherited from <see cref="Component{T}"/>. Entities
	/// also contain IO / Actions logic.
	/// </summary>
	public abstract class Entity : Behaviour
	{
		public static List<Entity> All { get; } = new();

		protected sealed override void Awake()
		{
			All.Add( this );
			Components = new( this );

			// Cache Components
			foreach ( var item in GetComponents<IComponent<Entity>>() )
			{
				Components.Add( item );
			}

			base.Awake();
		}

		protected sealed override void OnDestroy()
		{
			All.Remove( this );
			Components = null;

			base.OnDelete();
		}

		// Components

		public Components<Entity> Components { get; private set; }
	}
}