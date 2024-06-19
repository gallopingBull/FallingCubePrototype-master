using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImGuiNET;
using System;

public class ImguiDemo : MonoBehaviour
{
    private void OnEnable()
    {
        ImGuiUn.Layout += OnLayout;
    }
    private void OnDisable()
    {
        ImGuiUn.Layout -= OnLayout;
    }


    private void OnLayout()
    {
        ImGui.ShowDemoWindow();
    }
}
