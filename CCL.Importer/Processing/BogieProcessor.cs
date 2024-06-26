﻿using CCL.Types;
using CCL.Types.Components;
using CCL.Types.Proxies.Wheels;
using DV.Simulation.Brake;
using DV.Wheels;
using LocoSim.Implementations.Wheels;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using UnityEngine;

namespace CCL.Importer.Processing
{
    [Export(typeof(IModelProcessorStep))]
    [RequiresStep(typeof(ColliderProcessor))]
    internal class BogieProcessor : ModelProcessorStep
    {
        private const float TRACK_GAUGE_2 = 0.76f;

        public override void ExecuteStep(ModelProcessor context)
        {
            var newFab = context.Car.prefab;
            var colliders = context.GetCompletedStep<ColliderProcessor>();

            Bogie frontBogie, rearBogie;

            // Find existing bogie transforms
            Transform newFrontBogieTransform = newFab.transform.Find(CarPartNames.Bogies.FRONT);
            if (!newFrontBogieTransform)
            {
                CCLPlugin.Error("Front bogie transform is missing from prefab!");
            }

            Transform newRearBogieTransform = newFab.transform.Find(CarPartNames.Bogies.REAR);
            if (!newRearBogieTransform)
            {
                CCLPlugin.Error("Rear bogie transform is missing from prefab!");
            }

            // Front Bogie
            if (context.Car.UseCustomFrontBogie && newFrontBogieTransform)
            {
                // replacing the original bogie, only steal the script
                frontBogie = newFrontBogieTransform.gameObject.AddComponent<Bogie>();
            }
            else
            {
                frontBogie = StealBaseCarBogie(newFab.transform,
                    newFrontBogieTransform,
                    colliders.NewBogieColliderRoot,
                    colliders.BaseFrontBogie,
                    context.Car.FrontBogie.ToTypePrefab().GetComponent<TrainCar>().Bogies.Last());
            }

            // TODO: apply front bogie config

            // Rear Bogie
            if (context.Car.UseCustomRearBogie && newRearBogieTransform)
            {
                rearBogie = newRearBogieTransform.gameObject.AddComponent<Bogie>();
            }
            else
            {
                rearBogie = StealBaseCarBogie(newFab.transform,
                    newRearBogieTransform,
                    colliders.NewBogieColliderRoot,
                    colliders.BaseRearBogie,
                    context.Car.RearBogie.ToTypePrefab().GetComponent<TrainCar>().Bogies.First());
            }

            // TODO: apply rear bogie config

            SetupBrakeGlows(newFab, frontBogie, rearBogie);
            SetupWheelSlideSparks(newFab, frontBogie, rearBogie);
            SetupSlipSparks(newFab);
        }

        private static Bogie StealBaseCarBogie(Transform carRoot, Transform newBogieTransform, Transform bogieColliderRoot,
            CapsuleCollider baseBogieCollider, Bogie origBogie)
        {
            Vector3 bogiePosition = newBogieTransform.localPosition;
            Object.Destroy(newBogieTransform.gameObject);

            //GameObject origBogie = baseCar.Bogies[0].gameObject;
            GameObject copiedObject = Object.Instantiate(origBogie.gameObject, carRoot);
            copiedObject.name = CarPartNames.Bogies.FRONT;
            copiedObject.transform.localPosition = bogiePosition;

            Bogie newBogie = copiedObject.GetComponent<Bogie>();

            // grab collider as well
            CapsuleCollider newCollider = bogieColliderRoot.gameObject.AddComponent<CapsuleCollider>();

            newCollider.center = new Vector3(0, baseBogieCollider.center.y, bogiePosition.z);
            newCollider.direction = baseBogieCollider.direction;
            newCollider.radius = baseBogieCollider.radius;
            newCollider.height = baseBogieCollider.height;

            return newBogie;
        }

        private static BrakesOverheatingColorGradient? _defaultBrakeGradient = null;
        private static BrakesOverheatingColorGradient DefaultBrakeGradient =>
            Extensions.GetCached(ref _defaultBrakeGradient,
                () => Resources.FindObjectsOfTypeAll<BrakesOverheatingColorGradient>().FirstOrDefault(g => g.name == "BrakeShoeOverheatColorGradient"));

        private static void SetupBrakeGlows(GameObject newFab, Bogie front, Bogie rear)
        {
            List<Renderer> brakeRenderers = new();

            // Front bogie pads.
            Transform? padsF = front.transform.FindSafe(
                $"{CarPartNames.Bogies.BOGIE_CAR}/{CarPartNames.Bogies.BRAKE_ROOT}/{CarPartNames.Bogies.BRAKE_PADS}");

            if (padsF)
            {
                // Grab ALL the renderers.
                brakeRenderers.AddRange(padsF!.GetComponentsInChildren<Renderer>(true));
            }

            // Rear bogie pads.
            Transform? padsR = rear.transform.FindSafe(
                $"{CarPartNames.Bogies.BOGIE_CAR}/{CarPartNames.Bogies.BRAKE_ROOT}/{CarPartNames.Bogies.BRAKE_PADS}");

            if (padsR)
            {
                brakeRenderers.AddRange(padsR!.GetComponentsInChildren<Renderer>(true));
            }

            // Extra renderers (similar to how the S060, S282 and DM3 are set up).
            if (newFab.TryGetComponent(out ExtraBrakeRenderers extraBrakeRenderers))
            {
                brakeRenderers.AddRange(extraBrakeRenderers.Renderers);
            }

            if (!brakeRenderers.Any()) return;

            var brakeGlow = newFab.AddComponent<BrakesOverheatingController>();
            brakeGlow.brakeRenderers = brakeRenderers.ToArray();

            // Gradient.
            brakeGlow.overheatColor = ScriptableObject.CreateInstance<BrakesOverheatingColorGradient>();

            if (newFab.TryGetComponent(out CustomBrakeGlow customGlow))
            {
                // Use a custom one if available.
                brakeGlow.overheatColor.colorGradient = customGlow.ColourGradient;
            }
            else
            {
                brakeGlow.overheatColor.colorGradient = DefaultBrakeGradient.colorGradient;
                CCLPlugin.LogVerbose($"Apply default brake gradient to {newFab.name}");
            }
        }

        private static void SetupWheelSlideSparks(GameObject newFab, Bogie front, Bogie rear)
        {
            CustomWheelSlideSparks controller = newFab.GetComponentInChildren<CustomWheelSlideSparks>();

            // If the prefab has no proxy component, add one anyways and use it to automatically
            // setup the sparks, then treat it as usual.
            if (controller == null)
            {
                var sparks = new GameObject(CarPartNames.Particles.WHEEL_SPARKS);
                sparks.transform.parent = newFab.transform;
                sparks.transform.localPosition = Vector3.zero;
                controller = sparks.gameObject.AddComponent<CustomWheelSlideSparks>();
                controller.AutoSetupWithBogies(front.transform.Find(CarPartNames.Bogies.BOGIE_CAR), rear.transform.Find(CarPartNames.Bogies.BOGIE_CAR));
            }

            var temp = controller.gameObject.AddComponent<WheelSlideSparksController>();
            temp.sparkAnchors = controller.sparkAnchors.Where(x => ProcessSparkAnchor(x, front.transform, rear.transform)).ToArray();
            Object.Destroy(controller);
        }

        private static bool ProcessSparkAnchor(Transform anchor, Transform frontBogie, Transform rearBogie)
        {
            Transform parent = anchor.parent;

            if (parent.parent == frontBogie || parent.parent == rearBogie)
            {
                return true;
            }

            // Try to reparent if the anchors are in the fake bogies.
            if (parent.parent.name.Equals(CarPartNames.Bogies.BOGIE_CAR))
            {
                if (parent.parent.parent.name.Equals(CarPartNames.Bogies.FRONT))
                {
                    parent.parent = frontBogie;
                    return true;
                }
                if (parent.parent.parent.name.Equals(CarPartNames.Bogies.REAR))
                {
                    parent.parent = rearBogie;
                    return true;
                }

                return false;
            }

            // Not part of a bogie, take it.
            return true;
        }

        private static void SetupSlipSparks(GameObject newFab)
        {
            // Auto add wheelslip sparks if there are powered wheels.
            // The car needs to have both the powered wheels manager,
            // and powered wheels from default bogies (not proxies).
            if (!newFab.TryGetComponent(out PoweredWheelsManagerProxy _))
            {
                return;
            }

            var poweredWheels = newFab.GetComponentsInChildren<PoweredWheel>(newFab);

            if (poweredWheels.Length == 0)
            {
                return;
            }

            var slide = newFab.GetComponentInChildren<WheelSlideSparksController>();
            WheelslipSparksController controller;

            if (slide != null)
            {
                controller = slide.gameObject.AddComponent<WheelslipSparksController>();
            }
            else
            {
                var sparks = new GameObject(CarPartNames.Particles.WHEEL_SPARKS);
                sparks.transform.parent = newFab.transform;
                sparks.transform.localPosition = Vector3.zero;
                controller = sparks.gameObject.AddComponent<WheelslipSparksController>();
            }

            var list = new List<WheelslipSparksController.WheelSparksDefinition>();

            foreach (var wheel in poweredWheels)
            {
                Transform t = wheel.transform;

                Transform l = new GameObject($"L").transform;
                l.parent = t.parent;
                l.localPosition = new Vector3(-TRACK_GAUGE_2, 0, t.localPosition.z);

                Transform r = new GameObject($"R").transform;
                r.parent = t.parent;
                r.localPosition = new Vector3(TRACK_GAUGE_2, 0, t.localPosition.z);

                list.Add(new WheelslipSparksController.WheelSparksDefinition { poweredWheel = wheel, sparksLeftAnchor = l, sparksRightAnchor = r });
            }

            controller.wheelSparks = list.ToArray();
        }
    }
}
