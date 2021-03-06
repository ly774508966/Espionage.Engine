using System;
using Espionage.Engine.Components;

namespace Espionage.Engine
{
	/// <summary>
	/// Reflection Component that changes the Tile value on a Library or Property.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method, Inherited = false )]
	public sealed class TitleAttribute : Attribute, IComponent<Library>, IComponent<Property>, IComponent<Function>
	{
		private readonly string _title;

		public TitleAttribute( string title )
		{
			_title = title;
		}

		public void OnAttached( Library library )
		{
			library.Title = _title;
		}

		public void OnAttached( Property property )
		{
			property.Title = _title;
		}

		public void OnAttached( Function item )
		{
			item.Title = _title;
		}
	}
}
