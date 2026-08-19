using UnityEditor;
using UnityEngine;
using UnityTemplates.Attributes;

namespace UnityTemplates.Editor.Attributes
{
	[CustomPropertyDrawer(typeof(SceneAttribute))]
	public sealed class ScenePathDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			try
			{
				if (property.propertyType != SerializedPropertyType.String)
				{
					EditorGUI.LabelField(position, label.text, "[Scene] works only with string fields.");

					return;
				}

				SceneAsset currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);

				EditorGUI.BeginChangeCheck();

				SceneAsset selectedScene = EditorGUI.ObjectField(
						position,
						label,
						currentScene,
						typeof(SceneAsset),
						false
					)
					as SceneAsset;

				if (EditorGUI.EndChangeCheck())
				{
					property.stringValue = selectedScene == null
						? string.Empty
						: AssetDatabase.GetAssetPath(selectedScene);
				}
			}
			finally
			{
				EditorGUI.EndProperty();
			}
		}
	}
}
