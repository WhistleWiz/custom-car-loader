﻿using CCL.Types;
using System.Linq;
using UnityEngine;
using System.ComponentModel.Composition;
using CCL.Types.Proxies;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;

namespace CCL.Importer.Processing
{
    [Export(typeof(IModelProcessorStep))]
    internal class ColliderProcessor : ModelProcessorStep
    {
        public Transform NewBogieColliderRoot = null!;
        public CapsuleCollider[] BaseBogieColliders = null!;

        public CapsuleCollider BaseFrontBogie => BaseBogieColliders.First();
        public CapsuleCollider BaseRearBogie => BaseBogieColliders.Last();

        public override void ExecuteStep(ModelProcessor context)
        {
            var newFab = context.Car.prefab;

            // [colliders]
            Transform colliderRoot = newFab.transform.Find(CarPartNames.Colliders.ROOT);
            if (!colliderRoot)
            {
                // collider should be initialized in prefab, but make sure
                CCLPlugin.Warning("Adding collision root to car, should have been part of prefab!");

                GameObject colliders = new GameObject(CarPartNames.Colliders.ROOT);
                colliderRoot = colliders.transform;
                colliderRoot.parent = newFab.transform;
            }

            // [collision]
            Transform collision = colliderRoot.Find(CarPartNames.Colliders.COLLISION);
            if (!collision)
            {
                var collisionObj = new GameObject(CarPartNames.Colliders.COLLISION);
                collision = collisionObj.transform;
                collision.parent = colliderRoot.transform;
            }

            // Ensure PitStop detects this as a serviceable car
            foreach (Transform item in collision.GetComponentsInChildren<Transform>())
            {
                item.tag = "MainTriggerCollider";
            }

            // find [walkable]
            // copy walkable to items if items doesn't exist
            Transform walkable = colliderRoot.Find(CarPartNames.Colliders.WALKABLE);
            if (walkable)
            {
                Transform items = colliderRoot.Find(CarPartNames.Colliders.ITEMS);
                if (!items)
                {
                    CCLPlugin.LogVerbose("Reusing walkable colliders as item colliders");
                    GameObject newItemsObj = Object.Instantiate(walkable.gameObject, colliderRoot);
                    newItemsObj.name = CarPartNames.Colliders.ITEMS;
                    newItemsObj.SetLayersRecursive(DVLayer.Interactable);
                }

                // set layer
                walkable.gameObject.SetLayersRecursive(DVLayer.Train_Walkable);

                // automagic bounding box from walkable
                var boundingColliders = collision.GetComponentsInChildren<BoxCollider>();
                if (boundingColliders.Length == 0)
                {
                    // autogenerate bounding box from walkable extents (only works with box collider bits though)
                    var walkableColliders = walkable.GetComponentsInChildren<BoxCollider>();
                    if (walkableColliders.Length > 0)
                    {
                        CCLPlugin.LogVerbose("Building bounding collision box from walkable colliders");

                        Bounds boundBox = BoundsUtil.BoxColliderAABB(walkableColliders[0], newFab.transform);
                        for (int i = 1; i < walkableColliders.Length; i++)
                        {
                            boundBox.Encapsulate(BoundsUtil.BoxColliderAABB(walkableColliders[i], newFab.transform));
                        }

                        BoxCollider newCollisionBox = collision.gameObject.AddComponent<BoxCollider>();
                        newCollisionBox.center = boundBox.center - collision.localPosition;
                        newCollisionBox.size = boundBox.size;
                    }
                }

                // Setup pass through colliders.
                var passthru = walkable.GetComponentsInChildren<TeleportArcPassThroughProxy>();

                for (int i = 0; i < passthru.Length; i++)
                {
                    Mapper.MapComponent(passthru[i], out TeleportArcPassThrough _);
                }
            }

            // [bogies]
            NewBogieColliderRoot = colliderRoot.transform.Find(CarPartNames.Colliders.BOGIES);
            if (!NewBogieColliderRoot)
            {
                CCLPlugin.LogVerbose("Adding bogie collider root");

                GameObject bogiesRoot = new GameObject(CarPartNames.Colliders.BOGIES);
                NewBogieColliderRoot = bogiesRoot.transform;
                NewBogieColliderRoot.parent = colliderRoot.transform;
            }

            // No more base livery so what a mess this is.
            var basePrefab = context.Car.UseCustomFrontBogie ?
                TrainCarType.FlatbedEmpty.ToV2().prefab :
                context.Car.FrontBogie.ToTypePrefab();
            Transform baseBogieColliderRoot = basePrefab.transform.Find($"{CarPartNames.Colliders.ROOT}/{CarPartNames.Colliders.BOGIES}");
            var c1 = baseBogieColliderRoot.GetComponentsInChildren<CapsuleCollider>().First();

            basePrefab = context.Car.UseCustomRearBogie ?
                TrainCarType.FlatbedEmpty.ToV2().prefab :
                context.Car.RearBogie.ToTypePrefab();
            baseBogieColliderRoot = basePrefab.transform.Find($"{CarPartNames.Colliders.ROOT}/{CarPartNames.Colliders.BOGIES}");
            var c2 = baseBogieColliderRoot.GetComponentsInChildren<CapsuleCollider>().Last();
            BaseBogieColliders = new[] { c1, c2 };
        }
    }
}
