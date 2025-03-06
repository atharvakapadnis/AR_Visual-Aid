using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using XGI.Tools.Editor;
using XGI.Tools.Window;

namespace XGI.GUIOverrides
{
	[CustomEditor(typeof(WindowStorage))]
	public class WindowStorageGUI : Editor
	{
		SerializedProperty windowNames;	// Reference to all the names of the windows stored
		GameObject window;  // Reference to the currently assigned window in the inspector window of the target of this class

		private void OnEnable()
		{
			windowNames = serializedObject.FindProperty("windowNames");

			#region Creating the directories
			string path = "Assets";
			while (path != "Assets/Resources/XGI/WindowsDepository/Prefabs")
			{
				path += "/Resources";
				if (AssetDatabase.IsValidFolder("Assets/Resources"))
				{
					path += "/XGI";
					if (AssetDatabase.IsValidFolder("Assets/Resources/XGI"))
					{
						path += "/WindowsDepository";
						if (AssetDatabase.IsValidFolder("Assets/Resources/XGI/WindowsDepository"))
						{
							path += "/Prefabs";
							if (AssetDatabase.IsValidFolder("Assets/Resources/XGI/WindowsDepository/Prefabs")) break;
							else
							{
								AssetDatabase.CreateFolder("Assets/Resources/XGI/WindowsDepository", "Prefabs");
								Debug.Log("The said directory has just been created for you\nCreate the asset here - Resources/XGI/WindowsDepository");
							}
						}
						else AssetDatabase.CreateFolder("Assets/Resources/XGI", "WindowsDepository");
					}
					else AssetDatabase.CreateFolder("Assets/Resources", "XGI");
				}
				else AssetDatabase.CreateFolder("Assets", "Resources");
			}
			#endregion

			#region Validifying the directory
			AssetFinder finder = new AssetFinder(typeof(WindowStorage));
			WindowStorage[] assets = finder.FetchAllAssets<WindowStorage>(false, "XGI/WindowsDepository");

			foreach (WindowStorage asset in assets)
			{
				Debug.LogWarning("Destroying " + asset.name + " asset now\nCreate the registrar assets only in \"Resources/XGI/WindowsDepository\" folder");
				DestroyImmediate(asset, true);
			}
			#endregion
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			int size = windowNames.arraySize;

			#region Heading
			GUILayout.BeginHorizontal();
			GUILayout.Label("Stored Windows");

			if (GUILayout.Button("Clear All"))
			{
				List<string> assets = new List<string>();
				AssetFinder finder = new AssetFinder(typeof(WindowStorage));
				WindowStorage[] windowStorages = finder.FetchAllAssets<WindowStorage>();

				for (int index = 0; index < size; index++)
				{
					SerializedProperty name = windowNames.GetArrayElementAtIndex(index);
					assets.Add(name.stringValue);
				}

				foreach (string asset in assets)
				{
					bool isEnlistedElsewhere = false;

					foreach (WindowStorage storage in windowStorages)
					{
						if (isEnlistedElsewhere) break;
						else if (storage != (WindowStorage)target)
							foreach (string windowName in storage.windowNames)
								if (windowName == asset)
								{
									isEnlistedElsewhere = true;
									break;
								}
					}

					if (!isEnlistedElsewhere) AssetDatabase.DeleteAsset("Assets/Resources/XGI/WindowsDepository/Prefabs" + asset + ".prefab");
				}

				windowNames.ClearArray();
				serializedObject.ApplyModifiedProperties();
				return;
			}

			GUILayout.EndHorizontal();
			#endregion

			#region The List
			for (int index = 0; index < size; index++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label((index + 1).ToString());
				SerializedProperty name = windowNames.GetArrayElementAtIndex(index);
				GUILayout.Label(name.stringValue);
				if (GUILayout.Button("-"))
				{
					AssetFinder finder = new AssetFinder(typeof(WindowStorage));
					WindowStorage[] windowStorages = finder.FetchAllAssets<WindowStorage>();

					bool isEnlistedElsewhere = false;

					foreach(WindowStorage storage in windowStorages)
					{
						if (isEnlistedElsewhere) break;
						else if(storage != (WindowStorage)target)
							foreach(string windowName in storage.windowNames)
								if (windowName == name.stringValue)
								{
									isEnlistedElsewhere = true;
									break;
								}
					}

					if (!isEnlistedElsewhere) AssetDatabase.DeleteAsset("Assets/Resources/XGI/WindowsDepository/Prefabs/" + name.stringValue + ".prefab");
					windowNames.DeleteArrayElementAtIndex(index);
					serializedObject.ApplyModifiedProperties();
					return;
				}
				GUILayout.EndHorizontal();
			}
			#endregion

			#region Field to add new entry
			GUILayout.BeginHorizontal();
			GUILayout.Label((size + 1).ToString());
			window = (GameObject)EditorGUILayout.ObjectField(window, typeof(GameObject), true);

			if (window)
			{
				RectTransform windowRect = null;
				window.TryGetComponent(out windowRect);

				AssetFinder finder = new AssetFinder(window.name, "prefab");
				bool isDuplicate = finder.AssetExist();

				if (windowRect)
				{
					bool shouldStore = false;

					if (isDuplicate)
					{
						if (GUILayout.Button("Override"))
						{
							shouldStore = true;
							bool isEnlisted = false;
							for (int index = 0; index < size; index++)
							{
								SerializedProperty name = windowNames.GetArrayElementAtIndex(index);
								if (name.stringValue == window.name)
								{
									isEnlisted = true;
									break;
								}
							}

							if (!isEnlisted)
							{
								windowNames.InsertArrayElementAtIndex(size);
								SerializedProperty name = windowNames.GetArrayElementAtIndex(size);
								name.stringValue = window.name;
							}
						}
					}
					else
					{
						if (GUILayout.Button("+"))
						{
							windowNames.InsertArrayElementAtIndex(size);
							SerializedProperty name = windowNames.GetArrayElementAtIndex(size);
							name.stringValue = window.name;
							shouldStore = true;
						}
					}

					if (shouldStore)
					{
						PrefabUtility.SaveAsPrefabAssetAndConnect(window, "Assets/Resources/XGI/WindowsDepository/Prefabs/" + window.name + ".prefab", InteractionMode.AutomatedAction);
						PrefabUtility.UnpackPrefabInstance(window, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
						window = null;
					}
				}
				else GUILayout.Label("This is not a Window!");
			}
			else GUILayout.Label("<- Drag a Window in here");

			GUILayout.EndHorizontal();
			#endregion

			serializedObject.ApplyModifiedProperties();
		}
	}
}