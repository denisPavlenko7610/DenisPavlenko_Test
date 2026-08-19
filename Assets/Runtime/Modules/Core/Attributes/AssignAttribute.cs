using System;
using UnityEngine;

namespace UnityTemplates.Attributes
{
	public enum AssignMode
	{
		Self = 0,
		Parent = 1,
		Children = 2,
		Scene = 3
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class AssignAttribute : PropertyAttribute
	{
		public AssignAttribute(AssignMode mode = AssignMode.Self)
		{
			Mode = mode;
		}

		public AssignMode Mode { get; }
	}
}
