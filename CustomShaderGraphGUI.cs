using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGraphGUI : ShaderGUI
{
    public class GraphData
    {
        public string groupName; //组名
        public MaterialProperty title; //标题
        public List<GraphData> child;//表示有子节点
    }
    static Dictionary<string, GraphData> s_GraphProperty = new Dictionary<string, GraphData>();
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Shader shader = (materialEditor.target as Material).shader;
        s_GraphProperty.Clear();
        for (int i = 0; i < properties.Length; i++)
        {
            var propertie = properties[i];
            var displayName = propertie.displayName;
            GraphData data = new GraphData() { title = propertie };
            if (TryGetGroupName(displayName, out var groupName))
            {
                if (!s_GraphProperty.TryGetValue(groupName, out var graph))
                {
                    data.child = new List<GraphData>();
                    data.groupName = groupName;
                    s_GraphProperty[groupName] = data;
                }
                else
                {
                    var attributes = shader.GetPropertyAttributes(i);
                    bool keyword = Array.FindIndex(attributes, (t) => (t == "Toggle" || t.StartsWith("KeywordEnum"))) >= 0;
                    if (keyword)
                        graph.child.Insert(0, data);
                    else
                        graph.child.Add(data);
                }
            }
            else
            {
                s_GraphProperty[displayName] = data;
            }
        }
        PropertiesDefaultGUI(materialEditor);
    }

    private static int s_ControlHash = "EditorTextField".GetHashCode();
    public void PropertiesDefaultGUI(MaterialEditor materialEditor)
    {
        var f = materialEditor.GetType().GetField("m_InfoMessage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (f != null)
        {
            string m_InfoMessage = (string)f.GetValue(materialEditor);
            materialEditor.SetDefaultGUIWidths();
            if (m_InfoMessage != null)
            {
                EditorGUILayout.HelpBox(m_InfoMessage, MessageType.Info);
            }
            else
            {
                GUIUtility.GetControlID(s_ControlHash, FocusType.Passive, new Rect(0f, 0f, 0f, 0f));
            }
        }

        foreach (var props in s_GraphProperty.Values)
        {
            MaterialProperty prop = props.title;
            if ((prop.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) == MaterialProperty.PropFlags.None)
            {
                if (props.child != null && props.child.Count > 0)
                {
                    //如果发现有面板，使用Foldout来绘制
                    prop.floatValue = Convert.ToSingle(EditorGUILayout.Foldout(Convert.ToBoolean(prop.floatValue), prop.displayName));
                    if (prop.floatValue == 1f)
                    {
                        foreach (var child in props.child)
                        {
                            DrawGUI(materialEditor, child.title, true);
                        }
                    }
                }
                else
                {
                    DrawGUI(materialEditor, prop, false);
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
        {
            materialEditor.RenderQueueField();
        }
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();
        //unity 2020 新版本功能 ，老版本需要注释掉
        materialEditor.EmissionEnabledProperty();
    }

    void DrawGUI(MaterialEditor materialEditor, MaterialProperty prop, bool indentLevel)
    {
        float propertyHeight = materialEditor.GetPropertyHeight(prop, prop.displayName);
        Rect controlRect = EditorGUILayout.GetControlRect(true, propertyHeight, EditorStyles.layerMaskField);
        //如果是面板下面的元素统一向右缩进，否则不向右缩进
        if (indentLevel) EditorGUI.indentLevel++;
        materialEditor.ShaderProperty(controlRect, prop, prop.displayName);
        if (indentLevel) EditorGUI.indentLevel--;
    }



    const string INSPECTOR_TXT = "面板";
    bool TryGetGroupName(string displayName, out string groupName)
    {
        //根据displayName找到组名
        if (displayName.Contains(INSPECTOR_TXT))
        {
            groupName = Regex.Split(displayName, INSPECTOR_TXT, RegexOptions.IgnoreCase)[0];
            return true;
        }
        foreach (var property in s_GraphProperty.Values)
        {
            if (!string.IsNullOrEmpty(property.groupName))
            {
                if (displayName.StartsWith(property.groupName))
                {
                    groupName = property.groupName;
                    return true;
                }
            }
        }
        groupName = string.Empty;
        return false;
    }
}