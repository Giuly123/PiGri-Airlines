using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class DropdownSearch : MonoBehaviour
{

    public class OptionData: TMPro.TMP_Dropdown.OptionData
    {
        public object ExtraData = null;
    }

    [SerializeField]
    protected TMP_InputField inputField;
    [SerializeField]
    protected TMP_Text placeholder;
    [SerializeField]
    protected GameObject dropdownList;
    [SerializeField]
    protected GameObject itemsContainer;
    [SerializeField]
    protected GameObject itemTemplate;
    [SerializeField]
    protected List<OptionData> dropdownOptions = new List<OptionData>();
    [SerializeField]
    protected List<OptionData> filteredOptions;
    [SerializeField]
    protected int maxFilteredOptions = 10;

    public string Text => inputField.text;
    public int SelectedIndex { get; private set; }
    public OptionData SelectedOption => SelectedIndex > -1 && SelectedIndex < dropdownOptions.Count ? dropdownOptions[SelectedIndex] : null;

    public delegate void OnItemSelectedEvent(int value);
    public event OnItemSelectedEvent OnItemSelected;

    public delegate void OnItemDeselectedEvent();
    public event OnItemDeselectedEvent OnItemDeselected;

    #region UnityMessages

    protected virtual void Awake()
    {
        SelectedIndex = -1;
        inputField.onValueChanged.AddListener((_) => UpdateDropdownList());
        if (dropdownOptions == null) dropdownOptions = new List<OptionData>();
        filteredOptions = new List<OptionData>(dropdownOptions);
    }
    private void OnEnable()
    {
        ToggleDropdownList(false);
    }

    #endregion

    #region DropdownList

    protected void ClearDropdownList()
    {
        foreach (Transform child in itemsContainer.transform)
        {
            if (child.gameObject != itemTemplate)
            {
                Destroy(child.gameObject);
            }
        }
    }
    protected void PopulateDropdownList()
    {
        int filteredIndex = -1;
        if (SelectedIndex >= 0 && SelectedIndex < dropdownOptions.Count)
        {
            filteredIndex = filteredOptions.IndexOf(dropdownOptions[SelectedIndex]);
        }

        for (int i = 0; i < filteredOptions.Count; i++)
        {
            int index = i;
            OptionData option = filteredOptions[index];
            AddItemToList(option.image, option.text, filteredIndex == index, () => SelectFilteredOption(index));
        }
    }
    protected abstract void AddItemToList(Sprite icon, string text, bool isSelected, UnityAction callback);
    protected abstract void UpdateDropdownList();
    public void ToggleDropdownList() => ToggleDropdownList(!dropdownList.activeSelf);
    protected virtual void ToggleDropdownList(bool toggle)
    {
        if (toggle)
        {
            if (!dropdownList.activeSelf)
            {
                dropdownList.SetActive(true);
                UpdateDropdownList();
            }
        }
        else
        {
            dropdownList.SetActive(false);
        }
    }
    protected abstract void DisableDropdownList();
    protected abstract void EnableDropdownList();
    protected abstract void DisableClearButton();
    protected abstract void EnableClearButton();

    #endregion

    #region DropdownOptions
    private void AddOptions(List<string> options)
    {
        for (int i = 0; i < options.Count; i++)
        {
            dropdownOptions.Add(new OptionData() { text = options[i] });
        }
    }
    private void AddOptions(List<OptionData> options)
    {
        dropdownOptions.AddRange(options);
    }
    private void ClearOptions()
    {
        dropdownOptions.Clear();
    }

    protected virtual void OnSetOptions(int currentSelected) { }

    public void SetOptions(List<string> options, string placeholder = null, int currentSelected = -1)
    {
        ClearOptions();
        if (options != null)
        {
            AddOptions(options);
        }

        inputField.SetTextWithoutNotify(null);
        this.placeholder.text = placeholder != null ? placeholder : "Enter value...";
        SelectedIndex = currentSelected;

        bool isDropdownOpen = dropdownList.activeSelf;
        UpdateDropdownList();
        ToggleDropdownList(isDropdownOpen);
    }

    public void SetOptions(List<OptionData> options, string placeholder = null, int currentSelected = -1)
    {
        ClearOptions();
        if (options != null)
        {
            AddOptions(options);
        }

        inputField.SetTextWithoutNotify(null);
        this.placeholder.text = placeholder != null ? placeholder : "Enter value...";
        SelectedIndex = currentSelected;

        bool isDropdownOpen = dropdownList.activeSelf;
        UpdateDropdownList();
        ToggleDropdownList(isDropdownOpen);
    }

    protected void FilterOptions() => FilterOptions(inputField.text);
    public void FilterOptions(string input)
    {
        if (input.Length == 0)
        {
            filteredOptions = new List<OptionData>(dropdownOptions);
        }
        else
        {
            filteredOptions = dropdownOptions.FindAll(option => option.text.ToLower().IndexOf(input.ToLower()) >= 0);
        }

        if(filteredOptions.Count - maxFilteredOptions > 0)
        {
            filteredOptions.RemoveRange(maxFilteredOptions, filteredOptions.Count - maxFilteredOptions);
        }

        if (filteredOptions.Count <= 0)
        {
            DisableDropdownList();
        }
        else
        {
            //if(filteredOptions.Count == 1 && filteredOptions[0].text == input)
            //{
            //    SelectFilteredOption(0);
            //}
            EnableDropdownList();
            dropdownList.SetActive(true);
        }
    }
    #endregion

    #region SelectOption
    public void SelectFilteredOption(int filteredIndex)
    {
        if (filteredIndex < 0 || filteredIndex >= filteredOptions.Count)
        {
            return;
        }
        SelectOption(dropdownOptions.IndexOf(filteredOptions[filteredIndex]));
    }

    public void SelectOption(int index)
    {
        if (index < 0 || index >= dropdownOptions.Count)
        {
            return;
        }
        SelectedIndex = index;

        if (inputField.text != dropdownOptions[SelectedIndex].text)
        {
            inputField.text = dropdownOptions[SelectedIndex].text;
        }
        inputField.ReleaseSelection();

        if (OnItemSelected != null)
        {
            OnItemSelected.Invoke(SelectedIndex);
        }

        EnableClearButton();
        OnSetOptions(SelectedIndex);
    }
    public void SelectOption(string optionName)
    {
        int index = -1;
        for (int i = 0; i < dropdownOptions.Count; i++)
        {
            if (dropdownOptions[i].text == optionName)
            {
                index = i;
                break;
            }
        }
        SelectOption(index);
    }

    public void ClearSelection()
    {
        SelectedIndex = -1;
        inputField.text = "";
        if (OnItemDeselected != null)
        {
            OnItemDeselected.Invoke();
        }
    }
    #endregion

}
