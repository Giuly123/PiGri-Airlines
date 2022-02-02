using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonHandler : MonoBehaviour
{
    [SerializeField]
    protected Button button;
    public TMP_Text label;
    public Image background;
    public Image icon;
    public bool isSelected;

    protected List<UnityAction> persistentButtonActions;

    protected virtual void Reset()
    {
        label = GetComponentInChildren<TMP_Text>();
        background = GetComponent<Image>();

        Image[] images = GetComponentsInChildren<Image>();
        if (images.Length > 0)
        {
            if (!background)
            {
                icon = images[0];
            }
            else
            {
                for (int i = 0; i < images.Length; i++)
                {
                    Image image = images[i];
                    if (image != background)//get first image in childrens
                    {
                        icon = image;
                        return;
                    }
                }
            }
        }
    }

    #region Listeners
    public void AddListener(UnityAction action)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        button.onClick.AddListener(action);
    }

    public void RemoveListener(UnityAction action)
    {
        if (persistentButtonActions == null)
        {
            persistentButtonActions = new List<UnityAction>();
        }
        persistentButtonActions.Remove(action);

        if (button == null)
        {
            button = GetComponent<Button>();
        }
        button.onClick.RemoveListener(action);
    }
    public void RemoveAllListeners()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        button.onClick.RemoveAllListeners();
        if (persistentButtonActions == null)
        {
            persistentButtonActions = new List<UnityAction>();
        }
        for (int i = 0; i < persistentButtonActions.Count; i++)
        {
            button.onClick.AddListener(persistentButtonActions[i]);
        }
    }
    public void AddPersistentListener(UnityAction action)
    {
        if (persistentButtonActions == null)
        {
            persistentButtonActions = new List<UnityAction>();
        }
        persistentButtonActions.Add(action);

        if (button == null)
        {
            button = GetComponent<Button>();
        }
        button.onClick.AddListener(action);
    }
    #endregion
}