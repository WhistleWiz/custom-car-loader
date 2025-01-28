﻿using System;

namespace CCL.Types
{
    public static class IdV2
    {
        // ============
        // Last update:
        // B99.3
        // ============

        public static readonly string[] CarKinds =
        {
            "Car",
            "Loco",
            "Tender",
            "Slug",
            "Caboose",
        };
        public static readonly string[] CarTypes =
        {
            "Autorack",
            "Boxcar",
            "BoxcarMilitary",
            "Caboose",
            "Flatbed",
            "FlatbedMilitary",
            "FlatbedStakes",
            "FlatbedShort",
            "Gondola",
            "HandCar",
            "Hopper",
            "HopperCovered",
            "LocoDE2",
            "LocoDE6",
            "LocoDE6Slug",
            "LocoDH4",
            "LocoDM3",
            "LocoS282A",
            "LocoS282B",
            "LocoS060",
            "NuclearFlask",
            "Passenger",
            "Stock",
            "Refrigerator",
            "TankChem",
            "TankGas",
            "TankOil",
            "TankShortFood",
            "LocoMicroshunter",
            "LocoDM1U"
        };
        public static readonly string[] CarLiveries =
        {
            "LocoDE2",
            "LocoS282A",
            "LocoS282B",
            "LocoS060",
            "LocoDM1U",
            "LocoDE6",
            "LocoDE6Slug",
            "LocoDH4",
            "LocoDM3",
            "LocoMicroshunter",
            "FlatbedEmpty",
            "FlatbedStakes",
            "FlatbedMilitary",
            "FlatbedShort",
            "AutorackRed",
            "AutorackBlue",
            "AutorackGreen",
            "AutorackYellow",
            "TankOrange",
            "TankWhite",
            "TankYellow",
            "TankBlue",
            "TankChrome",
            "TankBlack",
            "TankShortMilk",
            "StockRed",
            "StockGreen",
            "StockBrown",
            "BoxcarBrown",
            "BoxcarGreen",
            "BoxcarPink",
            "BoxcarRed",
            "BoxcarMilitary",
            "RefrigeratorWhite",
            "HopperBrown",
            "HopperTeal",
            "HopperYellow",
            "HopperCoveredBrown",
            "GondolaRed",
            "GondolaGreen",
            "GondolaGray",
            "PassengerRed",
            "PassengerGreen",
            "PassengerBlue",
            "HandCar",
            "CabooseRed",
            "NuclearFlask"
        };

        public static readonly string[] Cargos =
        {
            "Coal",
            "IronOre",
            "CrudeOil",
            "Diesel",
            "Gasoline",
            "Methane",
            "Logs",
            "Boards",
            "Plywood",
            "RailwaySleepers",
            "Wheat",
            "Corn",
            "SunflowerSeeds",
            "Flour",
            "Pigs",
            "Cows",
            "Poultry",
            "Sheep",
            "Goats",
            "Fish",
            "Bread",
            "DairyProducts",
            "MeatProducts",
            "CannedFood",
            "CatFood",
            "TemperateFruits",
            "Vegetables",
            "Milk",
            "Eggs",
            "Cotton",
            "Wool",
            "TropicalFruits",
            "SteelRolls",
            "SteelBillets",
            "SteelSlabs",
            "SteelBentPlates",
            "SteelRails",
            "ScrapMetal",
            "WoodScrap",
            "WoodChips",
            "ScrapContainers",
            "ElectronicsIskar",
            "ElectronicsKrugmann",
            "ElectronicsAAG",
            "ElectronicsNovae",
            "ElectronicsTraeg",
            "ToolsIskar",
            "ToolsBrohm",
            "ToolsAAG",
            "ToolsNovae",
            "ToolsTraeg",
            "Furniture",
            "Pipes",
            "ClothingObco",
            "ClothingNeoGamma",
            "ClothingNovae",
            "ClothingTraeg",
            "Medicine",
            "ChemicalsIskar",
            "ChemicalsSperex",
            "NewCars",
            "ImportedNewCars",
            "Tractors",
            "Excavators",
            "MiningTrucks",
            "CityBuses",
            "SemiTrailers",
            "CraneParts",
            "Trams",
            "ForestryTrailers",
            "Alcohol",
            "Acetylene",
            "CryoOxygen",
            "CryoHydrogen",
            "Argon",
            "Nitrogen",
            "Ammonia",
            "SodiumHydroxide",
            "AmmoniumNitrate",
            "SpentNuclearFuel",
            "Ammunition",
            "Biohazard",
            "Tanks",
            "MilitaryTrucks",
            "MilitarySupplies",
            "AttackHelicopsters",
            "Missiles",
            "MilitaryCars",
            "TrainPartsDE2",
            "TrainPartsDE6",
            "TrainPartsDH4",
            "TrainPartsDM3",
            "TrainPartsS060",
            "TrainPartsS282A",
            "EmptySunOmni",
            "EmptyIskar",
            "EmptyObco",
            "EmptyGoorsk",
            "EmptyKrugmann",
            "EmptyBrohm",
            "EmptyAAG",
            "EmptySperex",
            "EmptyNovae",
            "EmptyTraeg",
            "EmptyChemlek",
            "EmptyNeoGamma"
        };

        public static readonly string[] GeneralLicenses =
        {
            "TrainDriver",
            "DE2",
            "DE6",
            "SH282",
            "S060",
            "DH4",
            "DM3",
            "ManualService",
            "ConcurrentJobs1",
            "ConcurrentJobs2",
            "MultipleUnit",
            "Dispatcher1",
            "MuseumCitySouth"
        };
        public static readonly string[] JobLicenses =
        {
            "Basic",
            "Fragile",
            "Hazmat1",
            "Hazmat2",
            "Hazmat3",
            "Military1",
            "Military2",
            "Military3",
            "FreightHaul",
            "Shunting",
            "LogisticalHaul",
            "TrainLength1",
            "TrainLength2"
        };

        // Not really an ID but oh well.
        public static readonly string[] Paints =
        {
            "DVRT",
            "DVRT_New",
            "Null",
            "Relic",
            "Relic_Rusty",
        };

        public static string[] AllLicenses
        {
            get
            {
                var all = new string[GeneralLicenses.Length + JobLicenses.Length];
                GeneralLicenses.CopyTo(all, 0);
                JobLicenses.CopyTo(all, GeneralLicenses.Length);
                return all;
            }
        }
    }
}
