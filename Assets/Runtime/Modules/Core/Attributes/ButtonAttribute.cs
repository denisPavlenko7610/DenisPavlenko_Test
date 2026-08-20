using System;

namespace UnityTemplates.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ButtonAttribute : Attribute
	{

		public string Label { get; }

		public ButtonAttribute(string label = null)
		{
			Label = label;
		}
	}
}