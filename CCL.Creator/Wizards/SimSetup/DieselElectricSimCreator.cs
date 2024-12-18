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
    internal class DieselElectricSimCreator : SimCreator
    {
        // TODO:
        // Headlights

        public DieselElectricSimCreator(GameObject prefabRoot) : base(prefabRoot) { }

        public override string[] SimBasisOptions => new[] { "DE2", "DE6" };

        public override void CreateSimForBasisImpl(int basisIndex)
        {
            var hasDynamic = HasDynamicBrake(basisIndex);
            var hasBell = HasBell(basisIndex);

            // Simulation components.
            var throttle = CreateOverridableControl(OverridableControlType.Throttle);
            var thrtPowr = CreateSimComponent<ThrottleGammaPowerConversionDefinitionProxy>("throttlePower");
            var reverser = CreateReverserControl();
            var trnBrake = CreateOverridableControl(OverridableControlType.TrainBrake);
            var brakeCut = CreateOverridableControl(OverridableControlType.TrainBrakeCutout);
            var indBrake = CreateOverridableControl(OverridableControlType.IndBrake);
            var dynBrake = hasDynamic ? CreateOverridableControl(OverridableControlType.DynamicBrake) : null!;

            // Headlights.

            var genericHornControl = CreateSimComponent<GenericControlDefinitionProxy>("hornControl");
            genericHornControl.defaultValue = 0;
            genericHornControl.smoothTime = 0.2f;
            var hornControl = CreateSibling<HornControlProxy>(genericHornControl);
            hornControl.portId = FullPortId(genericHornControl, "EXT_IN");
            hornControl.neutralAt0 = true;
            var horn = CreateSimComponent<HornDefinitionProxy>("horn");
            horn.controlNeutralAt0 = true;

            ExternalControlDefinitionProxy bellControl = null!;
            ElectricBellDefinitionProxy bell = null!;

            if (hasBell)
            {
                bellControl = CreateExternalControl("bellControl", true);
                bell = CreateSimComponent<ElectricBellDefinitionProxy>("bell");
                bell.smoothDownTime = 0.5f;
            }

            var fuel = CreateResourceContainer(ResourceContainerType.Fuel);
            var oil = CreateResourceContainer(ResourceContainerType.Oil);
            var sand = CreateResourceContainer(ResourceContainerType.Sand);
            var sander = CreateSanderControl();

            var engine = CreateSimComponent<DieselEngineDirectDefinitionProxy>("de");
            var engineOff = CreateSibling<PowerOffControlProxy>(engine);
            engineOff.portId = FullPortId(engine, "EMERGENCY_ENGINE_OFF_EXT_IN");
            var engineOn = CreateSibling<EngineOnReaderProxy>(engine);
            engineOn.portId = FullPortId(engine, "ENGINE_ON");
            var environmentDamage = CreateSibling<EnvironmentDamagerProxy>(engine);
            environmentDamage.damagerPortId = FullPortId(engine, "FUEL_ENV_DAMAGE_METER");
            environmentDamage.environmentDamageResource = BaseResourceType.EnvironmentDamageFuel;

            var loadTorque = CreateSimComponent<ConfigurableAddDefinitionProxy>("loadTorqueCalculator");
            loadTorque.aReader = new PortReferenceDefinition(DVPortValueType.TORQUE, "LOAD_TORQUE_0");
            loadTorque.bReader = new PortReferenceDefinition(DVPortValueType.TORQUE, "LOAD_TORQUE_1");
            loadTorque.addReadOut = new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.TORQUE, "LOAD_TORQUE_TOTAL");
            loadTorque.transform.parent = engine.transform;

            var compressor = CreateSimComponent<MechanicalCompressorDefinitionProxy>("compressor");
            var airController = CreateCompressorSim(compressor);

            var tractionGen = CreateSimComponent<TractionGeneratorDefinitionProxy>("tractionGenerator");
            var slugPowerCalc = CreateSimComponent<SlugsPowerCalculatorDefinitionProxy>("slugsPowerCalculator");
            slugPowerCalc.transform.parent = tractionGen.transform;
            var slugPowerProv = CreateSibling<SlugsPowerProviderModuleProxy>(slugPowerCalc);
            slugPowerProv.generatorVoltagePortId = FullPortId(tractionGen, "VOLTAGE");
            slugPowerProv.slugsEffectiveResistancePortId = FullPortId(slugPowerCalc, "EXTERNAL_EFFECTIVE_RESISTANCE_EXT_IN");
            slugPowerProv.slugsTotalAmpsPortId = FullPortId(slugPowerCalc, "EXTERNAL_AMPS_EXT_IN");

            var tm = CreateSimComponent<TractionMotorSetDefinitionProxy>("tm");
            var deadTMs = CreateSibling<DeadTractionMotorsControllerProxy>(tm);
            deadTMs.overheatFuseOffPortId = FullPortId(tm, "OVERHEAT_POWER_FUSE_OFF");
            var tmExplosion = CreateSibling<ExplosionActivationOnSignalProxy>(tm);
            tmExplosion.explosionSignalPortId = FullPortId(tm, "OVERSPEED_EXPLOSION_TRIGGER");
            tmExplosion.bodyDamagePercentage = 0.05f;
            tmExplosion.explosion = ExplosionPrefab.TMOverspeed;

            var cooler = CreateSimComponent<PassiveCoolerDefinitionProxy>("tmPassiveCooler");
            cooler.transform.parent = tm.transform;
            var heat = CreateSimComponent<HeatReservoirDefinitionProxy>("tmHeat");
            heat.transform.parent = tm.transform;
            heat.inputCount = 2;
            heat.OnValidate();

            var tmRpm = CreateSimComponent<ConfigurableMultiplierDefinitionProxy>("tmRpmCalculator");
            tmRpm.aReader = new PortReferenceDefinition(DVPortValueType.RPM, "WHEEL_RPM");
            tmRpm.bReader = new PortReferenceDefinition(DVPortValueType.GENERIC, "GEAR_RATIO");
            tmRpm.mulReadOut = new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.RPM, "TM_RPM");

            var transmission = CreateSimComponent<TransmissionFixedGearDefinitionProxy>("transmission");
            var traction = CreateSimComponent<TractionDefinitionProxy>("traction");
            var tractionFeeders = CreateTractionFeeders(traction);
            var wheelslip = CreateSibling<WheelslipControllerProxy>(traction);
            wheelslip.numberOfPoweredAxlesPortId = FullPortId(tm, "WORKING_TRACTION_MOTORS");
            wheelslip.sandCoefPortId = FullPortId(sander, "SAND_COEF");
            wheelslip.engineBrakingActivePortId = FullPortId(tm, "DYNAMIC_BRAKE_ACTIVE");

            // Fusebox and fuse connections.
            var fusebox = CreateSimComponent<IndependentFusesDefinitionProxy>("fusebox");
            fusebox.fuses = new[]
            {
                new FuseDefinition("ELECTRONICS_MAIN", false),
                new FuseDefinition("ENGINE_STARTER", false),
                new FuseDefinition("TM_POWER", false),
            };

            horn.powerFuseId = FullPortId(fusebox, "ELECTRONICS_MAIN");

            if (hasBell)
            {
                bell.powerFuseId = FullPortId(fusebox, "ELECTRONICS_MAIN");
            }

            sander.powerFuseId = FullPortId(fusebox, "ELECTRONICS_MAIN");
            engine.engineStarterFuseId = FullPortId(fusebox, "ENGINE_STARTER");
            tractionGen.powerFuseId = FullPortId(fusebox, "TM_POWER");
            tm.powerFuseId = FullPortId(fusebox, "TM_POWER");
            deadTMs.tmFuseId = FullPortId(fusebox, "TM_POWER");

            // Damage.
            _damageController.mechanicalPTDamagerPortIds = new[] { FullPortId(engine, "GENERATED_ENGINE_DAMAGE") };
            _damageController.mechanicalPTHealthStateExternalInPortIds = new[] { FullPortId(engine, "ENGINE_HEALTH_STATE_EXT_IN") };
            _damageController.mechanicalPTOffExternalInPortIds = new[] { FullPortId(engine, "COLLISION_ENGINE_OFF_EXT_IN") };

            _damageController.electricalPTDamagerPortIds = new[] { FullPortId(tm, "GENERATED_DAMAGE") };
            _damageController.electricalPTHealthStateExternalInPortIds = new[] { FullPortId(tm, "HEALTH_STATE_EXT_IN") };

            // Port connections.
            ConnectPorts(tm, "TORQUE_OUT", transmission, "TORQUE_IN");
            ConnectPorts(transmission, "TORQUE_OUT", traction, "TORQUE_IN");

            // Port reference connections.
            ConnectPortRef(throttle, "EXT_IN", thrtPowr, "THROTTLE");
            ConnectPortRef(engine, "IDLE_RPM_NORMALIZED", thrtPowr, "IDLE_RPM_NORMALIZED");
            ConnectPortRef(engine, "MAX_POWER_RPM_NORMALIZED", thrtPowr, "MAX_POWER_RPM_NORMALIZED");
            ConnectPortRef(engine, "MAX_POWER", thrtPowr, "MAX_POWER");

            ConnectPortRef(genericHornControl, "CONTROL", horn, "HORN_CONTROL");

            if (hasBell)
            {
                ConnectPortRef(bellControl, "EXT_IN", bell, "CONTROL");
            }

            ConnectPortRef(sand, "AMOUNT", sander, "SAND");
            ConnectPortRef(sand, "CONSUME_EXT_IN", sander, "SAND_CONSUMPTION");

            ConnectPortRef(tractionGen, "THROTTLE", engine, "THROTTLE");

            ConnectPortRef(fuel, "AMOUNT", engine, "FUEL");
            ConnectPortRef(fuel, "CONSUME_EXT_IN", engine, "FUEL_CONSUMPTION");
            ConnectPortRef(oil, "AMOUNT", engine, "OIL");
            ConnectPortRef(oil, "CONSUME_EXT_IN", engine, "OIL_CONSUMPTION");

            ConnectPortRef(loadTorque, "LOAD_TORQUE_TOTAL", engine, "LOAD_TORQUE");
            ConnectPortRef(compressor, "LOAD_TORQUE", loadTorque, "LOAD_TORQUE_0");
            ConnectPortRef(tractionGen, "LOAD_TORQUE", loadTorque, "LOAD_TORQUE_1");

            ConnectPortRef(engine, "RPM_NORMALIZED", compressor, "ENGINE_RPM_NORMALIZED");

            ConnectPortRef(tm, "EFFECTIVE_RESISTANCE", slugPowerCalc, "INTERNAL_EFFECTIVE_RESISTANCE");
            ConnectPortRef(tm, "TOTAL_AMPS", slugPowerCalc, "INTERNAL_AMPS");

            ConnectPortRef(thrtPowr, "GOAL_POWER", tractionGen, "GOAL_POWER");
            ConnectPortRef(thrtPowr, "GOAL_RPM_NORMALIZED", tractionGen, "GOAL_RPM_NORMALIZED");

            if (hasDynamic)
            {
                ConnectPortRef(dynBrake, "EXT_IN", tractionGen, "DYNAMIC_BRAKE");
            }

            ConnectPortRef(engine, "RPM", tractionGen, "RPM");
            ConnectPortRef(engine, "RPM_NORMALIZED", tractionGen, "RPM_NORMALIZED");

            ConnectPortRef(tm, "CURRENT_DROP_REQUEST", tractionGen, "CURRENT_DROP_REQUEST");

            ConnectPortRef(slugPowerCalc, "TOTAL_AMPS", tractionGen, "TOTAL_AMPS");
            ConnectPortRef(slugPowerCalc, "EFFECTIVE_RESISTANCE", tractionGen, "EFFECTIVE_RESISTANCE");

            ConnectPortRef(throttle, "EXT_IN", tm, "THROTTLE");
            ConnectPortRef(reverser, "REVERSER", tm, "REVERSER");

            if (basisIndex == 1)
            {
                ConnectPortRef(dynBrake, "EXT_IN", tm, "DYNAMIC_BRAKE");
            }

            ConnectPortRef(tmRpm, "TM_RPM", tm, "MOTOR_RPM");

            ConnectPortRef(tractionGen, "VOLTAGE", tm, "APPLIED_VOLTAGE");

            ConnectPortRef(heat, "TEMPERATURE", tm, "TM_TEMPERATURE");
            ConnectPortRef(heat, "TEMPERATURE", cooler, "TEMPERATURE");

            ConnectPortRef(tm, "HEAT_OUT", heat, "HEAT_IN_0");
            ConnectPortRef(cooler, "HEAT_OUT", heat, "HEAT_IN_1");

            ConnectPortRef(traction, "WHEEL_RPM_EXT_IN", tmRpm, "WHEEL_RPM");
            ConnectPortRef(transmission, "GEAR_RATIO", tmRpm, "GEAR_RATIO");

            // Apply defaults.
            switch (basisIndex)
            {
                case 0:
                    ApplyMethodToAll<IDE2Defaults>(s => s.ApplyDE2Defaults());
                    break;
                case 1:
                    ApplyMethodToAll<IDE6Defaults>(s => s.ApplyDE6Defaults());
                    break;
                default:
                    break;
            }

            // Control blockers.
            AddControlBlocker(reverser, throttle, "EXT_IN", 0, BlockType.BLOCK_ON_ABOVE_THRESHOLD)
                .blockedControlPortId = FullPortId(reverser, "CONTROL_EXT_IN");

            if (hasDynamic)
            {
                AddControlBlocker(reverser, dynBrake, "EXT_IN", 0, BlockType.BLOCK_ON_ABOVE_THRESHOLD);

                AddControlBlocker(throttle, dynBrake, "EXT_IN", 0, BlockType.BLOCK_ON_ABOVE_THRESHOLD)
                    .blockedControlPortId = FullPortId(throttle, "EXT_IN");

                AddControlBlocker(dynBrake, throttle, "EXT_IN", 0, BlockType.BLOCK_ON_ABOVE_THRESHOLD);
                AddControlBlocker(dynBrake, reverser, "REVERSER", 0, BlockType.BLOCK_ON_EQUAL_TO_THRESHOLD)
                    .blockedControlPortId = FullPortId(dynBrake, "EXT_IN");
            }
        }

        private static bool HasDynamicBrake(int index)
        {
            switch (index)
            {
                case 1:
                    return true;
                default:
                    return false;
            }
        }

        private static bool HasBell(int index)
        {
            switch (index)
            {
                case 1:
                    return true;
                default:
                    return false;
            }
        }
    }
}
