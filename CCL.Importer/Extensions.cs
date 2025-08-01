﻿using CCL.Types;
using DV.Localization;
using DV.Simulation.Cars;
using DV.Simulation.Controllers;
using DV.ThingTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

using UObject = UnityEngine.Object;

namespace CCL.Importer
{
    public static class Extensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (go.TryGetComponent(out T comp))
            {
                return comp;
            }

            return go.AddComponent<T>();
        }

        public static IEnumerable<T> GetComponentsByInterface<T>(this GameObject gameObject)
            where T : class
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException($"GetComponentsInChildrenByInterface - Type {typeof(T).Name} is not an interface");
            }
            if (!gameObject)
            {
                throw new ArgumentNullException("gameObject");
            }

            return gameObject.GetComponents<MonoBehaviour>()
                .Where(comp => comp && comp.GetType().GetInterfaces().Contains(typeof(T)))
                .Cast<T>();
        }

        public static T? GetComponentByInterface<T>(this GameObject gameObject)
            where T : class
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException($"GetComponentsInChildrenByInterface - Type {typeof(T).Name} is not an interface");
            }
            if (!gameObject)
            {
                throw new ArgumentNullException("gameObject");
            }

            return gameObject.GetComponents<MonoBehaviour>()
                .FirstOrDefault(comp => comp && comp.GetType().GetInterfaces().Contains(typeof(T)))
                as T;
        }

        public static IEnumerable<T> GetComponentsInChildrenByInterface<T>(this GameObject gameObject)
            where T : class
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException($"GetComponentsInChildrenByInterface - Type {typeof(T).Name} is not an interface");
            }
            if (!gameObject)
            {
                throw new ArgumentNullException("gameObject");
            }

            return gameObject.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(comp => comp && comp.GetType().GetInterfaces().Contains(typeof(T)))
                .Cast<T>();
        }

        public static T GetComponentInParentIncludingInactive<T>(this GameObject gameObject)
            where T : Component
        {
            var comps = gameObject.GetComponentsInParent<T>(true);

            if (comps.Length > 0)
            {
                return comps[0];
            }

            return null!;
        }

        public static T GetComponentInParentIncludingInactive<T>(this Component component)
            where T : Component
        {
            return GetComponentInParentIncludingInactive<T>(component.gameObject);
        }

        public static bool EqualsOneOf<T>(this T compare, params T[] values)
        {
            foreach (T v in values)
            {
                if (compare!.Equals(v)) return true;
            }
            return false;
        }

        public static bool SafeAny<T>(this IEnumerable<T> array)
        {
            return (array != null) && array.Any();
        }

        public static bool TryFind<T>(this List<T> list, Predicate<T> match, out T value)
        {
            value = list.Find(match);

            return value != null;
        }

        public static bool TryFind<T>(this T[] array, Predicate<T> match, out T value)
        {
            value = Array.Find(array, match);

            return value != null;
        }

        public static float Mapf(float fromMin, float fromMax, float toMin, float toMax, float value)
        {
            float fromRange = fromMax - fromMin;
            float toRange = toMax - toMin;
            return (value - fromMin) * (toRange / fromRange) + toMin;
        }

        public static T GetCached<T>(ref T? cacheValue, Func<T> getter) where T : class
        {
            cacheValue ??= getter();
            return cacheValue;
        }

        public static T GetCached<T>(ref T? cacheValue, Func<T> getter) where T : struct
        {
            if (!cacheValue.HasValue)
            {
                cacheValue = getter();
            }
            return cacheValue.Value;
        }

        //public static bool IsCustomCargoClass(this CargoContainerType containerType)
        //{
        //    return containerType == (CargoContainerType)BaseCargoContainerType.Custom;
        //}

        //public static bool IsCustomCargoType(this CargoType cargoType)
        //{
        //    return cargoType == (CargoType)BaseCargoType.Custom;
        //}
    }

    public static class AccessToolsExtensions
    {
        public static void SaveTo<T>(this FieldInfo field, out T dest, object? source = null)
        {
            if (field != null)
            {
                dest = (T)field.GetValue(source);
            }
            else
            {
                dest = default!;
            }
        }

        public static void SaveTo<T>(this PropertyInfo property, out T dest, object? source = null)
        {
            if (property != null)
            {
                dest = (T)property.GetValue(source);
            }
            else
            {
                dest = default!;
            }
        }
    }

    public static class DVExtensions
    {
        public static bool IsEnvironmental(this ResourceType_v2 type)
        {
            return type.canDamageEnvironment;
        }

        public static void RefreshChildren<T>(this ARefreshableChildrenController<T> controller)
            where T : MonoBehaviour
        {
            controller.entries = controller.gameObject.GetComponentsInChildren<T>(true);
        }

        public static void SetKeyAndUpdate(this Localize localize, string key)
        {
            localize.key = key;
            localize.UpdateLocalization();
        }

        public static void ManualLocalize(this Localize localize, string key)
        {
            TMPHelper.GetTMP(localize).SetTextAndUpdate(LocalizationAPI.L(key));
            UObject.DestroyImmediate(localize);
        }

        public static bool IsFrontCoupler(this TrainCar car, Coupler coupler)
        {
            return car.frontCoupler == coupler;
        }

        public static CoupleEventArgs CreateDummyArgs(this Coupler coupler)
        {
            return new CoupleEventArgs(coupler, coupler.coupledTo, false);
        }

        public static IEnumerable<TrainCar> GetAllLocos(this Trainset trainset)
        {
            foreach (var indice in trainset.locoIndices)
            {
                yield return trainset.cars[indice];
            }
        }

        public static bool TryGetTraincar(this CarSpawner instance, string id, out TrainCar car)
        {
            return instance.AllCars.TryFind(x => x.ID == id, out car);
        }

        public static void ForceTurnOffHeadlights(this HeadlightsMainController controller, bool front)
        {
            controller.UpdateHeadlights(controller.GetOffIndex(front), front, true);
        }
    }
}
