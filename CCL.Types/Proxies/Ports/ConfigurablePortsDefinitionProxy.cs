﻿using CCL.Types.Json;

namespace CCL.Types.Proxies.Ports
{
    public class ConfigurablePortsDefinitionProxy : SimComponentDefinitionProxy, ICustomSerialized
    {
        public PortStartValue[] Ports = new PortStartValue[0];

        private string? _json;

        public void OnValidate()
        {
            _json = JSONObject.ToJson(Ports);
        }

        public void AfterImport()
        {
            Ports = JSONObject.FromJson(_json, () => new PortStartValue[0]);
        }
    }

    public class PortStartValue
    {
        public PortDefinition Port;
        public float StartingValue;
    }
}
