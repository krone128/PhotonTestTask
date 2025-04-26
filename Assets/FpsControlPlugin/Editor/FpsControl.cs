// Copyright
// Vlad Taranenko 2025
// Free to use

using System.Threading;
using System.Timers;
using UnityEditor;
using UnityEngine;
using Timer = System.Timers.Timer;

namespace FpsLimiterPlugin.Editor
{
    //To add custom FPS cap, add new value to enum and copy-paste MenuItem function pair in On Power and On Battery regions
    public enum FpsMode
    {
        Unlimited = 0,
        _30 = 30,
        _60 = 60,
        _120 = 120,
        _144 = 144,
        _240 = 240,
        _360 = 360,
        _480 = 480,
    }
    
    /// <summary>
    /// This class adds "Tools/FPS Control" menu in Unity Editor,
    /// allowing to set Application.targetFramerate cap in Editor for project
    /// If current machine has battery power option, it allows to set different FPS cap when on battery power
    /// Configuration is stored in EditorPrefs
    /// </summary>
    public static class FpsControl
    {
        const string KeyPrefix = "editorFpsControl.";
        const string FpsModeKey = KeyPrefix + "fpsMode";
        const string BatterySaverFpsModeKey = KeyPrefix + "batterySaverFpsMode";
        const string ConsoleLogsEnabledKey = KeyPrefix + "enableConsoleLogs";
        const string PluginEnabledKey = KeyPrefix + "pluginEnabled";
        
        const double BatteryStatePollingRate = 5;
        
        static readonly Timer BatterySaverTimer = new Timer(BatteryStatePollingRate) { AutoReset = true };
        static readonly SynchronizationContext SyncContext = SynchronizationContext.Current;
        static BatteryStatus LAST_BATTERY_STATE = BatteryStatus.Unknown;

        static FpsMode FPSMode
        {
            get => (FpsMode)EditorPrefs.GetInt(FpsModeKey);
            set
            {
                EditorPrefs.SetInt(FpsModeKey, (int)value);
                if (!IsOnBatteryPower) SwitchFPSMode(value);
            }
        }

        static FpsMode BatterySaverFpsMode
        {
            get => (FpsMode)EditorPrefs.GetInt(BatterySaverFpsModeKey);
            set
            {
                EditorPrefs.SetInt(BatterySaverFpsModeKey, (int)value);
                if (IsOnBatteryPower) SwitchFPSMode(value);
                if (BatterySaverEnabled) StartBatteryStatePolling();
                else StopBatteryStatePolling();
            }
        }
        
        static bool ConsoleLogsEnabled
        {
            get => EditorPrefs.GetBool(ConsoleLogsEnabledKey);
            set => EditorPrefs.SetBool(ConsoleLogsEnabledKey, value);
        }
        
        static bool PluginEnabled
        {
            get => EditorPrefs.GetBool(PluginEnabledKey);
            set
            {
                EditorPrefs.SetBool(PluginEnabledKey, value);
                
                if (value) Initialize();
                else
                {
                    StopBatteryStatePolling();
                    SwitchFPSMode(FpsMode.Unlimited);
                }
            }
        }

        static bool BatterySaverEnabled => BatterySaverFpsMode != FpsMode.Unlimited;
        static bool IsOnBatteryPower => SystemInfo.batteryStatus == BatteryStatus.Discharging;
        
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            if(!PluginEnabled) return;
            SwitchFPSMode(IsOnBatteryPower ? BatterySaverFpsMode : FPSMode);
            if (BatterySaverEnabled) StartBatteryStatePolling();
        }

        static void SwitchFPSMode(FpsMode mode)
        {
            Application.targetFrameRate = (int)mode;
            
            LogMsg("Application.targetFrameRate set to " + 
                (Application.targetFrameRate > 0 ? Application.targetFrameRate : "Unlimited"));
        }

        static void StartBatteryStatePolling() 
        {
            if (BatterySaverTimer.Enabled) return;
            BatterySaverTimer.Elapsed += OnBatterySaverTimerElapsed;
            BatterySaverTimer.Start();
        }

        static void StopBatteryStatePolling()
        {
            if (!BatterySaverTimer.Enabled) return;
            BatterySaverTimer.Elapsed -= OnBatterySaverTimerElapsed;
            BatterySaverTimer.Stop();
        }

        static void OnBatterySaverTimerElapsed(object sender, ElapsedEventArgs args)
        {
            SyncContext.Post(_ => CheckForPowerStateUpdate(), null);
        }

        private static void CheckForPowerStateUpdate()
        {
            var status = SystemInfo.batteryStatus;
            if (LAST_BATTERY_STATE == status) return;
            LAST_BATTERY_STATE = status;
            SwitchFPSMode(IsOnBatteryPower ? BatterySaverFpsMode : FPSMode);
            
            LogMsg("Power state changed to " + status);
        }
        
        static void LogMsg(string msg)
        {
            const string prefix = "FpsLimiterPlugin\n";
            if (ConsoleLogsEnabled) Debug.Log(prefix + msg);
        }

        #region MenuItem

        #region On Power

        #region Unlimited

        [MenuItem("Tools/FPS Control/Unlimited", true)]
        static bool ValidateFpsUnlimited()
        {
            Menu.SetChecked("Tools/FPS Control/Unlimited", FPSMode == FpsMode.Unlimited);
            return PluginEnabled;
        }
                        
        [MenuItem("Tools/FPS Control/Unlimited", priority = 40)]
        static void SetFPS_Unlimited()
        {
            FPSMode = FpsMode.Unlimited;
        }
        #endregion

        #region 30 FPS
        [MenuItem("Tools/FPS Control/30 FPS", true)]
        static bool ValidateFps30()
        {
            Menu.SetChecked("Tools/FPS Control/30 FPS", FPSMode == FpsMode._30);
            return PluginEnabled;
        }
                        
        [MenuItem("Tools/FPS Control/30 FPS", priority = 21)]
        static void SetFPS_30()
        {
            FPSMode = FpsMode._30;
        }
        #endregion

        #region 60 FPS
        [MenuItem("Tools/FPS Control/60 FPS", true)]
        static bool ValidateFps60()
        {
            Menu.SetChecked("Tools/FPS Control/60 FPS", FPSMode == FpsMode._60);
            return PluginEnabled;
        }
                        
        [MenuItem("Tools/FPS Control/60 FPS", priority = 22)]
        static void SetFPS_60()
        {
            FPSMode = FpsMode._60;
        }
        #endregion

        #region 120 FPS
        [MenuItem("Tools/FPS Control/120 FPS", true)]
        static bool ValidateFps120()
        {
            Menu.SetChecked("Tools/FPS Control/120 FPS", FPSMode == FpsMode._120);
            return PluginEnabled;
        }
                        
        [MenuItem("Tools/FPS Control/120 FPS", priority = 23)]
        static void SetFPS_120()
        {
            FPSMode = FpsMode._120;
        }
        #endregion

        #region 144 FPS
        [MenuItem("Tools/FPS Control/144 FPS", true)]
        static bool ValidateFps144()
        {
            Menu.SetChecked("Tools/FPS Control/144 FPS", FPSMode == FpsMode._144);
            return PluginEnabled;
        }
                        
        [MenuItem("Tools/FPS Control/144 FPS", priority = 24)]
        static void SetFPS_144()
        {
            FPSMode = FpsMode._144;
        }
        #endregion

        #region 240 FPS
        [MenuItem("Tools/FPS Control/240 FPS", true)]
        static bool ValidateFps240()
        {
            Menu.SetChecked("Tools/FPS Control/240 FPS", FPSMode == FpsMode._240);
            return PluginEnabled;
        }
                        
        [MenuItem("Tools/FPS Control/240 FPS", priority = 25)]
        static void SetFPS_240()
        {
            FPSMode = FpsMode._240;
        }
        #endregion

        #region 360 FPS
        [MenuItem("Tools/FPS Control/360 FPS", true)]
        static bool ValidateFps360()
        {
            Menu.SetChecked("Tools/FPS Control/360 FPS", FPSMode == FpsMode._360);
            return PluginEnabled;
        }
                        
        [MenuItem("Tools/FPS Control/360 FPS", priority = 26)]
        static void SetFPS_360()
        {
            FPSMode = FpsMode._360;
        }
        #endregion

        #region 480 FPS
        [MenuItem("Tools/FPS Control/480 FPS", true)]
        static bool ValidateFps480()
        {
            Menu.SetChecked("Tools/FPS Control/480 FPS", FPSMode == FpsMode._480);
            return PluginEnabled;
        }
                        
        [MenuItem("Tools/FPS Control/480 FPS", priority = 27)]
        static void SetFPS_480()
        {
            FPSMode = FpsMode._480;
        }
        #endregion

        #endregion

        #region On Battery
                
        #region Battery Identical Fps
        [MenuItem("Tools/FPS Control/On Battery/Identical", true)]
        static bool ValidateBatteryFpsUnlimited()
        {
            var batteryStatusAvailable = SystemInfo.batteryStatus != BatteryStatus.Unknown;
            Menu.SetChecked("Tools/FPS Control/On Battery/Identical", BatterySaverFpsMode == FpsMode.Unlimited);
            return PluginEnabled && batteryStatusAvailable;
        }

        [MenuItem("Tools/FPS Control/On Battery/Identical", priority = 0)]
        static void DisableBatterySaver()
        {
            StopBatteryStatePolling();
            BatterySaverFpsMode = FPSMode;
        }
        #endregion
                    
        #region Battery 30 FPS
        [MenuItem("Tools/FPS Control/On Battery/30 FPS", true)]
        static bool ValidateBatteryFps30()
        {
            var batteryStatusAvailable = SystemInfo.batteryStatus != BatteryStatus.Unknown;
            Menu.SetChecked("Tools/FPS Control/On Battery/30 FPS", BatterySaverFpsMode == FpsMode._30);
            return PluginEnabled && batteryStatusAvailable;
        }

        [MenuItem("Tools/FPS Control/On Battery/30 FPS", priority = 11)]
        static void SetBatteryFPS_30()
        {
            BatterySaverFpsMode = FpsMode._30;
        }
        #endregion

        #region Battery 60 FPS
        [MenuItem("Tools/FPS Control/On Battery/60 FPS", true)]
        static bool ValidateBatteryFps60()
        {
            var batteryStatusAvailable = SystemInfo.batteryStatus != BatteryStatus.Unknown;
            Menu.SetChecked("Tools/FPS Control/On Battery/60 FPS", BatterySaverFpsMode == FpsMode._60);
            return PluginEnabled && batteryStatusAvailable;
        }

        [MenuItem("Tools/FPS Control/On Battery/60 FPS", priority = 12)]
        static void SetBatteryFPS_60()
        {
            BatterySaverFpsMode = FpsMode._60;
        }
        #endregion

        #region Battery 12O FPS
        [MenuItem("Tools/FPS Control/On Battery/120 FPS", true)]
        static bool ValidateBatteryFps120()
        {
            var batteryStatusAvailable = SystemInfo.batteryStatus != BatteryStatus.Unknown;
            Menu.SetChecked("Tools/FPS Control/On Battery/120 FPS", BatterySaverFpsMode == FpsMode._120);
            return PluginEnabled && batteryStatusAvailable;
        }

        [MenuItem("Tools/FPS Control/On Battery/120 FPS", priority = 13)]
        static void SetBatteryFPS_120()
        {
            BatterySaverFpsMode = FpsMode._120;
        }
        #endregion

        #region Battery 144 FPS
        [MenuItem("Tools/FPS Control/On Battery/144 FPS", true)]
        static bool ValidateBatteryFps144()
        {
            var batteryStatusAvailable = SystemInfo.batteryStatus != BatteryStatus.Unknown;
            Menu.SetChecked("Tools/FPS Control/On Battery/144 FPS", BatterySaverFpsMode == FpsMode._144);
            return PluginEnabled && batteryStatusAvailable;
        }

        [MenuItem("Tools/FPS Control/On Battery/144 FPS", priority = 14)]
        static void SetBatteryFPS_144()
        {
            BatterySaverFpsMode = FpsMode._144;
        }
        #endregion

        #region Battery 240 FPS
        [MenuItem("Tools/FPS Control/On Battery/240 FPS", true)]
        static bool ValidateBatteryFps240()
        {
            var batteryStatusAvailable = SystemInfo.batteryStatus != BatteryStatus.Unknown;
            Menu.SetChecked("Tools/FPS Control/On Battery/240 FPS", BatterySaverFpsMode == FpsMode._240);
            return PluginEnabled && batteryStatusAvailable;
        }

        [MenuItem("Tools/FPS Control/On Battery/240 FPS", priority = 15)]
        static void SetBatteryFPS_240()
        {
            BatterySaverFpsMode = FpsMode._240;
        }
        #endregion

        #region Battery 360 FPS
        [MenuItem("Tools/FPS Control/On Battery/360 FPS", true)]
        static bool ValidateBatteryFps360()
        {
            var batteryStatusAvailable = SystemInfo.batteryStatus != BatteryStatus.Unknown;
            Menu.SetChecked("Tools/FPS Control/On Battery/360 FPS", BatterySaverFpsMode == FpsMode._360);
            return PluginEnabled && batteryStatusAvailable;
        }

        [MenuItem("Tools/FPS Control/On Battery/360 FPS", priority = 16)]
        static void SetBatteryFPS_360()
        {
            BatterySaverFpsMode = FpsMode._360;
        }
        #endregion

        #region Battery 480 FPS
        [MenuItem("Tools/FPS Control/On Battery/480 FPS", true)]
        static bool ValidateBatteryFps480()
        {
            var batteryStatusAvailable = SystemInfo.batteryStatus != BatteryStatus.Unknown;
            Menu.SetChecked("Tools/FPS Control/On Battery/480 FPS", BatterySaverFpsMode == FpsMode._480);
            return PluginEnabled && batteryStatusAvailable;
        }

        [MenuItem("Tools/FPS Control/On Battery/480 FPS", priority = 17)]
        static void SetBatteryFPS_480()
        {
            BatterySaverFpsMode = FpsMode._480;
        }
        #endregion

        #endregion
        
        #region Enabled
        
        [MenuItem("Tools/FPS Control/Enabled", true)]
        static bool ValidateEnabledToggle()
        {
            Menu.SetChecked("Tools/FPS Control/Enabled", PluginEnabled);
            return true;
        }

        [MenuItem("Tools/FPS Control/Enabled", priority = -60)]
        static void EnablePluginToggle() => PluginEnabled = !PluginEnabled;

        #endregion
        
        #region Logs
        
        [MenuItem("Tools/FPS Control/Console Logs", true)]
        static bool ValidateEnableConsoleLogs()
        {
            Menu.SetChecked("Tools/FPS Control/Console Logs", ConsoleLogsEnabled);
            return PluginEnabled;
        }

        [MenuItem("Tools/FPS Control/Console Logs", priority = 60)]
        static void EnableConsoleLogs() => ConsoleLogsEnabled = !ConsoleLogsEnabled;

        #endregion

        #endregion
    }
}