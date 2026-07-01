using Atlas.Backend.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Core.Entities
{
    public class Asset
    {
        public required string Id { get; set;  }
        public required string Name { get; set; }
        public required AssetType Type { get; set; }
        public string? Description { get; set; }
        public required Criticality Criticality { get; set; }
        public required bool Bia { get; set; }
        public int? RpoH { get; set; }
        public int? MtoH { get; set; }
    }
}
