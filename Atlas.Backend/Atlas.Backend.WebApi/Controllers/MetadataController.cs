using Atlas.Backend.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Atlas.Backend.WebApi.Controllers
{
    [ApiController]
    [Route("api/metadata")]
    public class MetadataController : ControllerBase
    {
        [HttpGet("enums")]
        [Authorize(Policy = "ReaderPolicy")]
        public IActionResult GetEnums()
        {
            var metadata = new
            {
                assetTypes = Enum.GetNames(typeof(Core.Enums.AssetType)).ToList(),
                contractTypes = Enum.GetNames(typeof(Core.Enums.ContractType)).ToList(),
                criticalities = Enum.GetNames(typeof(Core.Enums.Criticality)).ToList(),
                envs = Enum.GetNames(typeof(Core.Enums.Env)).ToList(),
                statuses = Enum.GetNames(typeof(Core.Enums.Status)).ToList(),
                cloudProviderTypes = Enum.GetNames(typeof(Core.Enums.CloudProviderType)).ToList(),
                virtualMachineTypes = Enum.GetNames(typeof(Core.Enums.VirtualMachineType)).ToList(),
                acnCategoryOfRelevances = Enum.GetNames(typeof(Core.Enums.AcnCategoryOfRelevance)).ToList()
            };

            return Ok(metadata);
        }
    }
}
