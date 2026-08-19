using System;
using UnityEngine;

namespace UnityTemplates.Attributes
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class SceneAttribute : PropertyAttribute { }
}
