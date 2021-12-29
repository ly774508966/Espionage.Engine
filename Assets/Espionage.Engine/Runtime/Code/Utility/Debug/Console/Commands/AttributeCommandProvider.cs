using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using static Espionage.Engine.Console;

namespace Espionage.Engine.Internal
{
	public class AttributeCommandProvider : ICommandProvider
	{
		// Commands
		private static Dictionary<string, Command> _commands;
		public IReadOnlyCollection<Command> All => _commands.Values;

		// History
		private static HashSet<string> _history = new HashSet<string>();
		public IReadOnlyCollection<string> History => _history;


		public Task Initialize()
		{
			return Task.Run( () =>
				{
					// Get every CmdAttribute using Linq
					var types = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany( e => e.GetTypes()
										.SelectMany( e => e.GetMembers( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
										.Where( e => e.IsDefined( typeof( CmdAttribute ) ) ) ) );

					foreach ( var info in types )
					{
						foreach ( var item in info.GetCustomAttribute<CmdAttribute>().Create( info ) )
							_commandProvider.Add( item );
					}
				} );
		}

		public void Add( Command command )
		{
			_commands ??= new Dictionary<string, Command>( StringComparer.CurrentCultureIgnoreCase );

			try
			{
				_commands.Add( command.Name, command );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public void Invoke( string command, string[] args )
		{
			if ( !_commands.TryGetValue( command, out var consoleCommand ) )
			{
				Debug.Log( $"Couldn't find command \"{command}\"" );
				return;
			}

			// Add to history stack, for use later
			_history.Add( $"{command} {string.Join( ' ', args )}" );

			// Check if we are on the correct layer - This looks ultra aids
			if ( (Application.isEditor && consoleCommand.Layer.HasFlag( Layer.Editor )) || (Application.isPlaying && consoleCommand.Layer.HasFlag( Layer.Runtime )) )
			{
				if ( args is not null && args.Length > 0 )
					consoleCommand.Invoke( ConvertArgs( consoleCommand.Info.GetParameterTypes(), args ) );
				else
					consoleCommand.Invoke( null );

				return;
			}

			Debug.Log( $"Trying to invoke command on wrong layer [{consoleCommand.Layer}]" );
			return;
		}

		public void LaunchArgs( string arg )
		{

		}
	}
}