using System;

namespace Espionage.Engine.Internal.Callbacks
{
	public interface ICallbackProvider : IDisposable
	{
		void Add( string eventName, Function function );

		void Run( string name );
		object[] Run( string name, params object[] args );

		void Register( object item );
		void Unregister( object item );
	}
}
