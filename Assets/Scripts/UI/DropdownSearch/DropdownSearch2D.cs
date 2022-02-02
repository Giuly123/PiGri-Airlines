using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DropdownSearch2D : DropdownSearch
{
    [SerializeField]
    private Button arrow;
    [SerializeField]
    private ButtonHandler clearButton;
    [SerializeField]
    private float maxListHeight;
    [SerializeField]
    private Image background;

    private Coroutine fadeCoroutine;

    [SerializeField]
    private float fadeTime = 2;

    #region UnityMessages

    protected override void Awake()
    {
        arrow.onClick.AddListener(ToggleDropdownList);
        clearButton.AddListener(ClearSelection);
        base.Awake();
    }

    #endregion

    #region DropdownList
    protected override void AddItemToList(Sprite icon, string text, bool isSelected, UnityAction callback)
    {
        GameObject go = Instantiate(itemTemplate, itemsContainer.transform);
        go.SetActive(true);
        DropdownItemHandler2D dropdownItem = go.GetComponent<DropdownItemHandler2D>();
        if (dropdownItem.icon != null && icon != null) dropdownItem.icon.sprite = icon;
        if (dropdownItem.label != null) dropdownItem.label.text = text;
        if (dropdownItem.checkmark != null) dropdownItem.checkmark.gameObject.SetActive(isSelected);
        if (dropdownItem.button != null && callback != null) dropdownItem.button.onClick.AddListener(callback);
    }
    protected override void UpdateDropdownList()
    {
        ClearDropdownList();
        FilterOptions();
        PopulateDropdownList();
        UpdateDropdownSize();
    }
    private void UpdateDropdownSize()
    {
        RectTransform dropdownListTransform = (RectTransform)dropdownList.transform;
        float height = ((RectTransform)itemTemplate.transform).sizeDelta.y * filteredOptions.Count;
        dropdownListTransform.sizeDelta = new Vector2(dropdownListTransform.sizeDelta.x, height > maxListHeight ? maxListHeight : height);
    }
    protected override void DisableDropdownList()
    {
        arrow.gameObject.SetActive(false);
        ToggleDropdownList(false);
    }
    protected override void EnableDropdownList()
    {
        arrow.gameObject.SetActive(true);
        DisableClearButton();
    }
    protected override void DisableClearButton()
    {
        clearButton.gameObject.SetActive(false);
    }
    protected override void EnableClearButton()
    {
        clearButton.gameObject.SetActive(true);
        DisableDropdownList();
    }

    protected override void OnSetOptions(int currentSelected)
    {
        if (currentSelected != -1)
        {
            if(fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(CoBackgroundFadeColor());
        }
    }
    
    IEnumerator CoBackgroundFadeColor()
    {
        background.color = Color.yellow;
        float fadeStart = 0;

        while (background.color != Color.white)
        {
            fadeStart += Time.deltaTime * fadeTime;

            background.color = Color.Lerp(background.color, Color.white, fadeStart);
            yield return null;
        }
    }

    #endregion

}
