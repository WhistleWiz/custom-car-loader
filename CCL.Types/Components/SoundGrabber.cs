﻿using System.Collections.Generic;
using UnityEngine;

namespace CCL.Types.Components
{
    [AddComponentMenu("CCL/Components/Grabbers/Sound Grabber")]
    public class SoundGrabber : VanillaResourceGrabber<AudioClip>
    {
        // Last update B99.6
        public static HashSet<string> SoundNames = new HashSet<string>
        {
            "AirPressureExternal_01",
            "AirPressureInternal_01",
            "BookPageFlip_01",
            "BookPageFlip_02",
            "BookPageFlip_03",
            "Boombox_ButtonSwitch_01",
            "Boombox_CassetteInsert_01",
            "Boombox_CassetteRemove_01",
            "Boombox_KnobNotch_01",
            "Boombox_KnobNotch_02",
            "Boombox_RadioStatic_01",
            "Boombox_TrayClose",
            "Boombox_TrayOpen",
            "BrakeCylinderReleaseEmpty_01",
            "BrakeCylinderReleaseFull_01",
            "BrakeShoes_01",
            "BrakeShoes_02",
            "BrakeShoes_03",
            "BrakeShoes_04",
            "BrakeShoesOverheated_01",
            "Briefcase_Close",
            "Briefcase_Open",
            "ButtonClick_01_Radio",
            "ButtonClick_02_Push",
            "ChassisRattle_01",
            "ChassisRattle_02",
            "CommsRadio_Apply_01",
            "CommsRadio_Cancel_01",
            "CommsRadio_Confirm_01",
            "CommsRadio_Hover_01",
            "CommsRadio_Message_01",
            "CommsRadio_RerailDrop_01",
            "CommsRadio_RerailLift_01",
            "CommsRadio_Select_01",
            "Compressor_01",
            "Compressor_02",
            "Compressor_03",
            "Compressor_04_Internal",
            "CompressorSpit_01",
            "CompressorSpit_02",
            "CompressorSpit_03",
            "ContactorOff_01",
            "ContactorOn_01",
            "Coupler_ChainCouple_01",
            "Coupler_ChainDrop_01",
            "Coupler_ChainScrew_01",
            "Coupler_ChainStore_01",
            "Coupler_ChainTake_01",
            "Coupler_ChainUncouple_01",
            "Coupler_ChainUnscrew_01",
            "Coupler_CockCloseEqualized_01",
            "Coupler_CockOpenEqualized_01",
            "Coupler_CockOpenToHose_01",
            "Coupler_CockOpenToTrain_01",
            "Coupler_HoseAttach_01",
            "Coupler_HoseAttachPressure_01",
            "Coupler_HoseDetach_01",
            "Coupler_HoseDetachPressure_01",
            "Coupler_MUAttach_01",
            "Coupler_MUDetach_01",
            "DashWarning_01_Bell",
            "DashWarning_02_Buzz",
            "DashWarning_03_BuzzMultitone",
            "DashWarning_05_Beep",
            "DashWarning_07_BuzzContinuous",
            "Derailment_TerrainDrag_01",
            "DriveShaft_03",
            "ElectricWhine_01",
            "ElectricWhine_02",
            "ElectricWhine_03",
            "Env-Chicken-Clucking1",
            "Env-Chicken-Clucking2",
            "Env-Chicken-Clucking3",
            "Env-Chicken-Clucking4",
            "Env-Chicken-Clucking5",
            "Env-Chicken-Clucking6",
            "Env-Chicken-Clucking7",
            "Env-Chicken-Die",
            "Env-Chicken-Pregnant1",
            "Env-Chicken-Rooster1",
            "Env-Chicken-Rooster2",
            "Env-Chicken-Rooster3",
            "Env-Chicken-Rooster4",
            "Env-Chicken-Rooster5",
            "Env-Collision-MetalGate7",
            "Env-Cow-Die",
            "Env-Cow-Moo1",
            "Env-Cow-Moo2",
            "Env-Cow-Moo3",
            "Env-Cow-Moo4",
            "Env-Goat-Bleat1",
            "Env-Goat-Bleat2",
            "Env-Goat-Bleat3",
            "Env-Goat-Bleat4",
            "Env-Goat-Bleat5",
            "Env-Goat-Die",
            "Env-Pig-Die",
            "Env-Pig-Oink1",
            "Env-Pig-Oink10",
            "Env-Pig-Oink2",
            "Env-Pig-Oink3",
            "Env-Pig-Oink4",
            "Env-Pig-Oink5",
            "Env-Pig-Oink6",
            "Env-Pig-Oink7",
            "Env-Pig-Oink8",
            "Env-Pig-Oink9",
            "Env-Sheep-Bleat1",
            "Env-Sheep-Bleat2",
            "Env-Sheep-Bleat3",
            "Env-Sheep-Die",
            "ExplosionDebris_01",
            "ExplosionFar_01",
            "ExplosionMid_01",
            "ExplosionNear_01",
            "Fan_01",
            "FireCrackling_01",
            "FireCrackling_02",
            "FluidCoupler_01",
            "FluidCoupler_02_EQ",
            "FluidCouplerExplosion_01",
            "Folder_Close",
            "Folder_Open",
            "GadgetWarning_AmpLimiter_Active",
            "GadgetWarning_AntiWheelslipComputer_Active",
            "GadgetWarning_AntiWheelslipComputer_Enable",
            "GadgetWarning_AutomaticTrainStop_TimeoutDelay",
            "GadgetWarning_BrakeCylinderLEDBar_Blink",
            "GadgetWarning_DefectDetector_Defect",
            "GadgetWarning_DistanceTracker_Timeout2",
            "GadgetWarning_OverheatingProtection_Active",
            "GadgetWarning_OverheatingProtection_Enable",
            "GadgetWarning_ProximitySensor_Done",
            "GadgetWarning_ProximitySensor_Near",
            "GadgetWarning_ProximitySensor_Undone",
            "GadgetWarning_SwitchSetter_Detect",
            "GadgetWarning_SwitchSetter_Switch",
            "GadgetWarning_WirelessMUController_Conflict",
            "GadgetWarning_WirelessMUController_Connect",
            "GearChange_01",
            "GearChangeSpit_03",
            "GearExplosion_01",
            "GearGrind_02",
            "gui_alert",
            "gui_click",
            "gui_hover",
            "Horn_02_Power",
            "Horn_03_Tone",
            "Horn_LocoDE2_01",
            "Horn_LocoDE2_01_Pulse",
            "Horn_LocoDE6_01",
            "Horn_LocoDE6_01_Pulse",
            "horn_LocoDH4_01",
            "Horn_LocoDM3_01",
            "Horn_Microshunter_01",
            "Hose_Plug_01",
            "Hose_Unplug_01",
            "ICE_Broken_01",
            "ICE_CompressionBrake_01",
            "ICE_FuelCutoff_01",
            "ICE_LocoDE2_01_Engine",
            "ICE_LocoDE2_01_Idle",
            "ICE_LocoDE2_02_Engine",
            "ICE_LocoDE6_01",
            "ICE_LocoDE6_02_Engine",
            "ICE_LocoDE6_02_Exhaust",
            "ICE_LocoDH4_01_Engine",
            "ICE_LocoDH4_01_Exhaust",
            "ICE_LocoDH4_01_Idle",
            "ICE_LocoDM1U-150_01_EngineEQ",
            "ICE_LocoDM1U-150_01_ExhaustEQ",
            "ICE_LocoDM1U-150_01_Idle",
            "ICE_LocoDM1U-150_02_Exhaust",
            "ICE_LocoDM3_Engine_01",
            "ICE_LocoDM3_Exhaust_01",
            "ICE_Starter_01",
            "ICE_TurboWhine_02",
            "interface_alarm_01",
            "interface_alert_01",
            "interface_error_01",
            "interface_money_01",
            "interface_money_02",
            "interface_print_01",
            "interface_print_error_01",
            "interface_win_01",
            "Item_LanternAttach_01",
            "Item_LanternDetach_01",
            "Item_LanternHit_01",
            "Item_ProximitySensor_Attach",
            "Item_ProximitySensor_Detach",
            "ItemCollision_BookHeavy_01",
            "ItemCollision_BookLight_01",
            "ItemCollision_BottleGlass_01",
            "ItemCollision_BottlePlastic_03",
            "ItemCollision_BoxCarboard_01",
            "ItemCollision_CapPlastic_01",
            "ItemCollision_Coal_01",
            "ItemCollision_Coin_01",
            "ItemCollision_Coin_02",
            "ItemCollision_Coin_03",
            "ItemCollision_CratePlastic_01",
            "ItemCollision_CrateWooden_01",
            "ItemCollision_CupPlastic_01",
            "ItemCollision_CupPlastic_02",
            "ItemCollision_DeviceKeyboard_01",
            "ItemCollision_DevicePlastic_01",
            "ItemCollision_Eraser_01",
            "ItemCollision_Eraser_02",
            "ItemCollision_MetalBox",
            "ItemCollision_MetalLight_01",
            "ItemCollision_MetalSheet_02",
            "ItemCollision_MetalSheet_03",
            "ItemCollision_MetalSheet_04",
            "ItemCollision_Money_01",
            "ItemCollision_PenPlastic_01",
            "ItemCollision_RulerPlastic_01",
            "ItemCollision_Wallet_01",
            "LabelMaker_Create",
            "LabelMaker_Remove",
            "LeverDrag_01_MetalLightGrind",
            "LeverDrag_02_MetalLightDirt",
            "LeverDrag_03_MetalLightGrind",
            "LeverDrag_04_MetalLightGrind",
            "LeverDrag_05_MetalMediumGrind",
            "LeverDrag_06_MetalLightSqueak",
            "LeverDrag_07_MetalHeavyLubricated",
            "LeverDrag_08_MetalLightDirt",
            "LeverDrag_09_MetalSqueak",
            "LeverDrag_10_MetalSqueak",
            "LeverDrag_11_MegaSqueak",
            "LeverLimit_01_PlasticLight",
            "LeverLimit_02_RubberLight",
            "LeverLimit_03_MountLight",
            "LeverLimit_04_MetalMedium",
            "LeverLimit_05_MetalMedium",
            "LeverLimit_06_MetalHeavy",
            "LeverLimit_07_MetalSolid",
            "LeverLimit_08_MetalLight",
            "LeverLimit_09_Door",
            "LeverLimit_10_MetalStrong",
            "LeverNotch_01_Deep",
            "LeverNotch_04_Tick",
            "LeverNotch_05_Tick",
            "LeverNotch_06_Tick",
            "LeverNotch_07_Deep",
            "LeverNotch_08_Deep",
            "LeverNotch_09_Tick",
            "LeverNotch_10_Deep",
            "LeverNotch_11_Deep",
            "LeverNotch_12_DeepMetal",
            "LeverNotch_15_DeepMetal",
            "LighterClose_01",
            "LighterFlint_01",
            "LighterOpen_01",
            "LocoRemote_Charging_01",
            "LocoRemote_CoupleRange_01",
            "LocoRemote_TurnOff_01",
            "LocoRemote_TurnOn_01",
            "LocoRemote_Warning_01",
            "MetalCrate_Close",
            "MetalCrate_Open",
            "MountCollision",
            "Nameplate_Install",
            "OilCup_Close_01",
            "OilCup_Open_01",
            "PaintCan_Collision_01",
            "PaintCan_CollisionEmpty_01",
            "PaintCan_Detach_01",
            "PaintCan_Empty_01",
            "PaintCan_Install_01",
            "PaintSprayer_Spray_01",
            "radio_static",
            "Rain-Roof1",
            "Registrator_Close",
            "Registrator_Open",
            "ServicePoint_Full_01_Bell",
            "sfx_locodiesel_engine_gear_mech",
            "Shovel_Collision_01",
            "Shovel_Load_01",
            "Shovel_Unload_01",
            "Steam_AirFlow_01",
            "Steam_AirPump_01",
            "Steam_BellPump_01",
            "Steam_Blowback_01",
            "Steam_BoilerExplosion_01",
            "Steam_ChuffAshLoop2s_01",
            "Steam_ChuffL_01",
            "Steam_ChuffL_02",
            "Steam_ChuffL_03",
            "Steam_ChuffL_04",
            "Steam_ChuffLoop10.67s_01",
            "Steam_ChuffLoop16s_01",
            "Steam_ChuffLoop2.67s_01",
            "Steam_ChuffLoop3s_01",
            "Steam_ChuffLoop4s_01",
            "Steam_ChuffLoop5.33s_01",
            "Steam_ChuffLoop8s_01",
            "Steam_ChuffM_01",
            "Steam_ChuffM_02",
            "Steam_ChuffM_03",
            "Steam_ChuffM_04",
            "Steam_ChuffS_01",
            "Steam_ChuffS_02",
            "Steam_ChuffS_03",
            "Steam_ChuffS_04",
            "Steam_ChuffWaterLoop16s_01",
            "Steam_ChuffWaterLoop2s_01",
            "Steam_ChuffWaterLoop4s_01",
            "Steam_CylinderCock_02",
            "Steam_CylinderCrack_01",
            "Steam_Dynamo_01",
            "Steam_Injector_01",
            "Steam_Lubrication_OilPour_01",
            "Steam_RunningGear_Grind_01",
            "Steam_ValveGear16s_02",
            "Steam_ValveGear1s_02",
            "Steam_ValveGear2s_02",
            "Steam_ValveGear4s_02",
            "Steam_ValveGear8s_02",
            "SteamRelease_01",
            "Switch_02_Tick",
            "Switch_03_Tick",
            "Switch_04_Tick",
            "Switch_05_Solid",
            "Switch_06_Solid",
            "Switch_07_Plastic",
            "Switch_08_Electric",
            "Switch_09_Solid",
            "Switch_10_Solid",
            "TEMP_wop_p51_wind",
            "TM_Blow_01",
            "TM_DE2_02",
            "TM_DE6_01",
            "TM_DE6_02",
            "TM_E2_01",
            "TM_Explosion_01",
            "TM_Sparking_01",
            "Tool_CrimpingTool_Crimp_01",
            "Tool_CrimpingTool_Wiring_01",
            "Tool_Drill_Bit_01",
            "Tool_Drill_Fan_01",
            "Tool_Drill_Motor_01",
            "Tool_DuctTape_Stick_01",
            "Tool_FillerGun_Fill_01",
            "Tool_Hammer_Hit_01",
            "Tool_SolderingGun_Load_01",
            "Tool_SolderingGun_Solder_01",
            "Tool_SolderingGun_Unload_01",
            "Toolbox_Close",
            "Toolbox_Open",
            "Train_Bell-DE6_01",
            "Train_Bell-DH4_01",
            "Train_Bell-S060_01",
            "Train_Bell-S282_01",
            "Train_Chassis_Rattle_03",
            "train_engine_layer_throttle",
            "UI_InventoryMoney_01",
            "UI_InventoryOut_01",
            "watch_button",
            "watch_notch",
            "watch_ring",
            "watch_ticking",
            "WaterDump_01",
            "WaterDump_02",
            "Wheels_DamagedFast_01",
            "Wheels_DamagedSlow_01",
            "Wheels_Roll_05_Near",
            "Wheels_Roll_06",
            "Wheels_Roll_07_Locomotive",
            "Wheels_Wheelslide_01",
            "Wheels_Wheelslip_01",
            "Wheels_Wheelslip_02",
            "Whistle_03_Hoarse",
            "Whistle_07_Breathy",
            "Whistle_08_Sine",
            "WindowGlassBreak_01",
            "WiperGlassDry_01",
            "WiperGlassWet_01",
            "WiperMotor_01",
            "WiperMotor_02",
            "WiperWhip_01",
        };

        public override HashSet<string> GetNames()
        {
            return SoundNames;
        }
    }
}
