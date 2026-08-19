using UnityEditor;
using UnityEngine;

namespace UnityTemplates.Editor.Attributes
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
	public sealed class ButtonMonoBehaviourEditor : ButtonEditorBase { }
}
