﻿using System.Collections.Generic;
using UnityEngine;

namespace CCL.Types.Proxies.Customization
{
    public class TrainCarPaintProxy : MonoBehaviour
    {
        public enum Target : byte
        {
            Exterior,
            Interior
        }

        [PaintField]
        public string CurrentTheme = IdV2.Paints[0];
        public List<MaterialSet> Sets = new List<MaterialSet>();

        [RenderMethodButtons]
        [MethodButton(nameof(AddSet), "Add set")]
        public bool buttonRender;

        private void AddSet()
        {
            Sets.Add(gameObject.AddComponent<MaterialSet>());
        }
    }
}
