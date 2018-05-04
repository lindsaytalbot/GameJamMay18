using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : Panel
{
    [Header("Dialog Panel")]

    [SerializeField]
    private TextMeshProUGUI titleText; //not required

    [SerializeField]
    private TextMeshProUGUI bodyText; //required

    [SerializeField]
    private Image dialogImage;  //not required

    [SerializeField]
    private float lockTime = 0.5f; //Time to lock panel between taps

    private string title;
    private Sprite image;
    private Action onComplete;
    private List<string> dialogs;
    private bool lockSubmit;

    //Show a list of dialogs
    public void ShowDialog(List<string> dialogs, Action onComplete)
    {
        ShowDialogs("", dialogs, onComplete);
    }

    //Show a list of dialogs with a title
    public void ShowDialogs(string title, List<string> dialogs, Action onComplete)
    {
        ShowDialogs(null, title, dialogs, onComplete);
    }

    //Show a list of dialogs with an image
    public void ShowDialogs(Sprite image, List<string> dialogs, Action onComplete)
    {
        ShowDialogs(image, "", dialogs, onComplete);
    }

    //Show a list of dialogs with an image and title
    public void ShowDialogs(Sprite image, string title, List<string> dialogs, Action onComplete)
    {
        if(dialogImage != null && image != null)
            dialogImage.sprite = image;

        this.title = title;
        this.image = image;
        this.dialogs = dialogs;
        this.onComplete = onComplete;

        //Show the first dialog in the list
        Submit();
    }

    //Show a single dialog with just body
    public void ShowDialog(string body, Action onComplete)
    {
        ShowDialog(null, "", body, onComplete);
    }

    //Show a single dialog with an associated image
    public void ShowDialog(Sprite image, string body, Action onComplete)
    {
        ShowDialog(image, "", body, onComplete);
    }

    //Show a single dialog with tile and body
    public void ShowDialog(string title, string body, Action onComplete)
    {
        ShowDialog(null, title, body, onComplete);
    }

    //Show a single dialog with image, title, and body
    public void ShowDialog(Sprite Image, string title, string body, Action onComplete)
    {
        this.title = title;
        this.onComplete = onComplete;

        if (dialogImage != null && image != null)
            dialogImage.sprite = image;

        if (titleText != null && title != null)
            titleText.text = title;

        bodyText.text = body;

        PanelManager.Instance.ShowPanel(typeof(DialogPanel));
    }

    //User has tapped the screen
    public void Submit()
    {
        //Show the next dialog in the list
        if (dialogs != null && dialogs.Count > 0)
        {
            LockPanel();
            ShowDialog(image, title, dialogs[0], onComplete);
            dialogs.RemoveAt(0);
            return;
        }

        //Shown all message, close
        lockSubmit = true;
        HideThenDo(onComplete);
    }

    private void Update()
    {
        //Check for touches
        if (!lockSubmit && Input.anyKeyDown)
        {
            Submit();
        }
    }

    //Temporarily lock the panel to prevent accidental rapid touches
    private void LockPanel()
    {
        StartCoroutine(LockPanelRoutine());
    }

    private IEnumerator LockPanelRoutine()
    {
        lockSubmit = true;
        yield return new WaitForSecondsRealtime(lockTime);
        lockSubmit = false;
    }

    protected override void OnHide()
    {
        base.OnHide();
        lockSubmit = false;
    }
}
