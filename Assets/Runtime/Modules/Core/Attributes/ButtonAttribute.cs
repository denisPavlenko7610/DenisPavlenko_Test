using System;

namespace UnityTemplates.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ButtonAttribute : Attribute
	{
		public ButtonAttribute(string label = null)
		{
			Label = label;
		}

		public string Label { get; }
	}
}
