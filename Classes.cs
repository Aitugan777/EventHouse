using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Aituk.Plugin
{
	public class AitukBuilding
	{
		[XmlElement("Position")]
		public Vector3 Position;

		[XmlAttribute("Rotation")]
		public int Rotation_Y;

		[XmlAttribute("ID")]
		public ushort ID;
	}
	
	public class AitukEvent
    {
		public Vector3 Position;

		public int SecOnDestroy;
		public int SecOnOpenLocker;
		public bool StartedUnlock;
	}

	public class AitukEventCfg
	{
		[XmlAttribute("SecOnOpenLocker")]
		public int SecOnOpenLocker;
	}

}
