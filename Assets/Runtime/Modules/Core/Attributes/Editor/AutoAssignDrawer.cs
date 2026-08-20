using System;
using UnityEditor;
using UnityEngine;
using UnityTemplates.Attributes;
using Object = UnityEngine.Object;

namespace UnityTemplates.Editor.Attributes
{
	[CustomPropertyDrawer(typeof(AssignAttribute))]
	public sealed class AutoAssignDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			try
			{
				TryAssign(property);
				EditorGUI.PropertyField(position, property, label, true);
			}
			finally
			{
				EditorGUI.EndProperty();
			}
		}

		private void TryAssign(SerializedProperty property)
		{
			if (Event.current?.type != EventType.Layout ||
				property.propertyType != SerializedPropertyType.ObjectReference ||
				property.objectReferenceValue != null ||
				property.serializedObject.isEditingMultipleObjects ||
				property.serializedObject.targetObject is not Component source ||
				fieldInfo == null ||
				!typeof(Component).IsAssignableFrom(fieldInfo.FieldType))
			{
				return;
			}

			AssignMode mode = ((AssignAttribute)attribute).Mode;
			Object result = Find(source, fieldInfo.FieldType, mode);
			if (result == null)
			{
				return;
			}

			property.objectReferenceValue = result;
			property.serializedObject.ApplyModifiedProperties();
		}

		private static Object Find(Component source, Type type, AssignMode mode)
		{
			return mode switch
			{
				AssignMode.Self => source.GetComponent(type),
				AssignMode.Parent => FindInParents(source, type),
				AssignMode.Children => FindInChildren(source, type),
				AssignMode.Scene => FindUniqueInScene(source, type),
				_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
			};
		}

		private static Component FindInParents(Component source, Type type)
		{
			Transform parent = source.transform.parent;
			return parent == null ? null : parent.GetComponentInParent(type, true);
		}

		private static Component FindInChildren(Component source, Type type)
		{
			Component[] candidates = source.GetComponentsInChildren(type, true);
			for (int i = 0; i < candidates.Length; i++)
			{
				if (candidates[i].transform != source.transform)
				{
					return candidates[i];
				}
			}

			return null;
		}

		private static Component FindUniqueInScene(Component source, Type type)
		{
			Object[] candidates = Object.FindObjectsByType(type, FindObjectsInactive.Include);

			Component match = null;
			for (int i = 0; i < candidates.Length; i++)
			{
				if (candidates[i] is not Component candidate || candidate.gameObject.scene != source.gameObject.scene)
				{
					continue;
				}

				if (match != null)
				{
					return null;
				}

				match = candidate;
			}

			return match;
		}
	}
}