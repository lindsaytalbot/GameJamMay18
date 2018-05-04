using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

public class PanelManager : Singleton<PanelManager>
{
	public delegate void PanelDelegate( Panel panel );
	/// <summary>
	/// Triggered whenever a new panel is shown.
	/// </summary>
	public event PanelDelegate onPanelShown;
	/// <summary>
	/// Triggered whenever an active panel is hidden.
	/// </summary>
	public event PanelDelegate onPanelHidden;


    private Panel[] loadedPanels;
    private List<Panel> activePanels;

    protected override void Awake()
    {
        base.Awake();
        activePanels = new List<Panel>();
        LoadAllPanels();

#if UNITY_TVOS
			UnityEngine.Apple.TV.Remote.allowExitToHome = false;
#endif
    }

    private void LoadAllPanels()
    {
        // add any panels already present
        loadedPanels = transform.GetComponentsInChildren<Panel>(true);

        foreach (Panel panel in loadedPanels)
        {
            panel.gameObject.SetActive(panel.panelShowType == Panel.PanelShowType.AlwaysShow || panel.panelShowType == Panel.PanelShowType.EnabledButHidden);
        }
    }

    public T GetPanel<T>() where T : Panel
    {
        if (loadedPanels == null)
            return null;
        return loadedPanels.Select(loadedPanel => loadedPanel.GetComponent<T>()).FirstOrDefault(panel => panel != null);
    }

    public Panel GetPanel(Type T)
    {
        if (loadedPanels == null)
            return null;
        return (Panel)loadedPanels.Select(loadedPanel => loadedPanel.GetComponent(T)).FirstOrDefault(panel => panel != null);
    }

    public Panel GetActivePanel()
    {
        return activePanels.LastOrDefault();
    }

    public void ShowPanelInternal(Panel panel)
    {
        //if (activePanels.Contains(panel))
        //    return;

        if (panel.panelShowType != Panel.PanelShowType.AlwaysShow && panel.panelShowType != Panel.PanelShowType.Modal)
        {
            for (int i = activePanels.Count - 1; i >= 0; i--)
            {
                if (activePanels[i] == panel)
                    continue;

                if (activePanels[i].panelShowType == Panel.PanelShowType.Normal)
                    activePanels[i].gameObject.SetActive(false);

                if (activePanels[i].panelShowType == Panel.PanelShowType.EnabledButHidden)
                    activePanels[i].Hide();
            }
        }

        activePanels.Add(panel);

		if( onPanelShown != null )
		{
			onPanelShown( panel );
		}

        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {
        // the last active panel that is not modal
        Panel activePanel = activePanels.LastOrDefault(p => p.panelShowType != Panel.PanelShowType.Modal);

        Time.timeScale = activePanel != null && activePanel.PauseTime ? 0 : 1;
    }

    public void ShowPanel(Type panelType, bool clearActive = false)
    {
        Panel current = GetPanel(panelType);

        if (clearActive)
        {
            ClearPanels(current);
        }

        current.GetComponent<Panel>().Show();
    }

    public void ClearPanels(Panel current)
    {
        Panel[] panels = activePanels.ToArray();
        foreach (Panel p in panels)
        {
            //Clear all but one occurance of current
            if (p == current)
            {
                current = null;
                continue;
            }
            p.gameObject.SetActive(false);
            p.Hide();
        }
    }

    public void ShowPanelDelayed(Type panelType, float delay)
    {
        var result = GetPanel(panelType);
        StartCoroutine(ShowPanelCoroutine(result, delay));
    }

    private IEnumerator ShowPanelCoroutine(Panel panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.Show();
    }

    public void HidePanel(Type panelType)
    {
        var result = GetPanel(panelType);
        result.GetComponent<Panel>().Hide();
    }

    public void RemoveActivePanel(Panel panel)
    {
        if (activePanels.Contains(panel) == false)
            return;

        activePanels.Remove(panel);
		

		if( onPanelHidden != null )
		{
			onPanelHidden( panel );
		}

        UpdateTimeScale();
    }

    public void PanelDidHide(Panel panel)
    {
        if (panel.panelShowType == Panel.PanelShowType.Modal)
            return;

        // fix: wait until the end of the frame to choose the panel to show
        StartCoroutine(PanelDidHideCoroutine());
    }

    private IEnumerator PanelDidHideCoroutine()
    {
        yield return new WaitForEndOfFrame();
        if (activePanels.Count > 0)
        {
            Panel lastPanel = activePanels.Last();

            if (lastPanel.panelShowType == Panel.PanelShowType.Normal)
            {
                lastPanel.gameObject.SetActive(true);
                lastPanel.Show();
            }
            else if (lastPanel.panelShowType == Panel.PanelShowType.EnabledButHidden)
            {
                lastPanel.Show();
            }
        }
    }

    private void Update()
    {
#if UNITY_TVOS
        //TV OS back button
		if (Input.GetButtonDown("BackTVOS"))
#else
        //Android back button
        if (Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            var panel = GetActivePanel();
            if (panel != null)
                panel.OnBackPressed();
        }
    }

#if UNITY_EDITOR
	public List<Panel> GetPanelStack()
	{
		return activePanels;
	}
#endif
}
