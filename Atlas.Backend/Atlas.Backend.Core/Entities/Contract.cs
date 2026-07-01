using System;
using System.Collections.Generic;
using Atlas.Backend.Core.Enums;

namespace Atlas.Backend.Core.Entities
{
    public class Contract
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public List<ContractType> ContractTypes { get; set; } = new();
        public int? Sla { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Notes { get; set; }
    }
}
