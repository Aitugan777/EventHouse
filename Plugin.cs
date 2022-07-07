using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Aituk.Plugin
{
	public class Plugin : RocketPlugin<Config>
	{
		public static Plugin Instance;

		protected override void Load()
		{
			Plugin.Instance = this;
			SecOnStartEvent = base.Configuration.Instance.EventTime;
			AEvent = null;
			Console.WriteLine("Plugin by Aituk loaded!");
			if (base.Configuration.Instance.LoadWorkshop)
				WorkshopDownloadConfig.getOrLoad().File_IDs.Add(2790852026);
		}

		public static AitukEvent AEvent = null;
		public static List<Transform> lockers = new List<Transform>();
		public int SecOnStartEvent = 0;

		public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "start_event", "Евент начался, координаты в GPS!" },
            { "locker_opened", "Ящик наполнен вещами!" },
            { "end_event", "Евент закончился, след евент будет через {0}с.!" },
            { "start_unlock", "Через {0}с. ящик будет наполнен вещами!" },
            { "start_unlock_player", "Игрок {0} активировал ящик!" },
            { "duble_opened", "Уже начат! Осталось {0}с.!" },
            { "event_not_started", "Эвент еще не начался!" },
            { "not_started", "Пишите /start, что-бы наполнить ящик вещами!" },
            { "start_ui", "Через {0}c начнется евент!" },
            { "position_not_found", "Поизиция не найдена для постройки дома!" },
            { "locker_not_found", "Хранилище не найдена!" },
            { "not_mask", "Оденьте маску, для защиты от радиации!" },
            { "when_event", "Эвент начнется через {0}с." },
            { "when_event_started", "Эвент уже идет!" },
        };


        protected override void Unload()
		{
			if (AEvent != null)
				EndEvent();
			Console.WriteLine("Plugin by Aituk unloaded!"); 
		}

		public void FixedUpdate()
		{
			Timer();
		}

		private DateTime lastCalled = DateTime.Now;

		public void Timer()
		{
			if ((DateTime.Now - this.lastCalled).TotalSeconds > 1.0)
			{
				this.lastCalled = DateTime.Now;

				if (AEvent == null)
                {
					if (SecOnStartEvent == 0)
					{
						StartEvent();
					}
					else if (SecOnStartEvent > 0)
					{
						SecOnStartEvent--;
						if (SecOnStartEvent <= base.Configuration.Instance.SecOnShowStartUI)
                        {
							EffectManager.sendUIEffect(base.Configuration.Instance.UIID, (short)base.Configuration.Instance.UIID, true, Translate("start_ui", SecOnStartEvent));
                        }
					}
				}

				if (AEvent != null)
				{

					RadioactiveZone();
					if (AEvent.StartedUnlock)
					{
						if (AEvent.SecOnOpenLocker == 1)
						{
							AEvent.SecOnOpenLocker = 0;
							AEvent.SecOnDestroy = base.Configuration.Instance.SecOnDestroyIsOpened;
							System.Random random = new System.Random();
							foreach (Transform transform in lockers)
                            {

								InteractableStorage _storage;
								_storage = transform.GetComponent<InteractableStorage>();


								bool isFull = false;
								while (!isFull)
								{
									ushort randitem = base.Configuration.Instance.Items[random.Next(0, base.Configuration.Instance.Items.Count - 1)];
									if (!_storage.items.tryAddItem(new SDG.Unturned.Item(randitem, true)))
									{
										isFull = true;
									}
								}
							}
							lockers = new List<Transform>();
							SendAllMessage("locker_opened", null);

							foreach (SteamPlayer steamPlayer in Provider.clients)
							{
								UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);
								EffectManager.askEffectClearByID(base.Configuration.Instance.UIID, uplayer.CSteamID);
							}
						}
						else if (AEvent.SecOnOpenLocker > 0)
                        {
							AEvent.SecOnOpenLocker--;
							EffectManager.sendUIEffect(base.Configuration.Instance.UIID, (short)base.Configuration.Instance.UIID, true, Translate("start_unlock", AEvent.SecOnOpenLocker));
						}
					}

					if (AEvent.SecOnOpenLocker == 0)
					{
						if (AEvent.SecOnDestroy == 1)
						{
							AEvent.SecOnDestroy = 0;
							EndEvent();
						}
						else if (AEvent.SecOnDestroy > 0)
							AEvent.SecOnDestroy--;
					}
				}
			}
		}

		public void RadioactiveZone()
        {
			foreach (SteamPlayer steamPlayer in Provider.clients)
			{
				UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);
				if (Vector3.Distance(uplayer.Position, AEvent.Position) <= base.Configuration.Instance.RadiusRadiation)
				{
					if (uplayer.Player.clothing.maskAsset == null)
					{
						if (uplayer.Infection > 0)
							uplayer.Infection = (byte)((100 - uplayer.Infection) + 1);
						UnturnedChat.Say(uplayer, Translate("not_mask"));
					}
				}
			}
		}

		public void SendAllMessage(string key, object[] args)
        {
			foreach(SteamPlayer steamPlayer in Provider.clients)
            {
				UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);
				UnturnedChat.Say(uplayer, Translate(key, args));
            }
        }

		public void StartEvent()
        {
			Vector3 pos = GetRandPos();
			if (pos == new Vector3())
			{
				SendAllMessage("position_not_found", null);
				SecOnStartEvent = base.Configuration.Instance.EventTime;
				foreach (SteamPlayer steamPlayer in Provider.clients)
				{
					UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);
					EffectManager.askEffectClearByID(base.Configuration.Instance.UIID, uplayer.CSteamID);
				}
				return;
			}
			AEvent = new AitukEvent() { Position = pos, SecOnDestroy = base.Configuration.Instance.SecOnDestroy, SecOnOpenLocker = 0, StartedUnlock = false};
			SpawnHouse();
			SendAllMessage("start_event", null);


			foreach (SteamPlayer steamPlayer in Provider.clients)
			{
				UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);
				uplayer.Player.quests.replicateSetMarker(true, AEvent.Position, "Event");
				EffectManager.askEffectClearByID(base.Configuration.Instance.UIID, uplayer.CSteamID);
			}

		}

		public void EndEvent()
        {
			ClearHouse();
			if (base.Configuration.Instance.ClearAllItemsOnDestroy)
				ItemManager.askClearAllItems();
			AEvent = null;
			SecOnStartEvent = base.Configuration.Instance.EventTime;
			SendAllMessage("end_event", new object[] { base.Configuration.Instance.EventTime });
		}

		public void ClearHouse()
        {

			int upperBound;
			int upperBound2;
			bool clearedStr = false;
			bool clearedBar = false;
			if (StructureManager.regions != null)
			{
				StructureRegion[,] regions = StructureManager.regions;
				upperBound = regions.GetUpperBound(0);
				upperBound2 = regions.GetUpperBound(1);
				while (!clearedStr)
				{
					try
					{
						for (int i = regions.GetLowerBound(0); i <= upperBound; i++)
						{
							for (int j = regions.GetLowerBound(1); j <= upperBound2; j++)
							{
								StructureRegion structureRegion = regions[i, j];
								if (structureRegion != null)
								{
									int indx = 0;
									foreach (StructureData structureData in structureRegion.structures)
									{
										if (((structureData != null) ? structureData.structure : null) != null)
										{
											ItemStructureAsset itemStructureAsset = (ItemStructureAsset)Assets.find(EAssetType.ITEM, structureData.structure.id);
											if (itemStructureAsset != null)
											{
												if (Vector3.Distance(AEvent.Position, structureData.point) <= base.Configuration.Instance.RadiusHouse)
												{
													StructureManager.destroyStructure(structureRegion, (byte)i, (byte)j, (ushort)indx, structureData.point);
												}
											}
										}
										indx++;
									}
								}
							}
						}
						clearedStr = true;
					}
					catch { }
				}
			}
			if (BarricadeManager.regions == null)
			{
				return;
			}
			BarricadeRegion[,] regions2 = BarricadeManager.regions;
			upperBound2 = regions2.GetUpperBound(0);
			upperBound = regions2.GetUpperBound(1);
			while (!clearedBar)
			{
				try
				{
					for (int i = regions2.GetLowerBound(0); i <= upperBound2; i++)
					{
						for (int j = regions2.GetLowerBound(1); j <= upperBound; j++)
						{
							BarricadeRegion barricadeRegion = regions2[i, j];
							if (barricadeRegion != null)
							{
								int indx = 0;
								foreach (BarricadeData barricadeData in barricadeRegion.barricades)
								{
									if (((barricadeData != null) ? barricadeData.barricade : null) != null)
									{
										ItemBarricadeAsset itemBarricadeAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, barricadeData.barricade.id);
										if (itemBarricadeAsset != null)
										{
											if (Vector3.Distance(AEvent.Position, barricadeData.point) <= base.Configuration.Instance.RadiusHouse)
											{
												BarricadeManager.destroyBarricade(barricadeRegion, (byte)i, (byte)j, 65535, (ushort)indx);
											}
										}
									}
								}
							}
						}
					}
					clearedBar = true;
				}
				catch { }
			}

		}

		public Vector3 GetRandPos()
        {
			List<Vector3> list = new List<Vector3>();
			foreach(Vector3 vector in base.Configuration.Instance.Points)
            {
				list.Add(vector);
            }

			while (list.Count > 0)
			{
				System.Random random = new System.Random();
				int randomint = random.Next(0, base.Configuration.Instance.Points.Count - 1);
				Vector3 RandPoint = list[randomint];

				bool noplayers = true;
				foreach(SteamPlayer steamplayer in Provider.clients)
                {
					UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamplayer);
					if (Vector3.Distance(uplayer.Position, RandPoint) <= base.Configuration.Instance.RadiusNoPlayer)
						noplayers = false;
				}
				if (noplayers)
					return RandPoint;
				list.RemoveAt(randomint);
            }

			return new Vector3();
		}

		public void SpawnHouse()
        {
			lockers = new List<Transform>();
			foreach(AitukBuilding building in base.Configuration.Instance.Structures)
            {
				Structure structure = new Structure(building.ID);
				StructureManager.dropStructure(structure, GetSpawnPoint(building.Position), 0, building.Rotation_Y, 0, 0, 0);
			}
			foreach(AitukBuilding building in base.Configuration.Instance.Barricades)
			{
				Barricade barricade = new Barricade(building.ID);
				Quaternion rotation = BarricadeManager.getRotation(barricade.asset, 0, building.Rotation_Y, 0);

				Transform transform = BarricadeManager.dropNonPlantedBarricade(barricade, GetSpawnPoint(building.Position), rotation, 0, 0);
				barricade.health = ushort.MaxValue;

				if (building.ID == base.Configuration.Instance.GeneratorID)
				{
					InteractableGenerator generator = transform.GetComponent<InteractableGenerator>();
					generator.ReceiveFuel(250);
					generator.ReceivePowered(true);
				}
				if (building.ID == base.Configuration.Instance.SentryID)
				{
					InteractableStorage sentry;
					sentry = transform.GetComponent<InteractableStorage>();
					sentry.items.tryAddItem(new SDG.Unturned.Item(base.Configuration.Instance.GunSentry, true));
				}
				if (building.ID == base.Configuration.Instance.StorageID)
				{
					lockers.Add(transform);
				}
			}
        }


		public Vector3 GetSpawnPoint(Vector3 position)
        {
			Vector3 vector3 = AEvent.Position;

			vector3.x += position.x;
			vector3.y += position.y;
			vector3.z += position.z;

			return vector3;
        }

		public void CheckHouse(Vector3 vector3, float radius)
        {

			List<AitukBuilding> listS = new List<AitukBuilding>();
			List<AitukBuilding> listB = new List<AitukBuilding>();

			int upperBound;
			int upperBound2;
			if (StructureManager.regions != null)
			{
				StructureRegion[,] regions = StructureManager.regions;
				upperBound = regions.GetUpperBound(0);
				upperBound2 = regions.GetUpperBound(1);
				for (int i = regions.GetLowerBound(0); i <= upperBound; i++)
				{
					for (int j = regions.GetLowerBound(1); j <= upperBound2; j++)
					{
						StructureRegion structureRegion = regions[i, j];
						if (structureRegion != null)
						{
							int indx = 0;
							foreach (StructureData structureData in structureRegion.structures)
							{
								if (((structureData != null) ? structureData.structure : null) != null)
								{
									if (Vector3.Distance(vector3, structureData.point) <= radius)
									{
										listS.Add(new AitukBuilding() { ID = structureData.structure.id, Position = new Vector3(structureData.point.x - vector3.x, structureData.point.y - vector3.y, structureData.point.z - vector3.z), Rotation_Y = structureData.angle_y * 2 });
									}
								}
								indx++;
							}
						}
					}
				}
			}
			base.Configuration.Instance.Structures = listS;
			base.Configuration.Save();
			if (BarricadeManager.regions == null)
			{
				return;
			}
			BarricadeRegion[,] regions2 = BarricadeManager.regions;
			upperBound2 = regions2.GetUpperBound(0);
			upperBound = regions2.GetUpperBound(1);
			for (int i = regions2.GetLowerBound(0); i <= upperBound2; i++)
			{
				for (int j = regions2.GetLowerBound(1); j <= upperBound; j++)
				{
					BarricadeRegion barricadeRegion = regions2[i, j];
					if (barricadeRegion != null)
					{
						foreach (BarricadeData barricadeData in barricadeRegion.barricades)
						{
							if (((barricadeData != null) ? barricadeData.barricade : null) != null)
							{

								if (Vector3.Distance(vector3, barricadeData.point) <= radius)
								{
									listB.Add(new AitukBuilding() { ID = barricadeData.barricade.id, Position = new Vector3(barricadeData.point.x - vector3.x, barricadeData.point.y - vector3.y, barricadeData.point.z - vector3.z), Rotation_Y = barricadeData.angle_y * 2 });
								}
							}
						}
					}
				}
			}
			base.Configuration.Instance.Barricades = listB;
			base.Configuration.Save();
		}
	}
}
