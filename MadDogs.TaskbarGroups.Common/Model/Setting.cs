// Copyright © Mad-Dogs. All rights reserved.

namespace MadDogs.TaskbarGroups.Common.Model
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    internal class Setting
    {
        [XmlElement]
        public bool PortableMode { get; set; }
    }
}