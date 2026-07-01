using Atlas.Backend.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Core.Entities
{
    public class Service
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Category { get; set; }
        public string? Version { get; set; }
        public string? ProtocolPort { get; set; }
        public required Env Env { get; set; }
        public Status? Status { get; set; }
        public string? Description { get; set; }
    }
}
