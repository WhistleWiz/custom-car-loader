﻿using CCL_GameScripts;
using CCL_GameScripts.Effects;
using System.Collections;
using UnityEngine;

namespace DVCustomCarLoader.Effects
{
    public static class ParticleInitializer
    {
        private static GameObject de6EngineSmokeTemplate;
        private static GameObject shunterEngineSmokeTemplate;
        private static GameObject steamEngineParticleTemplate;

        private static bool fetched = false;

        public static AnimationCurve DieselEmissionRatePerThrottleCurve;
        public static AnimationCurve DieselStartSpeedMultPerThrottleCurve;
        public static AnimationCurve DieselMaxParticlesPerThrottle;

        public static AnimationCurve ShunterEmissionRatePerThrottleCurve;
        public static AnimationCurve ShunterStartSpeedMultPerThrottleCurve;
        public static AnimationCurve ShunterMaxParticlesPerThrottle;

        public static SteamLocoChuffSmokeParticles sh282ChuffParticles;

        public static GameObject de6SparkTemplate;
        public static GameObject shunterSparkTemplate;
        public static GameObject steamSparkTemplate;

        public static void FetchDefaults()
        {
            if (fetched) return;

            // DE6
            var carRoot = CarTypes.GetCarPrefab(TrainCarType.LocoDiesel);
            var de6Smoke = carRoot.GetComponent<EngineSmokeDiesel>();

            // grab particle systems
            var particles = carRoot.transform.Find(CarPartNames.PARTICLE_ROOT).gameObject;
            de6EngineSmokeTemplate = GameObject.Instantiate(particles);
            de6EngineSmokeTemplate.SetActive(false);

            var sparks = de6EngineSmokeTemplate.transform.Find(CarPartNames.WHEEL_SPARKS);
            de6SparkTemplate = GameObject.Instantiate(sparks.GetChild(0).gameObject);
            GameObject.DestroyImmediate(sparks.gameObject);

            DieselEmissionRatePerThrottleCurve    = de6Smoke.emissionRatePerThrottleCurve;
            DieselStartSpeedMultPerThrottleCurve  = de6Smoke.startSpeedMultPerThrottleCurve;
            DieselMaxParticlesPerThrottle         = de6Smoke.maxParticlesPerThrottle;


            // Shunter
            carRoot = CarTypes.GetCarPrefab(TrainCarType.LocoShunter);
            var shunterSmoke = carRoot.GetComponent<EngineSmokeShunter>();

            particles = carRoot.transform.Find(CarPartNames.PARTICLE_ROOT).gameObject;
            shunterEngineSmokeTemplate = GameObject.Instantiate(particles);
            shunterEngineSmokeTemplate.SetActive(false);

            sparks = shunterEngineSmokeTemplate.transform.Find(CarPartNames.WHEEL_SPARKS);
            shunterSparkTemplate = GameObject.Instantiate(sparks.GetChild(0).gameObject);
            GameObject.DestroyImmediate(sparks.gameObject);

            ShunterEmissionRatePerThrottleCurve     = shunterSmoke.emissionRatePerThrottleCurve;
            ShunterStartSpeedMultPerThrottleCurve   = shunterSmoke.startSpeedMultPerThrottleCurve;
            ShunterMaxParticlesPerThrottle          = shunterSmoke.maxParticlesPerThrottle;

            // Steam
            carRoot = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy);
            sh282ChuffParticles = carRoot.GetComponent<SteamLocoChuffSmokeParticles>();

            particles = carRoot.transform.Find(CarPartNames.PARTICLE_ROOT).gameObject;
            steamEngineParticleTemplate = GameObject.Instantiate(particles);
            steamEngineParticleTemplate.SetActive(false);

            sparks = steamEngineParticleTemplate.transform.Find(CarPartNames.WHEEL_SPARKS);
            steamSparkTemplate = GameObject.Instantiate(sparks.GetChild(0).gameObject);
            GameObject.DestroyImmediate(sparks.gameObject);

            fetched = true;
        }

        public static EngineSmokeParticles AddBigDieselParticles(EngineSmokeEmitter emitter, Transform root)
        {
            emitter.emissionRatePerThrottleCurve = DieselEmissionRatePerThrottleCurve;
            emitter.startSpeedMultPerThrottleCurve = DieselStartSpeedMultPerThrottleCurve;
            emitter.maxParticlesPerThrottle = DieselMaxParticlesPerThrottle;

            var newParticleRoot = GameObject.Instantiate(de6EngineSmokeTemplate, root);
            newParticleRoot.name = CarPartNames.SMOKE_EMITTER;
            newParticleRoot.transform.localPosition = Vector3.zero;

            var smoke = EngineSmokeParticles.FromParticleRoot(newParticleRoot.transform);
            smoke.AlignEmittersToRoot();
            return smoke;
        }

        public static EngineSmokeParticles AddSmallDieselParticles(EngineSmokeEmitter emitter, Transform root)
        {
            emitter.emissionRatePerThrottleCurve = ShunterEmissionRatePerThrottleCurve;
            emitter.startSpeedMultPerThrottleCurve = ShunterStartSpeedMultPerThrottleCurve;
            emitter.maxParticlesPerThrottle = ShunterMaxParticlesPerThrottle;

            var newParticleRoot = GameObject.Instantiate(shunterEngineSmokeTemplate, root);
            newParticleRoot.name = CarPartNames.SMOKE_EMITTER;
            newParticleRoot.transform.localPosition = Vector3.zero;

            var smoke = EngineSmokeParticles.FromParticleRoot(newParticleRoot.transform);
            smoke.AlignEmittersToRoot();
            return smoke;
        }

        public static SteamParticles AddSteamParticles(SteamParticlesController controller, SteamEmissionSetup spec)
        {
            var newParticleRoot = GameObject.Instantiate(steamEngineParticleTemplate, controller.transform);
            newParticleRoot.name = CarPartNames.PARTICLE_ROOT;
            newParticleRoot.transform.localPosition = Vector3.zero;

            var particles = new SteamParticles(newParticleRoot);
            particles.AlignParticles(spec);
            return particles;
        }

        public static void AddWheelSparks(GameObject prefab, WheelSparkSetup spec, LocoSimTemplate locoType)
        {
            var particleRoot = prefab.transform.FindSafe(CarPartNames.PARTICLE_ROOT);
            if (!particleRoot)
            {
                particleRoot = new GameObject(CarPartNames.PARTICLE_ROOT).transform;
                particleRoot.SetParent(prefab.transform, false);
            }

            var sparksRoot = particleRoot.FindSafe(CarPartNames.WHEEL_SPARKS);
            if (!sparksRoot)
            {
                sparksRoot = new GameObject(CarPartNames.WHEEL_SPARKS).transform;
                sparksRoot.SetParent(particleRoot, false);
            }

            GameObject sparkTemplate;
            switch (locoType)
            {
                case LocoSimTemplate.Shunter:
                    sparkTemplate = shunterSparkTemplate;
                    break;

                case LocoSimTemplate.DE6:
                    sparkTemplate = de6SparkTemplate;
                    break;

                case LocoSimTemplate.SH282:
                    sparkTemplate = steamSparkTemplate;
                    break;

                default:
                    Main.Error($"Unknown template type for wheel sparks {locoType}");
                    return;
            }

            for (int i = 0; i < spec.SparkPoints.Length; i++)
            {
                var sparkPoint = spec.SparkPoints[i];

                var newSparker = GameObject.Instantiate(sparkTemplate, sparksRoot);
                newSparker.name = $"sparkPoint_{i}L";
                newSparker.transform.localPosition = sparkPoint.localPosition + CarPartOffset.CENTER_TO_LEFT_RAIL;
                newSparker.transform.localRotation = sparkPoint.localRotation;

                newSparker = GameObject.Instantiate(sparkTemplate, sparksRoot);
                newSparker.name = $"sparkPoint_{i}R";
                newSparker.transform.localPosition = sparkPoint.localPosition + CarPartOffset.CENTER_TO_RIGHT_RAIL;
                newSparker.transform.localRotation = sparkPoint.localRotation;
            }
        }
    }

    public struct EngineSmokeParticles
    {
        public GameObject Root;
        public ParticleSystem EngineSmoke;
        public ParticleSystem HighTempSmoke;
        public ParticleSystem DamagedSmoke;

        public static EngineSmokeParticles FromParticleRoot(Transform root)
        {
            var engineSmoke = root.Find(CarPartNames.EXHAUST_SMOKE).GetComponent<ParticleSystem>();
            var highTemp = root.Find(CarPartNames.HIGH_TEMP_SMOKE).GetComponent<ParticleSystem>();
            var damage = root.Find(CarPartNames.DAMAGED_SMOKE).GetComponent<ParticleSystem>();

            return new EngineSmokeParticles()
            {
                Root = root.gameObject,
                EngineSmoke = engineSmoke,
                HighTempSmoke = highTemp,
                DamagedSmoke = damage
            };
        }

        public void AlignEmittersToRoot()
        {
            EngineSmoke.gameObject.transform.localPosition = Vector3.zero;
            HighTempSmoke.gameObject.transform.localPosition = Vector3.zero;
            DamagedSmoke.gameObject.transform.localPosition = Vector3.zero;
        }
    }

    public class SteamParticles
    {
        public GameObject Root;

        public ParticleSystem ChimneyParticles;
        public ParticleSystem ChuffParticlesLeft;
        public ParticleSystem ChuffParticlesRight;

        public ParticleSystem WhistleMid;
        public ParticleSystem WhistleFront;

        public ParticleSystem ReleaseLeft;
        public ParticleSystem ReleaseRight;

        public ParticleSystem SafetyRelease;

        // optional
        public ParticleSystem Dynamo;
        public ParticleSystem Compressor;

        public SteamParticles(GameObject root)
        {
            Root = root;
            var t = root.transform;

            ChimneyParticles = t.Find(CarPartNames.CHIMNEY_STEAM).GetComponentInChildren<ParticleSystem>();
            ChuffParticlesLeft = t.Find(CarPartNames.STEAM_CHUFF_L).GetComponentInChildren<ParticleSystem>();
            ChuffParticlesRight = t.Find(CarPartNames.STEAM_CHUFF_R).GetComponentInChildren<ParticleSystem>();

            var whistleTform = t.Find(CarPartNames.STEAM_WHISTLE_F);
            WhistleMid = t.Find(CarPartNames.STEAM_WHISTLE_M).GetComponentInChildren<ParticleSystem>();
            WhistleFront = whistleTform.GetComponentInChildren<ParticleSystem>();

            ReleaseLeft = t.Find(CarPartNames.STEAM_RELEASE_L).GetComponentInChildren<ParticleSystem>();
            ReleaseRight = t.Find(CarPartNames.STEAM_RELEASE_R).GetComponentInChildren<ParticleSystem>();
            SafetyRelease = t.Find(CarPartNames.STEAM_RELEASE_SAFETY).GetComponentInChildren<ParticleSystem>();
        }

        private void ApplyOffset(Transform tform, ParticleSystem particles)
        {
            var parent = particles.transform.parent;
            Vector3 offset = tform ? Root.transform.InverseTransformPoint(tform.position) : Vector3.zero;
            parent.localPosition = offset;
            if (tform)
            {
                parent.localRotation *= tform.localRotation;
            }
            particles.transform.localPosition = Vector3.zero;
        }

        public void AlignParticles(SteamEmissionSetup spec)
        {
            ApplyOffset(spec.ChimneyParticlesLocation, ChimneyParticles);
            ApplyOffset(spec.ChuffParticlesLeftLocation, ChuffParticlesLeft);
            ApplyOffset(spec.ChuffParticlesRightLocation, ChuffParticlesRight);

            ApplyOffset(spec.WhistleMidLocation, WhistleMid);
            ApplyOffset(spec.WhistleFrontLocation, WhistleFront);

            ApplyOffset(spec.ReleaseLeftLocation, ReleaseLeft);
            ApplyOffset(spec.ReleaseRightLocation, ReleaseRight);
            ApplyOffset(spec.SafetyReleaseLocation, SafetyRelease);

            

            if (spec.DynamoLocation)
            {
                CreateDynamoParticles();
                ApplyOffset(spec.DynamoLocation, Dynamo);
            }

            if (spec.CompressorLocation)
            {
                CreateCompressorParticles();
                ApplyOffset(spec.CompressorLocation, Compressor);
            }
        }

        private void CreateDynamoParticles()
        {
            var whistleObj = Root.transform.Find(CarPartNames.STEAM_WHISTLE_F).gameObject;
            var dynamoObj = GameObject.Instantiate(whistleObj, Root.transform, false);
            Dynamo = dynamoObj.GetComponentInChildren<ParticleSystem>();

            var main = Dynamo.main;
            main.gravityModifier = -0.1f;

            var emission = Dynamo.emission;
            emission.rateOverTimeMultiplier *= 0.5f;
        }

        private void CreateCompressorParticles()
        {
            var chuffObj = Root.transform.Find(CarPartNames.STEAM_WHISTLE_F).gameObject;
            var compressorObj = GameObject.Instantiate(chuffObj, Root.transform, false);
            Compressor = compressorObj.GetComponentInChildren<ParticleSystem>();

            var main = Compressor.main;
            main.startSpeed = 0.0f;
            main.gravityModifier = -0.01f;
            main.startLifetimeMultiplier *= 2;

            var emission = Compressor.emission;
            emission.rateOverTime = 2;
            emission.rateOverTimeMultiplier = 1;

        }
    }
}