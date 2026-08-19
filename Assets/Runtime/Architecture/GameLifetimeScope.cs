using UnityEngine;
using VContainer;
using VContainer.Unity;
using DenisPavlenko.Game;
using DenisPavlenko.Game.Core;
using UnityTemplates.Attributes;

namespace DenisPavlenko.Project.Architecture
{
	public sealed class GameLifetimeScope : LifetimeScope
	{
		[Assign(AssignMode.Scene)] [SerializeField] private GameFlowController _controller;

		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterInstance(GameConfig.CreateDefault());

			if (_controller != null)
			{
				builder.RegisterComponent(_controller);
			}
		}
	}
}