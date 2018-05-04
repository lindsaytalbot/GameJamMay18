using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using MightyKingdom;

[RequireComponent(typeof(Touchable))]
public class PushButton : Selectable, IPointerDownHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ISubmitHandler
{
    public bool debugText = false;
    public static bool IsTVPlatform = false; //Set to true by another when played on a TV platform (TVos or FireTV)

    public bool Interactable
    {
        get
        {
            return interactable;
        }
        set
        {
            interactable = value;
            if (!interactable && pressed && buttonAnimator != null) //disabled mid press. Complete anim
                buttonAnimator.SetTrigger("ClickButton");
        }
    }

    [Header("Interaction")]

    [SerializeField]
    //only allows the button to be pressed once until it is disable and re-enabled again
    bool disableOnClick = false;

    [Header("Animation")]

    [SerializeField]
    public Animator buttonAnimator;

    //Waits for animation to complete before allowing another pressed
    public bool waitForAnimation;

    [Header("Audio")]

    [SerializeField]
    public string clickSoundName;

    [Header("OnClickActions")]

    [SerializeField]
    public Button.ButtonClickedEvent onClickActions;

    private bool pressed;
    private bool locked;

    protected override void Awake()
    {
        base.Awake();

        //Register for button lock events 
        onLockStateUpdate += OnButtonsLocked;

        // Grab the animator assigned to the object	
        if (buttonAnimator == null)
            buttonAnimator = GetComponent<Animator>();
    }

    //Unregister for button lock events
    protected override void OnDestroy()
    {
        onLockStateUpdate -= OnButtonsLocked;
    }

    //Triggers the interactable state
    private void OnButtonsLocked(bool disabled)
    {
        Interactable = !disabled || !IsButtonLocked(this);

        //Trigger state transistion. Typically a colour change
        if (!Interactable)
            base.DoStateTransition(SelectionState.Disabled, false);
        else
            base.DoStateTransition(SelectionState.Normal, false);
    }

    //Unlock this button
    protected override void OnEnable()
    {
        locked = false;
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (!Application.isPlaying)
            return;

        //Only highlight if TV_OS or Amazon Fire TV
        if (IsTVPlatform)
            buttonAnimator.SetBool("Highlighted", state == SelectionState.Highlighted);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        OnPointerClick(null, true);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        //Cannot touch this button more than once or if it's locked
        if (Interactable == false || locked || pressed)
        {
            if (debugText) MKLog.Log("OnPointerDown disabled on " + name);
            return;
        }

        if (IsButtonLocked(this))
        {
            if (debugText) MKLog.Log("Buttons Locked. " + name + " is not permitted to be pressed");
            return;
        }

        pressed = true;

        //Press button animator
        if (buttonAnimator != null)
        {
            buttonAnimator.ResetTrigger("ClickButton");
            buttonAnimator.ResetTrigger("Release");
            buttonAnimator.SetTrigger("Pressed");
        }

        if (debugText) MKLog.Log("OnPointerDown " + name);
    }

    //Cursor moved off of the button
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if (!pressed)
            return;

        //Button no longer pressed and won't be triggered by click
        pressed = false;

        //Release button animator
        if (buttonAnimator != null)
        {
            buttonAnimator.ResetTrigger("Pressed");
            buttonAnimator.SetTrigger("Release");
        }

        if (debugText) MKLog.Log("OnPointerExit " + name);
    }

    public void Clicked()
    {
        OnPointerClick(null, true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerClick(eventData, false);
    }

    public void OnPointerClick(PointerEventData eventData, bool fakeClick)
    {
        //Click disabled, or click was called after moving off of button
        if (Interactable == false || locked || (!pressed && !fakeClick))
        {
            if (debugText) MKLog.Log("click failed " + name + " :" + Interactable + " : " + locked + " : " + pressed + ": " + fakeClick);
            return;
        }

        if (IsButtonLocked(this))
        {
            if (debugText) MKLog.Log("Buttons Locked. " + name + " is not permitted to be pressed");
            return;
        }

        //Release buttons
        if (buttonAnimator != null && !fakeClick)
        {
            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Button Pressed"))
                buttonAnimator.ResetTrigger("Pressed");
            buttonAnimator.SetTrigger("Release");
        }

        //Play sound
        string[] splitName = clickSoundName.Split('/');
        if (splitName.Length == 2)
        {
            string bankName = splitName[0];
            string clipName = splitName[1];
            MKAudioManager.Play(clipName, bankName);
        }

        //Trigger OnClick actions
        if (gameObject.activeInHierarchy)
            StartCoroutine(OnPressedRoutine());

        pressed = false;

        //Informs all listeners that this button has been pressed
        if (onPushButtonClicked != null)
            onPushButtonClicked(this);

        if (debugText) MKLog.Log("OnPointerClick " + name);
    }

    protected virtual IEnumerator OnPressedRoutine()
    {
        locked = true;

        //Wait for button to release
        if (waitForAnimation && buttonAnimator != null)
        {
            while (buttonAnimator.GetBool("ClickButton") || buttonAnimator.GetBool("Release"))
                yield return 0;
        }

        //Trigger on click actions
        if (onClickActions != null)
            onClickActions.Invoke();

        //Allow this button to be clicked again
        if (!disableOnClick)
        {
            locked = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (transform.parent != null)
        {
            GameObject parentDrag = ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);

            //Release the button
            if (pressed && parentDrag != null)
            {
                if (debugText) MKLog.Log("IBeginDragHandler passed up. Button released");
                OnPointerExit(eventData);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (transform.parent != null)
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent != null)
        {
            if (debugText) MKLog.Log("IEndDragHandler passed Up");
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
        }
    }

    //Used to lock all PushButtons. Useful for tutorials where only certain butons are allowed to be pressed
    #region ButtonLock 

    private static Action<bool> onLockStateUpdate;
    private static List<PushButton> unlockedButtons = new List<PushButton>(); //whitelist
    public static bool ButtonsLocked { get; protected set; }

    //Locks all buttons except the provided list. Null is allowed
    public static void LockButtons(List<PushButton> whiteList, bool disableOthers = true, bool pulseEnabled = true)
    {
        //Stop highlighting other buttons
        if (unlockedButtons != null)
        {
            foreach (PushButton button in unlockedButtons)
            {
                button.buttonAnimator.SetBool("Highlighted", false);
            }
        }

        unlockedButtons = whiteList;
        ButtonsLocked = true;

        if (pulseEnabled && unlockedButtons != null)
        {
            foreach (PushButton button in unlockedButtons)
            {
                button.buttonAnimator.SetBool("Highlighted", true);
            }
        }

        if (onLockStateUpdate != null)
            onLockStateUpdate.Invoke(disableOthers);
    }

    //Allows all buttons to be clicked again
    public static void UnlockButtons()
    {
        if (unlockedButtons != null)
        {
            foreach (PushButton button in unlockedButtons)
            {
                button.buttonAnimator.SetBool("Highlighted", false);
            }
        }

        unlockedButtons = null;
        ButtonsLocked = false;

        if (onLockStateUpdate != null)
            onLockStateUpdate.Invoke(false);
    }

    //Returns true if the current button is locked and cannot be pressed
    private static bool IsButtonLocked(PushButton button)
    {
        return ButtonsLocked && (unlockedButtons == null || !unlockedButtons.Contains(button));
    }

    #endregion

    //Allows other classes to listen in on all successful button clicks. Passes the button that was clicked
    public static Action<PushButton> onPushButtonClicked;

    [ContextMenu("Reset State Colours")]
    public void SetStateColours()
    {
        ColorBlock b = colors;
        b.normalColor = Color.white;
        b.pressedColor = Color.white;
        b.highlightedColor = Color.white;
        b.disabledColor = new Color(.78f, .78f, .78f, .5f);
        colors = b;
    }
}
