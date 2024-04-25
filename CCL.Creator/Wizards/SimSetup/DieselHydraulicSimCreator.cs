﻿using CCL.Types;
using CCL.Types.Proxies;
using CCL.Types.Proxies.Controllers;
using CCL.Types.Proxies.Controls;
using CCL.Types.Proxies.Ports;
using CCL.Types.Proxies.Resources;
using CCL.Types.Proxies.Simulation;
using CCL.Types.Proxies.Simulation.Diesel;
using CCL.Types.Proxies.Simulation.Electric;
using CCL.Types.Proxies.Wheels;
using UnityEngine;

using static CCL.Types.Proxies.Controls.ControlBlockerProxy.BlockerDefinition;

namespace CCL.Creator.Wizards.SimSetup
{
    internal class DieselHydraulicSimCreator : SimCreator
    {
        public DieselHydraulicSimCreator(GameObject prefabRoot) : base(prefabRoot) { }

        public override string[] SimBasisOptions => new[] { "DH4" };

        public override void CreateSimForBasisImpl(int basisIndex)
        {
            var throttle = CreateOverridableControl(OverridableControlType.Throttle);
            var thrtPowr = CreateSimComponent<ThrottleGammaPowerConversionDefinitionProxy>("throttlePower");
            var reverser = CreateReverserControl();
            var trnBrake = CreateOverridableControl(OverridableControlType.TrainBrake);
            var indBrake = CreateOverridableControl(OverridableControlType.IndBrake);
            var dynBrake = CreateOverridableControl(OverridableControlType.DynamicBrake, "hydroDynamicBrake");

            var genericHornControl = CreateSimComponent<GenericControlDefinitionProxy>("hornControl");
            genericHornControl.defaultValue = 0;
            genericHornControl.smoothTime = 0.2f;
            var hornControl = CreateSibling<HornControlProxy>(genericHornControl);
            hornControl.portId = FullPortId(genericHornControl, "EXT_IN");
            hornControl.neutralAt0 = true;
            var horn = CreateSimComponent<HornDefinitionProxy>("horn");
            horn.controlNeutralAt0 = true;

            var bellControl = CreateExternalControl("bellControl", true);
            var bell = CreateSimComponent<ElectricBellDefinitionProxy>("bell");
            bell.smoothDownTime = 0.05f;

            // Headlights.

            var fuel = CreateResourceContainer(ResourceContainerType.Fuel);
            var oil = CreateResourceContainer(ResourceContainerType.Oil);
            var sand = CreateResourceContainer(ResourceContainerType.Sand);
            var sander = CreateSanderControl();

            var engine = CreateSimComponent<DieselEngineDirectDefinitionProxy>("de");
            var engineOff = CreateSibling<OverridableControlProxy>(engine);
            engineOff.portId = FullPortId(engine, "EMERGENCY_ENGINE_OFF_EXT_IN");
            var engineOn = CreateSibling<EngineOnReaderProxy>(engine);
            engineOn.portId = FullPortId(engine, "ENGINE_ON");
            var environmentDamage = CreateSibling<EnvironmentDamagerProxy>(engine);
            environmentDamage.damagerPortId = FullPortId(engine, "FUEL_ENV_DAMAGE_METER");
            environmentDamage.environmentDamageResource = BaseResourceType.EnvironmentDamageFuel;
            var engineExplosion = CreateSibling<ExplosionActivationOnSignalProxy>(engine);
            engineExplosion.explosion = ExplosionPrefab.ExplosionMechanical;
            engineExplosion.bodyDamagePercentage = 0.1f;
            engineExplosion.explosionSignalPortId = FullPortId(engine, "IS_BROKEN");

            var loadTorque = CreateSimComponent<ConfigurableAddDefinitionProxy>("loadTorqueCalculator");
            loadTorque.aReader = new PortReferenceDefinition(DVPortValueType.TORQUE, "LOAD_TORQUE_0");
            loadTorque.bReader = new PortReferenceDefinition(DVPortValueType.TORQUE, "LOAD_TORQUE_1");
            loadTorque.addReadOut = new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.TORQUE, "LOAD_TORQUE_TOTAL");
            loadTorque.transform.parent = engine.transform;

            var compressor = CreateSimComponent<MechanicalCompressorDefinitionProxy>("compressor");
            var airController = CreateCompressorSim(compressor);

            var fluidCoupler = CreateSimComponent<HydraulicTransmissionDefinitionProxy>("fluidCoupler");
            var couplerExplosion = CreateSibling<ExplosionActivationOnSignalProxy>(fluidCoupler);
            couplerExplosion.explosion = ExplosionPrefab.ExplosionHydraulic;
            couplerExplosion.bodyDamagePercentage = 0.1f;
            couplerExplosion.windowsBreakingDelay = 0.4f;
            couplerExplosion.explosionSignalPortId = FullPortId(fluidCoupler, "IS_BROKEN");

            var cooler = CreateSimComponent<PassiveCoolerDefinitionProxy>("fcCooler");
            cooler.transform.parent = fluidCoupler.transform;
            var autoCooler = CreateSimComponent<AutomaticCoolerDefinitionProxy>("fcAutomaticCooler");
            autoCooler.transform.parent = fluidCoupler.transform;
            var heat = CreateSimComponent<HeatReservoirDefinitionProxy>("coolant");
            heat.transform.parent = fluidCoupler.transform;
            heat.inputCount = 4;
            heat.OnValidate();

            var poweredAxles = CreateSimComponent<ConstantPortDefinitionProxy>("poweredAxles");
            poweredAxles.value = 4;
            poweredAxles.port = new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.GENERIC, "NUM");

            var transmission = CreateSimComponent<TransmissionFixedGearDefinitionProxy>("transmission");
            var traction = CreateSimComponent<TractionDefinitionProxy>("traction");
            var tractionFeeders = CreateTractionFeeders(traction);
            var wheelslip = CreateSibling<WheelslipControllerProxy>(traction);
            wheelslip.numberOfPoweredAxlesPortId = FullPortId(poweredAxles, "NUM");
            wheelslip.sandCoefPortId = FullPortId(sander, "SAND_COEF");
            wheelslip.engineBrakingActivePortId = FullPortId(fluidCoupler, "HYDRO_DYNAMIC_BRAKE_EFFECT");
            var directWheelslip = CreateSibling<DirectDriveMaxWheelslipRpmCalculatorProxy>(traction);
            directWheelslip.engineRpmMaxPortId = FullPortId(engine, "RPM");
            directWheelslip.gearRatioPortId = FullPortId(fluidCoupler, "GEAR_RATIO");

            var fusebox = CreateSimComponent<IndependentFusesDefinitionProxy>("fusebox");
            fusebox.fuses = new[]
            {
                new FuseDefinition("ELECTRONICS_MAIN", false),
                new FuseDefinition("ENGINE_STARTER", false)
            };

            horn.powerFuseId = FullPortId(fusebox, "ELECTRONICS_MAIN");
            bell.powerFuseId = FullPortId(fusebox, "ELECTRONICS_MAIN");
            sander.powerFuseId = FullPortId(fusebox, "ELECTRONICS_MAIN");
            engine.engineStarterFuseId = FullPortId(fusebox, "ENGINE_STARTER");
            autoCooler.powerFuseId = FullPortId(fusebox, "ELECTRONICS_MAIN");

            _damageController.mechanicalPTDamagerPortIds = new[]
            {
                FullPortId(engine, "GENERATED_ENGINE_DAMAGE"),
                FullPortId(fluidCoupler, "GENERATED_DAMAGE")
            };
            _damageController.mechanicalPTHealthStateExternalInPortIds = new[]
            {
                FullPortId(engine, "ENGINE_HEALTH_STATE_EXT_IN"),
                FullPortId(fluidCoupler, "MECHANICAL_PT_HEALTH_EXT_IN")
            };
            _damageController.mechanicalPTOffExternalInPortIds = new[] { FullPortId(engine, "COLLISION_ENGINE_OFF_EXT_IN") };

            ConnectPorts(fluidCoupler, "OUTPUT_SHAFT_TORQUE", traction, "TORQUE_IN");

            ApplyMethodToAll<IDH4Defaults>(s => s.ApplyDH4Defaults());

            AddControlBlocker(throttle, dynBrake, "EXT_IN", 0, BlockType.BLOCK_ON_ABOVE_THRESHOLD)
                .blockedControlPortId = FullPortId(throttle, "EXT_IN");

            AddControlBlocker(reverser, throttle, "EXT_IN", 0, BlockType.BLOCK_ON_ABOVE_THRESHOLD);
            AddControlBlocker(reverser, dynBrake, "EXT_IN", 0, BlockType.BLOCK_ON_ABOVE_THRESHOLD);
            AddControlBlocker(reverser, traction, "WHEEL_RPM_EXT_IN", 40, BlockType.BLOCK_ON_ABOVE_THRESHOLD);
            AddControlBlocker(reverser, traction, "WHEEL_RPM_EXT_IN", -40, BlockType.BLOCK_ON_BELOW_THRESHOLD)
                .blockedControlPortId = FullPortId(reverser, "CONTROL_EXT_IN");

            AddControlBlocker(dynBrake, throttle, "EXT_IN", 0, BlockType.BLOCK_ON_ABOVE_THRESHOLD);
            AddControlBlocker(dynBrake, reverser, "REVERSER", 0, BlockType.BLOCK_ON_EQUAL_TO_THRESHOLD)
                .blockedControlPortId = FullPortId(dynBrake, "EXT_IN");
        }
    }
}
