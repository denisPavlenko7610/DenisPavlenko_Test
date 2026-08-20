using System;
using UnityEngine;

namespace UnityTemplates.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class SceneAttribute : PropertyAttribute { }
}