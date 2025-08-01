﻿using CCL.Types.Proxies.Controllers;
using CCL.Types;
using CCL.Types.Proxies.Controls;
using CCL.Types.Proxies.Simulation.Diesel;

namespace CCL.Creator.Wizards.SimSetup
{
    internal abstract partial class SimCreator
    {
        protected DieselEngineDirectDefinitionProxy CreateDieselEngine(bool createExplosion)
        {
            var engine = CreateSimComponent<DieselEngineDirectDefinitionProxy>("de");

            var engineOff = CreateSibling<PowerOffControlProxy>(engine);
            engineOff.portId = FullPortId(engine, "EMERGENCY_ENGINE_OFF_EXT_IN");

            var engineOn = CreateSibling<EngineOnReaderProxy>(engine);
            engineOn.portId = FullPortId(engine, "ENGINE_ON");

            var environmentDamage = CreateSibling<EnvironmentDamagerProxy>(engine);
            environmentDamage.damagerPortId = FullPortId(engine, "FUEL_ENV_DAMAGE_METER");
            environmentDamage.environmentDamageResource = BaseResourceType.EnvironmentDamageFuel;

            if (createExplosion)
            {
                var engineExplosion = CreateSibling<ExplosionActivationOnSignalProxy>(engine);
                engineExplosion.explosionPrefab = ExplosionPrefab.Mechanical;
                engineExplosion.bodyDamagePercentage = 0.1f;
                engineExplosion.explosionSignalPortId = FullPortId(engine, "IS_BROKEN");
            }

            var starter = CreateSibling<OverridableControlProxy>(engine);
            starter.ControlType = OverridableControlType.StarterControl;
            starter.portId = FullPortId(engine, "IGNITION_EXT_IN");

            return engine;
        }
    }
}
