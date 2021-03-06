using System.Collections.Generic;
using System.Linq;
using Espionage.Engine.Services;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Espionage.Engine
{
	/// <summary>
	/// Espionage.Engine Entry Point.
	/// Initializes all its services, and sets up the Game.
	/// </summary>
	[Library, Group( "Engine" )]
	public static class Engine
	{
		/// <summary>
		/// The Current Game that is in Session, this will
		/// be automatically created when the game launches, and
		/// will be based off the game you exported from
		/// the packager.
		/// </summary>
		public static Game Game { get; private set; }

		//
		// Initialization
		//

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
		private static void Initialize_Runtime()
		{
			using ( Debugging.Stopwatch( "Engine / Game Ready", true ) )
			{
				Debugging.Initialize();
				Library.Initialize();

				if ( Game == null && !SetupGame() )
				{
					Debugging.Log.Error( "Game couldn't be found. Make sure to make a class inherited from Game" );
					return;
				}

				HookUnity();
				Services = new();

				// TODO: THIS IS TEMP
				Local.Client = Client.Create( "Local" );

				// Tell Services we're ready
				foreach ( var service in Services.All )
				{
					service.OnReady();
				}

				Game?.OnReady();

				Application.quitting += OnShutdown;

				Callback.Run( "engine.ready" );
			}
		}

		#if UNITY_EDITOR

		[InitializeOnLoadMethod]
		private static void Initialize_Editor()
		{
			Debugging.Initialize();
			Library.Initialize();

			if ( Game == null && !SetupGame() )
			{
				Debugging.Log.Error( "Game couldn't be found. Make sure to make a class inherited from Game" );
			}

		}

		#endif

		private static bool SetupGame()
		{
			var target = Library.Database.Find<Game>();

			if ( target is null )
			{
				Callback.Run( "game.not_found" );
				return false;
			}

			Game = Library.Database.Create<Game>( target.Class );
			Callback.Run( "game.ready" );

			Debugging.Log.Info( $"Using {Game.ClassInfo.Title} as the Game, [{Game.ClassInfo.Name}]" );

			return true;
		}

		//
		// Services
		//

		public static Database Services { get; private set; }

		public class Database : IDatabase<IService, int>
		{
			public IEnumerable<IService> All => _services;
			public int Count => _services.Count;

			public IService this[ int key ] => _services[key];

			private readonly List<IService> _services = new();

			public Database()
			{
				foreach ( var service in Library.Database.GetAll<IService>() )
				{
					if ( !service.Class.IsAbstract )
					{
						Add( Library.Database.Create<IService>( service.Class ) );
					}
				}
			}

			public void Add( IService item )
			{
				_services.Add( item );
			}

			public bool Contains( IService item )
			{
				return _services.Contains( item );
			}

			public void Remove( IService item )
			{
				_services.Remove( item );
				item.Dispose();
			}

			public void Clear()
			{
				foreach ( var service in _services )
				{
					service.Dispose();
				}

				_services.Clear();
			}

			public T Get<T>() where T : class, IService
			{
				return All.FirstOrDefault( e => e is T ) as T;
			}

			public bool Has<T>() where T : class, IService
			{
				return All.OfType<T>().Any();
			}
		}

		//
		// Layer
		//

		/// <summary>
		/// The Engine Layer scene. Use this scene
		/// for persisting objects across map changes.
		/// This scene should never be unloaded.
		/// </summary>
		public static Scene Scene { get; private set; }

		private static void HookUnity()
		{
			// Create Update Loop
			// Jake: I think this is stupid?
			var loop = PlayerLoop.GetCurrentPlayerLoop();
			for ( var i = 0; i < loop.subSystemList.Length; ++i )
			{
				// Frame Update
				if ( loop.subSystemList[i].type == typeof( Update ) )
				{
					loop.subSystemList[i].updateDelegate += OnUpdate;
				}

				// Physics Update
				if ( loop.subSystemList[i].type == typeof( FixedUpdate ) )
				{
					loop.subSystemList[i].updateDelegate += OnPhysicsUpdate;
				}

				if ( loop.subSystemList[i].type == typeof( PostLateUpdate ) )
				{
					loop.subSystemList[i].updateDelegate += OnPostUpdate;
				}
			}

			PlayerLoop.SetPlayerLoop( loop );

			// Create engine layer scene
			Scene = SceneManager.CreateScene( "Engine Layer" );
			Callback.Run( "engine.layer_created" );
		}

		//
		// Callbacks
		//

		private static void OnUpdate()
		{
			if ( !Application.isPlaying )
			{
				return;
			}

			for ( var i = 0; i < Services.Count; i++ )
			{
				Services[i].OnUpdate();
			}

			Callback.Run( "application.frame" );

			// More temp - this should 
			// Be called at an engine level
			foreach ( var client in Client.All )
			{
				client.Simulate();
			}
		}

		private static void OnPhysicsUpdate()
		{
			if ( !Application.isPlaying )
			{
				return;
			}

			Callback.Run( "physics.step" );
		}

		private static void OnPostUpdate()
		{
			if ( !Application.isPlaying )
			{
				return;
			}

			for ( var i = 0; i < Services.Count; i++ )
			{
				Services[i].OnPostUpdate();
			}

			Callback.Run( "application.late_frame" );
		}

		public static bool IsQuitting { get; private set; }

		private static void OnShutdown()
		{
			IsQuitting = true;

			for ( var i = 0; i < Services.Count; i++ )
			{
				Services[i].OnShutdown();
			}

			Game?.OnShutdown();
			Callback.Run( "application.quit" );
		}
	}
}
