﻿using AutoMapper;
using CCL.Types.Proxies.Simulation.Steam;
using LocoSim.Definitions;

namespace CCL.Importer.Proxies.Simulation
{
    internal class SteamSimulationReplacer : Profile
    {
        public SteamSimulationReplacer()
        {
            CreateMap<FireboxDefinitionProxy, FireboxDefinition>().AutoCacheAndMap();
            CreateMap<BoilerDefinitionProxy, BoilerDefinition>().AutoCacheAndMap();
            CreateMap<SteamCompressorDefinitionProxy, SteamCompressorDefinition>().AutoCacheAndMap();
            CreateMap<DynamoDefinitionProxy, DynamoDefinition>().AutoCacheAndMap();
            CreateMap<MechanicalLubricatorDefinitionProxy, MechanicalLubricatorDefinition>().AutoCacheAndMap();
        }
    }
}
