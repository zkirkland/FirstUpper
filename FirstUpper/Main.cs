using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;
using System.Collections.Generic;

namespace Kbg.NppPluginNET
{
    class Main
    {
        static string version = "FirstUpper Notepad++ Plugin\nVersion 0.03.00\nCopyright Zach Kirkland\nzkirkland514@gmail.com\nAll Rights Reserved";
        internal const string PluginName = "FirstUpper";
        static string iniFilePath = null;
        static bool someSetting = false;
        static List<string> forbidden = new List<string>();
        static List<char> ignoreChars = new List<char> { ' ', '\t', '\n', '.', '!', '?' };

        public static void OnNotification(ScNotification notification)
        {
            // This method is invoked whenever something is happening in notepad++
            // use eg. as
            // if (notification.Header.Code == (uint)NppMsg.NPPN_xxx)
            // { ... }
            // or
            //
            // if (notification.Header.Code == (uint)SciMsg.SCNxxx)
            // { ... }
        }

        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");
            someSetting = (Win32.GetPrivateProfileInt("SomeSection", "SomeKey", 0, iniFilePath) != 0);

            PluginBase.SetCommand(0, "Capitalize First Letter", capitalizeFirstLetter, new ShortcutKey(true, true, false, Keys.U));
            PluginBase.SetCommand(1, "Capitalize First Word of Sentences", capSentence);
            PluginBase.SetCommand(2, "Plugin Info", firstUpperInfo);
        }

        internal static void PluginCleanUp()
        {
            Win32.WritePrivateProfileString("SomeSection", "SomeKey", someSetting ? "1" : "0", iniFilePath);
        }

        #region " Menu functions "
        internal static void capitalizeFirstLetter()
        {
            // only call this once per notepad++ session
            if (forbidden.Count == 0)
            {
                // get the list of forbidden words
                getForbiddenWords();
            }

            // get current scintilla instance
            var currentScint = PluginBase.GetCurrentScintilla();

            try
            {
                // get size of the selected text
                int bufSz = (int)Win32.SendMessage(currentScint, SciMsg.SCI_GETSELTEXT, 0, 0);

                // init stringbuilder object to size
                StringBuilder selectedText = new StringBuilder(bufSz);

                // get selected text from editor
                Win32.SendMessage(currentScint, SciMsg.SCI_GETSELTEXT, 0, selectedText);

                StringBuilder word = new StringBuilder();
                char firstLetter = new char();
                int firstLetterIndex = 0;

                // for the length of the selected text...
                for (int i = 0; i < selectedText.Length; i++)
                {
                    // if the current character is not a whitespace character...
                    if (ignoreChars.Contains(selectedText[i]) == false)
                    {
                        if (i > 0)
                        {
                            // if the previous char is a whitespace char then
                            // the current char is the first letter of the word
                            if (ignoreChars.Contains(selectedText[i - 1]) == true)
                            {
                                // set the index to the index of the first letter of this word
                                firstLetterIndex = i;
                            }
                        }

                        // build the string one char at a time.
                        word.Append(selectedText[i]);
                    }

                    // out of bounds check
                    if ((i + 1) < selectedText.Length - 1)
                    {
                        // if the next char is whitespace char that means the current
                        // char is the last char of the current word.
                        if (ignoreChars.Contains(selectedText[i + 1]) == true)
                        {
                            // make sure it is not a forbidden word
                            if (forbidden.Contains(word.ToString()) == false && word.Length != 0)
                            {
                                // convert the first letter of the word to uppercase
                                firstLetter = Convert.ToChar(word[0].ToString().ToUpper());
                                // replace the correct char in the selected text with its uppercase letter
                                selectedText.Replace(selectedText[firstLetterIndex], firstLetter, firstLetterIndex, 1);
                                // clear the word to make ready for the next one
                                word.Clear();
                            }
                            // otherwise we do not want the word to be capitalized...
                            else
                            {
                                // clear the word to make ready for the next one
                                word.Clear();
                            }
                        }
                    }
                    // otherwise this is the last word of the selected text
                    else if (i == selectedText.Length - 1)
                    {
                        // make sure it is not a forbidden word
                        if (forbidden.Contains(word.ToString()) == false && word.Length != 0)
                        {
                            // convert the first letter of the word to uppercase
                            firstLetter = Convert.ToChar(word[0].ToString().ToUpper());
                            // replace the correct char in the selected text with its uppercase letter
                            selectedText.Replace(selectedText[firstLetterIndex], firstLetter, firstLetterIndex, 1);
                            break;
                        }
                    }
                }
                // replace the selected text with the new string
                Win32.SendMessage(currentScint, SciMsg.SCI_REPLACESEL, 0, selectedText);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        // load the list of forbidden words
        internal static void getForbiddenWords()
        {
            using (StreamReader file = new StreamReader("plugins\\FirstUpperForbiddenWords.txt"))
            {
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    line = line.ToLower();
                    line.Trim(' ');
                    // only load words without "*" in front of them.
                    if (line[0] != '*')
                    {
                        forbidden.Add(line);
                    }
                } // end while readline
            }
        }

        internal static void firstUpperInfo()
        {
            MessageBox.Show(version);
        }

        internal static void capSentence()
        {
            // get current scintilla instance
            var currentScint = PluginBase.GetCurrentScintilla();

            try
            {
                // get size of the document
                int bufSz = (int)Win32.SendMessage(currentScint, SciMsg.SCI_GETLENGTH, 0, 0);
                // init stringbuilder object to size
                StringBuilder allText = new StringBuilder(bufSz + 1);

                // get all text from editor
                Win32.SendMessage(currentScint, SciMsg.SCI_GETTEXT, bufSz + 1, allText);

                char firstLetter = new char();
                bool capAfterPunct = true;

                // for the length of the selected text...
                for (int i = 0; i < allText.Length; i++)
                {
                    // if there is punctuation that ends a sentence
                    // the next word should be capitalized
                    if (allText[i] == '.' || allText[i] == '?' || allText[i] == '!')
                    {
                        capAfterPunct = true;
                    }

                    if (capAfterPunct == true)
                    {
                        // if the current character is not a whitespace character...
                        if (ignoreChars.Contains(allText[i]) == false)
                        {
                            // convert the first letter of the word to uppercase
                            firstLetter = Convert.ToChar(allText[i].ToString().ToUpper());
                            // replace the correct char in the selected text with its uppercase letter
                            allText.Replace(allText[i], firstLetter, i, 1);
                            // revert to false until beginning of next sentence
                            capAfterPunct = false;
                        }
                    }
                }
                // replace the document text with the new text
                Win32.SendMessage(currentScint, SciMsg.SCI_SETTEXT, 0, allText);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion
    }
}