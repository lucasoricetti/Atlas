using Atlas.Backend.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Core.Entities
{
    public class AcnMacroArea
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required AcnCategoryOfRelevance PreAssignedAcnCategory { get; set; }
        public AcnCategoryOfRelevance? CustomAcnCategory { get; set; }
    }
}
