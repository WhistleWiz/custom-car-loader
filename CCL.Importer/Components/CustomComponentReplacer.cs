﻿using AutoMapper;
using CCL.Importer.Components.Controllers;
using CCL.Importer.Components.Headlights;
using CCL.Importer.Components.Indicators;
using CCL.Importer.Components.MultipleUnit;
using CCL.Importer.Components.Simulation;
using CCL.Types.Components;
using CCL.Types.Components.Controllers;
using CCL.Types.Components.Headlights;
using CCL.Types.Components.Indicators;
using CCL.Types.Components.MultipleUnit;
using CCL.Types.Components.Simulation;

namespace CCL.Importer.Components
{
    internal class CustomComponentReplacer : Profile
    {
        public CustomComponentReplacer()
        {
            MapCoupling();
            MapHeadlights();
            MapIndicators();
            MapSimulation();
            MapMultipleUnit();
            MapControllers();

            CreateMap<ControlNameTMPDisplay, ControlNameTMPDisplayInternal>().AutoCacheAndMap();
            CreateMap<HideObjectsOnCargoLoad, HideObjectsOnCargoLoadInternal>().AutoCacheAndMap();
            CreateMap<CoupledAttachment, CoupledAttachmentInternal>().AutoCacheAndMap();

            CreateMap<SimPortPlotter, SimPortPlotterInternal>().AutoCacheAndMap();
        }

        private void MapCoupling()
        {
            CreateMap<CarAutoCoupler, CarAutoCouplerInternal>().AutoCacheAndMap();
            CreateMap<RigidCoupler, RigidCouplerInternal>().AutoCacheAndMap();
            CreateMap<VirtualHandbrakeOverrider, VirtualHandbrakeOverriderInternal>().AutoCacheAndMap();
            CreateMap<DuplicateHandbrakeOverrider, DuplicateHandbrakeOverriderInternal>().AutoCacheAndMap();
        }

        private void MapHeadlights()
        {
            CreateMap<FrontConnectedDualCarAutomaticHeadlightsController, FrontConnectedDualCarAutomaticHeadlightsControllerInternal>().AutoCacheAndMap();
            CreateMap<FrontAndRearConnectedTriCarAutomaticHeadlightsController, FrontAndRearConnectedTriCarAutomaticHeadlightsControllerInternal>().AutoCacheAndMap();
            CreateMap<NoCableHeadlightsController, NoCableHeadlightsControllerInternal>().AutoCacheAndMap();
        }

        private void MapIndicators()
        {
            CreateMap<IndicatorShaderCustomValue, IndicatorShaderCustomValueInternal>().AutoCacheAndMap();
            CreateMap<IndicatorTMP, IndicatorTMPInternal>().AutoCacheAndMap();
        }

        private void MapSimulation()
        {
            CreateMap<TickingOutputDefinition, TickingOutputDefinitionInternal>().AutoCacheAndMap();
            CreateMap<FuseInverterDefinition, FuseInverterDefinitionInternal>().AutoCacheAndMap()
                .AfterMap(FuseInverterAfter);
            CreateMap<CombinedThrottleDynamicBrakeDefinition, CombinedThrottleDynamicBrakeDefinitionInternal>().AutoCacheAndMap();
            CreateMap<TimeReaderDefinition, TimeReaderDefinitionInternal>().AutoCacheAndMap();
            CreateMap<SteamAutostokerDefinition, SteamAutostokerDefinitionInternal>().AutoCacheAndMap();
        }

        private void MapMultipleUnit()
        {
            CreateMap<MultipleUnitCombinedThrottleDynamicBrakeMode, MultipleUnitCombinedThrottleDynamicBrakeModeInternal>().AutoCacheAndMap();
        }

        private void MapControllers()
        {
            CreateMap<ResourceSharerController, ResourceSharerControllerInternal>().AutoCacheAndMap();
        }

        private void FuseInverterAfter(FuseInverterDefinition fake, FuseInverterDefinitionInternal real)
        {
            int length = fake.FusesToInvert.Length;
            real.SourceFuses = new string[length];
            real.InvertedFuses = new string[length];

            for (int i = 0; i < length; i++)
            {
                real.SourceFuses[i] = fake.FusesToInvert[i].SourceFuseId;
                real.InvertedFuses[i] = fake.FusesToInvert[i].InvertedFuseId;
            }
        }
    }
}
