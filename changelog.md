##### Version 0.03.01 -- 06.10.2016
New options in menu.
* New option to "Capitalize All Markdown Titles" in a document.
* Option "Capitalize Selected Title" now corrects any erroneous capitalization automatically.

##### Version 0.03.00 -- 06.09.2016
Refactor code with new [Notepad++ .NET template](https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net/releases "Notepad++ .NET template") from Kasper Graversen.

#####Version 0.02 -- 06.06.2016
Improvements to how the code handles the text.

* Now leaves the original string intact and finds the first letter of each word and capitalizes the letter without splitting the string into individial words.
* Added menu option to show plugin info such as version number and etc.
* Fix bug where selected words were sometimes not being capitalized when they were supposed to be.
* Fix bug where plugin would crash if more than two spaces were selected in between words.
* Add menu option and capability to capitalize the first letter of the first word of all of the sentences in the document.

#####Version 0.01 -- 06.03.2016
Initial commit of code and small bug fixes.

* Splits text based on whitespace and capitalizes the resulting words if the word is not in the "forbidden" list.
