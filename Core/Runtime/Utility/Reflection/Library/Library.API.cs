using System;
using System.Collections.Generic;
using System.Linq;
using Espionage.Engine.Internal;
using Random = System.Random;

namespace Espionage.Engine
{
	public partial class Library
	{
		/// <summary> Attribute that skips the attached class from generating a library reference </summary>
		[AttributeUsage( AttributeTargets.Class, Inherited = true )]
		public class Skip : Attribute { }
	}
}