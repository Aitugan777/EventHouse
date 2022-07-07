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
	public class CMDScanHouse : IRocketCommand
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
					"scanhouse"
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
				return "scanhouse";
			}
		}

		public string Syntax
		{
			get
			{
				return "scanhouse";
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
			if (command.Length == 0)
            {
				UnturnedChat.Say(caller, "/scanhouse radius");
            }
			Plugin.Instance.CheckHouse(uplayer.Position, float.Parse(command[0]));
			Plugin.Instance.Configuration.Instance.RadiusHouse = float.Parse(command[0]);
			Plugin.Instance.Configuration.Save();
			UnturnedChat.Say(uplayer, "House sueccessfull scaned!");
		}
	}
}
