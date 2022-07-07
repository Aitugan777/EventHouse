using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;
using UnityEngine;

namespace Aituk.Plugin
{
	public class Config : IRocketPluginConfiguration, IDefaultable
	{
		public void LoadDefaults()
		{
			this.Items = new List<ushort>()
			{
				363,
				17,
				126,
				132,
				253
			};
		}

		public bool LoadWorkshop = true;
		public ushort UIID = 7700;

		public int SecOnShowStartUI = 30;

		public int SecOnOpenLocker = 60;
		public int SecOnDestroyIsOpened = 60;
		public bool ClearAllItemsOnDestroy = true;

		public int SecOnDestroy = 120;

		public int EventTime = 120;

		public float RadiusNoPlayer = 200f;
		public float RadiusRadiation = 200f;

		public ushort SentryID = 1373;
		public ushort GunSentry = 363;

		public ushort GeneratorID = 458;

		public ushort StorageID = 368;

		public float RadiusHouse = 0f;

		[XmlArray("Structures"), XmlArrayItem("Structure")]
		public List<AitukBuilding> Structures = new List<AitukBuilding>();

		[XmlArray("Barricades"), XmlArrayItem("Barricade")]
		public List<AitukBuilding> Barricades = new List<AitukBuilding>();

		[XmlArray("Items"), XmlArrayItem("Item")]
		public List<ushort> Items = new List<ushort>();

		[XmlArray("Points"), XmlArrayItem("Point")]
		public List<Vector3> Points = new List<Vector3>();


	}

}
