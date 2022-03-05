﻿using System;
using System.Text;

namespace Espionage.Engine.Serializers
{
	[Group( "Serializers" )]
	internal class StringSerializer : ISerializer<char>, ISerializer<string>, IDeserializer<string>, IDeserializer<char>
	{
		public Library ClassInfo => null;
		internal static readonly UTF8Encoding UTF8 = new();

		// Char

		public byte[] Serialize( char item )
		{
			throw new InvalidOperationException( "Why?" );
		}

		public byte[] Serialize( char[] item )
		{
			return UTF8.GetBytes( item );
		}

		char IDeserializer<char>.Deserialize( byte[] item )
		{
			throw new InvalidOperationException( "Why?" );
		}

		// String

		public byte[] Serialize( string item )
		{
			return UTF8.GetBytes( item );
		}

		public byte[] Serialize( string[] item )
		{
			return UTF8.GetBytes( string.Join( '\n', item ) );
		}

		string IDeserializer<string>.Deserialize( byte[] item )
		{
			return UTF8.GetString( item );
		}
	}
}