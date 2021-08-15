//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    /// <summary>
    /// Use this attribute to cause <see cref="BuildLayoutView"/> to appear in the View menu.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class BuildLayoutViewAttribute : System.Attribute
    {
    }
}
