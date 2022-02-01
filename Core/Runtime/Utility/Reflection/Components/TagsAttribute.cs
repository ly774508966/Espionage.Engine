using System;
using System.Linq;
using Espionage.Engine.Components;

namespace Espionage.Engine
{
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Property )]
	public sealed class TagsAttribute : Attribute, IComponent<Library>, IComponent<Property>
	{
		public string[] Tags { get; }

		public TagsAttribute( params string[] tags )
		{
			Tags = tags;
		}

		public void OnAttached( Library library ) { }

		public void OnAttached( Property property ) { }
	}
}