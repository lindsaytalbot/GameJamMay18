using System.Collections;
using UnityEngine;
using MightyKingdom;

public class Panel : MonoBehaviour
{
    public bool PauseTime = false;

    [Tooltip("Should the panel hide be driven by an animator or otherwise deactivate immediately")]
    public bool HideWithAnimation = true;

    //Normal = Hide on start, hide when another panel opens on top.
    //AlwaysShow = Show on start, keep showing when other panels show on top
    //EnabledButHidden = AlwaysShow, but keep in hide position rather than disabling the game object when something opens on top and at start
    //Modal = Like Normal, but don't do any hiding/showing of panels underneath
    public enum PanelShowType { Normal, AlwaysShow, EnabledButHidden, Modal }

    public PanelShowType panelShowType = PanelShowType.Normal;
    protected bool isBeingPresented = false;
    protected Animator panelAnimator;
    protected System.Action hideAction;

    [Header("Audio")]

    [SerializeField]
    protected string appearSound;
    [SerializeField]
    protected string disappearSound;

    protected virtual void Awake()
    {
        panelAnimator = GetComponent<Animator>();
    }

    public void Show()
    {
        // already being shown, do nothing!
        if (isBeingPresented == true && gameObject.activeSelf)
        {
            OnResume();
            return;
        }

        // fix: wait for end of frame to avoid flicker from animation not being applied
        // note: panel is not active yet, so we start the coroutine on the PanelManager
        isBeingPresented = true;

        PanelManager.Instance.StartCoroutine(ShowIE());
    }

    protected IEnumerator ShowIE()
    {
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(true);
        PanelManager.Instance.ShowPanelInternal(this);

        //Play appear sound
        string[] splitName = appearSound.Split('/');
        if (splitName.Length == 2)
        {
            string bankName = splitName[0];
            string clipName = splitName[1];
            MKAudioManager.Play(clipName, bankName);
        }

        OnShow();
    }

    public virtual void Hide()
    {
        if (panelShowType == PanelShowType.AlwaysShow)
            return;

        PanelManager.Instance.RemoveActivePanel(this);

        if (gameObject.activeSelf == false)
        {
            //In case where showing 2 panels at once, the bottom panel is disabled before it can run the hide anim / run DidHide. Hide it properly here.
            HideComplete();
            return;
        }

        //Play disappear sound
        string[] splitName = disappearSound.Split('/');
        if (splitName.Length == 2)
        {
            string bankName = splitName[0];
            string clipName = splitName[1];
            MKAudioManager.Play(clipName, bankName);
        }

        if (panelAnimator && HideWithAnimation)
        {
            panelAnimator.SetTrigger("Close");
            // DidHide will be called by the hide animation
        }
        else
        {
            HideComplete();
        }
    }

    public virtual void HideComplete()
    {
        // fix: wait a frame after hiding the panel before starting actions that may cause lag
        gameObject.SetActive(false);
        PanelManager.Instance.StartCoroutine(HideCompleteRoutine());
    }

    protected IEnumerator HideCompleteRoutine()
    {
        yield return null;

        isBeingPresented = false;

        PanelManager.Instance.PanelDidHide(this);

        if (hideAction != null)
        {
            hideAction();
            hideAction = null;
        }
        OnHide();
    }

    protected void HideThenDo(System.Action action)
    {
        hideAction = action;
        Hide();
    }

    public virtual void OnBackPressed()
    {
        Hide();
    }

    protected virtual void OnShow()
    { }

    protected virtual void OnResume()
    { }

    protected virtual void OnHide()
    { }
}
