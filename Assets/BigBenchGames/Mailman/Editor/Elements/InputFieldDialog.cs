#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BigBenchGames.Editor.Elements
{
    /// <summary>
    /// A custom input dialog window for editor use
    /// </summary>
    public class InputFieldDialog : EditorWindow
    {
        /// <summary>
        /// A delegate signature for when the submit button is pressed
        /// </summary>
        /// <param name="name">The text inputted</param>
        /// <param name="path">The path is present</param>
        public delegate void OnSubmit(string name, string path, List<string> duplicateNames);

        /// <summary>
        /// The on cancel delegate signature
        /// </summary>
        public delegate void OnCancel();

        private static string description = "";
        private static string titleText = "";
        private static string inputText = "";
        private static string path = "";
        private static string[] autoComplete;
        private static bool useAutoComplete = false;
        private static int autoCompleteDisplayAmount = 5;
        private static float autoCompleteTolerance = 5;
        private static OnCancel onCancel;
        private static OnSubmit onSubmit;
        private static bool duplicationMode = false;
        private static List<string> duplicationNames = new List<string>();

        /// <summary>
        /// Shows the dialog 
        /// </summary>
        /// <param name="_title">The title of the window</param>
        /// <param name="_description">The description of the window</param>
        /// <param name="_onSubmit">The on submit callback</param>
        /// <param name="_onCancel">The on cancel callback</param>
        public static void Display(string _title, string _description, OnSubmit _onSubmit, OnCancel _onCancel)
        {
            titleText = _title;
            description = _description;
            onCancel = _onCancel;
            onSubmit = _onSubmit;
            useAutoComplete = false;

            var window = (InputFieldDialog)EditorWindow.GetWindow(typeof(InputFieldDialog), true, titleText, true);
            window.minSize = new Vector2(500, 250);
            window.Show();
        }

        /// <summary>
        /// Shows the dialog 
        /// </summary>
        /// <param name="_title">The title of the window</param>
        /// <param name="_description">The description of the window</param>
        /// <param name="_defaultText">The default text</param>
        /// <param name="_onSubmit">The on submit callback</param>
        /// <param name="_onCancel">The on cancel callback</param>
        public static void Display(string _title, string _description, string _defaultText, OnSubmit _onSubmit, OnCancel _onCancel)
        {
            titleText = _title;
            description = _description;
            onCancel = _onCancel;
            onSubmit = _onSubmit;
            inputText = _defaultText;
            useAutoComplete = false;

            var window = (InputFieldDialog)EditorWindow.GetWindow(typeof(InputFieldDialog), true, titleText, true);
            window.minSize = new Vector2(500, 250);
            window.Show();
        }

        /// <summary>
        /// Shows the dialog with auto complete capabilities
        /// </summary>
        /// <param name="_title">The title of the window</param>
        /// <param name="_description">The description of the window</param>
        /// <param name="_defaultText">The default text</param>
        /// <param name="_autoComplete">A list of strings to auto complete to</param>
        /// <param name="_onSubmit">The on submit callback</param>
        /// <param name="_onCancel">The on cancel callback</param>
        /// <param name="_autoCompleteDisplayAmount">The amount of choices to show (default = 5)</param>
        /// <param name="_autoCompleteTolerance">The tolerance of the auto complete (default = 0.8)</param>
        public static void DisplayWithAutoComplete(string _title, string _description, string _defaultText, string[] _autoComplete, OnSubmit _onSubmit, OnCancel _onCancel, int _autoCompleteDisplayAmount = 5, float _autoCompleteTolerance = 0.8f)
        {
            titleText = _title;
            description = _description;
            onCancel = _onCancel;
            onSubmit = _onSubmit;
            inputText = _defaultText;
            autoComplete = _autoComplete;
            useAutoComplete = true;
            autoCompleteDisplayAmount = _autoCompleteDisplayAmount;
            autoCompleteTolerance = _autoCompleteTolerance;

            if (!EditorPrefs.HasKey(MailmanDispatcher.MailEditor.MAILMAN_PREF_DISPCOUNT_KEY))
                EditorPrefs.SetInt(MailmanDispatcher.MailEditor.MAILMAN_PREF_DISPCOUNT_KEY, autoCompleteDisplayAmount);
            else
                autoCompleteDisplayAmount = EditorPrefs.GetInt(MailmanDispatcher.MailEditor.MAILMAN_PREF_DISPCOUNT_KEY);

            var window = (InputFieldDialog)EditorWindow.GetWindow(typeof(InputFieldDialog), true, titleText, true);
            window.minSize = new Vector2(500, 250);
            window.Show();
        }

        /// <summary>
        /// Shows the dialog with a field for inputting a custom path
        /// </summary>
        /// <param name="_title">The title of the window</param>
        /// <param name="_description">The description of the window</param>
        /// <param name="_path">The default path to start at</param>
        /// <param name="_onSubmit">The on submit callback</param>
        /// <param name="_onCancel">The on cancel callback</param>
        public static void DisplayWithPathSelection(string _title, string _description, string _path, OnSubmit _onSubmit, OnCancel _onCancel)
        {
            path = _path;
            Display(_title, _description, _onSubmit, _onCancel);
        }

        /// <summary>
        /// Shows the dialog with a field for adding multiple duplication options for multi-create
        /// </summary>
        /// <param name="_title">The title of the window</param>
        /// <param name="_description">The description of the window</param>
        /// <param name="_onSubmit">The on submit callback</param>
        /// <param name="_onCancel">The on cancel callback</param>
        public static void DisplayWithDuplicationOptions(string _title, string _description, OnSubmit _onSubmit, OnCancel _onCancel)
        {
            duplicationNames.Clear();
            duplicationMode = true;
            Display(_title, _description, _onSubmit, _onCancel);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
            if (useAutoComplete)
            {
                int tempCount = EditorGUILayout.IntField("Display amount:", autoCompleteDisplayAmount);
                if(tempCount != autoCompleteDisplayAmount)
                {
                    autoCompleteDisplayAmount = tempCount;
                    EditorPrefs.SetInt(MailmanDispatcher.MailEditor.MAILMAN_PREF_DISPCOUNT_KEY, autoCompleteDisplayAmount);
                }
            }
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Name:", GUILayout.MaxWidth(50));
                if (!useAutoComplete)
                    inputText = EditorExtend.WithoutSelectAll(() => EditorGUILayout.TextField(inputText));
                else
                    inputText = EditorExtend.TextFieldAutoComplete(inputText, autoComplete, autoCompleteDisplayAmount, autoCompleteTolerance);
            }
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(path)) 
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Path:", GUILayout.MaxWidth(50));
                    if (GUILayout.Button(path, EditorStyles.miniButton))
                    {
                        string tempPath = EditorUtility.OpenFolderPanel("New Mail Location", path, "");
                        if (!string.IsNullOrEmpty(tempPath))
                        {
                            path = tempPath;
                            EditorPrefs.SetString(MailmanDispatcher.MailEditor.MAILMAN_PREF_PATH_KEY, path);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            if(duplicationMode)
            {
                // Draw the list items
                for (int i = 0; i < duplicationNames.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        // Draw the elements of your list item
                        EditorGUILayout.LabelField("Name:", GUILayout.MaxWidth(50));
                        duplicationNames[i] = EditorGUILayout.TextField(duplicationNames[i]);

                        // Add buttons to modify or remove list items
                        if (GUILayout.Button("Remove"))
                        {
                            duplicationNames.RemoveAt(i);
                            GUIUtility.ExitGUI();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Add a button to add new items to the list
                if (GUILayout.Button("Add Item"))
                {
                    duplicationNames.Add("");
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Cancel", EditorStyles.miniButtonRight))
                {
                    onCancel?.DynamicInvoke();
                    this.Close();
                }
                if (GUILayout.Button("Submit", EditorStyles.miniButtonLeft))
                {
                    onSubmit?.DynamicInvoke(inputText, path, duplicationNames);
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif