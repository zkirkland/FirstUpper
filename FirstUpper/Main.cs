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
            // This method is invoked whenever something is happening in notepad-plus-plus
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

            PluginBase.SetCommand(0, "Capitalize First Letter", CapitalizeFirstLetter, new ShortcutKey(true, true, false, Keys.U));
            PluginBase.SetCommand(1, "Capitalize First Word of Sentences", CapSentence);
            PluginBase.SetCommand(2, "Plugin Info", FirstUpperInfo);
        }

        internal static void PluginCleanUp()
        {
            Win32.WritePrivateProfileString("SomeSection", "SomeKey", someSetting ? "1" : "0", iniFilePath);
        }

        #region " Menu functions "
        internal static void CapitalizeFirstLetter()
        {
            // Only call this once per notepad-plus-plus session.
            if (forbidden.Count == 0)
            {
                // Get the list of forbidden words.
                GetForbiddenWords();
            }

            // Get current scintilla instance.
            IntPtr currentScint = PluginBase.GetCurrentScintilla();
            ScintillaGateway scintillaGateway = new ScintillaGateway(currentScint);

            try
            {
                // Get scintilla gateway.
                string selectedText = scintillaGateway.GetSelText();
                MessageBox.Show("selText created: " + selectedText);

                // Change to char array for easy editing of single chars.
                char[] charArraySelText = selectedText.ToCharArray();

                StringBuilder word = new StringBuilder();
                char firstLetter = new char();
                int firstLetterIndex = 0;

                // For the length of the selected text...
                for (int i = 0; i < selectedText.Length; i++)
                {
                    // If the current character is not a whitespace character...
                    if (ignoreChars.Contains(selectedText[i]) == false)
                    {
                        // If the previous char is a whitespace char then
                        // the current char is the first letter of the word.
                        if (i > 0 && ignoreChars.Contains(selectedText[i - 1]))
                        {
                            // Set the index to the index of the first letter of this word.
                            firstLetterIndex = i;
                        }

                        // Build the string one char at a time.
                        word.Append(selectedText[i]);
                    }

                    // Out of bounds check.
                    if ((i + 1) < selectedText.Length - 1)
                    {
                        // If the next char is whitespace char that means the current
                        // char is the last char of the current word.
                        if (ignoreChars.Contains(selectedText[i + 1]) == true)
                        {
                            // Make sure it is not a forbidden word.
                            if (forbidden.Contains(word.ToString()) == false && word.Length != 0)
                            {
                                // Convert the first letter of the word to uppercase.
                                firstLetter = Convert.ToChar(word[0].ToString().ToUpperInvariant());
                                // Replace the correct char in the selected text with its uppercase letter.
                                charArraySelText.SetValue(firstLetter, firstLetterIndex);
                                // Clear the word to make ready for the next one.
                                word.Clear();
                            }
                            // Otherwise we do not want the word to be capitalized...
                            else
                            {
                                // Clear the word to make ready for the next one.
                                word.Clear();
                            }
                        }
                    }
                    // Otherwise this is the last word of the selected text.
                    else if (i == selectedText.Length - 1)
                    {
                        // Make sure it is not a forbidden word.
                        if (!forbidden.Contains(word.ToString()) && word.Length != 0)
                        {
                            // Convert the first letter of the word to uppercase.
                            firstLetter = Convert.ToChar(word[0].ToString().ToUpperInvariant());
                            // Replace the correct char in the selected text with its uppercase letter.
                            charArraySelText.SetValue(firstLetter, firstLetterIndex);
                            break;
                        }
                    }
                }
                // Convert the char array back to string.
                selectedText = new string(charArraySelText);
                // Replace the selected text with the new string
                scintillaGateway.ReplaceSel(selectedText);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        // load the list of forbidden words
        internal static void GetForbiddenWords()
        {
            using (StreamReader file = new StreamReader("plugins\\FirstUpperForbiddenWords.txt"))
            {
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    line = line.ToLowerInvariant();
                    line = line.Trim(' ');
                    // only load words without "*" in front of them.
                    if (line[0] != '*')
                    {
                        forbidden.Add(line);
                    }
                } // end while readline
            }
        }

        internal static void FirstUpperInfo()
        {
            MessageBox.Show(version);
        }

        internal static void CapSentence()
        {
            // Get current scintilla instance.
            IntPtr currentScint = PluginBase.GetCurrentScintilla();
            ScintillaGateway scintillaGateway = new ScintillaGateway(currentScint);

            try
            {
                // Get the length of the document.
                int length = scintillaGateway.GetLength();
                // Get the text in the document.
                string allText = scintillaGateway.GetText(length);

                // Convert the text to char array for easy manipulation.
                char[] charArrayAllText = allText.ToCharArray();

                char firstLetter = new char();
                bool capAfterPunct = true;

                // For the length of the selected text...
                for (int i = 0; i < allText.Length; i++)
                {
                    // If there is punctuation that ends a sentence
                    // the next word should be capitalized.
                    if (allText[i] == '.' || allText[i] == '?' || allText[i] == '!')
                    {
                        capAfterPunct = true;
                    }

                    if (capAfterPunct && !ignoreChars.Contains(allText[i]))
                    {
                        // If the current character is not a whitespace character
                        // convert the first letter of the word to uppercase.
                        firstLetter = Convert.ToChar(allText[i].ToString().ToUpperInvariant());
                        // Replace the correct char in the selected text with its uppercase letter.
                        charArrayAllText.SetValue(firstLetter, i);
                        // Revert to false until beginning of next sentence.
                        capAfterPunct = false;
                    }
                }
                // Convert char array back to string.
                allText = new string(charArrayAllText);
                // Replace the document text with the new text.
                scintillaGateway.SelectAll();
                scintillaGateway.ReplaceSel(allText);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion
    }
}