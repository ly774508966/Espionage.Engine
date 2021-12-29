using UnityEngine;

namespace Espionage.Engine
{
	public static partial class Console
	{
		[Console.Cmd( "help" )]
		private static void HelpCmd()
		{
			foreach ( var item in _commandProvider.All )
			{
				AddLog( new Entry( $"{item.Name}", "", LogType.Log ) );
			}
			AddLog( new Entry( "Commands", "", LogType.Log ) );
		}

		[Console.Cmd( "clear", "cls" )]
		private static void ClearCmd()
		{
			_logs.Clear();
			OnClear?.Invoke();
		}

		[Console.Cmd( "quit", "exit" )]
		private static void QuitCmd()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#else
			Application.Quit();
#endif
		}
	}
}
