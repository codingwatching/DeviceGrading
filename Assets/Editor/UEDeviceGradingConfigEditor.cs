using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.iOS;
//using LitJson;

[CustomEditor(typeof(UEDeviceGradingConfig))]
public class UEDeviceGradingConfigEditor : Editor
{
    private UEDeviceGradingConfig deviceGradingConfig;

    void OnEnable()
    {
        deviceGradingConfig = target as UEDeviceGradingConfig;
        for (int i = 0; i < deviceGradingConfig.m_settings.m_Ranks.Length; i++)
            m_ranks.Add(false);
        SortRank(deviceGradingConfig);
    }

    Dictionary<int, string> m_dicRank = new Dictionary<int, string>();
    
    List<bool> m_ranks = new List<bool>();
    private bool m_preview = false;
    private bool m_edit = false;

    public void SortRank(UEDeviceGradingConfig deviceGradingConfig)
    {
        m_dicRank.Clear();
        foreach (var gpu in deviceGradingConfig.m_listGPUInfo)
        {
            var s = gpu.m_name + "  " + gpu.m_quality;
            if (m_dicRank.ContainsKey(gpu.m_id))
            {
                m_dicRank[gpu.m_id] += "\n\t" + s;
            }
            else
            {
                m_dicRank[gpu.m_id] = s;
            }
        }
    }

    private void OnRankDraw()
    {
        string rank = string.Empty;
        for (int i = 0; i < deviceGradingConfig.m_settings.m_Ranks.Length; i++)
        {
            if (i == 0)
                rank = "GRADE:LOW";
            else if (i == 1)
                rank = "GRADE:MEDIUM";
            else if (i == 2)
                rank = "GRADE:HIGH";
            else if (i == 3)
                rank = "GRADE:VERY HIGH";
            else
                rank = "GRADE:UNKNOWN";
            m_ranks[i] = EditorGUILayout.Foldout(m_ranks[i], rank);
            if (m_ranks[i] == true)
            {
                //GUILayout.BeginHorizontal();               
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                deviceGradingConfig.m_settings.m_Ranks[i].iosDevice =
                (DeviceGeneration)EditorGUILayout.EnumPopup("IOS device rank",
                    deviceGradingConfig.m_settings.m_Ranks[i].iosDevice);
                //m_previewIOS = EditorGUILayout.Foldout(m_previewIOS, "IOS Detail:");
                //if (m_previewIOS)
                //{
                //    GUILayout.Label("\tContains:\n\t" + deviceGradingConfig.m_settings.m_Ranks[i].iosDevice);
                //}
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                GUILayout.Label("Default rank info");
                deviceGradingConfig.m_settings.m_Ranks[i].coreCount =
                    EditorGUILayout.IntSlider("\tCore count:", deviceGradingConfig.m_settings.m_Ranks[i].coreCount, 1, 16);
                deviceGradingConfig.m_settings.m_Ranks[i].frequency =
                    EditorGUILayout.IntSlider("\tCpu frequency:", deviceGradingConfig.m_settings.m_Ranks[i].frequency, 1500, 3000);
                deviceGradingConfig.m_settings.m_Ranks[i].memorySize =
                    EditorGUILayout.IntSlider("\tMemory size:", deviceGradingConfig.m_settings.m_Ranks[i].memorySize, 2048, 8192);
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                //GUILayout.EndVertical();                
            }
        }
    }
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();
        deviceGradingConfig = target as UEDeviceGradingConfig;

        if (deviceGradingConfig != null)
        {
            //if (m_dicRank.Keys.Count == 0 || m_keys.Count == 0)
            //    SortRank(deviceGradingConfig);

            GUI.skin.label.wordWrap = true;
            OnRankDraw();
            m_preview = EditorGUILayout.Foldout(m_preview, "Android GPU Info Overview");
            if (m_preview)
            {
                foreach (var info in deviceGradingConfig.m_listGPUInfo)
                {
                    string label = info.m_id.ToString() + "\t" + info.m_name + "\t" + info.m_quality.ToString();
                    GUILayout.Label(label);
                }
            }

            m_edit = EditorGUILayout.Foldout(m_edit, "Edit Android GPU Info");
            if (m_edit)
            {
                DrawGPUList(serializedObject.FindProperty("m_listGPUInfo"));
            }
            GUI.contentColor = Color.green;
            if (GUILayout.Button("Save Asset", EditorStyles.toolbarButton))
            {
                AssetDatabase.SaveAssets();
            }
            GUI.contentColor = Color.white;
        }

        EditorUtility.SetDirty(deviceGradingConfig);
        serializedObject.ApplyModifiedProperties();
    }

    void DrawGPUList(SerializedProperty list)
    {
        EditorGUILayout.PropertyField(list, true);
        //for (int i = 0; i < list.arraySize; ++i)
        //{
        //    EditorGUILayout.BeginHorizontal();
        //    GUI.contentColor = Color.cyan;
        //    EditorGUILayout.LabelField(deviceGradingConfig.m_listGPUInfo[i].m_id.ToString(), GUILayout.Width(20));
        //}
    }
}

public class GPUInfo
{
    public int id;
    public string name;
    public int quality;
}

public static class deviceGradingConfig
{
    public const string strDeviceGradingConfigPath = "Assets/ResourcesAssets/DeviceGradingConfig/UEDeviceGradingForGOT.asset";

    [MenuItem("Tools/画质分级/Create DeviceGrading Config")]
    private static void CreateDeviceGradingConfig()
    {
        var path = EditorUtility.OpenFilePanel("Open DeviceGPUConfig File", Application.dataPath, "");
        var asset = ScriptableObject.CreateInstance<UEDeviceGradingConfig>();
        if (path != null && path != "")
        {
            using (var f = File.OpenRead(path))
            {
                if (f.CanRead)
                {
                    using (StreamReader sr = new StreamReader(f))
                    {
                        string strContent = string.Empty;
                        while (!string.IsNullOrEmpty(strContent = sr.ReadLine()))
                        {
                            string[] _gpu = strContent.Split('\t');
                            int id = Convert.ToInt32(_gpu[0]);
                            string gpuName = _gpu[1].ToLower();
                            int quality = Convert.ToInt32(_gpu[2]);
                            asset.m_listGPUInfo.Add(new UEDeviceGradingConfig.GPUInfo(id, gpuName, quality));
                        }
                    }

                    f.Close();
                }
            }
            AssetDatabase.CreateAsset(asset, strDeviceGradingConfigPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;

            EditorUtility.DisplayDialog("DeviceGradingKit", "数据创建成功！", "Ok");
        }
    }

    [MenuItem("Tools/画质分级/Update DeviceGrading Config")]
    private static void UpdateDeviceGradingConfig()
    {
        string[] strs = Selection.assetGUIDs;
        if (strs.Length == 0)
        {
            EditorUtility.DisplayDialog("DeviceGrading", "请选择要更新的设备分级数据(UEDeviceGradingForGOT.asset)！", "Ok");
            return;
        }
        string strPath = AssetDatabase.GUIDToAssetPath(strs[0]);
        UEDeviceGradingConfig config = AssetDatabase.LoadAssetAtPath<UEDeviceGradingConfig>(strPath);
        if (null == config)
        {
            EditorUtility.DisplayDialog("DeviceGrading", "UEDeviceGradingForGOT.asset读取失败，请检查！", "Ok");
            return;
        }

        config.m_listGPUInfo.Clear();
        var path = EditorUtility.OpenFilePanel("Open DeviceGPUConfig File", Application.dataPath, "");
        if (path != null && path != "")
        {
            using (var f = File.OpenRead(path))
            {
                if (f.CanRead)
                {
                    using (StreamReader sr = new StreamReader(f))
                    {
                        string strContent = string.Empty;
                        while (!string.IsNullOrEmpty(strContent = sr.ReadLine()))
                        {
                            string[] _gpu = strContent.Split('\t');
                            int id = Convert.ToInt32(_gpu[0]);
                            string gpuName = _gpu[1].ToLower();
                            int quality = Convert.ToInt32(_gpu[2]);
                            config.m_listGPUInfo.Add(new UEDeviceGradingConfig.GPUInfo(id, gpuName, quality));
                        }
                    }

                    f.Close();
                }
            }
        }

        AssetDatabase.SaveAssets();
        Selection.activeObject = config;

        EditorUtility.DisplayDialog("DeviceGradingKit", "数据更新成功！", "Ok");
    }

    public static bool Contains(this string source, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
    {
        return source.IndexOf(toCheck, comp) >= 0;
    }

    private static void RunCommand(string cmd, string arguments = "")
    {
        string output = "";

        System.Diagnostics.Process process = new System.Diagnostics.Process();

        process.StartInfo.FileName = cmd;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.Arguments = arguments;
        process.Start();

        StreamReader reader = process.StandardOutput;
        output = reader.ReadToEnd();

        process.WaitForExit();
        process.Close();
    }
}
