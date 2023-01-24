﻿using System.Collections;
using UnityEngine;
using DV.Util.EventWrapper;
using CCL_GameScripts.CabControls;
using System;
using System.Collections.Generic;

namespace DVCustomCarLoader.LocoComponents
{
    public class CustomFuseController : MonoBehaviour, ILocoEventProvider, ICabControlAcceptor
    {
        public LocoEventManager EventManager { get; set; }
        public IEnumerable<WatchableValue> Watchables => null;

        protected IFusedLocoController locoController;

        public List<CabInputRelay> SideFuses { get; protected set; } = new List<CabInputRelay>();
        public CabInputRelay MainFuse { get; protected set; } = null;

        protected Coroutine MainFuseOffRoutine = null;

        protected const float SWITCH_THRESHOLD = 0.5f;
        protected const float LATE_INIT_DELAY = 0.5f;
        protected const float MAIN_BREAKER_DELAY = 0.2f;

        protected bool StartupComplete = false;

        protected bool AreAllSideFusesOn()
        {
            return SideFuses.TrueForAll(fuse => fuse.Value > SWITCH_THRESHOLD);
        }

        protected bool IsMainFuseOn() => MainFuse.Value > SWITCH_THRESHOLD;

        public void SetMasterPower( bool on )
        {
            float relayPos = on ? 1 : 0;

            foreach( CabInputRelay sideFuse in SideFuses )
            {
                sideFuse.Value = relayPos;
            }

            MainFuse.Value = relayPos;
            SetLocoPowerState(on);
        }

        public void TryStarter()
        {
            if( !(AreAllSideFusesOn() && IsMainFuseOn()) ) return;

            if( locoController.CanEngineStart )
            {
                locoController.EngineRunning = true;
                // TODO: VRTK haptics
            }
        }

        public void KillEngine()
        {
            locoController.EngineRunning = false;
        }

        protected void SetLocoPowerState(bool newState)
        {
            EventManager.Dispatch(this, SimEventType.PowerOn, newState);
            locoController.MasterPower = newState;
        }

        public void Start()
        {
            var car = TrainCar.Resolve(gameObject);
            if( car == null || !car )
            {
                Main.Error($"Couldn't find TrainCar for interior {gameObject.name}");
                return;
            }

            locoController = car.gameObject.GetComponentByInterface<IFusedLocoController>();
            if( locoController == null )
            {
                Main.Error("Couldn't find loco controller for fuse box");
                return;
            }

            StartupComplete = false;
            StartCoroutine(DelayedEnable());
        }

        private IEnumerator DelayedEnable()
        {
            yield return WaitFor.SecondsRealtime(LATE_INIT_DELAY);
            SetMasterPower(locoController.EngineRunning);
            StartupComplete = true;
            yield break;
        }

        private IEnumerator DelayedMainFuseOff()
        {
            yield return WaitFor.SecondsRealtime(MAIN_BREAKER_DELAY);
            if( !AreAllSideFusesOn() )
            {
                SetLocoPowerState(false);
                MainFuse.Value = 0;
            }

            MainFuseOffRoutine = null;
            yield break;
        }

        //-------------------------------------------------------------------------------------
        #region ICabControlAcceptor

        protected void OnSideFuseChanged( float newVal )
        {
            if (!StartupComplete) return;
            if( newVal <= SWITCH_THRESHOLD )
            {
                if( locoController.EngineRunning ) KillEngine();
                if( MainFuseOffRoutine == null )
                {
                    MainFuseOffRoutine = StartCoroutine(DelayedMainFuseOff());
                }
            }
        }

        protected void OnMainFuseChanged( float newVal )
        {
            if (!StartupComplete) return;
            bool nowOn = (newVal > SWITCH_THRESHOLD);

            if( nowOn )
            {
                if( !AreAllSideFusesOn() )
                {
                    if( MainFuseOffRoutine == null )
                    {
                        MainFuseOffRoutine = StartCoroutine(DelayedMainFuseOff());
                    }
                    return;
                }
                SetLocoPowerState(nowOn);
            }
            else
            {
                // turned off
                SetLocoPowerState(nowOn);
                KillEngine();
            }
        }

        protected void OnStarterChanged( float newVal )
        {
            if (!StartupComplete) return;
            if( (newVal > SWITCH_THRESHOLD) && !locoController.EngineRunning )
            {
                TryStarter();
            }
            else if( (newVal < -SWITCH_THRESHOLD) && locoController.EngineRunning )
            {
                // handle control that is mapped into negative values as shutoff switch
                KillEngine();
            }
        }

        protected void OnEStopChanged( float newVal )
        {
            if (!StartupComplete) return;
            if( (newVal > SWITCH_THRESHOLD) && locoController.EngineRunning )
            {
                KillEngine();
            }
        }

        public bool AcceptsControlOfType( CabInputType inputType )
        {
            return inputType.EqualsOneOf(
                CabInputType.Fuse,
                CabInputType.MainFuse,
                CabInputType.Starter,
                CabInputType.EngineStop
            );
        }

        public void RegisterControl( CabInputRelay inputRelay )
        {
            switch( inputRelay.Binding )
            {
                case CabInputType.Fuse:
                    SideFuses.Add(inputRelay);
                    inputRelay.SetIOHandlers(OnSideFuseChanged);
                    break;

                case CabInputType.MainFuse:
                    MainFuse = inputRelay;
                    inputRelay.SetIOHandlers(OnMainFuseChanged);
                    break;

                case CabInputType.Starter:
                    inputRelay.SetIOHandlers(OnStarterChanged);
                    break;

                case CabInputType.EngineStop:
                    inputRelay.SetIOHandlers(OnEStopChanged);
                    break;
            }
        }

        #endregion

        public void ForceDispatchAll()
        {
            if (locoController != null) EventManager.Dispatch(this, SimEventType.PowerOn, locoController.MasterPower);
        }
    }
}