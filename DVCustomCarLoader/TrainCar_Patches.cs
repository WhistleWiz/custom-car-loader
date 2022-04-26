﻿using DV.ServicePenalty;
using DVCustomCarLoader.LocoComponents;
using HarmonyLib;
using UnityEngine;

namespace DVCustomCarLoader
{
    [HarmonyPatch(typeof(TrainCar))]
    public static class TrainCar_LoadInterior_Patch
    {
        [HarmonyPatch(nameof(TrainCar.LoadInterior))]
        public static void Postfix(TrainCar __instance, GameObject ___loadedInterior)
        {
            if (!___loadedInterior) return;

            if (!___loadedInterior.activeSelf)
            {
                ___loadedInterior.gameObject.SetActive(true);
                Main.LogVerbose($"Activating interior on {___loadedInterior.gameObject.name}");

                var eventMgr = __instance.gameObject.GetComponent<LocoEventManager>();
                if (eventMgr)
                {
                    eventMgr.OnInteriorLoaded(___loadedInterior);
                }
            }
        }

        [HarmonyPatch(nameof(TrainCar.UnloadInterior))]
        public static void Prefix(TrainCar __instance)
        {
            var eventMgr = __instance.gameObject.GetComponent<LocoEventManager>();
            if (eventMgr)
            {
                eventMgr.OnInteriorUnloaded();
            }
        }
    }

    [HarmonyPatch]
    public static class LocoController_Debt_Patches
    {
        // On initialized
        [HarmonyPatch(typeof(TrainCar), "InitializeLogicCarRelatedScript")]
        [HarmonyPostfix]
        public static void LogicCarRelatedScript(TrainCar __instance, ref CarDebtController ___carDebtController, CarStateSave ___carStateSave)
        {
            if (__instance.playerSpawnedCar && !__instance.IsLoco)
            {
                bool isCustomCar = CarTypeInjector.IsInCustomRange(__instance.carType);
                if (Main.Settings.FeesForAllLocos || (isCustomCar && Main.Settings.FeesForCCLLocos))
                {
                    ___carDebtController.SetDebtTracker(__instance.CarDamage, __instance.CargoDamage);
                    ___carStateSave.SetDebtTrackerCar(___carDebtController.CarDebtTracker);
                }
            }
        }

        [HarmonyPatch(typeof(LocoControllerDiesel), "OnLogicCarInitialized")]
        [HarmonyPostfix]
        public static void DieselInitialized(LocoControllerDiesel __instance, ref LocoDebtTrackerDiesel ___locoDebt,
            DamageControllerDiesel ___damageController, DieselLocoSimulation ___sim)
        {
            if (__instance.train.playerSpawnedCar && Main.Settings.FeesForAllLocos)
            {
                ___locoDebt = new LocoDebtTrackerDiesel(___damageController, ___sim, __instance.train.ID, __instance.train.carType);
                LocoDebtController.Instance.RegisterLocoDebtTracker(___locoDebt);
            }
        }

        [HarmonyPatch(typeof(LocoControllerShunter), "OnLogicCarInitialized")]
        [HarmonyPostfix]
        public static void ShunterInitialized(LocoControllerShunter __instance, ref LocoDebtTrackerShunter ___locoDebt,
            DamageControllerShunter ___damageController, ShunterLocoSimulation ___sim)
        {
            if (__instance.train.playerSpawnedCar && Main.Settings.FeesForAllLocos)
            {
                ___locoDebt = new LocoDebtTrackerShunter(___damageController, ___sim, __instance.train.ID, __instance.train.carType);
                LocoDebtController.Instance.RegisterLocoDebtTracker(___locoDebt);
            }
        }

        [HarmonyPatch(typeof(LocoControllerSteam), "OnLogicCarInitialized")]
        [HarmonyPostfix]
        public static void SteamInitialized(LocoControllerSteam __instance, ref LocoDebtTrackerSteam ___locoDebt,
            DamageController ___damageController, SteamLocoSimulation ___sim)
        {
            if (__instance.train.playerSpawnedCar && Main.Settings.FeesForAllLocos)
            {
                ___locoDebt = new LocoDebtTrackerSteam(___damageController, ___sim, __instance.train.ID, __instance.train.carType);
                LocoDebtController.Instance.RegisterLocoDebtTracker(___locoDebt);
            }
        }

        [HarmonyPatch(typeof(TenderSimulation), "OnLogicCarInitialized")]
        [HarmonyPostfix]
        public static void TenderInitialized(TenderSimulation __instance, ref LocoDebtTrackerTender ___tenderDebt,
            TrainCar ___train)
        {
            if (___train.playerSpawnedCar && Main.Settings.FeesForAllLocos)
            {
                var damageModel = __instance.GetComponent<CarDamageModel>();
                ___tenderDebt = new LocoDebtTrackerTender(__instance, damageModel, ___train.ID, ___train.carType);
                LocoDebtController.Instance.RegisterLocoDebtTracker(___tenderDebt);
            }
        }

        // On destroyed
        [HarmonyPatch(typeof(LocoControllerDiesel), "OnLocoDestroyed")]
        [HarmonyPatch(typeof(LocoControllerShunter), "OnLocoDestroyed")]
        [HarmonyPatch(typeof(LocoControllerSteam), "OnLocoDestroyed")]
        [HarmonyPostfix]
        public static void LocoDestroyed(TrainCar train, LocoDebtTrackerBase ___locoDebt)
        {
            if (train.playerSpawnedCar && Main.Settings.FeesForAllLocos && (___locoDebt != null))
            {
                LocoDebtController.Instance.StageLocoDebtOnLocoDestroy(___locoDebt);
            }
        }

        [HarmonyPatch(typeof(TenderSimulation), "OnLocoDestroyed")]
        [HarmonyPostfix]
        public static void TenderDestroyed(TrainCar ___train, LocoDebtTrackerTender ___tenderDebt)
        {
            if (___train.playerSpawnedCar && Main.Settings.FeesForAllLocos && (___tenderDebt != null))
            {
                LocoDebtController.Instance.StageLocoDebtOnLocoDestroy(___tenderDebt);
            }
        }
    }
}
