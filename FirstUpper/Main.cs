using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DawgSharp;

namespace Kbg.NppPluginNET
{
    static class Main
    {
        static string version = "FirstUpper Notepad++ Plugin\nVersion 0.04.00\nCopyright Zach Kirkland\nzkirkland514@gmail.com\nAll Rights Reserved";
        internal const string PluginName = "FirstUpper";
        static string iniFilePath = null;
        static bool someSetting = false;
        static List<string> forbidden = new List<string>();
        static List<char> ignoreChars = new List<char> { ' ', '\t', '\n', '.', '!', '?', '#', '\r' };

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

            PluginBase.SetCommand(0, "Capitalize Selected Title", CapitalizeTitle, new ShortcutKey(true, true, false, Keys.U));
            PluginBase.SetCommand(1, "Capitalize All Markdown Titles", CapitalizeMDTitles);
            PluginBase.SetCommand(2, "Capitalize First Word of All Sentences", CapSentence);
            PluginBase.SetCommand(3, "CamelCase Last Word Typed", CamelCaseLastWord, new ShortcutKey(false, true, true, Keys.U));
            PluginBase.SetCommand(4, "Plugin Info", FirstUpperInfo);
        }

        internal static void PluginCleanUp()
        {
            Win32.WritePrivateProfileString("SomeSection", "SomeKey", someSetting ? "1" : "0", iniFilePath);
        }

        #region " Menu functions "
        /// <summary>
        /// Capitalize user selected text based on Chicago Manual of Style.
        /// </summary>
        internal static void CapitalizeTitle()
        {
            // Only call this once per notepad-plus-plus session.
            if (forbidden.Count == 0)
            {
                // Get the list of forbidden words.
                forbidden = GetForbiddenWords(@"plugins\doc\FirstUpper\FirstUpperForbiddenWords.txt");
            }

            IntPtr currentScint = PluginBase.GetCurrentScintilla();
            ScintillaGateway scintillaGateway = new ScintillaGateway(currentScint);

            try
            {
                // Get selected text.
                string selectedText = scintillaGateway.GetSelText();
                // Convert to lower case in case some words are already
                // capitalized that should not be capitalized.
                selectedText = selectedText.ToLowerInvariant();
                // Change to char array for easy editing of single chars.
                char[] charArraySelText = selectedText.ToCharArray();

                StringBuilder word = new StringBuilder();
                char firstLetter = new char();
                int firstLetterIndex = -1;
                bool firstWord = true;

                // For the length of the selected text...
                for (int i = 0; i < selectedText.Length; i++)
                {
                    // If the current character is not a whitespace character...
                    if (!ignoreChars.Contains(selectedText[i]))
                    {
                        // If first letter index is negative that means this
                        // is the first letter of the word. Set the index accordingly.
                        if (i >= 0 && firstLetterIndex < 0)
                        {
                            // Set the first letter index to the index of the first letter of this word.
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
                        if (ignoreChars.Contains(selectedText[i + 1]))
                        {
                            // Make sure it is not a forbidden word.
                            if ((firstWord || !forbidden.Contains(word.ToString())) && word.Length != 0)
                            {
                                // Convert the first letter of the word to uppercase.
                                firstLetter = Convert.ToChar(word[0].ToString().ToUpperInvariant());
                                // Replace the correct char in the selected text with its uppercase letter.
                                charArraySelText.SetValue(firstLetter, firstLetterIndex);
                                // Clear the word to make ready for the next one.
                                Clear(word);
                                // Reset first letter and first word indicators.
                                firstLetterIndex = -1;
                                firstWord = false;
                            }
                            // Otherwise we do not want the word to be capitalized...
                            else
                            {
                                // Clear the word to make ready for the next one.
                                Clear(word);
                                // Reset first letter indicator.
                                firstLetterIndex = -1;
                            }
                        }
                    }
                    // Otherwise this is the last word of the selected text.
                    else if (i == selectedText.Length - 1)
                    {
                        // Convert the first letter of the word to uppercase.
                        firstLetter = Convert.ToChar(word[0].ToString().ToUpperInvariant());
                        // Replace the correct char in the selected text with its uppercase letter.
                        charArraySelText.SetValue(firstLetter, firstLetterIndex);
                        // Reset first letter indicator.
                        firstLetterIndex = -1;
                        break;
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

        /// <summary>
        /// Get the list of "forbidden" words from the text file.
        /// </summary>
        internal static List<string> GetForbiddenWords(string fileString = null)
        {
            using (StreamReader file = new StreamReader(fileString))
            {
                string line;
                List<string> wordList = new List<string>();

                while ((line = file.ReadLine()) != null)
                {
                    line = line.ToLowerInvariant();
                    line = line.Trim(' ');
                    // only load words without "*" in front of them.
                    if (line[0] != '*')
                    {
                        wordList.Add(line);
                    }
                } // end while readline
                return wordList;
            }
        }

        /// <summary>
        /// Display a message box with plugin information in it.
        /// </summary>
        internal static void FirstUpperInfo()
        {
            MessageBox.Show(version);
        }

        /// <summary>
        /// Find and capitalize the first letter of the beginning of all
        /// sentences in a document.
        /// </summary>
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
                string allText = scintillaGateway.GetText(length + 1);

                // Convert the text to char array for easy manipulation.
                char[] charArrayAllText = allText.ToCharArray();

                char firstLetter = new char();
                bool capAfterPunct = true;

                // For the length of the selected text...
                for (int i = 0; i < allText.Length; i++)
                {
                    // If there is punctuation that ends a sentence
                    // the next word should be capitalized.
                    if (allText[i] == '.' || allText[i] == '?' || allText[i] == '!' || allText[i] == '\r')
                    {
                        capAfterPunct = true;
                    }
                    // Don't capitalize Markdown titles in this method.
                    else if (allText[i] == '#')
                    {
                        capAfterPunct = false;
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

        /// <summary>
        /// Find and capitalize all Markdown Titles in the document.
        /// </summary>
        internal static void CapitalizeMDTitles()
        {
            // Get scintilla gateway.
            IntPtr currentScint = PluginBase.GetCurrentScintilla();
            ScintillaGateway scintillaGateway = new ScintillaGateway(currentScint);

            string line;

            // Get the number of lines in the document.
            int numLines = scintillaGateway.GetLineCount();

            // Traverse through each line.
            for (int i = 0; i < numLines; i++)
            {
                // Set line to the current line.
                line = scintillaGateway.GetLine(i);

                // Check each character in the line to see if it begins
                // with a '#'.
                for (int j = 0; j < line.Length; j++)
                {
                    if (line[j] == '#')
                    {
                        // If it begins with '#', select the line and call
                        // the CapitalizeTitle method with these parameters.
                        scintillaGateway.GotoLine(i);
                        scintillaGateway.SetSel(scintillaGateway.PositionFromLine(i), scintillaGateway.GetLineEndPosition(i));
                        CapitalizeTitle();
                        break;
                    }
                    // Support for Setext headers.
                    else if ((line[j] == '-' || line[j] == '=') && !IsAlphaNumeric(line))
                    {
                        // If it begins with '-' or '=', select the previous line and call
                        // the CapitalizeTitle method with these parameters.
                        scintillaGateway.GotoLine(i - 1);
                        scintillaGateway.SetSel(scintillaGateway.PositionFromLine(i - 1), scintillaGateway.GetLineEndPosition(i - 1));
                        CapitalizeTitle();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Check to see if the string has letters or numbers in it.
        /// </summary>
        /// <param name="strToCheck"></param>
        /// <returns></returns>
        internal static bool IsAlphaNumeric(string strToCheck)
        {
            // If the line that contains '=' or '-' has letters or numbers
            // on it, then the previous line is not a title and therefore
            // should not be capitalized.
            Regex rg = new Regex(@"[a-zA-Z0-9]+");
            return rg.IsMatch(strToCheck);
        }

        /// <summary>
        /// Turn the last word typed into camel case.
        /// Uses "Directed Acyclic Word Graph" or "DAWG" for short.
        /// </summary>
        internal static void CamelCaseLastWord()
        {
            //MessageBox.Show("Camel Case Last Word");
            Dawg<bool> dawg;

            try
            {
                Stream fs = File.Open("./plugins/doc/FirstUpper/FirstUpperDAWG.bin", FileMode.Open, FileAccess.Read);

                dawg = Dawg<bool>.Load(fs);

                IntPtr currentScint = PluginBase.GetCurrentScintilla();
                ScintillaGateway scintillaGateway = new ScintillaGateway(currentScint);

                string word = getLastWord().ToLower();
                var wordCharArray = word.ToCharArray();

                var index = 0;
                var wordCount = 0;
                string wordSubstr;

                #region "Repeat steps until no more words."
                // See if the text is a word.
                // If it is not a word, we remove the last letter
                // until we find a word. Then we capitalize this word.
                // We repeat this process for the entire variable.
                while (true)
                {
                    //MessageBox.Show("Index: " + index + "\nWord End: " + (word.Length - 1));
                    if (index >= word.Length - 1)
                    {
                        break;
                    }

                    // Get the substring word based on the start position.
                    wordSubstr = word.Substring(index);

                    while (true)
                    {
                        // Is the substring word an actual word?
                        if (dawg[wordSubstr])
                        {
                            // Increment the word count.
                            ++wordCount;
                            // Do not capitalize the first word of the variable.
                            if (wordCount > 1)
                            {
                                // Capitalize word.
                                wordCharArray[index] = Convert.ToChar(wordCharArray[index].ToString().ToUpper()); 
                            }

                            //MessageBox.Show("Word Length: “" + word.Length + "”\nSubstr Length: “" + wordSubstr.Length + "”");
                            index += wordSubstr.Length;
                            //MessageBox.Show("Substr After Caps: " + word.Substring(index));
                            break;
                        }
                        else
                        {
                            //MessageBox.Show("Substr before removal: “" + wordSubstr + "”\nLength: " + wordSubstr.Length);
                            if (wordSubstr.Length > 0)
                            {
                                wordSubstr = wordSubstr.Remove(wordSubstr.Length - 1);
                                //MessageBox.Show("Substr after removal: “" + wordSubstr + "”\nLength: " + wordSubstr.Length);
                            }
                            else
                            {
                                index = word.Length;
                                break;
                            }
                        }
                    }
                }
                #endregion "Repeat steps until no more words."

                // Replace selected word with new word.
                word = new string(wordCharArray);
                scintillaGateway.ReplaceSel(word);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Returns a string containing the last word typed.
        /// </summary>
        /// <returns>string</returns>
        internal static string getLastWord()
        {
            IntPtr currentScint = PluginBase.GetCurrentScintilla();
            ScintillaGateway scintillaGateway = new ScintillaGateway(currentScint);

            var selectionStart = scintillaGateway.GetCurrentPos();
            var selectionEnd = scintillaGateway.GetCurrentPos();
            var endOfVariable = scintillaGateway.GetCurrentPos();

            scintillaGateway.WordLeft();
            selectionStart = scintillaGateway.GetCurrentPos();
            scintillaGateway.WordRight();
            selectionEnd = scintillaGateway.GetCurrentPos();
            endOfVariable = selectionEnd;

            scintillaGateway.SetSel(selectionStart, selectionEnd);

            return scintillaGateway.GetSelText();
        }

        /// <summary>
        /// Since we are using .net 2.0, need a "clear" capability for
        /// string builder objects.
        /// </summary>
        /// <param name="value"></param>
        internal static void Clear(StringBuilder value)
        {
            value.Length = 0;
            value.Capacity = 1;
            value.Capacity = 16;
        }
    }
    #endregion
}