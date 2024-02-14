﻿using CCL.Types.Proxies.Indicators;
using DV.Localization;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CCL.Importer.Processing
{
    [Export(typeof(IModelProcessorStep))]
    internal class LabelProcessor : ModelProcessorStep
    {
        private static readonly GameObject _offsetLabelComplete;
        private static readonly GameObject _offsetLabelModel;

        private static readonly GameObject _flushLabelComplete;
        private static readonly GameObject _flushLabelModel;

        static LabelProcessor()
        {
            var s282Labels = TrainCarType.LocoSteamHeavy.ToV2().interiorPrefab.transform.Find("LocoS282A_Interior_Labels");

            _offsetLabelComplete = s282Labels.Find("BrakeTrain/Label_RM_1").gameObject;
            _offsetLabelModel = _offsetLabelComplete.transform.Find("Label").gameObject;

            _flushLabelComplete = s282Labels.Find("Regulator/Label_RM_2").gameObject;
            _flushLabelModel = _flushLabelComplete.transform.Find("Label").gameObject;
        }

        private static readonly Quaternion _labelModelRotation = Quaternion.AngleAxis(180, Vector3.up);

        public override void ExecuteStep(ModelProcessor context)
        {
            foreach (var prefab in context.Car.AllPrefabs)
            {
                foreach (var labelProxy in prefab.GetComponentsInChildren<LabelLocalizer>())
                {
                    Localize targetLocalize;

                    if (labelProxy.ModelType != LabelModelType.None)
                    {
                        bool useDefaultText = !labelProxy.ModelType.HasFlag(LabelModelType.CustomText);

                        // create new holder for text and model
                        var holder = new GameObject($"{labelProxy.gameObject.name}_text");
                        holder.transform.SetParent(labelProxy.transform.parent, false);
                        holder.transform.localPosition = labelProxy.transform.localPosition;
                        holder.transform.localRotation = labelProxy.transform.localRotation;

                        // reparent original text obj to holder
                        labelProxy.transform.SetParent(holder.transform, false);
                        labelProxy.transform.localPosition = Vector3.zero;
                        labelProxy.transform.localRotation = Quaternion.identity;

                        if (labelProxy.ModelType.HasFlag(LabelModelType.Offset))
                        {
                            GameObject toCopy = useDefaultText ? _offsetLabelComplete : _offsetLabelModel;
                            var instantiated = UnityEngine.Object.Instantiate(toCopy, holder.transform, false);
                            instantiated.transform.localPosition = Vector3.zero;
                            instantiated.transform.localRotation = _labelModelRotation;
                        }
                        else if (labelProxy.ModelType.HasFlag(LabelModelType.Flush))
                        {
                            GameObject toCopy = useDefaultText ? _flushLabelComplete : _flushLabelModel;
                            var instantiated = UnityEngine.Object.Instantiate(toCopy, holder.transform, false);
                            instantiated.transform.localPosition = Vector3.zero;
                            instantiated.transform.localRotation = _labelModelRotation;
                        }

                        if (useDefaultText)
                        {
                            targetLocalize = holder.GetComponentInChildren<Localize>();
                        }
                        else
                        {
                            targetLocalize = labelProxy.gameObject.AddComponent<Localize>();
                        }
                    }
                    else
                    {
                        targetLocalize = labelProxy.gameObject.AddComponent<Localize>();
                    }

                    Mapper.M.Map(labelProxy, targetLocalize);
                    UnityEngine.Object.Destroy(labelProxy);
                }
            }
        }
    }
}
