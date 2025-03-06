using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using XGI.Tools.Editor;
using XGI.Tools.Window;

namespace XGI.GUIOverrides
{
	/// <summary> This is a GUI override for targetClass with added features </summary>
	[CustomEditor(typeof(WindowUISettings))]
	public class WindowSettingsAssetGUI : Editor
	{
		#region GUI Header Toggles
		// Toggles for different foldout sections in the inspector window
		bool mainSettings;
		bool primaryProperties;
		bool primaryTextProperties;
		bool secondaryProperties;
		bool secondaryTextProperties;
		bool buttonProperties;
		bool buttonPositioning;
		bool themeAnElement;
		bool presets;
		#endregion

		#region Preview Properties
		// These properties will be used for previewing the result
		GameObject canvas;
		GameObject window;
		GameObject previewPanel;

		Image primaryImage;
		TextMeshProUGUI primaryText;
		
		Image secondaryImage;
		Image hindererImage;
		TextMeshProUGUI secondaryText;

		int buttonCountValue;
		int buttonCount
		{
			get { return buttonCountValue; }
			set
			{
				if(buttonCountValue != value)
				{
					buttonCountValue = value;
					RefreshPreview();
				}
			}
		}
		List<Image> buttonImage;
		List<TextMeshProUGUI> buttonText;
		List<Button> buttonCharacteristics;
		GridLayoutGroup buttonLayout;
		
		ColorBlock buttonColors;

		#endregion

		#region Serialized Properties
		// These properties can refer to data members of targetClass
		// These references will further be used to preview the results
		// They will also be able to be modified based on inputs from the inspector panel

		SerializedProperty orientationState;
		SerializedProperty scaleMajorAxis;
		SerializedProperty scaleMinorAxis;
		SerializedProperty titleBarSize;

		// Primary attributes
		SerializedProperty primarySourceImage;
		SerializedProperty primaryColor;
		SerializedProperty primaryMaterial;
		SerializedProperty primaryType;

		// Primary font attributes
		SerializedProperty primaryFontAsset;
		SerializedProperty primaryFontStyle;
		SerializedProperty primaryFontSize;
		SerializedProperty primaryTextColor;
		SerializedProperty primaryTextAlignmentOptions;

		// Secondary attributes
		SerializedProperty secondarySourceImage;
		SerializedProperty secondaryColor;
		SerializedProperty secondaryMaterial;
		SerializedProperty secondaryType;
		SerializedProperty backgroundHindererColor;

		// Secondary font attributes
		SerializedProperty secondaryFontAsset;
		SerializedProperty secondaryFontStyle;
		SerializedProperty secondaryFontSize;
		SerializedProperty secondaryTextColor;
		SerializedProperty secondaryTextAlignmentOptions;

		// Button attributes
		SerializedProperty buttonType;
		SerializedProperty buttonImageType;
		SerializedProperty buttonAlignment;
		SerializedProperty buttonSpacing;
		SerializedProperty buttonLength;
		SerializedProperty buttonHeight;
		
		SerializedProperty buttonNormalSprite;
		SerializedProperty buttonHighlightedSprite;
		SerializedProperty buttonPressedSprite;
		SerializedProperty buttonSelectedSprite;
		SerializedProperty buttonDisabledSprite;

		SerializedProperty buttonNormalColor;
		SerializedProperty buttonHighlightedColor;
		SerializedProperty buttonPressedColor;
		SerializedProperty buttonSelectedColor;
		SerializedProperty buttonDisabledColor;
		SerializedProperty buttonFadeDuration;

		// Button font attributes
		SerializedProperty buttonFontAsset;
		SerializedProperty buttonFontStyle;
		SerializedProperty buttonFontSize;
		SerializedProperty buttonTextColor;
		SerializedProperty buttonTextAlignmentOptions;
		#endregion

		#region Miscellaneous Properties
		bool isPreviewActive;   // Editor property
		WindowUISettings targetClass;
		bool firstRun;
		GameObject element;

		// Window Customization properties
		int buttonTypeIndex;
		
		int orientationValue;
		int orientation
		{
			get { return orientationValue; }
			set
			{
				if (orientationValue != value)
				{
					orientationValue = value;
					RefreshPreview();
				}
			}
		}

		float majorSizeValue;
		float majorSize
		{
			get { return majorSizeValue; }
			set
			{
				if (majorSizeValue != value)
				{
					majorSizeValue = value;
					RefreshPreview();
				}
			}
		}

		float minorSizeValue;
		float minorSize
		{
			get { return minorSizeValue; }
			set
			{
				if (minorSizeValue != value)
				{
					minorSizeValue = value;
					RefreshPreview();
				}
			}
		}

		float headerSizeValue;
		float headerSize
		{
			get { return headerSizeValue; }
			set
			{
				if(headerSizeValue != value)
				{
					headerSizeValue = value;
					RefreshPreview();
				}
			}
		}

		// Enum names containers, used to access enum data from serialized object
		string[] panelTypeEnumNames;
		string[] TextAlignmentEnumNames;

		// Enum properties for Editor UI
		FontStyles primaryFontStyleEnum;
		FontStyles secondaryFontStyleEnum;
		FontStyles buttonFontStyleEnum;
		#endregion

		private void OnEnable()
		{
			# region Defaulting out
			targetClass = (WindowUISettings)target;
			isPreviewActive = false;
			buttonImage = new List<Image>();
			buttonCharacteristics = new List<Button>();
			buttonText = new List<TextMeshProUGUI>();
			buttonCount = 2;
			#endregion

			#region Creating the directory
			string path = "Assets";
			while (path != "Assets/Resources/XGI/WindowsGUI")
			{
				if (AssetDatabase.IsValidFolder("Assets/Resources"))
				{
					if (AssetDatabase.IsValidFolder("Assets/Resources/XGI"))
					{
						if (AssetDatabase.IsValidFolder("Assets/Resources/XGI/WindowsGUI")) break;
						else
						{
							AssetDatabase.CreateFolder("Assets/Resources/XGI", "WindowsGUI");
							path += "/WindowsGUI";
						}
					}
					else
					{
						AssetDatabase.CreateFolder("Assets/Resources", "XGI");
						path += "/XGI";
					}
				}
				else
				{
					AssetDatabase.CreateFolder("Assets", "Resources");
					path += "/Resources";
				}
			}
			#endregion

			#region Validifying the directory
			AssetFinder finder = new AssetFinder(typeof(WindowUISettings));
			WindowUISettings[] assets = finder.FetchAllAssets<WindowUISettings>(false, "XGI/WindowsGUI");

			foreach (WindowUISettings asset in assets)
			{
				Debug.LogWarning("Destroying " + asset.name + " asset now\nCreate the registrar assets only in \"Resources/XGI/WindowsGUI\" folder");
				DestroyImmediate(asset, true);
			}

			if (path != "Assets") Debug.Log("The said directory has just been created for you\nCreate the asset here - Resources/XGI/WindowsGUI");
			#endregion

			#region Finding properties in the target class
			// Linking serialized object properties to their corresponding properties in targetClass

			orientationState = serializedObject.FindProperty("orientationState");
			scaleMajorAxis = serializedObject.FindProperty("scaleMajorAxis");
			scaleMinorAxis = serializedObject.FindProperty("scaleMinorAxis");
			titleBarSize = serializedObject.FindProperty("titleBarSize");

			// Primary properties
			primarySourceImage = serializedObject.FindProperty("primarySourceImage");
			primaryColor = serializedObject.FindProperty("primaryColor");
			primaryMaterial = serializedObject.FindProperty("primaryMaterial");
			primaryType = serializedObject.FindProperty("primaryType");

			// Primary font properties
			primaryFontAsset = serializedObject.FindProperty("primaryFontAsset");
			primaryFontStyle = serializedObject.FindProperty("primaryFontStyleIndex");
			primaryFontSize = serializedObject.FindProperty("primaryFontSize");
			primaryTextColor = serializedObject.FindProperty("primaryTextColor");
			primaryTextAlignmentOptions = serializedObject.FindProperty("primaryTextAlignmentOptions");

			// Secondary properties
			secondarySourceImage = serializedObject.FindProperty("secondarySourceImage");
			secondaryColor = serializedObject.FindProperty("secondaryColor");
			secondaryMaterial = serializedObject.FindProperty("secondaryMaterial");
			secondaryType = serializedObject.FindProperty("secondaryType");
			backgroundHindererColor = serializedObject.FindProperty("backgroundHindererColor");
			
			// Secondary font properties
			secondaryFontAsset = serializedObject.FindProperty("secondaryFontAsset");
			secondaryFontStyle = serializedObject.FindProperty("secondaryFontStyleIndex");
			secondaryFontSize = serializedObject.FindProperty("secondaryFontSize");
			secondaryTextColor = serializedObject.FindProperty("secondaryTextColor");
			secondaryTextAlignmentOptions = serializedObject.FindProperty("secondaryTextAlignmentOptions");

			// Button properties
			buttonType = serializedObject.FindProperty("buttonType");
			buttonImageType = serializedObject.FindProperty("buttonImageType");
			buttonAlignment = serializedObject.FindProperty("buttonAlignment");
			buttonSpacing = serializedObject.FindProperty("buttonSpacing");
			buttonLength = serializedObject.FindProperty("buttonLength");
			buttonHeight = serializedObject.FindProperty("buttonHeight");

			buttonNormalSprite = serializedObject.FindProperty("buttonNormalSprite");
			buttonHighlightedSprite = serializedObject.FindProperty("buttonHighlightedSprite");
			buttonPressedSprite = serializedObject.FindProperty("buttonPressedSprite");
			buttonSelectedSprite = serializedObject.FindProperty("buttonSelectedSprite");
			buttonDisabledSprite = serializedObject.FindProperty("buttonDisabledSprite");

			buttonNormalColor = serializedObject.FindProperty("buttonNormalColor");
			buttonHighlightedColor = serializedObject.FindProperty("buttonHighlightedColor");
			buttonPressedColor = serializedObject.FindProperty("buttonPressedColor");
			buttonSelectedColor = serializedObject.FindProperty("buttonSelectedColor");
			buttonDisabledColor = serializedObject.FindProperty("buttonDisabledColor");
			buttonFadeDuration = serializedObject.FindProperty("buttonFadeDuration");

			// Button font properties
			buttonFontAsset = serializedObject.FindProperty("buttonFontAsset");
			buttonFontStyle = serializedObject.FindProperty("buttonFontStyleIndex");
			buttonFontSize = serializedObject.FindProperty("buttonFontSize");
			buttonTextColor = serializedObject.FindProperty("buttonTextColor");
			buttonTextAlignmentOptions = serializedObject.FindProperty("buttonTextAlignmentOptions");
			#endregion

			#region Fetching initial values from the target class
			orientation = orientationState.intValue;
			majorSize = scaleMajorAxis.floatValue;
			minorSize = scaleMinorAxis.floatValue;
			headerSize = titleBarSize.floatValue;
			buttonTypeIndex = buttonType.intValue;
			primaryFontStyleEnum = (FontStyles)primaryFontStyle.intValue;
			secondaryFontStyleEnum = (FontStyles)secondaryFontStyle.intValue;
			buttonFontStyleEnum = (FontStyles)buttonFontStyle.intValue;
			#endregion

			#region Initial checks and preperations
			// Checking if this asset is just created
			firstRun = majorSize <= 0 ? true : false;

			// Storing enum names in strings for data retrieval
			panelTypeEnumNames = primaryType.enumNames;
			TextAlignmentEnumNames = primaryTextAlignmentOptions.enumNames;

			if(!firstRun) SecondaryToButtonTheme();
			#endregion
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();  // Updating values of serialized objects in this class based on thier corresponding values in targetClass

			#region Drawing Attribute Settings GUI in the inspector
			mainSettings = EditorGUILayout.BeginFoldoutHeaderGroup(mainSettings, "Main Settings");
			if (mainSettings)
			{
				presets = themeAnElement = primaryProperties = primaryTextProperties = secondaryProperties = secondaryTextProperties = buttonProperties = buttonPositioning = false;
			
				EditorGUILayout.Separator();
				
				// Drawing a dropdown to enable selection of orientation of the window
				orientation = EditorGUILayout.Popup("Orientation", orientationState.intValue, new string[3] { "Portrait", "Landscape", "Auto-Detect" });
				orientationState.intValue = orientation;

				// Drawing a slider for enabling tweeking of window size
				EditorGUILayout.Slider(scaleMajorAxis, 400, 1000, new GUIContent("Major Length"));
				majorSize = scaleMajorAxis.floatValue;

				EditorGUILayout.Slider(scaleMinorAxis, 100, 400, new GUIContent("Minor Length"));
				minorSize = scaleMinorAxis.floatValue;

				EditorGUILayout.Slider(titleBarSize, 30, 90, new GUIContent("Header Size"));
				headerSize = titleBarSize.floatValue;

				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			// Drawing GUI for window attributes
			primaryProperties = EditorGUILayout.BeginFoldoutHeaderGroup(primaryProperties, "Primary Theme");
			if (primaryProperties)
			{
				presets = themeAnElement = mainSettings = primaryTextProperties = secondaryProperties = secondaryTextProperties = buttonProperties = buttonPositioning = false;

				EditorGUILayout.Separator();
				EditorGUILayout.PropertyField(primarySourceImage, new GUIContent("Source Image"));
				EditorGUILayout.PropertyField(primaryColor, new GUIContent("Color"));
				EditorGUILayout.PropertyField(primaryMaterial, new GUIContent("Material"));
				EditorGUILayout.PropertyField(primaryType, new GUIContent("Type"));
				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			primaryTextProperties = EditorGUILayout.BeginFoldoutHeaderGroup(primaryTextProperties, "Primary Text");
			if (primaryTextProperties)
			{
				presets = themeAnElement = mainSettings = primaryProperties = secondaryProperties = secondaryTextProperties = buttonProperties = buttonPositioning = false;

				EditorGUILayout.Separator();
				EditorGUILayout.PropertyField(primaryFontAsset, new GUIContent("Asset"));
				primaryFontStyleEnum = (FontStyles)EditorGUILayout.EnumFlagsField("Font Style", primaryFontStyleEnum);
				primaryFontStyle.intValue = (int)primaryFontStyleEnum;
				targetClass.primaryFontStyle = primaryFontStyleEnum;
				EditorGUILayout.PropertyField(primaryFontSize, new GUIContent("Size"));
				EditorGUILayout.PropertyField(primaryTextColor, new GUIContent("Color"));
				EditorGUILayout.PropertyField(primaryTextAlignmentOptions, new GUIContent("Alignment"));
				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			secondaryProperties = EditorGUILayout.BeginFoldoutHeaderGroup(secondaryProperties, "Secondary Theme");
			if (secondaryProperties)
			{
				presets = themeAnElement = mainSettings = primaryProperties = primaryTextProperties = secondaryTextProperties = buttonProperties = buttonPositioning = false;

				EditorGUILayout.Separator();
				EditorGUILayout.PropertyField(secondarySourceImage, new GUIContent("Source Image"));
				EditorGUILayout.PropertyField(secondaryColor, new GUIContent("Color"));
				EditorGUILayout.PropertyField(secondaryMaterial, new GUIContent("Material"));
				EditorGUILayout.PropertyField(secondaryType, new GUIContent("Type"));
				EditorGUILayout.PropertyField(backgroundHindererColor, new GUIContent("Hinderer Color"));
				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			secondaryTextProperties = EditorGUILayout.BeginFoldoutHeaderGroup(secondaryTextProperties, "Secondary Text");
			if (secondaryTextProperties)
			{
				presets = themeAnElement = mainSettings = primaryProperties = primaryTextProperties = secondaryProperties = buttonProperties = buttonPositioning = false;

				EditorGUILayout.Separator();
				EditorGUILayout.PropertyField(secondaryFontAsset, new GUIContent("Asset"));
				secondaryFontStyleEnum = (FontStyles)EditorGUILayout.EnumFlagsField("Font Style", secondaryFontStyleEnum);
				secondaryFontStyle.intValue = (int)secondaryFontStyleEnum;
				targetClass.secondaryFontStyle = secondaryFontStyleEnum;
				EditorGUILayout.PropertyField(secondaryFontSize, new GUIContent("Size"));
				EditorGUILayout.PropertyField(secondaryTextColor, new GUIContent("Color"));
				EditorGUILayout.PropertyField(secondaryTextAlignmentOptions, new GUIContent("Alignment"));
				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			buttonProperties = EditorGUILayout.BeginFoldoutHeaderGroup(buttonProperties, "Button Theme");
			if (buttonProperties)
			{
				presets = themeAnElement = mainSettings = primaryProperties = primaryTextProperties = secondaryProperties = secondaryTextProperties = buttonPositioning = false;

				EditorGUILayout.Separator();
				buttonTypeIndex = EditorGUILayout.Popup("Button Type", buttonType.intValue, new string[3] { "Secondary Themed", "Color Based", "Sprite Based" });
				buttonType.intValue = buttonTypeIndex;

				EditorGUILayout.Separator();
				EditorGUILayout.Separator();

				// If button is not secondary themed
				if(buttonTypeIndex != 0) 
				{
					if (buttonTypeIndex == 1)
					{
						EditorGUILayout.PropertyField(buttonNormalSprite, new GUIContent("Sprite"));
						EditorGUILayout.PropertyField(buttonImageType, new GUIContent("Sprite Type"));
						EditorGUILayout.PropertyField(buttonNormalColor, new GUIContent("Normal Color"));
						EditorGUILayout.PropertyField(buttonHighlightedColor, new GUIContent("Highlighted Color"));
						EditorGUILayout.PropertyField(buttonPressedColor, new GUIContent("Pressed Color"));
						EditorGUILayout.PropertyField(buttonSelectedColor, new GUIContent("Selected Color"));
						EditorGUILayout.PropertyField(buttonDisabledColor, new GUIContent("Disabled Color"));
						EditorGUILayout.PropertyField(buttonFadeDuration, new GUIContent("Fade Duration"));
						EditorGUILayout.Separator();
					}
					else if (buttonTypeIndex == 2)
					{
						buttonNormalColor.colorValue = Color.white;
						EditorGUILayout.PropertyField(buttonImageType, new GUIContent("Sprite Type"));
						EditorGUILayout.PropertyField(buttonNormalSprite, new GUIContent("Normal Sprite"));
						EditorGUILayout.PropertyField(buttonHighlightedSprite, new GUIContent("Highlighted Sprite"));
						EditorGUILayout.PropertyField(buttonPressedSprite, new GUIContent("Pressed Sprite"));
						EditorGUILayout.PropertyField(buttonSelectedSprite, new GUIContent("Selected Sprite"));
						EditorGUILayout.PropertyField(buttonDisabledSprite, new GUIContent("Disabled Sprite"));
					}

					EditorGUILayout.Separator();
					EditorGUILayout.Separator();

					EditorGUILayout.PropertyField(buttonFontAsset, new GUIContent("Font Asset"));
					buttonFontStyleEnum = (FontStyles)EditorGUILayout.EnumFlagsField("Font Style", buttonFontStyleEnum);
					buttonFontStyle.intValue = (int)buttonFontStyleEnum;
					targetClass.buttonFontStyle = buttonFontStyleEnum;
					EditorGUILayout.PropertyField(buttonFontSize, new GUIContent("Font Size"));
					EditorGUILayout.PropertyField(buttonTextColor, new GUIContent("Font Color"));
					EditorGUILayout.PropertyField(buttonTextAlignmentOptions, new GUIContent("Font Alignment"));
				}
				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			buttonPositioning = EditorGUILayout.BeginFoldoutHeaderGroup(buttonPositioning, "Button Positioning");
			if (buttonPositioning)
			{
				presets = themeAnElement = mainSettings = primaryProperties = primaryTextProperties = secondaryProperties = secondaryTextProperties = buttonProperties = false;

				EditorGUILayout.Separator();
				EditorGUILayout.PropertyField(buttonAlignment, new GUIContent("Position"));
				EditorGUILayout.Slider(buttonLength, 75, 350, new GUIContent("Length"));
				EditorGUILayout.Slider(buttonHeight, 25, 100, new GUIContent("Height"));
				EditorGUILayout.Slider(buttonSpacing, 0, 250, new GUIContent("Spacing"));
				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			EditorGUILayout.Separator();
			#endregion

			serializedObject.ApplyModifiedProperties(); // Updating the newly modified data (via Inspector GUI) to correspoding properties in targetClass

			// If preview is supposed to be displayed
			if (isPreviewActive)
			{
				// Recieving reference to a canvas in the scene
				canvas = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Canvas"), canvas, typeof(GameObject), true);

				// Implementing Button GUI for scene view preview
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Auto Detect"))
				{
					try
					{
						canvas = FindObjectOfType<Canvas>().gameObject;
						Debug.Log("A canvas reference is detected and added\nNow proceed for \"Initiate Preview\"");
					}
					catch (Exception e)
					{
						Debug.LogError("No canvas found in the scene\n" + e.Message);
						Debug.Log("Add a canvas to the scene and try again\nIf a canvas is already available, make sure it is enabled");
					}
				}
				else if (GUILayout.Button("Initiate Preview"))
				{
					if (!canvas)
					{
						Debug.LogError("Add the canvas reference and try again");
						Debug.Log("You can use \"Auto Detect\" if you only have one canvas in the entire scene\nOtherwise drag and drop it manually");
					}
					else if (!window) // Instantiate a new window if no preview panel instance is found
					{
						DrawPreviewWindow();
						SecondaryToButtonTheme();
						Debug.Log("Sample window created\nYou can now change settings values to see the effects");
					}
					else Debug.LogWarning("Preview initiated already");
				}
				else if (GUILayout.Button("Disable Preview"))
				{
					DestroyImmediate(previewPanel);
					isPreviewActive = false;
				}
				GUILayout.EndHorizontal();
				UpdatePreviewWindow();

				// Implementing variable button preview
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				buttonCount = EditorGUILayout.IntSlider(new GUIContent("Number of buttons"), buttonCount, 0, 9);
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				if (window)
				{
					if (GUILayout.Button("Generate a usable copy of this preview"))
					{
						GameObject previewInstance = Instantiate(window);
						previewInstance.transform.SetParent(canvas.transform, false);
						previewInstance.transform.localPosition = Vector3.zero;
						previewInstance.transform.localScale = Vector3.one;
						previewInstance.name = "Preview Panel Instance";

						WindowFramework.AttachDefaultTweeningPositions(previewInstance); // Generating default Show and Hide positions for tweening

						DestroyImmediate(previewPanel);
						isPreviewActive = false;
					}
					EditorGUILayout.Separator();
					EditorGUILayout.Separator();
				}
			}
			else
			{
				if (GUILayout.Button("Enable Preview"))
				{
					isPreviewActive = true;
					Debug.Log("Click \"Initiate Preview\" to see the preview");
				}
			}
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();

			themeAnElement = EditorGUILayout.BeginFoldoutHeaderGroup(themeAnElement, "Theme an Element");
			if (themeAnElement)
			{
				presets = mainSettings = primaryTextProperties = primaryProperties = secondaryProperties = secondaryTextProperties = buttonProperties = buttonPositioning = false;

				EditorGUILayout.Separator();
				element = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Element"), element, typeof(GameObject), true);
				if (element)
				{
					if (GUILayout.Button("Apply theme to \"" + element.name + "\""))
					{
						Button buttonReference = null;
						CanvasRenderer panelReference = null;
						if (element.TryGetComponent(out buttonReference))
						{
							ColorBlock colorBlock = new ColorBlock();
							colorBlock.normalColor = buttonNormalColor.colorValue;
							colorBlock.highlightedColor = buttonHighlightedColor.colorValue;
							colorBlock.pressedColor = buttonPressedColor.colorValue;
							colorBlock.selectedColor = buttonSelectedColor.colorValue;
							colorBlock.disabledColor = buttonDisabledColor.colorValue;
							colorBlock.fadeDuration = buttonFadeDuration.floatValue;
							colorBlock.colorMultiplier = 1;

							buttonReference.colors = colorBlock;
							buttonReference.transition = buttonTypeIndex == 2 ? Selectable.Transition.SpriteSwap : Selectable.Transition.ColorTint;

							buttonReference.image.sprite = (Sprite)buttonNormalSprite.objectReferenceValue;
							buttonReference.image.type = (Image.Type)Enum.Parse(typeof(Image.Type), panelTypeEnumNames[buttonImageType.enumValueIndex]);
							buttonReference.image.fillCenter = true;

							TextMeshProUGUI buttonTextReference = buttonReference.GetComponentInChildren<TextMeshProUGUI>();
							buttonTextReference.font = (TMP_FontAsset)buttonFontAsset.objectReferenceValue;
							buttonTextReference.fontStyle = (FontStyles)buttonFontStyle.intValue;
							buttonTextReference.fontSize = buttonFontSize.floatValue;
							buttonTextReference.color = buttonTextColor.colorValue;
							buttonTextReference.alignment = (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), TextAlignmentEnumNames[buttonTextAlignmentOptions.enumValueIndex]);
						}
						else if (element.TryGetComponent(out panelReference))
						{
							Image panelImageReference = null;
							element.TryGetComponent(out panelImageReference);
							if (!panelImageReference) panelImageReference = element.AddComponent<Image>();
							if (!panelImageReference.enabled) panelImageReference.enabled = true;

							panelImageReference.sprite = (Sprite)primarySourceImage.objectReferenceValue;
							panelImageReference.color = primaryColor.colorValue;
							panelImageReference.material = (Material)primaryMaterial.objectReferenceValue;
							panelImageReference.type = (Image.Type)Enum.Parse(typeof(Image.Type), panelTypeEnumNames[primaryType.enumValueIndex]);
							panelImageReference.fillCenter = true;

							TextMeshProUGUI[] texts = element.GetComponentsInChildren<TextMeshProUGUI>();
							foreach (TextMeshProUGUI text in texts)
							{
								text.font = (TMP_FontAsset)primaryFontAsset.objectReferenceValue;
								text.fontStyle = (FontStyles)primaryFontStyle.intValue;
								text.fontSize = primaryFontSize.floatValue;
								text.color = primaryTextColor.colorValue;
								text.alignment = (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), TextAlignmentEnumNames[primaryTextAlignmentOptions.enumValueIndex]);
							}
						}
						else throw new Exception("\"" + element.name + "\" is not a UI element");
						Selection.activeObject = element;
					}
				}
				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			presets = EditorGUILayout.BeginFoldoutHeaderGroup(presets, "Presets");
			if (presets || firstRun)
			{
				themeAnElement = mainSettings = primaryProperties = primaryTextProperties = secondaryProperties = secondaryTextProperties = buttonProperties = buttonPositioning = false;

				/* Following attributes will be taken care of, by Defaultify()
				- Orientation
				- Primary, Secondary and Button source images
				- Primary, Secondary and Button panel types
				- Primary and Secondary materials
				- Primary, Secondary and Button font assets
				- Button Sprites */

				if (GUILayout.Button("Unity Default Values"))
				{
					Defaultify();
					scaleMajorAxis.floatValue = majorSize = 640;
					scaleMinorAxis.floatValue = minorSize = 360;
					headerSize = titleBarSize.floatValue = 30;
					primaryColor.colorValue = new Color(1, 1, 1, 0.3921569f);
					primaryFontStyle.intValue = 0;
					targetClass.primaryFontStyle = primaryFontStyleEnum = (FontStyles)0;
					primaryFontSize.floatValue = 36;
					primaryTextColor.colorValue = Color.white;
					primaryTextAlignmentOptions.enumValueIndex = 0;
					secondaryColor.colorValue = Color.white;
					secondaryFontStyle.intValue = 0;
					targetClass.secondaryFontStyle = secondaryFontStyleEnum = (FontStyles)0;
					secondaryFontSize.floatValue = 24;
					secondaryTextColor.colorValue = new Color(0.1960784f, 0.1960784f, 0.1960784f);
					secondaryTextAlignmentOptions.enumValueIndex = 7;
					buttonType.intValue = buttonTypeIndex = 0;
					SecondaryToButtonTheme();
					buttonAlignment.enumValueIndex = 4;
					buttonSpacing.floatValue = 0;
					buttonLength.floatValue = 100;
					buttonHeight.floatValue = 100;
					serializedObject.ApplyModifiedProperties();
					RefreshPreview();
					presets = true;
				}
				else if (GUILayout.Button("Simple"))
				{
					Defaultify();
					scaleMajorAxis.floatValue = majorSize = 628;
					scaleMinorAxis.floatValue = minorSize = 300;
					headerSize = titleBarSize.floatValue = 74;
					primaryColor.colorValue = new Color(1, 1, 1, 0.8156863f);
					primaryFontStyle.intValue = 0;
					targetClass.primaryFontStyle = primaryFontStyleEnum = (FontStyles)0;
					primaryFontSize.floatValue = 25;
					primaryTextColor.colorValue = Color.black;
					primaryTextAlignmentOptions.enumValueIndex = 7;
					secondaryColor.colorValue = new Color(0, 0, 0, 0.8156863f);
					secondaryFontStyle.intValue = 0;
					targetClass.secondaryFontStyle = secondaryFontStyleEnum = (FontStyles)0;
					secondaryFontSize.floatValue = 28;
					secondaryTextColor.colorValue = Color.white;
					secondaryTextAlignmentOptions.enumValueIndex = 7;
					buttonType.intValue = buttonTypeIndex = 0;
					SecondaryToButtonTheme();
					buttonAlignment.enumValueIndex = 4;
					buttonSpacing.floatValue = 30;
					buttonLength.floatValue = 184;
					buttonHeight.floatValue = 64;
					serializedObject.ApplyModifiedProperties();
					RefreshPreview();
					presets = true;
				}
				else if(GUILayout.Button("Classy") || firstRun)
				{
					Defaultify();
					buttonCount = 2;
					
					// orientation done

					scaleMajorAxis.floatValue = majorSize = 560;
					scaleMinorAxis.floatValue = minorSize = 330;
					headerSize = titleBarSize.floatValue = 74;

					// primarySourceImage done
					primaryColor.colorValue = new Color(1, 1, 1, 0.3921569f);
					// primaryMaterial done
					// primaryType done

					//primaryFontAsset done
					primaryFontStyle.intValue = 0;
					targetClass.primaryFontStyle = primaryFontStyleEnum = (FontStyles)0;
					primaryFontSize.floatValue = 25;
					primaryTextColor.colorValue = Color.black;
					primaryTextAlignmentOptions.enumValueIndex = 0;

					// secondarySourceImage done
					secondaryColor.colorValue = new Color(0.1320755f, 0.1320755f, 0.1320755f, 0.8509804f);
					// secondaryMaterial done
					// secondaryType done

					//secondaryFontAsset done
					secondaryFontStyle.intValue = 33;
					targetClass.secondaryFontStyle = secondaryFontStyleEnum = (FontStyles)33;
					secondaryFontSize.floatValue = 28;
					secondaryTextColor.colorValue = Color.white;
					secondaryTextAlignmentOptions.enumValueIndex = 8;

					buttonType.intValue = buttonTypeIndex = 0;
					SecondaryToButtonTheme();
					buttonType.intValue = buttonTypeIndex = 1;	// if necessary
					// buttonImageType done
					buttonAlignment.enumValueIndex = 2;
					buttonSpacing.floatValue = 8f;
					buttonLength.floatValue = 140;
					buttonHeight.floatValue = 70;

					// buttonNormalSprite done
					// Sprite Attirbutes done
					// Button Color Attribute(s) optional
					// buttonFontAssets done
					// buttonsFontStyle optional
					buttonFontSize.floatValue = 21.5f;		// optional
					// buttonTextColor optional
					buttonTextAlignmentOptions.enumValueIndex = 7;	// optional

					serializedObject.ApplyModifiedProperties();
					RefreshPreview();

					if (firstRun) presets = firstRun = false;
					else presets = true;
				}
				else if (GUILayout.Button("XGI Dark"))
				{
					Defaultify();
					scaleMajorAxis.floatValue = majorSize = 620;
					scaleMinorAxis.floatValue = minorSize = 275;
					headerSize = titleBarSize.floatValue = 64.5f;
					primaryColor.colorValue = new Color(0, 0, 0, 0.58f);
					primaryFontStyle.intValue = 0;
					targetClass.primaryFontStyle = primaryFontStyleEnum = (FontStyles)0;
					primaryFontSize.floatValue = 25;
					primaryTextColor.colorValue = new Color(1,1, 1, 0.9019608f);
					primaryTextAlignmentOptions.enumValueIndex = 14;
					secondaryColor.colorValue = new Color(0, 0, 0, 0.38f);
					secondaryFontStyle.intValue = 33;
					targetClass.secondaryFontStyle = secondaryFontStyleEnum = (FontStyles)33;
					secondaryFontSize.floatValue = 28;
					secondaryTextColor.colorValue = Color.red;
					secondaryTextAlignmentOptions.enumValueIndex = 14;
					buttonType.intValue = buttonTypeIndex = 0;
					SecondaryToButtonTheme();
					buttonType.intValue = buttonTypeIndex = 1;
					buttonAlignment.enumValueIndex = 3;
					buttonSpacing.floatValue = 12;
					buttonLength.floatValue = 125;
					buttonHeight.floatValue = 56.5f;
					buttonFontSize.floatValue = 18;
					buttonNormalColor.colorValue = new Color(1, 0, 0, 0.9098039f);
					buttonPressedColor.colorValue = Color.white;
					buttonTextColor.colorValue = Color.white;
					buttonTextAlignmentOptions.enumValueIndex = 14;
					serializedObject.ApplyModifiedProperties();
					RefreshPreview();
					presets = true;
				}
				else if (GUILayout.Button("XGI Light"))
				{
					Defaultify();
					scaleMajorAxis.floatValue = majorSize = 620;
					scaleMinorAxis.floatValue = minorSize = 275;
					headerSize = titleBarSize.floatValue = 64.5f;
					primaryColor.colorValue = new Color(1, 1, 1, 0.6666667f);
					primaryFontStyle.intValue = 0;
					targetClass.primaryFontStyle = primaryFontStyleEnum = (FontStyles)0;
					primaryFontSize.floatValue = 25;
					primaryTextColor.colorValue = new Color(1, 0, 0, 0.9019608f);
					primaryTextAlignmentOptions.enumValueIndex = 14;
					secondaryColor.colorValue = Color.red;
					secondaryFontStyle.intValue = 33;
					targetClass.secondaryFontStyle = secondaryFontStyleEnum = (FontStyles)33;
					secondaryFontSize.floatValue = 28;
					secondaryTextColor.colorValue = Color.white;
					secondaryTextAlignmentOptions.enumValueIndex = 14;
					buttonType.intValue = buttonTypeIndex = 0;
					SecondaryToButtonTheme();
					buttonType.intValue = buttonTypeIndex = 1;
					buttonAlignment.enumValueIndex = 3;
					buttonSpacing.floatValue = 12;
					buttonLength.floatValue = 125;
					buttonHeight.floatValue = 56.5f;
					buttonFontSize.floatValue = 18;
					buttonNormalColor.colorValue = new Color(1, 0, 0, 0.9098039f);
					buttonPressedColor.colorValue = Color.white;
					buttonTextColor.colorValue = Color.white;
					buttonTextAlignmentOptions.enumValueIndex = 14;
					serializedObject.ApplyModifiedProperties();
					RefreshPreview();
					presets = true;
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
			//Debug.Log();
		}
		public void DrawPreviewWindow()
		{
			// Drawing the window panel
			window = new GameObject("Window");
			window.transform.parent = canvas.transform;
			window.transform.localPosition = Vector3.zero;
			RectTransform rectTransformPanel = (RectTransform)window.AddComponent(typeof(RectTransform));

			rectTransformPanel.anchoredPosition = Vector3.zero;
			rectTransformPanel.sizeDelta =
			orientation switch
			{
				0 => new Vector2(minorSize, majorSize),
				1 => new Vector2(majorSize, minorSize),
				_ => Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown ? new Vector2(minorSize, majorSize) : new Vector2(majorSize, minorSize)
			};
			// If you are getting an error in above line, comment it and uncomment the following snippet.

			#region Inactive code for systems not running on C# 8
			/* switch (orientation)
			{
				case 0:
					rectTransformPanel.sizeDelta = new Vector2(minorSize, majorSize);
					break;
				case 1:
					rectTransformPanel.sizeDelta = new Vector2(majorSize, minorSize);
					break;
				default:
					rectTransformPanel.sizeDelta = Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown ? new Vector2(minorSize, majorSize) : new Vector2(majorSize, minorSize);
					break;
			} */
			#endregion

			rectTransformPanel.localScale = Vector3.one;
			primaryImage = (Image)window.AddComponent(typeof(Image));

			// Adding text content to the panel
			GameObject primaryTextGameObject = new GameObject("Primary Text");
			primaryTextGameObject.transform.parent = window.transform;
			primaryTextGameObject.transform.localPosition = Vector3.zero;
			RectTransform rectTransformPrimaryText = (RectTransform)primaryTextGameObject.AddComponent(typeof(RectTransform));
			rectTransformPrimaryText.anchorMin = new Vector2(0, 0);
			rectTransformPrimaryText.anchorMax = new Vector2(1, 1);
			rectTransformPrimaryText.offsetMin = new Vector2(24, 110);
			rectTransformPrimaryText.offsetMax = new Vector2(-24, -headerSize-10);
			rectTransformPrimaryText.localScale = Vector3.one;
			primaryText = (TextMeshProUGUI)primaryTextGameObject.AddComponent(typeof(TextMeshProUGUI));
			primaryText.text = "This is a preview of the window generated by the currently selected Window Settings Asset";

			GameObject titleBar = new GameObject("Title Bar");
			titleBar.transform.parent = window.transform;
			titleBar.transform.localPosition = Vector3.zero;
			RectTransform rectTransformTitleBar = (RectTransform)titleBar.AddComponent(typeof(RectTransform));
			rectTransformTitleBar.anchorMin = new Vector2(0, 1);
			rectTransformTitleBar.anchorMax = new Vector2(1, 1);
			rectTransformTitleBar.offsetMin = new Vector2(0, 0);
			rectTransformTitleBar.offsetMax = new Vector2(0, headerSize);
			rectTransformTitleBar.anchoredPosition = new Vector2(0, -headerSize/2);
			rectTransformTitleBar.localScale = Vector3.one;
			secondaryImage = (Image)titleBar.AddComponent(typeof(Image));

			GameObject titleTextGameObject = new GameObject("Title Text");
			titleTextGameObject.transform.parent = titleBar.transform;
			titleTextGameObject.transform.localPosition = Vector3.zero;
			RectTransform rectTransformtitleText = (RectTransform)titleTextGameObject.AddComponent(typeof(RectTransform));
			rectTransformtitleText.anchorMin = Vector2.zero;
			rectTransformtitleText.anchorMax = Vector2.one;
			rectTransformtitleText.offsetMin = new Vector2(20, 0);
			rectTransformtitleText.offsetMax = new Vector2(-20, 0);
			rectTransformtitleText.localScale = Vector3.one;
			secondaryText = (TextMeshProUGUI)titleTextGameObject.AddComponent(typeof(TextMeshProUGUI));
			secondaryText.text = "Sample Window";

			GameObject buttonHolder = new GameObject("Button Holder");
			buttonHolder.transform.parent = window.transform;
			buttonHolder.transform.localPosition = Vector3.zero;
			RectTransform rectTransformButtonHolder = (RectTransform)buttonHolder.AddComponent(typeof(RectTransform));
			rectTransformButtonHolder.anchorMin = new Vector2(0, 0);
			rectTransformButtonHolder.anchorMax = new Vector2(1, 0);
			rectTransformButtonHolder.offsetMin = new Vector2(24, 0);
			rectTransformButtonHolder.offsetMax = new Vector2(-24, 100);
			rectTransformButtonHolder.anchoredPosition = new Vector2(0, 50f);
			rectTransformButtonHolder.localScale = Vector3.one;
			buttonLayout = (GridLayoutGroup)buttonHolder.AddComponent(typeof(GridLayoutGroup));
			buttonLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
			buttonLayout.constraintCount = 1;

			buttonImage.Clear();
			buttonCharacteristics.Clear();
			buttonText.Clear();
			// Creating non functional sample buttons
			for (int i = 0; i < buttonCount; i++)
			{
				// Creating button
				GameObject previewButton = new GameObject("Preview Button");
				previewButton.transform.parent = buttonLayout.transform;
				previewButton.transform.localPosition = Vector3.zero;
				RectTransform rectTransformButton = (RectTransform)previewButton.AddComponent(typeof(RectTransform));
				rectTransformButton.anchorMin = new Vector2(0.5f, 0);
				rectTransformButton.anchorMax = new Vector2(0.5f, 0);
				rectTransformButton.anchoredPosition = new Vector3(0, 60, 0);
				rectTransformButton.sizeDelta = new Vector2(150, 72.5f);
				rectTransformButton.localScale = Vector3.one;

				// Adding and storing Image and Button components to the created button to later change thier properties
				buttonImage.Add((Image)previewButton.AddComponent(typeof(Image)));
				buttonCharacteristics.Add((Button)previewButton.AddComponent(typeof(Button)));

				// Writing text on the button
				// Creating a placeholder for the text
				GameObject secondaryTextGameObject = new GameObject("Secondary Text");
				secondaryTextGameObject.transform.parent = previewButton.transform;
				secondaryTextGameObject.transform.localPosition = Vector3.zero;
				RectTransform rectTransformButtonText = (RectTransform)secondaryTextGameObject.AddComponent(typeof(RectTransform));
				rectTransformButtonText.anchorMin = Vector2.zero;
				rectTransformButtonText.anchorMax = Vector2.one;
				rectTransformButtonText.offsetMin = new Vector2(10, 10);
				rectTransformButtonText.offsetMax = new Vector2(-10, -10);
				rectTransformButtonText.localScale = Vector3.one;

				// Adding and storing the text references in a list to modify their properties as per user's preferences
				buttonText.Add((TextMeshProUGUI)secondaryTextGameObject.AddComponent(typeof(TextMeshProUGUI)));

				// Labeling the created buttons
				int buttonId = 1;
				foreach (TextMeshProUGUI label in buttonText) label.text = "Button " + buttonId++;
			}

			previewPanel = new GameObject("Preview Panel");
			previewPanel.transform.parent = canvas.transform;
			previewPanel.transform.localPosition = Vector3.zero;
			previewPanel.transform.localScale = Vector3.one;
			RectTransform previewRect = (RectTransform)previewPanel.AddComponent(typeof(RectTransform));
			previewRect.anchorMin = Vector2.zero;
			previewRect.anchorMax = Vector2.one;
			previewRect.pivot = new Vector2(0.5f, 0.5f);
			previewRect.offsetMin = previewRect.offsetMax = Vector2.zero;
			hindererImage = (Image)previewPanel.AddComponent(typeof(Image));
			window.transform.SetParent(previewPanel.transform);

		}
		public void UpdatePreviewWindow()
		{
			// Updating the preview
			if (window)
			{
				primaryImage.sprite = (Sprite)primarySourceImage.objectReferenceValue;
				primaryImage.color = primaryColor.colorValue;
				primaryImage.material = (Material)primaryMaterial.objectReferenceValue;
				primaryImage.type = (Image.Type)Enum.Parse(typeof(Image.Type), panelTypeEnumNames[primaryType.enumValueIndex]);

				secondaryImage.sprite = (Sprite)secondarySourceImage.objectReferenceValue;
				secondaryImage.color = secondaryColor.colorValue;
				secondaryImage.material = (Material)secondaryMaterial.objectReferenceValue;
				secondaryImage.type = (Image.Type)Enum.Parse(typeof(Image.Type), panelTypeEnumNames[secondaryType.enumValueIndex]);
				hindererImage.color = backgroundHindererColor.colorValue;

				primaryText.font = (TMP_FontAsset)primaryFontAsset.objectReferenceValue;
				primaryText.fontStyle = (FontStyles)primaryFontStyle.intValue;
				primaryText.fontSize = primaryFontSize.floatValue;
				primaryText.color = primaryTextColor.colorValue;
				primaryText.alignment = (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), TextAlignmentEnumNames[primaryTextAlignmentOptions.enumValueIndex]);

				secondaryText.font = (TMP_FontAsset)secondaryFontAsset.objectReferenceValue;
				secondaryText.fontStyle = (FontStyles)secondaryFontStyle.intValue;
				secondaryText.fontSize = secondaryFontSize.floatValue;
				secondaryText.color = secondaryTextColor.colorValue;
				secondaryText.alignment = (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), TextAlignmentEnumNames[secondaryTextAlignmentOptions.enumValueIndex]);

				buttonColors.normalColor = buttonNormalColor.colorValue;
				buttonColors.highlightedColor = buttonHighlightedColor.colorValue;
				buttonColors.pressedColor = buttonPressedColor.colorValue;
				buttonColors.selectedColor = buttonSelectedColor.colorValue;
				buttonColors.disabledColor = buttonDisabledColor.colorValue;
				buttonColors.fadeDuration = buttonFadeDuration.floatValue;
				buttonColors.colorMultiplier = 1;

				foreach (Button button in buttonCharacteristics)
				{
					button.colors = buttonColors;
				    button.transition = buttonTypeIndex == 2 ? Selectable.Transition.SpriteSwap : Selectable.Transition.ColorTint;
				}
				foreach (Image button in buttonImage)
				{
					button.sprite = (Sprite)buttonNormalSprite.objectReferenceValue;
					button.type = (Image.Type)Enum.Parse(typeof(Image.Type), panelTypeEnumNames[buttonImageType.enumValueIndex]);
				}
				foreach(TextMeshProUGUI button in buttonText)
				{
					button.font = (TMP_FontAsset)buttonFontAsset.objectReferenceValue;
					button.fontStyle = (FontStyles)buttonFontStyle.intValue;
					button.fontSize = buttonFontSize.floatValue;
					button.color = buttonTextColor.colorValue;
					button.alignment = (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), TextAlignmentEnumNames[buttonTextAlignmentOptions.enumValueIndex]);
				}

				buttonLayout.cellSize = new Vector2(buttonLength.floatValue, buttonHeight.floatValue);
				buttonLayout.spacing = new Vector2(buttonSpacing.floatValue, 0);
				buttonLayout.childAlignment = (TextAnchor)buttonAlignment.enumValueIndex;

				if (!presets) SecondaryToButtonTheme();
			}
		}
		private void RefreshPreview()
		{
			DestroyImmediate(previewPanel);
			if (isPreviewActive && canvas)
			{
				DrawPreviewWindow();
				UpdatePreviewWindow();
			}
		}
		public void Defaultify()
		{
			orientationState.intValue = orientation = 2;

			primarySourceImage.objectReferenceValue = secondarySourceImage.objectReferenceValue = buttonNormalSprite.objectReferenceValue = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
			buttonHighlightedSprite.objectReferenceValue = buttonSelectedSprite.objectReferenceValue = buttonPressedSprite.objectReferenceValue = buttonDisabledSprite.objectReferenceValue = null;
			primaryType.enumValueIndex = secondaryType.enumValueIndex = buttonImageType.enumValueIndex = 1;
			primaryMaterial.objectReferenceValue = secondaryMaterial.objectReferenceValue = null;
			backgroundHindererColor.colorValue = Color.clear;

			AssetFinder finder = new AssetFinder("LiberationSans SDF");
			TMP_FontAsset defaultTextAsset = finder.FetchAsset<TMP_FontAsset>();
			primaryFontAsset.objectReferenceValue = secondaryFontAsset.objectReferenceValue = buttonFontAsset.objectReferenceValue = defaultTextAsset;
		}
		public void SecondaryToButtonTheme()
		{
			// Copying secondary theme and applying them to button theme if button type is set to secondary themed
			if (buttonTypeIndex == 0)
			{
				buttonNormalSprite.objectReferenceValue = secondarySourceImage.objectReferenceValue;
				buttonImageType.enumValueIndex = secondaryType.enumValueIndex;
				buttonHighlightedSprite.objectReferenceValue = buttonPressedSprite.objectReferenceValue = buttonSelectedSprite.objectReferenceValue = buttonDisabledSprite.objectReferenceValue = null;
				buttonNormalColor.colorValue = secondaryColor.colorValue;
				Color mainColor = secondaryColor.colorValue;
				buttonPressedColor.colorValue = new Color(0.7843137f, 0.7843137f, 0.7843137f);
				buttonHighlightedColor.colorValue = buttonSelectedColor.colorValue = new Color(1 - mainColor.r, 1 - mainColor.g, 1 - mainColor.b, mainColor.a);
				buttonDisabledColor.colorValue = new Color(mainColor.r, mainColor.g, mainColor.b, mainColor.a / 2);

				buttonFontAsset.objectReferenceValue = secondaryFontAsset.objectReferenceValue;
				buttonFontStyle.intValue = secondaryFontStyle.intValue;
				buttonFontStyleEnum = secondaryFontStyleEnum;
				buttonFontSize.floatValue = secondaryFontSize.floatValue;
				buttonTextColor.colorValue = secondaryTextColor.colorValue;
				buttonTextAlignmentOptions.enumValueIndex = secondaryTextAlignmentOptions.enumValueIndex;

				serializedObject.ApplyModifiedProperties();
				presets = true;
				if(presets) RefreshPreview();
				presets = false;
			}
		}
		private void OnDisable()
		{
			if (window)
			{
				DestroyImmediate(previewPanel); // Discarding the preview instance
				Debug.Log("Preview mode aborted");
			}
		}
	}
}