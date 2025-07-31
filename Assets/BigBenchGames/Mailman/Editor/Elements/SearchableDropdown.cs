#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace BigBenchGames.Editor.Elements
{
    /// <summary>
    /// A searchable dropdown editor UI element
    /// </summary>
    public class SearchableDropdown : AdvancedDropdown
    {
        /// <summary>
        /// The return delegate signature for the result
        /// </summary>
        /// <param name="name">The return result</param>
        public delegate void OnSelection(string name);

        private List<string> entries = new List<string>();
        private string title = "";
        private System.Delegate OnItemSelected;

        /// <summary>
        /// A constructer for the searchable dropdown
        /// </summary>
        /// <param name="state">The state</param>
        /// <param name="_title">The title of the dropdown</param>
        /// <param name="_entries">A list of entries</param>
        /// <param name="_OnItemSelected">the result callback</param>
        public SearchableDropdown(AdvancedDropdownState state, string _title, List<string> _entries, OnSelection _OnItemSelected, Vector2 _minSize) : base(state)
        {
            minimumSize = _minSize;
            title = _title;
            entries = _entries;
            OnItemSelected = _OnItemSelected;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(title);
            foreach (string entry in entries)
            {
                var temp = new AdvancedDropdownItem(entry);
                root.AddChild(temp);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            OnItemSelected.DynamicInvoke(item.name);
        }
    }
}
#endif