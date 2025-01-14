﻿using CCL.Types.Proxies.Ports;
using System.Collections.Generic;

namespace CCL.Types.Proxies.Simulation.Diesel
{
    public class MechanicalCompressorDefinitionProxy : SimComponentDefinitionProxy, IDM3Defaults, IDH4Defaults, IDE2Defaults, IDE6Defaults
    {
        public float loadTorque = 400f;
        public float maxProductionRate = 250f;
        public float activationPressureThreshold = 7f;
        public float mainReservoirVolume = 15f;
        public float smoothTime = 0.3f;

        public override IEnumerable<PortDefinition> ExposedPorts => new[]
        {
            new PortDefinition(DVPortType.EXTERNAL_IN, DVPortValueType.CONTROL, "ACTIVATION_SIGNAL_EXT_IN"),
            new PortDefinition(DVPortType.EXTERNAL_IN, DVPortValueType.STATE, "COMPRESSOR_HEALTH_EXT_IN"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.TORQUE, "LOAD_TORQUE"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.STATE, "PRODUCTION_RATE"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.STATE, "PRODUCTION_RATE_NORMALIZED"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.GENERIC, "MAIN_RES_VOLUME"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.GENERIC, "ACTIVATION_PRESSURE_THRESHOLD")
        };

        public override IEnumerable<PortReferenceDefinition> ExposedPortReferences => new[]
        {
            new PortReferenceDefinition(DVPortValueType.RPM, "ENGINE_RPM_NORMALIZED"),
        };

        #region Defaults

        public void ApplyDM3Defaults()
        {
            loadTorque = 250;
            maxProductionRate = 45;
            activationPressureThreshold = 7;
            mainReservoirVolume = 50;
            smoothTime = 0.3f;
        }

        public void ApplyDH4Defaults()
        {
            loadTorque = 270;
            maxProductionRate = 75;
            activationPressureThreshold = 7;
            mainReservoirVolume = 80;
            smoothTime = 0.3f;
        }

        public void ApplyDE2Defaults()
        {
            loadTorque = 200.0f;
            maxProductionRate = 50.0f;
            activationPressureThreshold = 7.0f;
            mainReservoirVolume = 100.0f;
            smoothTime = 0.3f;
        }

        public void ApplyDE6Defaults()
        {
            loadTorque = 400.0f;
            maxProductionRate = 100.0f;
            activationPressureThreshold = 7.0f;
            mainReservoirVolume = 200.0f;
            smoothTime = 0.3f;
        }

        #endregion
    }
}