using System;
using System.Collections.Generic;
using System.Reflection;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Aituk.Plugin
{
	public class CMDWhenEvent : IRocketCommand
	{
		public bool AllowFromConsole
		{
			get
			{
				return false;
			}
		}

		public List<string> Permissions
		{
			get
			{
				return new List<string>
				{
					"whenenevt"
				};
			}
		}

		public AllowedCaller AllowedCaller
		{
			get
			{
				return AllowedCaller.Player;
			}
		}

		public bool RunFromConsole
		{
			get
			{
				return false;
			}
		}

		public string Name
		{
			get
			{
				return "whenenevt";
			}
		}

		public string Syntax
		{
			get
			{
				return "whenenevt";
			}
		}

		public string Help
		{
			get
			{
				return "";
			}
		}

		public List<string> Aliases
		{
			get
			{
				return new List<string>();
			}
		}

		public void Execute(IRocketPlayer caller, string[] command)
		{
			UnturnedPlayer uplayer = (UnturnedPlayer)caller;

			if (Plugin.AEvent == null)
				UnturnedChat.Say(uplayer, Plugin.Instance.Translate("when_event", Plugin.Instance.SecOnStartEvent));
			else
				UnturnedChat.Say(uplayer, Plugin.Instance.Translate("when_event_started", null));

		}
	}
}
