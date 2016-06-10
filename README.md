# FirstUpper
Notepad++ plugin to capitalize the first letter of a word or words in selected text.

### Summary
This is a basic plugin for Notepad++ that will convert the first letter of highlighted words to uppercase.

### Features
##### Capitalize the First Letter of Every Word in a Selection
Select some text and press ```Ctrl+Alt+U``` to capitalize the first letter of the words in the selection. This is meant for proper capitalization of titles based on the Chicaco Manual of Style and specific words will not be capitalized. The plugin uses the text file "FirstUpperForbiddenWords.txt" as the list of words that should not be capitalized.

##### Capitalize the First Letter of the First Word of All Sentences in the Document
This does not have a keyboard shortcut and must be accessed from the Plugins menu. This feature will look for typical punctuation that you use to end a sentence such as periods or question marks and it will capitalize the first letter of the first word directly after that punctuation.

##### Find and Capitalize All Titles in a Markdown File
The option "Capitalize All Markdown Titles" will find and properly capitalize all of the markdown titles in the document. It supports Setext style ("=" or "-") headings and Atx style (one or more "#") headings.

### How to Use
##### Installation
To install this plugin download the files from the folder labeled "PluginInstallFiles" and simply place the files "FirstUpper.dll", and "FirstUpperForbiddenWords.txt" in your Notepad++ plugins directory.

##### Usage
Either select an option from the plugins menu for most of the options or for the "Capitalize Selected Title" you can use ```Ctrl+Alt+U```.

##### Forbidden Word List
If there are words you wish to add to the list of "forbidden" words so as to cause the plugin to not capitalize them, simply add the word(s) to the "FirstUpperForbiddenWords.txt" file. **Make sure each word is on its own line.** If there are specific words in the "forbidden" list you wish to capitalize, you must open the text file and you may either delete the line that contains the word or place an asterisk (```*```) in front of the word.

### Planned Features
Although this is a very simple and largely useless plugin, I enjoy playing with it and therefore I will continue adding features as I have time.

##### Settings Menu
I wish to implement a simple settings window of some kind. Perhaps instead of using a text file for the forbidden words, I will use the settings window to allow the user to edit the forbidden words within Notepad++. This settings window will also allow you the option to capitalize the first letter of *all* words. I would also like to implement support for capitalizing initials so that "j.f.k" becomes "J.F.K". Many of these features can be in the plugins menu such that the user may simply select the desired command in the menu.

zkirkland
