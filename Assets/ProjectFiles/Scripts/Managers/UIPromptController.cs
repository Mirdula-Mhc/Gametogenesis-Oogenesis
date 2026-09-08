using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIPromptController : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private PageData[] pages;

    private int currentPageIndex = -1;

    private void OnEnable()
    {
        PageNavigationController.OnPageChanged += HandlePageChanged;
    }

    private void OnDisable()
    {
        PageNavigationController.OnPageChanged -= HandlePageChanged;
    }

    private void Start()
    {
        HandlePageChanged(PageNavigationController.CurrentIndex);
    }

    private void HandlePageChanged(int index)
    {
        if (index < 0 || index >= pages.Length)
            return;

        currentPageIndex = index;
        ShowPage(index);
    }

    private void ShowPage(int index)
    {
        PageData page = pages[index];

        ResetAllMainPanels();
        ResetAllAlternatePanels();

        if (page.showMainPanel && page.mainPanel != null)
            page.mainPanel.SetActive(true);

        if (page.showAlternatePanels)
            ApplyPanelVisibility(index);
    }

    private void ResetAllMainPanels()
    {
        foreach (var p in pages)
        {
            if (p.mainPanel != null)
                p.mainPanel.SetActive(false);
        }
    }

    private void ResetAllAlternatePanels()
    {
        foreach (var p in pages)
        {
            if (p.alternatePanels == null)
                continue;

            foreach (var panelData in p.alternatePanels)
            {
                if (panelData != null && panelData.panel != null)
                    panelData.panel.SetActive(false);
            }
        }
    }

    private void ApplyPanelVisibility(int currentIndex)
    {
        for (int i = 0; i <= currentIndex; i++)
        {
            PageData page = pages[i];

            if (!page.showAlternatePanels || page.alternatePanels == null)
                continue;

            foreach (var panelData in page.alternatePanels)
            {
                if (panelData == null || panelData.panel == null)
                    continue;

                if (i == currentIndex)
                {
                    // Always show on its own page, even on revisit.
                    panelData.panel.SetActive(true);
                    panelData.hasBeenEnabledOnce = true;
                }
                else if (panelData.stayInUpcomingPages)
                {
                    // Only suppress the "carry forward" behavior once enableOnce has fired.
                    if (!panelData.enableOnce || !panelData.hasBeenEnabledOnce)
                    {
                        panelData.panel.SetActive(true);
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class PageData
{
    [Header("Page Name / Page No")]
    public string pageName;

    [Header("Main Panel For This Page")]
    [Tooltip("The panel to show for this page e.g. Info, DragAndDrop, etc. Each panel carries its own content/mechanism.")]
    public bool showMainPanel;
    public GameObject mainPanel;

    [Header("Alternate Panels For This Page")]
    public bool showAlternatePanels;
    public List<AlternatePanelData> alternatePanels;
}

[System.Serializable]
public class AlternatePanelData
{
    public GameObject panel;

    [Tooltip("If enabled, this panel will remain active in upcoming pages")]
    public bool stayInUpcomingPages;

    [Header("Enable Once Feature")]
    [Tooltip("If enabled, panel will activate only once and never again on revisit")]
    public bool enableOnce;

    [HideInInspector] public bool hasBeenEnabledOnce;
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIPromptController))]
[CanEditMultipleObjects]
public class UIPromptControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Name Pages"))
        {
            foreach (var t in targets)
            {
                UIPromptController controller = (UIPromptController)t;
                NamePages(controller);
            }
        }
    }

    private void NamePages(UIPromptController controller)
    {
        SerializedObject so = new SerializedObject(controller);
        SerializedProperty pagesProp = so.FindProperty("pages");

        if (pagesProp == null || pagesProp.arraySize == 0)
        {
            Debug.LogWarning("No pages found to rename.");
            return;
        }

        for (int i = 0; i < pagesProp.arraySize; i++)
        {
            SerializedProperty page = pagesProp.GetArrayElementAtIndex(i);
            SerializedProperty nameProp = page.FindPropertyRelative("pageName");

            if (nameProp != null)
            {
                nameProp.stringValue = $"Page {i + 1}";
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(controller);
    }
}
#endif