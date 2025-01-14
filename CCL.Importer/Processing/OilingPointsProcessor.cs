﻿using CCL.Types.Components;
using DV.Simulation.Ports;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Util;
using LocoSim.Definitions;
using System.ComponentModel.Composition;
using UnityEngine;
using VLB;

namespace CCL.Importer.Processing
{
    [Export(typeof(IModelProcessorStep))]
    [RequiresStep(typeof(ProxyScriptProcessor))]
    internal class OilingPointsProcessor : ModelProcessorStep
    {
        private static GameObject? s_oilingPoint;
        private static GameObject OilingPoint = Extensions.GetCached(ref s_oilingPoint, () =>
            TrainCarType.LocoSteamHeavy.ToV2().externalInteractablesPrefab.transform.Find("ManualOilingPoints/OilingPoint0").gameObject);

        public override void ExecuteStep(ModelProcessor context)
        {
            var def = context.Car.prefab.GetComponentInChildren<ManualOilingPointsDefinition>();

            if (def == null) return;

            if (context.Car.externalInteractablesPrefab != null) ProcessPrefab(context.Car.externalInteractablesPrefab, def);
            if (context.Car.explodedExternalInteractablesPrefab != null) ProcessPrefab(context.Car.explodedExternalInteractablesPrefab, def);
        }

        private void ProcessPrefab(GameObject prefab, ManualOilingPointsDefinition definition)
        {
            var oilingPoints = prefab.GetComponentsInChildren<ManualOilingPoint>();

            if (oilingPoints.Length == 0) return;

            for (int i = 0; i < oilingPoints.Length; i++)
            {
                var dummy = oilingPoints[i].transform;
                var point = Object.Instantiate(OilingPoint, dummy.parent);

                point.transform.localPosition = dummy.localPosition;
                point.transform.localRotation = dummy.localRotation;
                point.transform.localScale = dummy.localScale;
                point.name = dummy.name;

                point.GetComponent<PositionSyncConsumer>().syncTag = oilingPoints[i].SyncTag;
                point.GetComponentInChildren<InteractablePortFeeder>().portId = $"{definition.ID}.POINT_DOOR_EXT_IN_{i}";
                var oilPort = point.GetComponentInChildren<OilingPointPortFeederReader>();
                oilPort.refillPortId = $"{definition.ID}.REFILL_EXT_IN_{i}";
                oilPort.refillingFlowNormalizedPortId = $"{definition.ID}.REFILLING_FLOW_NORMALIZED_{i}";
                point.GetComponentInChildren<IndicatorPortReader>().portId = $"{definition.ID}.OIL_LEVEL_NORMALIZED_{i}";
                point.GetComponentInChildren<LayeredAudioPortReader>().portId = $"{definition.ID}.REFILLING_FLOW_NORMALIZED_{i}";

                Object.Destroy(dummy.gameObject);
            }
        }
    }
}