#if UNITY_EDITOR
using BigBenchGames.Tools.MailmanDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBenchGames.Editor.MailmanDispatcher
{
    /// <summary>
    /// A class used to generate the custom mail types through code templates and code generation
    /// </summary>
    public static class MailCodeGenerator
    {
        /// <summary>
        /// The current version of the mail class, will be updated in future updates if the mail template class gets changed
        /// </summary>
        public const int VERSION = 1;

        /// <summary>
        /// Generates a mail class file from a mail view class
        /// </summary>
        /// <param name="view">The view class populated in the editor</param>
        /// <returns>The full path to the generated file</returns>
        public static string GenerateMailFromTemplate(MailEditor.MailView view)
        {
            string assetPath = Application.dataPath;
            MAILNAME templateInstance = Activator.CreateInstance<MAILNAME>();
            string templatePath = templateInstance.GetSourcePath();
            string mailDirectory = templatePath.Substring(0, templatePath.LastIndexOf('\\'));
            string template = File.ReadAllText(templatePath);

            string variables = "";
            foreach(var attribute in view.Attributes)
            {
                if(attribute.Type != MailEditor.MailView.FieldType.COMPLEX)
                    variables += string.Format("\t\tpublic {0} {1};\r\n", attribute.Type.ToString().ToLower(), attribute.Name);
                else
                    variables += string.Format("\t\tpublic {0} {1};\r\n", attribute.ComplexSignature, attribute.Name);
            }

            string clears = "";
            foreach(var attribute in view.Attributes)
            {
                string ending = "\r\n";
                if (view.Attributes.IndexOf(attribute) == view.Attributes.Count - 1)
                    ending = "";

                if(attribute.Type != MailEditor.MailView.FieldType.COMPLEX)
                    clears += string.Format("\t\t\t{0} = {1};{2}", attribute.Name, GetClearStringFromType(attribute.Type), ending);
            }

            int hashCode = view.Name.GetHashCode();
            string cleanedDesc = view.Description.Replace("\r\n", "\r\n    /// ");

            template = template.Replace("MAILNAME", view.Name);
            template = template.Replace("INSERTDESC", @cleanedDesc);
            template = template.Replace("0;//INSERTHASH", string.Format("{0};", hashCode));
            template = template.Replace("//INSERTTYPES", variables);
            template = template.Replace("//INSERTTYPECLEAR", clears);
            template = template.Replace("//ATTRIBUTES", string.Format("\t[ReadOnly({0}), Version({1}), CachedHash({2})]", view.IsReadOnly.ToString().ToLower(), VERSION, hashCode));

            string mailPath = "";
            if (!string.IsNullOrEmpty(view.Path))
                mailPath = view.Path;
            else
                mailPath = string.Format("{0}\\{1}.cs", mailDirectory, view.Name);
            File.WriteAllText(mailPath, template);
            view.HasBeenChanged = false;
            return mailPath;
        }

        /// <summary>
        /// Deleted a mail file and its meta file from the project
        /// </summary>
        /// <param name="path">The path to delete at</param>
        public static void DeleteMail(string path)
        {
            File.Delete(path);
            File.Delete(string.Format("{0}.meta", path));
        }

        private static string GetClearStringFromType(MailEditor.MailView.FieldType type)
        {
            switch (type)
            {
                case MailEditor.MailView.FieldType.SBYTE:
                case MailEditor.MailView.FieldType.BYTE:
                case MailEditor.MailView.FieldType.SHORT:
                case MailEditor.MailView.FieldType.USHORT:
                case MailEditor.MailView.FieldType.INT:
                case MailEditor.MailView.FieldType.UINT:
                case MailEditor.MailView.FieldType.LONG:
                case MailEditor.MailView.FieldType.ULONG:
                    return "0";
                case MailEditor.MailView.FieldType.CHAR:
                    return "\' \'";
                case MailEditor.MailView.FieldType.FLOAT:
                    return "0f";
                case MailEditor.MailView.FieldType.DOUBLE:
                    return "0f";
                case MailEditor.MailView.FieldType.BOOL:
                    return "false";
                case MailEditor.MailView.FieldType.DECIMAL:
                    return "0f";
                case MailEditor.MailView.FieldType.STRING:
                    return "\"\"";
                default:
                    return "null";
            }
        }
    }
}
#endif