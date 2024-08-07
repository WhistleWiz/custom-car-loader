﻿using CCL.Creator.Utility;
using CCL.Types;
using DVLangHelper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CCL.Creator.Wizards
{
    public class CarWizard : EditorWindow
    {
        private const string CAR_FOLDER = "_CCL_CARS";
        private const string CAR_TEMPLATE_PATH = "Assets/CarCreator/Prefabs/car_template.prefab";
        private const string FREIGHT_INTERACTABLES_PATH = "Assets/CarCreator/Prefabs/freight_interactables.prefab";

        private static CarWizard? _window;
        private static string GetCarFolder() => Path.Combine(Application.dataPath, CAR_FOLDER);

        private CarSettings _carSettings = new CarSettings();
        private Vector2 _scrollPosition = Vector2.zero;

        [MenuItem("CCL/Create New Car Type")]
        static void AddCarType()
        {
            _window = GetWindow<CarWizard>();
            _window.Refresh();
            _window.Show();
        }

        private void Refresh()
        {
            titleContent = new GUIContent("CCL - New Car Type");
            _carSettings = new CarSettings();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("box");
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorStyles.label.wordWrap = true;
            EditorGUILayout.LabelField(
                "This wizard will automagically create car type and livery assets for your new vehicle. " +
                "Simply fill out the fields below to get started!");

            _carSettings.Kind = RenderEnum(
                "This is the \"category\" of vehicle",
                "Kind", _carSettings.Kind);

            _carSettings.ID = RenderTextbox(
                "This will be the unique identifier for your car - we will also apply it to the default livery",
                "Car ID", _carSettings.ID);

            _carSettings.Name = RenderTextbox(
                "Pick an in-game name for your car - you will be able to add translations later",
                "Car Name", _carSettings.Name);

            _carSettings.Author = RenderTextbox(
                "Your (user)name for publishing the car pack",
                "Author", _carSettings.Author);

            _carSettings.BaseCarType = RenderEnum(
                "Pick the base car type that you would like to use bogies and buffers from",
                "Base Type", _carSettings.BaseCarType);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = _carSettings.IsValid;
            if (GUILayout.Button("Create Car"))
            {
                CreateNewCar(_carSettings);
                Close();
                return;
            }
            GUI.enabled = true;

            if (GUILayout.Button("Cancel"))
            {
                Close();
                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private static string RenderTextbox(string help, string label, string? value)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            string val = EditorGUILayout.TextField(label, value);
            EditorGUILayout.LabelField(help);
            return val;
        }

        private static T RenderEnum<T>(string help, string label, T value) where T : Enum
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            T val = (T)EditorGUILayout.EnumPopup(label, value);
            EditorGUILayout.LabelField(help);
            return val;
        }

        private static void CreateNewCar(CarSettings settings)
        {
            string carFolder = Path.Combine(GetCarFolder(), settings.Name);
            Directory.CreateDirectory(carFolder);

            string relativeCarFolder = Path.Combine("Assets", CAR_FOLDER, settings.Name);

            // create scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string scenePath = Path.Combine(relativeCarFolder, $"{settings.ID}_create.unity");

            // create prefab
            string carPrefabPath = Path.Combine(relativeCarFolder, $"{settings.ID}_template.prefab");
            AssetDatabase.CopyAsset(CAR_TEMPLATE_PATH, carPrefabPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(carPrefabPath);

            PrefabUtility.InstantiatePrefab(prefab);

            // create interactables prefab
            GameObject? interactables = null;
            //if (settings.Kind == DVTrainCarKind.Car)
            //{
                interactables = CreateFreightInteractables(settings.ID!, relativeCarFolder);
            //}

            // create car type
            var carType = CreateInstance<CustomCarType>();
            carType.id = settings.ID!;
            carType.name = $"{settings.ID}_cartype";
            carType.author = settings.Author!;
            carType.KindSelection = settings.Kind;
            carType.NameTranslations = TranslationData.Default(settings.Name!);
            carType.carIdPrefix = GetCarIdPrefix(settings.Name!, settings.Kind);
            carType.mass = 25000;
            carType.wheelRadius = 0.459f;

            bool isLoco = settings.Kind == DVTrainCarKind.Loco;
            carType.brakes = new CustomCarType.BrakesSetup()
            {
                hasCompressor = isLoco,
                brakeValveType = isLoco ? CustomCarType.BrakesSetup.TrainBrakeType.SelfLap : CustomCarType.BrakesSetup.TrainBrakeType.None,
                hasIndependentBrake = isLoco,
                hasHandbrake = true,
            };

            // create livery
            var livery = CreateInstance<CustomCarVariant>();
            livery.id = settings.ID;
            livery.name = $"{settings.ID}_livery";
            livery.parentType = carType;
            livery.NameTranslations = TranslationData.Default(settings.Name!);
            livery.prefab = prefab;
            livery.externalInteractablesPrefab = interactables;
            livery.BufferType = GetBufferFromBase(settings.BaseCarType);
            livery.FrontBogie = GetBogieFromBase(settings.BaseCarType);
            livery.RearBogie = livery.FrontBogie;

            string liveryPath = Path.Combine(relativeCarFolder, $"{settings.ID}_livery.asset");
            AssetDatabase.CreateAsset(livery, liveryPath);
            var savedLivery = AssetDatabase.LoadAssetAtPath<CustomCarVariant>(liveryPath);

            carType.liveries = new List<CustomCarVariant>() { savedLivery };
            carType.ForceValidation();

            string carTypePath = Path.Combine(relativeCarFolder, $"{settings.ID}_cartype.asset");
            AssetDatabase.CreateAsset(carType, carTypePath);
            EditorSceneManager.SaveScene(scene, scenePath);

            savedLivery.parentType = carType;
            Selection.activeObject = carType;

            AssetHelper.SaveAsset(savedLivery);
        }

        private static GameObject CreateFreightInteractables(string carId, string relativeCarFolder)
        {
            string interactablesPath = Path.Combine(relativeCarFolder, $"{carId}_interactables.prefab");
            AssetDatabase.CopyAsset(FREIGHT_INTERACTABLES_PATH, interactablesPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(interactablesPath);

            PrefabUtility.InstantiatePrefab(prefab);
            return prefab;
        }

        private static string GetCarIdPrefix(string name, DVTrainCarKind kind) => kind switch
        {
            DVTrainCarKind.Car => GetInitials(name),
            _ => "-",
        };

        private static string GetInitials(string value)
        {
            return string.Concat(value
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Length >= 1 && char.IsLetter(x[0]))
                .Select(x => char.ToUpper(x[0])));
        }

        private static BogieType GetBogieFromBase(BaseTrainCarType carType)
        {
            switch (carType)
            {
                case BaseTrainCarType.LocoDE2:
                    return BogieType.DE2;
                case BaseTrainCarType.LocoS282:
                    return BogieType.S282;
                case BaseTrainCarType.LocoDE6:
                case BaseTrainCarType.LocoDE6Slug:
                    return BogieType.DE6;
                case BaseTrainCarType.LocoDH4:
                    return BogieType.DH4;
                case BaseTrainCarType.LocoMicroshunter:
                    return BogieType.Microshunter;
                case BaseTrainCarType.Handcar:
                    return BogieType.Handcar;

                case BaseTrainCarType.Custom:
                    return BogieType.Custom;
                default:
                    return BogieType.Default;
            }
        }

        private static BufferType GetBufferFromBase(BaseTrainCarType carType)
        {
            switch (carType)
            {
                case BaseTrainCarType.Boxcar:
                    return BufferType.Buffer02;

                case BaseTrainCarType.LocoDE2:
                case BaseTrainCarType.LocoDH4:
                case BaseTrainCarType.LocoDM3:
                case BaseTrainCarType.Stock:
                    return BufferType.Buffer03;

                case BaseTrainCarType.LocoS060:
                case BaseTrainCarType.Gondola:
                    return BufferType.Buffer04;

                case BaseTrainCarType.LocoDE6:
                case BaseTrainCarType.LocoDE6Slug:
                case BaseTrainCarType.LocoMicroshunter:
                    return BufferType.Buffer05;

                case BaseTrainCarType.BoxcarMilitary:
                case BaseTrainCarType.Refrigerator:
                    return BufferType.Buffer06;

                case BaseTrainCarType.Caboose:
                case BaseTrainCarType.TankGas:
                case BaseTrainCarType.TankOil:
                case BaseTrainCarType.TankChem:
                case BaseTrainCarType.TankFood:
                    return BufferType.Buffer07;

                case BaseTrainCarType.Passenger:
                    return BufferType.Buffer08;

                case BaseTrainCarType.LocoS282:
                    return BufferType.S282A;

                case BaseTrainCarType.S282Tender:
                    return BufferType.S282B;

                case BaseTrainCarType.Custom:
                    return BufferType.Custom;

                default:
                    return BufferType.Buffer09;
            }
        }

        private class CarSettings
        {
            public DVTrainCarKind Kind;
            public string? ID;
            public string? Name;
            public string? Author;
            public BaseTrainCarType BaseCarType = BaseTrainCarType.Flatbed;

            public bool IsValid =>
                !string.IsNullOrWhiteSpace(ID) &&
                !string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(Author);
        }
    }
}