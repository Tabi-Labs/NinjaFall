using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(TestUIData))]
public class GameUIEditor : Editor
{
    public VisualTreeAsset VisualTree;

    private TestUIData m_UIData;
    private Button m_MainMenuButton;

    private void OnEnable()
    {
        m_UIData = (TestUIData)target;
    }
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        //Add all the UI Builder assets
        VisualTree.CloneTree(root);
        
        // Get buttons to assign functions
        m_MainMenuButton = root.Q<Button>("b_MainMenu");
        m_MainMenuButton.RegisterCallback<ClickEvent>(LoadMainMenu);

        return root;
    }

    #region ----- BUTTON CALLBACKS --------

    private void LoadMainMenu(ClickEvent evt)
    {
        m_UIData.LoadScene(m_UIData.MainMenuScene);
    }
    #endregion
}
