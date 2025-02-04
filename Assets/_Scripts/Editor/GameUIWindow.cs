using System.Linq;
using UnityEditor;

using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUIWindow : EditorWindow
{
    [SerializeField] 
    TestUIData m_Data;

    [MenuItem("Window/Game UI/Manager")]
    static void CreateMenu()
    {
        var window = GetWindow<GameUIWindow>();
        window.titleContent = new UnityEngine.GUIContent("Game UI Manager");
    }

    public void OnEnable()
    {
        m_Data = GameObject.FindGameObjectsWithTag("GameUIData").FirstOrDefault()?.GetComponent<TestUIData>();
    }

    public void CreateGUI()
    {
        if(m_Data == null) return;

        var scrollView = new ScrollView() { viewDataKey = "WindowScrollView"};
        scrollView.Add(new InspectorElement(m_Data));
        rootVisualElement.Add(scrollView);
    }
}
