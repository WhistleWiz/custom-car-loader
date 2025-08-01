﻿using CCL.Types.Proxies.Ports;
using CCL.Types.Proxies.Simulation.Steam;
using System.Collections.Generic;
using UnityEngine;

namespace CCL.Types.Proxies.Controllers
{
    [AddComponentMenu("CCL/Proxies/Controllers/Firebox Sim Controller Proxy")]
    public class FireboxSimControllerProxy : MonoBehaviourWithVehicleDefaults, IHasPortIdFields, IS060Defaults, IS282Defaults
    {
        public float coalConsumptionMultiplier = 1f;
        [PortId(null, null, false)]
        public string fireboxCapacityPortId = string.Empty;
        [PortId(null, null, false)]
        public string fireboxContentsPortId = string.Empty;
        [PortId(null, null, false)]
        public string fireboxDoorPortId = string.Empty;
        [PortId(null, null, false)]
        public string combustionRateNormalizedPortId = string.Empty;
        [PortId(null, null, false)]
        public string fireOnPortId = string.Empty;
        [PortId(null, null, false)]
        public string fireboxCoalControlPortId = string.Empty;
        [PortId(null, null, false)]
        public string fireboxIgnitionPortId = string.Empty;

        public IEnumerable<PortIdField> ExposedPortIdFields => new[]
        {
            new PortIdField(this, nameof(fireboxCapacityPortId), fireboxCapacityPortId),
            new PortIdField(this, nameof(fireboxContentsPortId), fireboxContentsPortId),
            new PortIdField(this, nameof(fireboxDoorPortId), fireboxDoorPortId),
            new PortIdField(this, nameof(combustionRateNormalizedPortId), combustionRateNormalizedPortId),
            new PortIdField(this, nameof(fireOnPortId), fireOnPortId),
            new PortIdField(this, nameof(fireboxCoalControlPortId), fireboxCoalControlPortId),
            new PortIdField(this, nameof(fireboxIgnitionPortId), fireboxIgnitionPortId),
        };

        private void Reset()
        {
            if (gameObject.TryGetComponent(out FireboxDefinitionProxy firebox))
            {
                ConnectFirebox(firebox);
            }
        }

        public void ConnectFirebox(FireboxDefinitionProxy firebox)
        {
            fireboxCapacityPortId = firebox.GetFullPortId("COAL_CAPACITY");
            fireboxContentsPortId = firebox.GetFullPortId("COAL_LEVEL");
            combustionRateNormalizedPortId = firebox.GetFullPortId("COMBUSTION_RATE_NORMALIZED");
            fireOnPortId = firebox.GetFullPortId("FIRE_ON");
            fireboxCoalControlPortId = firebox.GetFullPortId("COAL_CONTROL_EXT_IN");
            fireboxIgnitionPortId = firebox.GetFullPortId("IGNITION_EXT_IN");
        }

        #region Defaults

        public void ApplyS060Defaults()
        {
            coalConsumptionMultiplier = 2.0f;
        }

        public void ApplyS282Defaults()
        {
            coalConsumptionMultiplier = 4.0f;
        }

        #endregion
    }
}
