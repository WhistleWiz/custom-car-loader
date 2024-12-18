﻿using CCL.Types;
using CCL.Types.Proxies;
using CCL.Types.Proxies.Controllers;
using CCL.Types.Proxies.Controls;
using CCL.Types.Proxies.Ports;
using CCL.Types.Proxies.Resources;
using CCL.Types.Proxies.Simulation;
using CCL.Types.Proxies.Simulation.Steam;
using CCL.Types.Proxies.Wheels;
using UnityEngine;

using static CCL.Types.Proxies.Controls.BaseControlsOverriderProxy;
using static CCL.Types.Proxies.Ports.ConfigurablePortsDefinitionProxy;

namespace CCL.Creator.Wizards.SimSetup
{
    internal class SteamerSimCreator : SimCreator
    {
        // TODO:
        // Headlights

        public SteamerSimCreator(GameObject prefabRoot) : base(prefabRoot) { }

        public override string[] SimBasisOptions => new[] { "S060", "S282" };

        public override void CreateSimForBasisImpl(int basisIndex)
        {
            // Simulation components.
            var trnBrake = CreateOverridableControl(OverridableControlType.TrainBrake);
            var brakeCut = CreateOverridableControl(OverridableControlType.TrainBrakeCutout);
            var indBrake = CreateOverridableControl(OverridableControlType.IndBrake);

            // Lights.

            var blower = CreateExternalControl("blower");
            var whistle = CreateExternalControl("whistle");
            var hornControl = CreateSibling<HornControlProxy>(whistle);
            hornControl.portId = FullPortId(whistle, "EXT_IN");
            hornControl.neutralAt0 = true;
            var damper = CreateExternalControl("damper", true, 1.0f);
            var cylCock = CreateExternalControl("cylinderCock", true);
            var fireDoor = CreateExternalControl("fireboxDoor", true);
            var injector = CreateExternalControl("injector", true);
            var blowdown = CreateExternalControl("blowdown", true);

            var reverser = CreateReverserControl(isAnalog: true);
            var throttle = CreateOverridableControl(OverridableControlType.Throttle);

            var poweredAxles = CreateSimComponent<ConstantPortDefinitionProxy>("poweredAxles");
            poweredAxles.value = PoweredAxleCount(basisIndex);
            poweredAxles.port = new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.GENERIC, "NUM");

            var sand = CreateResourceContainer(ResourceContainerType.Sand);
            var sander = CreateSanderControl();

            var waterDetector = CreateSimComponent<WaterDetectorDefinitionProxy>("waterDetector");
            var waterPortFeeder = CreateSibling<WaterDetectorPortFeederProxy>(waterDetector);
            waterPortFeeder.statePortId = FullPortId(waterDetector, "STATE_EXT_IN");

            var coalDump = CreateExternalControl("coalDumpControl", true);
            coalDump.defaultValue = 0.5f;

            var firebox = CreateSimComponent<FireboxDefinitionProxy>("firebox");
            var fireboxSimController = CreateSibling<FireboxSimControllerProxy>(firebox);
            fireboxSimController.ConnectFirebox(firebox);
            fireboxSimController.fireboxDoorPortId = FullPortId(fireDoor, "EXT_IN");
            var engineOn = CreateSibling<EngineOnReaderProxy>(firebox);
            engineOn.portId = FullPortId(firebox, "FIRE_ON");
            var engineOff = CreateSibling<PowerOffControlProxy>(firebox);
            engineOff.portId = FullPortId(firebox, "EXTINGUISH_EXT_IN");
            engineOff.signalClearedBySim = true;
            var environmentDamage = CreateSibling<EnvironmentDamagerProxy>(firebox);
            environmentDamage.damagerPortId = FullPortId(firebox, "COAL_ENV_DAMAGE_METER");
            environmentDamage.environmentDamageResource = BaseResourceType.EnvironmentDamageCoal;
            var fireMass = CreateSibling<ResourceMassPortReaderProxy>(firebox);
            fireMass.resourceMassPortId = FullPortId(firebox, "COAL_LEVEL");

            var steamCalc = CreateSimComponent<MultiplePortSumDefinitionProxy>("steamConsumptionCalculator");
            steamCalc.inputs = new[]
            {
                new PortReferenceDefinition(DVPortValueType.MASS_RATE, "EXHAUST"),
                new PortReferenceDefinition(DVPortValueType.MASS_RATE, "COMPRESSOR"),
                new PortReferenceDefinition(DVPortValueType.MASS_RATE, "ENGINE"),
                new PortReferenceDefinition(DVPortValueType.MASS_RATE, "DYNAMO"),
                new PortReferenceDefinition(DVPortValueType.MASS_RATE, "BELL"),
            };
            steamCalc.output = new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.MASS_RATE, "OUT");

            var boiler = CreateSimComponent<BoilerDefinitionProxy>("boiler");
            var boilerSimController = CreateSibling<BoilerSimControllerProxy>(boiler);
            boilerSimController.anglePortId = FullPortId(boiler, "BOILER_ANGLE_EXT_IN");
            var explosion = CreateSibling<ExplosionActivationOnSignalProxy>(boiler);
            explosion.bodyDamagePercentage = 1.0f;
            explosion.wheelsDamagePercentage = 1.0f;
            explosion.mechanicalPTDamagePercentage = 1.0f;
            explosion.electricalPTDamagePercentage = 1.0f;
            explosion.explosion = ExplosionPrefab.Boiler;
            explosion.explosionSignalPortId = FullPortId(boiler, "IS_BROKEN");
            explosion.explodeTrainCar = true;
            var boilerMass = CreateSibling<ResourceMassPortReaderProxy>(boiler);
            boilerMass.resourceMassPortId = FullPortId(boiler, "WATER_MASS");

            var compressorControl = CreateExternalControl("compressorControl", true);
            var compressor = CreateSimComponent<SteamCompressorDefinitionProxy>("compressor");
            var airController = CreateCompressorSim(compressor);
            airController.mainResPressureNormalizedPortId = FullPortId(compressor, "MAIN_RES_PRESSURE_NORMALIZED");

            var dynamoControl = CreateExternalControl("dynamoControl", true);
            var dynamo = CreateSimComponent<DynamoDefinitionProxy>("dynamo");

            var fuseController = CreateSimComponent<FuseControllerDefinitionProxy>("electronicsFuseController");
            fuseController.controllingPort = new PortReferenceDefinition(DVPortValueType.STATE, "CONTROLLING_PORT");

            var oil = CreateResourceContainer(ResourceContainerType.Oil);
            var lubricatorControl = CreateExternalControl("lubricatorControl", true);
            var lubricatorSmoothing = CreateSibling<SmoothedOutputDefinitionProxy>(lubricatorControl, "lubricatorControlSmoothing");
            lubricatorSmoothing.smoothTime = 0.2f;
            var lubricator = CreateSimComponent<MechanicalLubricatorDefinitionProxy>("lubricator");

            var oilingPoints = CreateSimComponent<ManualOilingPointsDefinitionProxy>("oilingPoints");

            var bellControl = CreateExternalControl("bellControl", true);
            var bell = CreateSimComponent<SteamBellDefinitionProxy>("bell");

            var steamEngine = CreateSimComponent<ReciprocatingSteamEngineDefinitionProxy>("steamEngine");
            var exhaust = CreateSimComponent<SteamExhaustDefinitionProxy>("exhaust");

            var traction = CreateSimComponent<TractionDefinitionProxy>("traction");
            var tractionFeeders = CreateTractionFeeders(traction);
            var wheelslip = CreateSibling<WheelslipControllerProxy>(traction);
            wheelslip.numberOfPoweredAxlesPortId = FullPortId(poweredAxles, "NUM");
            wheelslip.sandCoefPortId = FullPortId(sander, "SAND_COEF");

            // Handle tender and non tender steam engines.
            SimComponentDefinitionProxy water = null!;
            SimComponentDefinitionProxy coal = null!;

            if (HasTender(basisIndex))
            {
                var tenderWater = CreateSimComponent<ConfigurablePortsDefinitionProxy>("tenderWater");
                tenderWater.Ports = new[]
                {
                        new PortStartValue(new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.WATER, "NORMALIZED"), 0),
                        new PortStartValue(new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.WATER, "CAPACITY"), 30000),
                        new PortStartValue(new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.WATER, "AMOUNT"), 0),
                        new PortStartValue(new PortDefinition(DVPortType.EXTERNAL_IN, DVPortValueType.WATER, "CONSUME_EXT_IN"), 0)
                    };
                tenderWater.OnValidate();
                CreateBroadcastConsumer(tenderWater, "NORMALIZED", DVPortForwardConnectionType.COUPLED_REAR, "TENDER_WATER_NORMALIZED", 0, false);
                CreateBroadcastConsumer(tenderWater, "CAPACITY", DVPortForwardConnectionType.COUPLED_REAR, "TENDER_WATER_CAPACITY", 1, false);
                CreateBroadcastConsumer(tenderWater, "AMOUNT", DVPortForwardConnectionType.COUPLED_REAR, "TENDER_WATER_AMOUNT", 0, false);
                CreateBroadcastProvider(tenderWater, "CONSUME_EXT_IN", DVPortForwardConnectionType.COUPLED_REAR, "TENDER_WATER_CONSUME");

                var tenderCoal = CreateSimComponent<ConfigurablePortsDefinitionProxy>("tenderCoal");
                tenderCoal.Ports = new[]
                {
                        new PortStartValue(new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.COAL, "NORMALIZED"), 0),
                        new PortStartValue(new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.COAL, "CAPACITY"), 10000),
                    };
                tenderCoal.OnValidate();
                CreateBroadcastConsumer(tenderCoal, "NORMALIZED", DVPortForwardConnectionType.COUPLED_REAR, "TENDER_COAL_NORMALIZED", 0, false);
                CreateBroadcastConsumer(tenderCoal, "CAPACITY", DVPortForwardConnectionType.COUPLED_REAR, "TENDER_COAL_CAPACITY", 1, false);

                water = tenderWater;
                coal = tenderCoal;
            }
            else
            {
                var locoWater = CreateResourceContainer(ResourceContainerType.Water);
                var locoCoal = CreateResourceContainer(ResourceContainerType.Coal);
                CreateCoalPile(locoCoal);

                water = locoWater;
                coal = locoCoal;
            }

            // Fusebox and fuse connections.
            var fusebox = CreateSimComponent<IndependentFusesDefinitionProxy>("fuseboxDummy");
            fusebox.fuses = new[]
            {
                new FuseDefinition("ELECTRONICS_MAIN", false)
            };

            fuseController.fuseId = FullPortId(fusebox, "ELECTRONICS_MAIN");

            switch (basisIndex)
            {
                case 1:
                    _baseControls.propagateNeutralStateToRear = true;
                    CreateBroadcastProvider(dynamo, "DYNAMO_FLOW_NORMALIZED", DVPortForwardConnectionType.COUPLED_REAR, "DYNAMO_FLOW");
                    break;
                default:
                    break;
            }

            // Neutral states.
            _baseControls.neutralStateSetters = new[]
            {
                new PortSetter(FullPortId(injector, "EXT_IN"), 0),
                new PortSetter(FullPortId(blower, "EXT_IN"), 0),
                new PortSetter(FullPortId(blowdown, "EXT_IN"), 0),
                new PortSetter(FullPortId(bellControl, "EXT_IN"), 0),
                new PortSetter(FullPortId(lubricatorControl, "EXT_IN"), 0),
                new PortSetter(FullPortId(dynamoControl, "EXT_IN"), 0),
                new PortSetter(FullPortId(compressorControl, "EXT_IN"), 0)
            };

            // Damage.
            _damageController.bodyHealthStateExternalInPortIds = new[]
            {
                FullPortId(boiler, "BODY_HEALTH_EXT_IN")
            };

            _damageController.mechanicalPTDamagerPortIds = new[]
            {
                FullPortId(steamEngine, "GENERATED_MECHANICAL_DAMAGE"),
                FullPortId(oilingPoints, "MECHANICAL_DAMAGE")
            };
            _damageController.mechanicalPTHealthStateExternalInPortIds = new[]
            {
                FullPortId(steamEngine, "HEALTH_STATE_EXT_IN"),
                FullPortId(lubricator, "MECHANICAL_PT_HEALTH_EXT_IN"),
                FullPortId(oilingPoints, "MECHANICAL_PT_HEALTH_EXT_IN")
            };

            // Port connections.
            ConnectPorts(steamEngine, "TORQUE_OUT", traction, "TORQUE_IN");

            // Port reference connections.
            ConnectPortRef(lubricatorControl, "EXT_IN", lubricatorSmoothing, "CONTROL");
            ConnectPortRef(oil, "AMOUNT", lubricator, "OIL");
            ConnectPortRef(oil, "CONSUME_EXT_IN", lubricator, "OIL_CONSUMPTION");
            ConnectPortRef(lubricatorSmoothing, "OUTPUT", lubricator, "MANUAL_FILL_RATE_NORMALIZED");
            ConnectPortRef(traction, "WHEEL_RPM_EXT_IN", lubricator, "WHEEL_RPM");

            ConnectPortRef(oil, "AMOUNT", oilingPoints, "OIL_STORAGE");
            ConnectPortRef(oil, "CONSUME_EXT_IN", oilingPoints, "OIL_CONSUMPTION");
            ConnectPortRef(traction, "WHEEL_RPM_EXT_IN", oilingPoints, "WHEEL_RPM");

            ConnectPortRef(bellControl, "EXT_IN", bell, "CONTROL");
            ConnectPortRef(boiler, "PRESSURE", bell, "STEAM_PRESSURE");

            ConnectPortRef(coalDump, "EXT_IN", firebox, "COAL_DUMP_CONTROL");
            ConnectPortRef(waterDetector, "STATE_EXT_IN", firebox, "INTAKE_WATER_CONTENT");
            ConnectPortRef(exhaust, "AIR_FLOW", firebox, "AIR_FLOW");
            ConnectPortRef(traction, "FORWARD_SPEED_EXT_IN", firebox, "FORWARD_SPEED");
            ConnectPortRef(boiler, "PRESSURE", firebox, "BOILER_PRESSURE");
            ConnectPortRef(boiler, "TEMPERATURE", firebox, "BOILER_TEMPERATURE");
            ConnectPortRef(boiler, "IS_BROKEN", firebox, "BOILER_BROKEN_STATE");

            ConnectPortRef(exhaust, "STEAM_CONSUMPTION", steamCalc, "EXHAUST");
            ConnectPortRef(compressor, "STEAM_CONSUMPTION", steamCalc, "COMPRESSOR");
            ConnectPortRef(steamEngine, "INTAKE_FLOW", steamCalc, "ENGINE");
            ConnectPortRef(dynamo, "STEAM_CONSUMPTION", steamCalc, "DYNAMO");
            ConnectPortRef(bell, "STEAM_CONSUMPTION", steamCalc, "BELL");

            ConnectPortRef(sand, "AMOUNT", sander, "SAND");
            ConnectPortRef(sand, "CONSUME_EXT_IN", sander, "SAND_CONSUMPTION");

            ConnectPortRef(injector, "EXT_IN", boiler, "INJECTOR");
            ConnectPortRef(blowdown, "EXT_IN", boiler, "BLOWDOWN");
            ConnectPortRef(firebox, "HEAT", boiler, "HEAT");
            ConnectPortRef(firebox, "TEMPERATURE", boiler, "FIREBOX_TEMPERATURE");
            switch (basisIndex)
            {
                case 1:
                    ConnectPortRef(steamEngine, "EXHAUST_TEMPERATURE", boiler, "FEEDWATER_TEMPERATURE");
                    break;
                default:
                    ConnectPortRef("-EMPTY-", FullPortId(boiler, "FEEDWATER_TEMPERATURE"));
                    break;
            }
            ConnectPortRef(steamCalc, "OUT", boiler, "STEAM_CONSUMPTION");
            ConnectPortRef(water, "AMOUNT", boiler, "WATER");
            ConnectPortRef(water, "CONSUME_EXT_IN", boiler, "WATER_CONSUMPTION");

            ConnectPortRef(compressorControl, "EXT_IN", compressor, "COMPRESSOR_CONTROL");
            ConnectPortRef(boiler, "PRESSURE", compressor, "STEAM_PRESSURE");
            ConnectPortRef(dynamoControl, "EXT_IN", dynamo, "CONTROL");
            ConnectPortRef(boiler, "PRESSURE", dynamo, "STEAM_PRESSURE");

            ConnectPortRef(dynamo, "DYNAMO_FLOW_NORMALIZED", fuseController, "CONTROLLING_PORT");

            ConnectPortRef(throttle, "EXT_IN", steamEngine, "THROTTLE_CONTROL");
            ConnectPortRef(reverser, "REVERSER", steamEngine, "REVERSER_CONTROL");
            ConnectPortRef(cylCock, "EXT_IN", steamEngine, "CYLINDER_COCK_CONTROL");
            ConnectPortRef(boiler, "PRESSURE", steamEngine, "INTAKE_PRESSURE");
            if (HasSuperheater(basisIndex))
            {
                ConnectPortRef(firebox, "TEMPERATURE", steamEngine, "INTAKE_TEMPERATURE");
            }
            else
            {
                ConnectPortRef(boiler, "TEMPERATURE", steamEngine, "INTAKE_TEMPERATURE");
            }
            ConnectPortRef(boiler, "OUTLET_STEAM_QUALITY", steamEngine, "INTAKE_QUALITY");
            ConnectPortRef(traction, "WHEEL_RPM_EXT_IN", steamEngine, "CRANK_RPM");
            ConnectPortRef(lubricator, "LUBRICATION_NORMALIZED", steamEngine, "LUBRICATION_NORMALIZED");

            ConnectPortRef(steamEngine, "EXHAUST_FLOW", exhaust, "EXHAUST_FLOW");
            ConnectPortRef(steamEngine, "MAX_FLOW", exhaust, "ENGINE_MAX_FLOW");
            ConnectPortRef(boiler, "PRESSURE", exhaust, "BOILER_PRESSURE");
            ConnectPortRef(blower, "EXT_IN", exhaust, "BLOWER_CONTROL");
            ConnectPortRef(whistle, "EXT_IN", exhaust, "WHISTLE_CONTROL");
            ConnectPortRef(damper, "EXT_IN", exhaust, "DAMPER_CONTROL");

            // Apply defaults.
            switch (basisIndex)
            {
                case 0:
                    ApplyMethodToAll<IS060Defaults>(s => s.ApplyS060Defaults());
                    break;
                case 1:
                    ApplyMethodToAll<IS282Defaults>(s => s.ApplyS282Defaults());
                    break;
                default:
                    break;
            }

            // Shovelling.
            if (!_root.TryGetComponent(out MagicShovellingProxy shovelling))
            {
                shovelling = _root.AddComponent<MagicShovellingProxy>();
            }
        }

        private static int PoweredAxleCount(int basis) => basis switch
        {
            0 => 3,
            1 => 4,
            _ => 0,
        };

        private static bool HasTender(int basis)
        {
            switch (basis)
            {
                case 1:
                    return true;
                default:
                    return false;
            }
        }

        private static bool HasSuperheater(int basis)
        {
            switch (basis)
            {
                case 1:
                    return true;
                default:
                    return false;
            }
        }
    }
}
