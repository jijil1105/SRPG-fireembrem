using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public static class CustomMenuFile
{
	[MenuItem("File/Open Unity &amp;o")]
	public static void OpenUnity()
	{

		// プロジェクトフォルダを選択
		string projectPath = EditorUtility.OpenFolderPanel("フォルダを選択して下さい", Application.dataPath + "/../", "FolderName");
		if (string.IsNullOrEmpty(projectPath))
		{
			return;
		}

		// 開いているUnityエディタのパスを取得する		
		string editorPath = GetUnityEditorPath();
		if (string.IsNullOrEmpty(editorPath))
		{
			return;
		}

		// processを使ってコマンド起動させる
		var process = new System.Diagnostics.Process();
		process.StartInfo.FileName = editorPath;
		process.StartInfo.Arguments = "-projectPath " + projectPath;
		process.Start();
	}

	private static string GetUnityEditorPath()
	{
		string path = string.Empty;
		string assemblypath = InternalEditorUtility.GetEngineAssemblyPath();

		switch (Application.platform)
		{
			case RuntimePlatform.OSXEditor:
				{
					int index = assemblypath.IndexOf("Frameworks");
					if (0 > index)
					{
						index = assemblypath.IndexOf("Managed");
						if (0 > index)
						{
							break;
						}
					}

					path = assemblypath.Substring(0, index) + "MacOS/Unity";
				}
				break;

			case RuntimePlatform.WindowsEditor:
				{
					path = EditorApplication.applicationPath;
				}
				break;

			default:
				break;
		}

		return path;
	}
}
