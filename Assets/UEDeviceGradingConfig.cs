using UnityEngine;
using System;
using System.Collections.Generic;

public class UEDeviceGradingConfig : ScriptableObject
{
    [Serializable]
    public class GPUInfo
    {
        public GPUInfo(int id, string name, int quality)
        {
            m_id = id;
            m_name = name;
            m_quality = quality;
        }
        public int m_id;        //编号
        public string m_name;   //GPU名字
        public int m_quality;   //画质 0——低 1——中 2——高 3——完美
    }

    [Serializable]
    public class Hardware
    {
        public int coreCount;
        public int frequency;
        public int memorySize;
#if UNITY_IOS || UNITY_EDITOR
        public UnityEngine.iOS.DeviceGeneration iosDevice;
#endif
    }

    [Serializable]
    public class HardwareSetting
    {
        public Hardware[] m_Ranks;
    }

    public HardwareSetting m_settings = new HardwareSetting
    {
        m_Ranks = new Hardware[4] {
            new Hardware()
            {
                coreCount = 4,
                frequency = 2000,
                memorySize = 2048,
        #if UNITY_IOS || UNITY_EDITOR
                iosDevice = UnityEngine.iOS.DeviceGeneration.iPhone6S,
        #endif
            },
            new Hardware()
            {
                coreCount = 4,
                frequency = 2000,
                memorySize = 3072,
        #if UNITY_IOS || UNITY_EDITOR
                iosDevice = UnityEngine.iOS.DeviceGeneration.iPhone7Plus,
        #endif
            },
            new Hardware()
            {
                coreCount = 4,
                frequency = 2000,
                memorySize = 6144,
        #if UNITY_IOS || UNITY_EDITOR
                iosDevice = UnityEngine.iOS.DeviceGeneration.iPhoneX,
        #endif
            },
            new Hardware()
            {
                coreCount = 8,
                frequency = 2400,
                memorySize = 6144,
        #if UNITY_IOS || UNITY_EDITOR
                iosDevice = UnityEngine.iOS.DeviceGeneration.iPhoneXS,
        #endif
            },
        }
    };
    
    public List<GPUInfo> m_listGPUInfo = new List<GPUInfo>();

    public Dictionary<string, int> DicGPUInfo = new Dictionary<string, int>();

    [NonSerialized]
    private bool m_init = false;
    public void Init()
    {
        if (m_init) return;

        foreach(var gpu in m_listGPUInfo)
        {
            DicGPUInfo[gpu.m_name] = gpu.m_quality;
        }
        
        m_init = true;
    }
}
