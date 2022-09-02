using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class UEDevicesGrading : MonoBehaviour
{
    public enum DeviceGrade
    {
        NOT_SUPPORT = -2,
        NONE = -1,
        LOW = 0,
        MEDIUM,
        HIGH,
        VERY_HIGH,
    }

    private UEDeviceGradingConfig m_deviceGradingConfig;
    private DeviceGrade m_grade = DeviceGrade.NONE;

    public delegate void GradeChange(DeviceGrade grade);

    public event GradeChange OnGradeChange;

    public delegate void GradeLog(object o);

    public GradeLog m_logFunction;

    private bool m_init = false;

    private Dictionary<string, int> m_dicGPUInfo = new Dictionary<string, int>();

    private const string DevicesGradingLevelKey = "TAPostProcessLevel";
    private const string DeviceGradingConfigPath = "Assets/ResourcesAssets/DeviceGradingConfig/UEDeviceGradingForGOT.asset";

    public DeviceGrade DeviceGradeInfo
    {
        get { return m_grade; }
    }

    private static UEDevicesGrading s_instance;
    public static UEDevicesGrading Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = FindObjectOfType<UEDevicesGrading>();
            }

            return s_instance;
        }
    }

    /// <summary>
    /// 设备分级初始化
    /// 优先从本地缓存读取，再根据config判断，最后保底通过cpu count\frequency\memory判断
    /// 默认不缓存，只有在设置界面设置手动画质后才缓存
    /// </summary>
    /// <param name="changeFunc">分级结果回调</param>
    /// <param name="logFunc"></param>
    public void Init(GradeChange changeFunc, GradeLog logFunc)
    {
        if (IsInit())
        {
            //防止多次初始化
            return;
        }

        m_init = true;
        //var loader = ResLoader.Alloc();
        //loader.Add2Load(DeviceGradingConfigPath,(bool success, string assetName, object asset) =>
        //{
        //    if (asset == null)
        //    {
        //        if (Debug.unityLogger.logEnabled)
        //        {
        //            Debug.LogError("没有找到设备分级配置->" + DeviceGradingConfigPath);
        //        }
        //        return;
        //    }
        //    m_deviceGradingConfig = asset as UEDeviceGradingConfig;
        //    m_logFunction = logFunc != null ? logFunc : null;
        //    if (changeFunc == null)
        //    {
        //        Log("UEDevicesGrading: should specify changeFunc!");
        //        return;
        //    }
        //    OnGradeChange -= changeFunc;
        //    OnGradeChange += changeFunc;
        //    if (m_deviceGradingConfig == null)
        //    {
        //        Log("UEDevicesGrading: should specify config!");
        //        return;
        //    }

        //    if (CheckPlayerPrefs())
        //    {
        //        Log("UEDevicesGrading: get quality from player prefs, " + m_grade);
        //    }
        //    else if (CheckDeviceGrade())
        //    {
        //        Log("UEDevicesGrading: set quality from device grade, " + m_grade);
        //    }
        //    else if (CheckDeviceGradeByNormal())
        //    {
        //        Log("UEDevicesGrading: set quality from device hardware, " + m_grade);
        //    }
        //    else
        //    {
        //        m_grade = DeviceGrade.LOW;
        //    }
        //    NotifyQuality();
        //});
        //loader.Load();
        
    }

    public bool IsInit()
    {
        return m_init;
    }

    public void SaveQuality2PlayerPrefs(int grade)
    {
        m_grade = (DeviceGrade)grade;
        PlayerPrefs.SetInt(DevicesGradingLevelKey, grade);
    }

    bool CheckDeviceGrade()
    {
        m_deviceGradingConfig.Init();
        m_grade = DeviceGrade.NONE;

#if UNITY_ANDROID && !UNITY_EDITOR
        m_grade = CheckDeviceGradeByConfig();
#elif UNITY_IOS && !UNITY_EDITOR
        m_grade = CheckDeviceGradeByConfig(); 
#else
        m_grade = DeviceGrade.VERY_HIGH;
#endif
        return m_grade != DeviceGrade.NONE;
    }

    private bool CheckPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey(DevicesGradingLevelKey))
        {
            return false;
        }

        int level = PlayerPrefs.GetInt(DevicesGradingLevelKey);
        if (0 == level)
        {
            m_grade = DeviceGrade.HIGH;
        }
        else if (1 == level)
        {
            m_grade = DeviceGrade.MEDIUM;
        }
        else
        {
            m_grade = DeviceGrade.LOW;
        }
        return true;
    }

    private void Log(object o)
    {
        if (m_logFunction != null)
        {
            m_logFunction(o);
        }
    }

    private void NotifyQuality()
    {
        if (OnGradeChange != null)
        {
            OnGradeChange(m_grade);
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    DeviceGrade CheckDeviceGradeByConfig()
    {
        string gpuName = SystemInfo.graphicsDeviceName.ToLower();
        Log("CheckDeviceGradeByConfig——>SystemInfo.graphicsDeviceName : " + gpuName);
        if (m_deviceGradingConfig.DicGPUInfo.ContainsKey(gpuName))
        {
            return (DeviceGrade)m_deviceGradingConfig.DicGPUInfo[gpuName];
        }
        else
        {
            return DeviceGrade.NONE;
        }
    }
#elif UNITY_IOS && !UNITY_EDITOR
    DeviceGrade CheckDeviceGradeByConfig()
    {
        var currentDevice = UnityEngine.iOS.Device.generation;
        if (currentDevice >= m_deviceGradingConfig.m_settings.m_Ranks[3].iosDevice)
        {
            return DeviceGrade.VERY_HIGH;
        }
        else if (currentDevice >= m_deviceGradingConfig.m_settings.m_Ranks[2].iosDevice)
        {
            return DeviceGrade.HIGH;
        }
        else if (currentDevice >= m_deviceGradingConfig.m_settings.m_Ranks[1].iosDevice)
        {
            return DeviceGrade.MEDIUM;
        }
        else
        {
            return DeviceGrade.LOW;
        }
    }
#endif


    bool CheckDeviceGradeByNormal()
    {
        m_grade = DeviceGrade.NONE;

        var coreCount = SystemInfo.processorCount;
        var frequency = SystemInfo.processorFrequency;
        var memorySize = SystemInfo.systemMemorySize;
        Log("CheckDeviceGradeByNormal: corecount " + coreCount + "  frequency: " + frequency + "  memorysize" + memorySize);
        for (int i = m_deviceGradingConfig.m_settings.m_Ranks.Length - 1; i >= 0; i--)
        {
            if (coreCount >= m_deviceGradingConfig.m_settings.m_Ranks[i].coreCount
                && frequency >= m_deviceGradingConfig.m_settings.m_Ranks[i].frequency
                && memorySize >= m_deviceGradingConfig.m_settings.m_Ranks[i].memorySize)
            {
                m_grade = (DeviceGrade)i;
                break;
            }
        }
        return m_grade != DeviceGrade.NONE;
    }
}
