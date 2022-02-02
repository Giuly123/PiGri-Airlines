using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HevolusPackage.UXExtensions
{
    public class DropdownMultiselect2D : DropdownMultiselect
    {
        [SerializeField]
        private Button arrow;
        [SerializeField]
        protected float maxListHeight;

        #region UnityMessages

        protected override void Awake()
        {
            arrow.onClick.AddListener(ToggleDropdownList);
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
            if (dropdownItem.checkmark != null) dropdownItem.checkmark.sprite = isSelected ? selectedIcon : deselectedIcon;
            if (dropdownItem.button != null && callback != null) dropdownItem.button.onClick.AddListener(callback);
        }
        protected override void UpdateDropdownList()
        {
            ClearDropdownList();
            UpdateSelection();
            PopulateDropdownList();
            UpdateDropdownSize();
        }
        private void UpdateDropdownSize()
        {
            RectTransform dropdownListTransform = (RectTransform)dropdownList.transform;
            float height = ((RectTransform)itemTemplate.transform).sizeDelta.y * dropdownOptions.Count;
            dropdownListTransform.sizeDelta = new Vector2(dropdownListTransform.sizeDelta.x, height > maxListHeight ? maxListHeight : height);
        }
        protected override void DisableDropdownList()
        {
            arrow.gameObject.SetActive(false);
            HideDropdownList();
        }
        protected override void EnableDropdownList()
        {
            arrow.gameObject.SetActive(true);
        }

        #endregion
    }
}
