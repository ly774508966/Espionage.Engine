using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Espionage.Engine.Resources
{
	public abstract class Asset<T> : Asset where T : Object
	{
		public T asset;

		public override bool CanCompile()
		{
			return asset != null;
		}
	}

	public abstract class Asset : ScriptableObject, ILibrary
	{
		public Library ClassInfo { get; private set; }

		private void OnEnable()
		{
			ClassInfo = Library.Database[GetType()];
		}

		public virtual bool CanCompile()
		{
			return true;
		}

		public virtual void Compile()
		{
#if UNITY_EDITOR
			var exportPath = $"Exports/{ClassInfo.Group}/{name}/";
			var extension = ClassInfo.Components.Get<FileAttribute>()?.Extension;

			if ( string.IsNullOrEmpty( extension ) )
			{
				Debugging.Log.Error( $"{ClassInfo.Title} doesn't have an extension. Not compiling" );
				return;
			}

			using ( Debugging.Stopwatch( $"{ClassInfo.Title} Compiled" ) )
			{
				try
				{
					if ( !Directory.Exists( Path.GetFullPath( exportPath ) ) )
					{
						Directory.CreateDirectory( Path.GetFullPath( exportPath ) );
					}

					var builds = new[]
					{
						new AssetBundleBuild()
						{
							assetNames = new[] { AssetDatabase.GetAssetPath( this ) },
							assetBundleName = $"{name}.{extension}"
						}
					};

					var bundle = BuildPipeline.BuildAssetBundles( exportPath, builds, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows );

					if ( bundle is null )
					{
						EditorUtility.DisplayDialog( "ERROR", $"Map asset bundle compile failed.", "Okay" );
						return;
					}
				}
				finally
				{
					AssetDatabase.Refresh();
				}
			}
#else
			Debugging.Log.Error("You can only compile while in the editor.");
#endif
		}
	}
}
