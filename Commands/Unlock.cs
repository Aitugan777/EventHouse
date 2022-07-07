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
	public class CMDUnlock : IRocketCommand
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
					"unlock"
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
				return "start";
			}
		}

		public string Syntax
		{
			get
			{
				return "start";
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
            {
				UnturnedChat.Say(uplayer, Plugin.Instance.Translate("event_not_started", null));
				return;
            }
			if (Plugin.AEvent.StartedUnlock)
            {
				UnturnedChat.Say(uplayer, Plugin.Instance.Translate("duble_opened", Plugin.AEvent.SecOnOpenLocker));
				return;
            }



			byte b = 0; 
			byte b2 = 0;
			ushort num = 0;
			ushort index = 0;
			RaycastHit raycastHit;
			Physics.Raycast(uplayer.Player.look.aim.position, uplayer.Player.look.aim.forward, out raycastHit, float.PositiveInfinity, -67108864);
			if (raycastHit.transform != null)
			{
				BarricadeRegion barricadeRegion;
				if (BarricadeManager.tryGetInfo(raycastHit.transform, out b, out b2, out num, out index, out barricadeRegion))
				{
					BarricadeData barricadeData = barricadeRegion.barricades[(int)index];
					if (barricadeData.barricade.id == Plugin.Instance.Configuration.Instance.StorageID)
                    {
						if (Vector3.Distance(Plugin.AEvent.Position, barricadeData.point) <= Plugin.Instance.Configuration.Instance.RadiusHouse)
                        {
							Plugin.Instance.SendAllMessage("start_unlock_player", new object[] { uplayer.CharacterName });
							Plugin.AEvent.SecOnOpenLocker = Plugin.Instance.Configuration.Instance.SecOnOpenLocker;
							Plugin.AEvent.StartedUnlock = true;
                        }
					}
					else
					{
						UnturnedChat.Say(uplayer, Plugin.Instance.Translate("locker_not_found", null));
					}
				}
                else
                {
					UnturnedChat.Say(uplayer, Plugin.Instance.Translate("locker_not_found", null));
                }

			}

		}
	}
}
