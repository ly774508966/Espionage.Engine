using System;
using Espionage.Engine.Components;
using Espionage.Engine.Internal.Commands;

namespace Espionage.Engine
{
	/// <summary>
	/// A Terminal is either a Var or Function that can be invoked through Espionage.Engines
	/// debugging library. This allows us to easily change the values or invoke functions
	/// inside libraries, instanced or not.
	/// </summary>
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Method, Inherited = false )]
	public sealed class TerminalAttribute : Attribute, IComponent<Property>, IComponent<Function>
	{
		public void OnAttached( Function item )
		{
			if ( !item.Info.IsStatic )
			{
				Debugging.Log.Error( $"Function \"{item.Name}\" Must be Static!" );
				return;
			}

			var command = new Command()
			{
				Name = item.Name,
				Help = item.Help,
				Info = item.Info
			}.WithAction(
				( e ) => item.Info?.Invoke( null, e )
			);

			Debugging.Console.Add( command );
		}

		public void OnAttached( Property item )
		{
			if ( !item.IsStatic )
			{
				Debugging.Log.Error( $"Property \"{item.Name}\" Must be Static!" );
				return;
			}

			var command = new Command()
			{
				Name = item.Name,
				Help = item.Help,
				Info = item.Info
			}.WithAction(
				( parameters ) =>
				{
					if ( parameters is { Length: > 0 } )
					{
						var value = parameters[0];
						item[null] = value;

						Debugging.Log.Info( $"{item.Name} now equals {value}" );
					}
					else
					{
						Debugging.Log.Info( $"{item.Name} = {item[null]}" );
					}
				}
			);

			Debugging.Console.Add( command );
		}
	}
}
