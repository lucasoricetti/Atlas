using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Core.Entities
{
    public class Process
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
