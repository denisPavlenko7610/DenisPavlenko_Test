using UnityEditor;
using UnityEngine;

namespace UnityTemplates.Editor.Attributes
{
	[CanEditMultipleObjects, CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
	public sealed class ButtonScriptableObjectEditor : ButtonEditorBase { }
}