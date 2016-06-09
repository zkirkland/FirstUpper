# FirstUpper
Notepad++ plugin to capitalize the first letter of a word or words in selected text.

### Summary
This is a basic plugin for Notepad++ that will convert the first letter of highlighted words to uppercase.

### Features
##### Capitalize the First Letter of Every Word in a Selection
Select some text and press ```Ctrl+Alt+U``` to capitalize the first letter of the words in the selection. This is meant for proper capitalization of titles based on the Chicaco Manual of Style and specific words will not be capitalized. The plugin uses the text file "FirstUpperForbiddenWords.txt" as the list of words that should not be capitalized.

##### Capitalize the First Letter of the First Word of All Sentences in the Document.
This does not have a keyboard shortcut and must be accessed from the Plugins menu. This feature will look for typical punctuation that you use to end a sentence such as periods or question marks and it will capitalize the first letter of the first word directly after that punctuation.

### How to Use
##### Installation
To install this plugin download the files from the folder labeled "PluginInstallFiles" and simply place the files "FirstUpper.dll", and "FirstUpperForbiddenWords.txt" in your Notepad++ plugins directory. To invoke the plugin you must first select some text then hit ```Ctrl+Alt+U``` to convert the words. You can also use the "Plugins" menu in the Notepad++ menu bar.

##### Forbidden Word List
If there are words you wish to add to the list of "forbidden" words so as to cause the plugin to not capitalize them, simply add the word(s) to the "FirstUpperForbiddenWords.txt" file. **Make sure each word is on its own line.** If there are specific words in the "forbidden" list you wish to capitalize, you must open the text file and you may either delete the line that contains the word or place an asterisk (```*```) in front of the word.

### Planned Features
Although this is a very simple and largely useless plugin, I enjoy playing with it and therefore I will continue adding features as I have time.

##### Find and Capitalize All Titles in a Markdown File
Wouldn't it be nice if you could just type without having to make sure all of your titles are capitalized properly? This would be a cool feature to implement. I want to make a menu option such as "Capitalize All Titles In Document" that will go through the entire document line by line, find all of the titles, and correctly capitalize them.

##### Always Capitalize First and Last Words
According to many proper writing styles, it is correct to always capitalize the first and last words in a title regardless of the word.

##### Settings Menu
I wish to implement a simple settings window of some kind. Perhaps instead of using a text file for the forbidden words, I will use the settings window to allow the user to edit the forbidden words within Notepad++. This settings window will also allow you the option to capitalize the first letter of *all* words. I would also like to implement support for capitalizing initials so that "j.f.k" becomes "J.F.K". Many of these features can be in the plugins menu such that the user may simply select the desired command in the menu.

zkirkland
