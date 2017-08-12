﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace Buttplug.Apps.XInputInjector.Interface
{
    [Serializable]
    public struct Vibration
    {
        public ushort LeftMotorSpeed;
        public ushort RightMotorSpeed;
    }


    public class ButtplugXInputInjectorInterface : MarshalByRefObject
    {
        // This will be used as a singleton in the IPC Server, and we should only ever have one process hooked 
        // with this interface. Just make the EventHandler static so we can attach as needed from anywhere.
        public static event EventHandler<Vibration> VibrationCommandReceived;
        public static event EventHandler<Exception> VibrationExceptionReceived;
        public static event EventHandler<string> VibrationPingMessageReceived;
        public static event EventHandler<bool> VibrationExitReceived;
        private static bool _shouldStop;

        private Timer _exitTimer = new Timer();
        private bool _hasPinged = false;
        public ButtplugXInputInjectorInterface()
        {
            // Every time we create a new instance, reset the static stopping variable.
            _shouldStop = false;
            // Check 4 times a second
            _exitTimer.Interval = 250;
            _exitTimer.Enabled = true;
            _exitTimer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object aObj, EventArgs aEventArgs)
        {
            if (!_hasPinged || _shouldStop)
            {
                _exitTimer.Enabled = false;
                Exit();
                return;
            }
            _hasPinged = false;
        }

        public void Exit()
        {
            VibrationExitReceived?.Invoke(this, true);
        }

        public static void Detach()
        {
            _shouldStop = true;
        }

        public void Report(Int32 aPid, Queue<Vibration> aCommands)
        {
            foreach (var command in aCommands)
            {
                VibrationCommandReceived?.Invoke(this, command);
            }
        }

        public void ReportError(Int32 aPid, Exception aEx)
        {
            VibrationExceptionReceived?.Invoke(this, aEx);
        }

        public bool Ping(Int32 aPid, string aMsg)
        {
            _hasPinged = true;
            if (aMsg.Length > 0)
            {
                VibrationPingMessageReceived?.Invoke(this, aMsg);
            }
            return !_shouldStop;
        }
    }
}