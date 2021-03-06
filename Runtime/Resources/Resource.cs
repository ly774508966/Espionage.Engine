using System;
using System.Collections.Generic;

namespace Espionage.Engine.Resources
{
	/// <summary>
	/// A Resource is a loadable asset, that is loaded in editor or at runtime.
	/// Resources will keep references to the instances and the assets identifier.
	/// </summary>
	[Group( "Resources" )]
	public abstract class Resource : IResource, IDisposable, ILibrary
	{
		public Library ClassInfo { get; }

		protected Resource()
		{
			ClassInfo = Library.Register( this );
		}

		~Resource()
		{
			Library.Unregister( this );
		}

		//
		// Resource
		//

		/// <summary> How many instances of this Resource are currently being used. </summary>
		public int Instances { get; private set; }

		/// <summary> The path / database entry to this resource. </summary>
		public abstract string Identifier { get; }

		/// <summary> Is this resource currently being loaded into memory </summary>
		public virtual bool IsLoading { get; protected set; }

		/// <summary> Should we destroy / unload this resource after there are no more instances? </summary>
		public bool Persistant { get; internal set; }

		void IResource.Load( Action onLoad )
		{
			if ( !Database.Contains( this ) )
			{
				Database.Add( this );
				OnLoad( onLoad );
			}

			Instances++;
		}

		/// <summary> What should we do when this resource is loaded </summary>
		protected virtual void OnLoad( Action onLoad ) { }

		void IResource.Unload( Action onUnload )
		{
			if ( Persistant )
			{
				return;
			}

			Instances--;

			if ( Instances == 0 )
			{
				Database.Remove( this );
				OnUnload( onUnload );
			}
		}

		/// <summary> What should we do when this resource is unloaded </summary>
		protected virtual void OnUnload( Action onUnload ) { }

		/// <summary> Forcefully Unload this Resource </summary>
		public void Dispose()
		{
			Persistant = false;
			((IResource)this).Unload();
		}

		public interface IProvider<T, out TOutput> where T : IResource
		{
			TOutput Output { get; }
			float Progress { get; }

			string Identifier { get; }
			bool IsLoading { get; }

			void Load( Action onLoad = null );
			void Unload( Action onUnload = null );
		}

		//
		// Database
		//

		///	<summary> A reference to all resources that are loaded. </summary>
		public static IDatabase<Resource, string> Database { get; } = new InternalDatabase();

		private class InternalDatabase : IDatabase<Resource, string>
		{
			public IEnumerable<Resource> All => _records.Values;
			private readonly Dictionary<string, Resource> _records = new();

			public Resource this[ string key ] => _records.ContainsKey( key ) ? _records[key] : null;

			public void Add( Resource item )
			{
				// Store it in Database
				if ( _records.ContainsKey( item.Identifier! ) )
				{
					_records[item.Identifier] = item;
					Debugging.Log.Warning( $"For some reason we're replacing a resource? [{item.Identifier}]" );
				}
				else
				{
					_records.Add( item.Identifier!, item );
				}
			}

			public void Clear()
			{
				_records.Clear();
			}

			public bool Contains( Resource item )
			{
				return _records.ContainsKey( item.Identifier );
			}

			public void Remove( Resource item )
			{
				_records.Remove( item.Identifier );
			}

			public int Count => _records.Count;
		}
	}
}
