#if UNITY_EDITOR
using BigBenchGames.Tools.MailmanDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BigBenchGames.Editor.MailmanDispatcher
{
    /// <summary>
    /// The editor class for mail, used to create, change, and delete mail using a GUI
    /// </summary>
    public class MailEditor : EditorWindow
    {
        private const string SELECTED_MAIL_NAME = "SelectedMailName";

        /// <summary>
        /// The key used for accessing the mail path from editorprefs
        /// </summary>
        public static string MAILMAN_PREF_PATH_KEY = "Mailman.Mail.Path";

        /// <summary>
        /// The key used for accssing the mail autocomplete number from editorprefs
        /// </summary>
        public static string MAILMAN_PREF_DISPCOUNT_KEY = "Mailman.Mail.Dispcount";

        /// <summary>
        /// A class used to store editing data about a mail class
        /// </summary>
        public class MailView
        {
            /// <summary>
            /// Different types of variable types supported by the editor system
            /// </summary>
            public enum FieldType { SBYTE, BYTE, SHORT, USHORT, INT, UINT, LONG, ULONG, CHAR, FLOAT, DOUBLE, BOOL, DECIMAL, STRING, COMPLEX }

            /// <summary>
            /// A data structure used to store information about the individual attributes of the mail class
            /// </summary>
            public class AttributeData
            {
                /// <summary>
                /// The name of the variable
                /// </summary>
                public string Name;

                /// <summary>
                /// The type of the variable
                /// </summary>
                public FieldType Type;

                /// <summary>
                /// If complex, the variable signature for the variable.
                /// </summary>
                public string ComplexSignature;
            }

            /// <summary>
            /// The name of the mail class
            /// </summary>
            public string Name;

            /// <summary>
            /// The class type
            /// </summary>
            public Type Type;

            /// <summary>
            /// A list of attributes this class has
            /// </summary>
            public List<AttributeData> Attributes;

            /// <summary>
            /// Has the content of the mail view been changed in editor
            /// </summary>
            public bool HasBeenChanged;

            /// <summary>
            /// The path of the mail class file
            /// </summary>
            public string Path;

            /// <summary>
            /// Is the mail class marked as read only
            /// </summary>
            public bool IsReadOnly;

            /// <summary>
            /// The version of the mail class
            /// </summary>
            public int Version;

            /// <summary>
            /// The description of the class
            /// </summary>
            public string Description;

            /// <summary>
            /// Are the changes valid
            /// </summary>
            public bool IsCurrentChangesValid => report != null && report.IsValid;

            private List<AttributeData> flaggedForDeletion = new List<AttributeData>();
            private MailEditorValidator.MailEditorValidationReport report;
            private Vector2 scrollPos = new Vector2(0, 0);
            private string fileText = "";
            private List<string> loadedClasses = new List<string>();

            public MailView()
            {
                Attributes = new List<AttributeData>();
                loadedClasses = GetAllLoadedClasses();
                Description = "";
            }

            public MailView(MailView other)
            {
                Name = other.Name;
                Attributes = other.Attributes;
                IsReadOnly = other.IsReadOnly;
                Version = other.Version;
                Description = other.Description;
                loadedClasses = GetAllLoadedClasses();
            }

            /// <summary>
            /// The constructure for the mail view, if the type is passed in, it generates the full view
            /// </summary>
            /// <param name="type">The type of the class</param>
            public MailView(Type type)
            {
                Name = type.Name;
                Type = type;

                HasBeenChanged = false;
                var instance = Activator.CreateInstance(Type);
                Path = (instance as Mail).GetSourcePath();
                IsReadOnly = (instance as Mail).GetReadOnlyAttribute(Type);
                Version = (instance as Mail).GetVersionAttribute(Type);

                if (string.IsNullOrEmpty(fileText))
                    fileText = File.ReadAllText(Path);

                string newLineString = "";
                TryDetectNewLine(Path, out newLineString);
                string summeryString = string.Format("/// <summary>{0}", newLineString);
                int firstSummery = fileText.IndexOf(summeryString) + summeryString.Length;
                int endSummery = fileText.IndexOf(string.Format("{0}    /// </summary>", newLineString));
                Description = fileText.Substring(firstSummery, endSummery - firstSummery);
                Description = Description.Replace("    /// ", "");

                Attributes = new List<AttributeData>();
                foreach (var attribute in type.GetFields())
                {
                    if (attribute.IsStatic)
                        continue;
                    var attributeData = new AttributeData() { Name = attribute.Name, Type = GetEnumTypeFromSystemType(attribute.FieldType.Name) };
                    if (attributeData.Type == FieldType.COMPLEX)
                    {
                        string line = GetSubstring(attributeData.Name, fileText);
                        int firstSpace = line.IndexOf(" ");
                        int lastSpace = line.LastIndexOf(" ");
                        attributeData.ComplexSignature = line.Substring(firstSpace + 1, lastSpace - firstSpace - 1);
                    }
                    Attributes.Add(attributeData);
                }

                loadedClasses = GetAllLoadedClasses();
            }

            /// <summary>
            /// Draws the mail view in editor
            /// </summary>
            public void DrawMail()
            {
                var oldColor = GUI.backgroundColor;
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Description", GUILayout.MaxWidth(70));
                        var descStyle = new GUIStyle(EditorStyles.textArea);
                        descStyle.wordWrap = true;

                        if (IsReadOnly)
                            GUI.enabled = false;
                        string tempDesc = Elements.EditorExtend.WithoutSelectAll(() => EditorGUILayout.TextArea(Description, descStyle));
                        if (IsReadOnly)
                            GUI.enabled = true;

                        if (tempDesc != Description)
                        {
                            Description = tempDesc;
                            HasBeenChanged = true;
                        }
                    }
                    GUILayout.EndHorizontal();

                    Color original = GUI.color;
                    if (Version != MailCodeGenerator.VERSION)
                        GUI.color = Color.red;
                    EditorGUILayout.LabelField(string.Format("Version: {0}", Version));
                    GUI.color = original;

                    bool tempReadOnly = EditorGUILayout.Toggle("Is Read Only", IsReadOnly);
                    if (tempReadOnly != IsReadOnly)
                    {
                        IsReadOnly = tempReadOnly;
                        HasBeenChanged = true;
                    }

                    if (Attributes.Count > 0)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Name", EditorStyles.boldLabel);
                            GUILayout.Label("Type", EditorStyles.boldLabel);
                        }
                        GUILayout.EndHorizontal();
                    }

                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    {
                        foreach (var attribute in Attributes)
                        {
                            if (IsReadOnly)
                                GUI.enabled = false;
                            GUILayout.BeginHorizontal();
                            {
                                if (report != null && report.variableNameViolations.Contains(attribute.Name))
                                    GUI.backgroundColor = Color.red;

                                string tempName = Elements.EditorExtend.WithoutSelectAll(() => EditorGUILayout.TextField(attribute.Name, GUILayout.MinWidth(200)));
                                    
                                GUI.backgroundColor = oldColor;

                                if (tempName != attribute.Name)
                                {
                                    attribute.Name = tempName;
                                    HasBeenChanged = true;
                                }

                                GUILayout.BeginHorizontal(GUILayout.MinWidth(400), GUILayout.MaxWidth(600));
                                {
                                    var names = System.Enum.GetNames(typeof(FieldType));
                                    names = names.ToList().ConvertAll(x => x.ToLower()).ToArray();
                                    FieldType tempType = (FieldType)EditorGUILayout.Popup((int)attribute.Type, names);
                                    if (tempType != attribute.Type)
                                    {
                                        attribute.Type = tempType;
                                        HasBeenChanged = true;
                                    }

                                    if (attribute.Type == FieldType.COMPLEX)
                                    {
                                        string tempComplex = Elements.EditorExtend.WithoutSelectAll(() => EditorGUILayout.TextField(attribute.ComplexSignature, GUILayout.MinWidth(300)));
                                        if(tempComplex != attribute.ComplexSignature)
                                        {
                                            attribute.ComplexSignature = tempComplex;
                                            HasBeenChanged = true;
                                        }

                                        if (GUILayout.Button(EditorGUIUtility.IconContent("Search Icon").image, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight), GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight * 2)))
                                        {
                                            Elements.InputFieldDialog.DisplayWithAutoComplete("Input complex signature", "Please input the correct complex signature", attribute.ComplexSignature, loadedClasses.ToArray(), (name, path, dNames) =>
                                            {
                                                if (name != attribute.ComplexSignature)
                                                {
                                                    attribute.ComplexSignature = name;
                                                    HasBeenChanged = true;
                                                }
                                            }, null);
                                        }
                                    }
                                }
                                GUILayout.EndHorizontal();

                                GUI.backgroundColor = Color.red;
                                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.MaxWidth(20)))
                                {
                                    flaggedForDeletion.Add(attribute);
                                    HasBeenChanged = true;
                                }
                                GUI.backgroundColor = oldColor;
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndScrollView();

                    foreach (var flagged in flaggedForDeletion)
                    {
                        Attributes.Remove(flagged);
                    }
                    flaggedForDeletion.Clear();

                    MailEditor.DrawUILine(Color.grey, 2, 10);

                    GUI.backgroundColor = Color.cyan;
                    if (GUILayout.Button("Add Field", EditorStyles.miniButton))
                    {
                        Attributes.Add(new AttributeData() { Name = FindLowestFreeString("NewField"), Type = FieldType.INT });
                        HasBeenChanged = true;
                    }
                    GUI.backgroundColor = oldColor;

                    GUI.enabled = true;

                    GUILayout.FlexibleSpace();

                    if (report != null && !string.IsNullOrEmpty(report.Errors))
                    {
                        var s = new GUIStyle(EditorStyles.textField);
                        s.normal.textColor = Color.red;
                        s.wordWrap = true;
                        EditorGUILayout.LabelField(report.Errors, s);
                    }

                    GUILayout.BeginHorizontal();
                    {
                        GUI.backgroundColor = Color.red;

                        GUIStyle deleteStyle = new GUIStyle(EditorStyles.miniButtonLeft);
                        deleteStyle.fontSize = 30;
                        deleteStyle.fixedHeight = 40;
                        deleteStyle.margin.right = 10;

                        if (GUILayout.Button("Delete", deleteStyle))
                        {
                            if (EditorUtility.DisplayDialog("Delete Mail?", string.Format("Are you sure you want to delete the mail: {0}", Name), "Yes", "No"))
                            {
                                EditorPrefs.SetString(SELECTED_MAIL_NAME, string.Empty);
                                MailCodeGenerator.DeleteMail(Path);
                                AssetDatabase.Refresh();
                            }
                        }

                        GUI.backgroundColor = oldColor;

                        if (HasBeenChanged)
                            GUI.backgroundColor = Color.green;

                        if (report != null)
                            GUI.enabled = report.IsValid;
                        if (IsReadOnly && !HasBeenChanged)
                            GUI.enabled = false;

                        GUIStyle saveStyle = new GUIStyle(EditorStyles.miniButtonRight);
                        saveStyle.fontSize = 30;
                        saveStyle.fixedHeight = 40;
                        saveStyle.margin.left = 10;

                        if (GUILayout.Button("Save", saveStyle))
                        {
                            if (EditorUtility.DisplayDialog("Save Mail", "Are you sure you want to save? This will override the mail class", "Yes", "No"))
                            {
                                EditorPrefs.SetString(SELECTED_MAIL_NAME, Name);
                                MailCodeGenerator.GenerateMailFromTemplate(this);
                                AssetDatabase.Refresh();
                            }
                        }
                        GUI.enabled = true;

                        GUI.backgroundColor = oldColor;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Select File", EditorStyles.miniButtonLeft))
                        {
                            string fixedDataPath = Application.dataPath.Replace("/", "\\");
                            string relativePath = Path.Replace(fixedDataPath, "Assets");
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(relativePath);
                        }

                        if (GUILayout.Button("Rename", EditorStyles.miniButtonRight))
                        {
                            if (AskForSaveIfChanged())
                            {
                                Elements.InputFieldDialog.Display("Rename", "Enter the new name of the Mail class", Name, (name, path, dNames) =>
                                {
                                    Name = name;
                                    MailCodeGenerator.DeleteMail(Path);
                                    Path = Path.Substring(0, Path.LastIndexOf('\\')) + '\\' + Name + ".cs";
                                    Path = MailCodeGenerator.GenerateMailFromTemplate(this);
                                    AssetDatabase.Refresh();
                                }, null);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                report = MailEditorValidator.ValidateMail(this);
            }

            /// <summary>
            /// Asks the user to save this mail class before continuing
            /// </summary>
            /// <returns>Returns true if saved</returns>
            public bool AskForSaveIfChanged()
            {
                if (this != null && this.HasBeenChanged && EditorUtility.DisplayDialog("Unsaved Mail", "You have unsaved work on this Mail type, do you want to save before continuing?", "Yes", "No"))
                {
                    if (this.IsCurrentChangesValid)
                    {
                        MailCodeGenerator.GenerateMailFromTemplate(this);
                        AssetDatabase.Refresh();
                        return true;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Unsaved Mail", "Changes are invalid and cannot be saved", "Ok");
                        return false;
                    }
                }

                return true;
            }

            private bool TryDetectNewLine(string path, out string newLine)
            {
                using (var fileStream = File.OpenRead(path))
                {
                    char prevChar = '\0';

                    // Read the first 4000 characters to try and find a newline
                    for (int i = 0; i < 4000; i++)
                    {
                        int b;
                        if ((b = fileStream.ReadByte()) == -1) break;

                        char curChar = (char)b;

                        if (curChar == '\n')
                        {
                            newLine = prevChar == '\r' ? "\r\n" : "\n";
                            return true;
                        }

                        prevChar = curChar;
                    }

                    // Returning false means could not determine linefeed convention
                    newLine = Environment.NewLine;
                    Debug.LogWarning("Newline string not found...");
                    return false;
                }
            }

            private string FindLowestFreeString(string defaultString)
            {
                string final = defaultString;

                if (Attributes.Find(x => x.Name == string.Format("{0}", final)) == null)
                    return final;

                int i = 0;
                while (i < 100000)
                {
                    if (Attributes.Find(x => x.Name == string.Format("{0}{1}", final, i)) == null)
                        return string.Format("{0}{1}", final, i);
                    i++;
                }

                return final;
            }

            private FieldType GetEnumTypeFromSystemType(string type)
            {
                switch (type)
                {
                    case "SByte":
                        return FieldType.SBYTE;
                    case "Byte":
                        return FieldType.BYTE;
                    case "Int16":
                        return FieldType.SHORT;
                    case "UInt16":
                        return FieldType.USHORT;
                    case "Int32":
                        return FieldType.INT;
                    case "UInt32":
                        return FieldType.UINT;
                    case "Int64":
                        return FieldType.LONG;
                    case "UInt64":
                        return FieldType.ULONG;
                    case "Char":
                        return FieldType.CHAR;
                    case "Single":
                        return FieldType.FLOAT;
                    case "Double":
                        return FieldType.DOUBLE;
                    case "Boolean":
                        return FieldType.BOOL;
                    case "Decimal":
                        return FieldType.DECIMAL;
                    case "String":
                        return FieldType.STRING;
                    default:
                        return FieldType.COMPLEX;
                }
            }

            private string GetSubstring(string searchPattern, string inputText)
            {
                string pattern = string.Format(@"public\s.*\s{0}", searchPattern);

                if (Regex.IsMatch(inputText, pattern))
                {
                    return Regex.Match(inputText, pattern).Value;
                }
                return "";
            }

            private List<string> GetAllLoadedClasses()
            {
                List<string> names = new List<string>();
                foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!ass.IsDynamic)
                    {
                        foreach (Type t in ass.GetExportedTypes())
                        {
                            bool isStatic = t.GetConstructor(Type.EmptyTypes) == null && t.IsAbstract && t.IsSealed;
                            //Removed IsAbstract Check to allow for abstract classes
                            if (t.IsPublic && !isStatic)
                                names.Add(t.FullName);
                        }
                    }
                }
                return names;
            }
        }

        static MailEditor editor;
        private MailView selectedMail;
        private string trySelectMail = string.Empty;
        private string newMailPath;
        private List<Type> mailTypes = new List<Type>();

        [MenuItem("Tools/Big Bench Games/Mail Editor")]
        private static void ShowEditor()
        {
            editor = (MailEditor)EditorWindow.GetWindow(typeof(MailEditor));
            editor.titleContent = new GUIContent("Mail Editor");
            editor.minSize = new Vector2(500, 500);
            editor.Show();
        }

        private void OnEnable()
        {
            UnityEditor.Compilation.CompilationPipeline.compilationStarted += OnScriptCompilationStarted;
        }

        private void OnDisable()
        {
            if (selectedMail != null)
                selectedMail.AskForSaveIfChanged();
            UnityEditor.Compilation.CompilationPipeline.compilationStarted -= OnScriptCompilationStarted;
        }

        private void OnGUI()
        {
            var oldColor = GUI.backgroundColor;
            if (editor == null)
                editor = (MailEditor)EditorWindow.GetWindow(typeof(MailEditor));
            Event e = Event.current;
            var windowClickArea = GUI.Window(0, new Rect(0, 0, editor.position.width, editor.position.height), drawWindow, "Mail Editor");
            if (e.type == EventType.MouseDown && windowClickArea.Contains(e.mousePosition))
                GUI.FocusControl(null);
            EditorGUI.DrawRect(windowClickArea, new Color(0.25f, 0.25f, 0.25f));

            if (selectedMail != null && !File.Exists(selectedMail.Path))
                selectedMail = null;

            GUILayout.BeginVertical();
            {
                GUILayout.Label("Mail Editor", EditorStyles.boldLabel);

                var rect = GUILayoutUtility.GetRect(new GUIContent(selectedMail == null ? "Click here to Select Mail" : selectedMail.Name), EditorStyles.toolbarDropDown);
                if (GUI.Button(rect, new GUIContent(selectedMail == null ? "Click here to Select Mail" : selectedMail.Name), EditorStyles.miniButton))
                {
                    mailTypes = GetInheritedClasses(typeof(Mail));
                    List<string> mailNames = new List<string>();
                    foreach (var mail in mailTypes)
                    {
                        mailNames.Add(mail.Name);
                    }

                    var dropdown = new BigBenchGames.Editor.Elements.SearchableDropdown(new AdvancedDropdownState(), "Mail", mailNames, OnItemSelected, new Vector2(200, 200));

                    dropdown.Show(rect);
                }

                DrawUILine(Color.grey, 2, 10);

                if (selectedMail == null && EditorPrefs.GetString(SELECTED_MAIL_NAME) != string.Empty)
                {
                    trySelectMail = EditorPrefs.GetString(SELECTED_MAIL_NAME);
                    EditorPrefs.SetString(SELECTED_MAIL_NAME, string.Empty);
                }

                if (trySelectMail != string.Empty)
                {
                    mailTypes = GetInheritedClasses(typeof(Mail));
                    OnItemSelected(trySelectMail);
                    if (selectedMail != null)
                        trySelectMail = string.Empty;
                }

                if (selectedMail != null)
                    selectedMail.DrawMail();

                if(selectedMail != null)
                    DrawUILine(Color.grey, 2, 30);

                if(selectedMail != null)
                {
                    if (GUILayout.Button("Duplicate", EditorStyles.miniButton))
                    {
                        Elements.InputFieldDialog.DisplayWithDuplicationOptions("Duplicate", "Duplicate current mail once or many times", (name, path, dNames) =>
                        {
                            if (selectedMail.AskForSaveIfChanged())
                            {
                                dNames.Add(name);
                                for (int i = 0; i < dNames.Count; i++)
                                {
                                    mailTypes = GetInheritedClasses(typeof(Mail));
                                    MailView duplicatedView = new MailView(selectedMail);
                                    string finalName = dNames[i];
                                    string tempFilePath = string.Format("{0}{1}.cs", selectedMail.Path.Substring(0, selectedMail.Path.LastIndexOf("\\") + 1), finalName);
                                    if (mailTypes.Find((x) => x.Name == dNames[i]) != null || File.Exists(tempFilePath))
                                    {
                                        int j = 1;
                                        while (j < 10000)
                                        {
                                            string tempName = string.Format("{0}{1}", dNames[i], j);
                                            tempFilePath = string.Format("{0}{1}.cs", selectedMail.Path.Substring(0, selectedMail.Path.LastIndexOf("\\") + 1), tempName).Replace(@"\\", @"\");
                                            if (mailTypes.Find((x) => x.Name == tempName) == null && !File.Exists(tempFilePath))
                                            {
                                                finalName = tempName;
                                                EditorUtility.DisplayDialog("Warning", string.Format("A mail class by the name of {0} already exists. Creating new mail under altered name: {1}", dNames[i], finalName), "Ok");
                                                break;
                                            }
                                            j++;
                                        }
                                    }
                                    else if (!MailEditorValidator.IsStringValidVariable(finalName))
                                    {
                                        int j = 1;
                                        while (j < 10000)
                                        {
                                            string tempName = string.Format("{0}{1}", selectedMail.Name, j);
                                            tempFilePath = string.Format("{0}{1}.cs", selectedMail.Path.Substring(0, selectedMail.Path.LastIndexOf("\\") + 1), tempName).Replace(@"\\", @"\");
                                            if (mailTypes.Find((x) => x.Name == tempName) == null && !File.Exists(tempFilePath))
                                            {
                                                finalName = tempName;
                                                EditorUtility.DisplayDialog("Warning", string.Format("Tried to make a mail class by the name of {0}. This is an illegal name. Creating new mail under altered name: {1}", dNames[i], finalName), "Ok");
                                                break;
                                            }
                                            j++;
                                        }
                                    }

                                    duplicatedView.Name = finalName;
                                    duplicatedView.Path = string.Format("{0}{1}.cs", selectedMail.Path.Substring(0, selectedMail.Path.LastIndexOf("\\") + 1), duplicatedView.Name);
                                    string duplcatedMailPath = MailCodeGenerator.GenerateMailFromTemplate(duplicatedView);
                                    duplcatedMailPath = duplcatedMailPath.Replace("\\", "/");
                                    AssetDatabase.ImportAsset(string.Format("Assets{0}", duplcatedMailPath.Replace(Application.dataPath, "")));
                                    AssetDatabase.Refresh();
                                }
                            }
                        }, null);
                    }
                }

                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("Create New Mail", EditorStyles.miniButton))
                {
                    GUI.backgroundColor = oldColor;
                    if(selectedMail != null)
                        selectedMail.AskForSaveIfChanged();

                    string defaultPath = "";
                    //string relativeDefaultPath = "";
                    if (!EditorPrefs.HasKey(MAILMAN_PREF_PATH_KEY))
                    {
                        defaultPath = Activator.CreateInstance<MAILNAME>().GetSourcePath();
                        defaultPath = defaultPath.Substring(0, defaultPath.LastIndexOf("\\"));
                        //relativeDefaultPath = defaultPath.Substring(defaultPath.IndexOf("Assets"), defaultPath.LastIndexOf('\\') - defaultPath.IndexOf("Assets"));
                        EditorPrefs.SetString(MAILMAN_PREF_PATH_KEY, defaultPath);
                    }
                    else
                    {
                        defaultPath = EditorPrefs.GetString(MAILMAN_PREF_PATH_KEY);
                        //relativeDefaultPath = defaultPath.Substring(defaultPath.IndexOf("Assets"), defaultPath.LastIndexOf('\\') - defaultPath.IndexOf("Assets"));
                    }

                    Elements.InputFieldDialog.DisplayWithPathSelection("Create New", "Create new mail class", defaultPath, (name, path, dNames) =>
                    {
                        mailTypes = GetInheritedClasses(typeof(Mail));
                        if (mailTypes.Find((x) => x.Name == name) != null)
                            EditorUtility.DisplayDialog("Error", string.Format("A mail class by the name of {0} already exists. Please use another name.", name), "Ok");
                        else if(!MailEditorValidator.IsStringValidVariable(name))
                            EditorUtility.DisplayDialog("Error", string.Format("You have entered an invalid class name, please be sure to follow the c# standards for identifier names", name), "Ok");
                        else
                        {
                            selectedMail = null;
                            MailView newView = new MailView();
                            newView.Name = name;
                            //if (path == relativeDefaultPath)
                            //    path = defaultPath.Substring(0, defaultPath.LastIndexOf('\\'));
                            path = string.Format("{0}/{1}.cs", path, newView.Name);
                            newView.Path = path;
                            newMailPath = MailCodeGenerator.GenerateMailFromTemplate(newView);
                            ImportAndTrySelectNewMail(name);
                        }
                    }, null);
                }
                GUI.backgroundColor = oldColor;

                if (GUILayout.Button("Regenerate all mail", EditorStyles.miniButton))
                {
                    if (EditorUtility.DisplayDialog("Proceed", "Would you like to rebuild all mail types? Note: this will not rebuild mail set to ReadOnly", "Yes", "No"))
                    {
                        if (selectedMail != null)
                            selectedMail.AskForSaveIfChanged();

                        mailTypes = GetInheritedClasses(typeof(Mail));
                        foreach (var mType in mailTypes)
                        {
                            MailView newView = new MailView(mType);
                            if (!newView.IsReadOnly)
                            {
                                MailCodeGenerator.GenerateMailFromTemplate(newView);
                                selectedMail = newView;
                            }
                        }
                        AssetDatabase.Refresh();
                    }
                }
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a UI line in the editor OnGUI call
        /// </summary>
        /// <param name="color">The color of the line</param>
        /// <param name="thickness">The thickness of the line</param>
        /// <param name="padding">The top and bottom padding of the line</param>
        public static void DrawUILine(Color color = default, int thickness = 1, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        private void drawWindow(int aID)
        {
            // it's a dummy proc
        }

        private void ImportAndTrySelectNewMail(string name)
        {
            newMailPath = newMailPath.Replace("\\", "/");
            AssetDatabase.ImportAsset(string.Format("Assets{0}", newMailPath.Replace(Application.dataPath, "")));
            AssetDatabase.Refresh();
            trySelectMail = name;
        }

        private void OnItemSelected(string item)
        {
            if (selectedMail != null)
                selectedMail.AskForSaveIfChanged();
            mailTypes = GetInheritedClasses(typeof(Mail));
            Type type = mailTypes.Find(x => x.Name == item);
            if (type != null)
                selectedMail = new MailView(type);
        }

        private void OnScriptCompilationStarted(object obj)
        {
            if (selectedMail != null && File.Exists(selectedMail.Path))
                EditorPrefs.SetString(SELECTED_MAIL_NAME, selectedMail.Name);
        }

        private List<Type> GetInheritedClasses(Type MyType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(TheType => TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(MyType) && TheType != typeof(MAILNAME))).ToList();
        }
    }
}
#endif