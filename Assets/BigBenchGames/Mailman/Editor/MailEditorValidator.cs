#if UNITY_EDITOR
using Microsoft.CSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.CodeDom;
using static BigBenchGames.Editor.MailmanDispatcher.MailEditor;
using System.Linq;

namespace BigBenchGames.Editor.MailmanDispatcher
{
    /// <summary>
    /// A class used to validate the content of a mail class in the mail editor
    /// </summary>
    public static class MailEditorValidator
    {
        private static readonly string[] RESERVED_KEYWORDS = {"abstract","as","base","bool","break","byte","case","catch","char","checked","class","const","continue","decimal","default","delegate","do","double","else","enum","event","explicit","extern","false","finally","fixed","float","for","foreach","goto","if","implicit","in","int","interface","internal","is","lock","long","namespace","new","null","object","operator","out","override","params","private","protected","public","readonly","ref","return","sbyte","sealed","short","sizeof","stackalloc","static","string","struct","switch","this","throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using","virtual","void","volatile","while"};
        
        private static readonly string[] INVALID_SYMBOLS = { "?", "-", "+", "!", "@", "#", "%", "^", "&", "*", "(", ")", "[", "]", "{", "}", ".", ";", ":", "\"", "\'", "/", "\\", ",", ">", "<", "|", "=", "$", "`", "~", " " };

        /// <summary>
        /// The validation report that is passed back to the editor window
        /// </summary>
        public class MailEditorValidationReport
        {
            /// <summary>
            /// A list of variable names that violate naming conversions
            /// </summary>
            public List<string> variableNameViolations;
            /// <summary>
            /// A string listing all the errors generated
            /// </summary>
            public string Errors;
            /// <summary>
            /// A bool determining if the mail class is vaild, cannot save if not
            /// </summary>
            public bool IsValid = true;
        }

        /// <summary>
        /// The function that checks to see if all variable parameters of a mail class in the mail editor are valid
        /// </summary>
        /// <param name="view">The current mail class being checked</param>
        /// <returns>A report with potential errors in it</returns>
        public static MailEditorValidationReport ValidateMail(MailView view)
        {
            MailEditorValidationReport report = new MailEditorValidationReport();
            report.variableNameViolations = new List<string>();

            if(view.Version != MailCodeGenerator.VERSION)
                report.Errors += string.Format("The most recent version is {0} but this mail version is {1}. Please regenerate this mail class or manually change it in the code.\n", MailCodeGenerator.VERSION, view.Version);

            foreach (var attribute in view.Attributes)
            {
                if (RESERVED_KEYWORDS.ToList().Contains(attribute.Name))
                {
                    report.variableNameViolations.Add(attribute.Name);
                    report.Errors += string.Format("The Varaible [{0}] name is a reserved word and is not allowed.\n", attribute.Name);
                    report.IsValid = false;
                }

                if(view.Attributes.Count((x) => x.Name == attribute.Name) > 1)
                {
                    if(!report.variableNameViolations.Contains(attribute.Name))
                        report.variableNameViolations.Add(attribute.Name);
                    report.Errors += string.Format("The Varaible [{0}] name is used multiple times and is not allowed.\n", attribute.Name);
                    report.IsValid = false;
                }

                if(string.IsNullOrEmpty(attribute.Name) || string.IsNullOrWhiteSpace(attribute.Name))
                {
                    if (!report.variableNameViolations.Contains(attribute.Name))
                        report.variableNameViolations.Add(attribute.Name);
                    report.Errors += string.Format("The Varaible [{0}] name uses an empty string or only whitespaces and is not allowed.\n", attribute.Name);
                    report.IsValid = false;
                }

                if (!string.IsNullOrEmpty(attribute.Name))
                {
                    if (char.IsDigit(attribute.Name[0]))
                    {
                        if (!report.variableNameViolations.Contains(attribute.Name))
                            report.variableNameViolations.Add(attribute.Name);
                        report.Errors += string.Format("The Varaible [{0}] name starts with a number and is not allowed.\n", attribute.Name);
                        report.IsValid = false;
                    }
                }

                if(attribute.Type == MailView.FieldType.COMPLEX && string.IsNullOrEmpty(attribute.ComplexSignature))
                {
                    report.Errors += string.Format("The Varaible [{0}] is complex but the signature is empty, which is not allowed.\n", attribute.Name);
                    report.IsValid = false;
                }

                foreach(var symbol in INVALID_SYMBOLS)
                {
                    if (!string.IsNullOrEmpty(attribute.Name))
                    {
                        if (attribute.Name.Contains(symbol))
                        {
                            if(string.IsNullOrWhiteSpace(symbol))
                                report.Errors += string.Format("The Varaible [{0}] contains invalid whitespace\n", attribute.Name);
                            else
                                report.Errors += string.Format("The Varaible [{0}] contains the invalid symbol {1}\n", attribute.Name, symbol);
                            report.IsValid = false;
                            if (!report.variableNameViolations.Contains(attribute.Name))
                                report.variableNameViolations.Add(attribute.Name);
                        }
                    }
                }
            }

            if(!string.IsNullOrEmpty(report.Errors))
                report.Errors.Remove(report.Errors.LastIndexOf("\n"), 1);
            return report;
        }

        /// <summary>
        /// Checks to see if the passed string is a valid identifier
        /// </summary>
        /// <param name="name">The string to check</param>
        /// <returns>True if valid, otherwise false</returns>
        public static bool IsStringValidVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (char.IsDigit(name[0]))
                return false;

            if (RESERVED_KEYWORDS.ToList().Contains(name))
                return false;
            foreach (string symbol in INVALID_SYMBOLS)
            {
                if (name.Contains(symbol))
                    return false;
            }

            return true;
        }
    }
}
#endif