using ImGuiNET;
using UnityEngine;
using UnityEngine.Events;

public class ImguiDemo : MonoBehaviour
{
    private static UnityAction OnUpdateLighting;
    private static UnityAction OnLoadGame;
    private static UnityAction OnSaveGame;

    private void OnEnable() => ImGuiUn.Layout += OnLayout;

    private void OnDisable() => ImGuiUn.Layout -= OnLayout;

    private static readonly Vector4 TEXT_COLOR = new Vector4(1f, 0.8f, 0f, 1.0f);

    private float m_Timeline = 0.5f;
    private System.TimeSpan m_Time = System.TimeSpan.FromHours(12.0f);
    private string m_Username = string.Empty;
    private string m_Password = string.Empty;
    private Vector3 m_SunColor = new Vector4(1.0f, 0.9f, 0f);
    private bool m_WindowEnabled = false;
    private bool m_EnableRandomizeGridSize = false;
    private int m_DragInt = 0;
    private int m_AccountCount = 0;
    private bool m_ShowImGuiDemoWindow;
    private static uint s_tab_bar_flags = (uint)ImGuiTabBarFlags.Reorderable;
    static bool[] s_opened = { true, true, true, true }; // Persistent user state

    /**
     * DearImGui
     * Manual - https://pthom.github.io/imgui_manual_online/manual/imgui_manual.html
     */
    private int selected = 0;
    private void OnLayout()
    {
        ImGui.SetNextWindowSize(new Vector2(900,500), ImGuiCond.Always);
        // Begins ImGui window
        if (!ImGui.Begin("Level Editor",
                         ref m_WindowEnabled))
            return;

        // Display contents in a scrolling region
        ImGui.TextColored(TEXT_COLOR, "Saved Levels");
        // Left
        ImGui.BeginChild("left pane", new Vector2(150, 0), true, ImGuiWindowFlags.None);
        for (int i = 0; i < 15; i++)
        {
            string label = $"MyObject {i}";
            if (ImGui.Selectable(label, selected == i))
                selected = i;
        }
        ImGui.EndChild();

        ImGui.SameLine();

        // Right
        ImGui.BeginGroup();
        ImGui.BeginChild("item view", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()), false, ImGuiWindowFlags.None); // Leave room for 1 line below us
        ImGui.Text($"MyObject: {selected}");
        ImGui.Separator();
        if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
        {
            if (ImGui.BeginTabItem("Description"))
            {
                ImGui.TextWrapped("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Details"))
            {
                ImGui.Text("ID: 0123456789");
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
        ImGui.EndChild();
        if (ImGui.Button("Load"))
        {
            OnLoadGame?.Invoke();
            Debug.Log("Loading level...");
        }

        ImGui.SameLine();
        if (ImGui.Button("Delete")) { }
        ImGui.EndGroup();
        ImGui.Separator();

        ImGui.BeginGroup();
        ImGui.Checkbox("Randomize Grid Size", ref m_EnableRandomizeGridSize);
        if (ImGui.Button("Generate Level"))
        {
            //OnLoadGame?.Invoke();
            Debug.Log("Generating random level...");
        }
        ImGui.NewLine();
        ImGui.EndGroup();
        ImGui.Separator();

        ImGui.Checkbox("Randomize Grid Size", ref m_EnableRandomizeGridSize);
        if (ImGui.Button("Generate Random Level"))
        {
            //OnLoadGame?.Invoke();
            Debug.Log("Generating random level...");
        }
        ImGui.NewLine();
        ImGui.Separator();

        // Make a float slider (label, value, min, max) 
        if (ImGui.SliderFloat("Time [%]", ref m_Timeline, 0f, 1f) && m_EnableRandomizeGridSize)
        {
            m_Time = System.TimeSpan.FromSeconds(m_Timeline * 86400f);
            OnUpdateLighting?.Invoke();
        }

        // Display text of current in-game time
        ImGui.TextColored(TEXT_COLOR, $"In-game Time: {m_Time:hh\\:mm}");


        if (ImGui.Button("Save Level"))
        {
            OnSaveGame?.Invoke();
            Debug.Log("Saving level...");
        }



        ImGui.SameLine(0, -1);
        ImGui.Text($"Account Count = {m_AccountCount}");

        // Create input field (label, value, maxLength [uint])
        ImGui.InputText("Level Name", ref m_Username, maxLength: 12u);
        ImGui.InputText("Level Description", ref m_Password, maxLength: 16u);

        ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");

        // Display contents in a scrolling region
        ImGui.TextColored(TEXT_COLOR, "Important Stuff");

        // Generate samples and plot them
        float[] samples = new float[100];

        for (int n = 0; n < 100; n++)
            samples[n] = Mathf.Sin((float)(n * 0.2f + ImGui.GetTime() * 1.5f));

        ImGui.PlotLines("Samples", ref samples[0], 100);

        ImGui.DragInt("Draggable Int", ref m_DragInt);

        float framerate = ImGui.GetIO().Framerate;
        ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");

        if (m_ShowImGuiDemoWindow)
        {
            // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
            // Here we just want to make the demo initial state a bit more friendly!
            ImGui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
            ImGui.ShowDemoWindow(ref m_ShowImGuiDemoWindow);
        }


        if (ImGui.Button("Clear Level"))
        {
            Debug.Log("Clearing level...");
            //m_AccountCount++;
        }
        // Ends ImGui window
        ImGui.End();
    }
}
