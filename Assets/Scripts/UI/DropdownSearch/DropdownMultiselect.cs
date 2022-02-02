using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

namespace HevolusPackage.UXExtensions
{
    public abstract class DropdownMultiselect : MonoBehaviour
    {
        [SerializeField]
        private string unselectedPlaceholdrMsg = "Select...";
        [SerializeField]
        private string singleSelectedPlaceholdrMsg = "Selected: ";
        [SerializeField]
        private string multipleSelectedPlaceholdrMsg = "Selected ";

        [SerializeField]
        protected TMP_Text placeholder;
        [SerializeField]
        protected GameObject dropdownList;
        [SerializeField]
        protected GameObject itemsContainer;
        [SerializeField]
        protected GameObject itemTemplate;

        [SerializeField]
        protected Sprite selectedIcon;
        [SerializeField]
        protected Sprite deselectedIcon = null;

        [SerializeField]
        protected List<OptionData> dropdownOptions;

        [SerializeField]
        public ulong selectedItems { get; private set; }

        public delegate void OnItemSelectedEvent(ulong value);
        public event OnItemSelectedEvent OnItemSelected;

        #region UnityMessages

        protected virtual void Awake()
        {
            selectedItems = 0u;
            if (dropdownOptions == null) dropdownOptions = new List<OptionData>();
        }
        private void OnEnable()
        {
            HideDropdownList();
        }

        #endregion

        #region selectedItems
        protected void UpdateSelection()
        {
            int counter = 0;
            string lastSelected = "";
            for (int i = 0; i < dropdownOptions.Count; ++i)
            {
                if (CheckSelected(i))
                {
                    //dropdownOptions[i].image = selectedIcon;
                    ++counter;
                    lastSelected = dropdownOptions[i].text;
                }
                else
                {
                    //dropdownOptions[i].image = deselectedIcon;
                }
            }
            if (counter == 0)
            {
                placeholder.text = unselectedPlaceholdrMsg;
            }
            else if (counter == 1)
            {
                placeholder.text = $"{singleSelectedPlaceholdrMsg}{lastSelected}";
            }
            else
            {
                placeholder.text = $"{multipleSelectedPlaceholdrMsg}({counter})";
            }
        }
        private bool CheckSelected(int index)
        {
            return (1 == ((selectedItems >> index) & 1));
        }
        private void ToggleSelected(int index)
        {
            selectedItems ^= (1u << index);
        }
        #endregion

        #region DropdownList
        public void ClearDropdownList()
        {
            foreach (Transform child in itemsContainer.transform)
            {
                if (child.gameObject != itemTemplate)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        public void PopulateDropdownList()
        {
            for (int i = 0; i < dropdownOptions.Count; i++)
            {
                int index = i;
                OptionData option = dropdownOptions[i];
                AddItemToList(option.image, option.text, CheckSelected(index), () => SelectOption(index));
            }
        }
        protected abstract void AddItemToList(Sprite icon, string text, bool isSelected, UnityAction callback);
        protected abstract void UpdateDropdownList();
        protected void ShowDropdownList()
        {
            dropdownList.SetActive(true);
            UpdateDropdownList();
        }
        protected void HideDropdownList()
        {
            dropdownList.SetActive(false);
        }
        protected virtual void ToggleDropdownList()
        {
            if (dropdownList.activeSelf)
            {
                HideDropdownList();
            }
            else
            {
                ShowDropdownList();
            }
        }
        protected abstract void DisableDropdownList();
        protected abstract void EnableDropdownList();

        #endregion

        #region DropdownOptions
        public void AddOptions(List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                dropdownOptions.Add(new OptionData() { text = options[i] });
            }
        }
        public void AddOptions(List<OptionData> options)
        {
            dropdownOptions.AddRange(options);
        }
        private void ClearOptions()
        {
            dropdownOptions.Clear();
        }
        public void SetOptions(List<string> options, string placeholder = null, ulong selectedItems = 0u)
        {
            ClearOptions();
            AddOptions(options);

            this.placeholder.text = placeholder != null ? placeholder : "Select...";
            this.selectedItems = selectedItems;

            UpdateDropdownList();
        }
        public void SetOptions(List<OptionData> options, string placeholder = null, ulong selectedItems = 0u)
        {
            ClearOptions();
            AddOptions(options);

            this.placeholder.text = placeholder != null ? placeholder : "Select...";
            this.selectedItems = selectedItems;

            UpdateDropdownList();
        }
        #endregion

        #region SelectOption
        public void SelectOption(int index)
        {
            if (index < 0 || index >= dropdownOptions.Count)
            {
                return;
            }

            ToggleSelected(index);
            UpdateDropdownList();

            if (OnItemSelected != null)
            {
                OnItemSelected.Invoke(selectedItems);
            }
        }
        #endregion
    }
}
