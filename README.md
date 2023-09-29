# The-Outforce-dev_tool-GUI
Devtool for the game "The Outforce"
Developer  tool V2.6 gui
=
By: Krisztian Kispeti.

Changelog history:
{V1.0}
+Support for some *.box files
You can now extract the content of it.

{V2.0}
+Full support for the *.box archive
Now you can extract every *.box archive files.

{V2.1}
+Box builder updated.
+Added "Extract entire archive" function
+Added Search (find file) has been implemented
+Added a Progress bar (extracting)
+Added a warning message if the main project
file ".opf" is selected.

*Fixes

{2.2}
+Redesigned application
+Added loaded file offset field
+Added selected file offset field
+Added image inspector 
+Added image SizeMode
+Added Image rotation 
+Added Tools for image inspector

*Fixes

{2.3}
+Added a file filter!
+Added Number of filtered files!

*2 bugs has been fixed!

{2.4}
+Added Mission builder beta!
+Added Syntax checker beta!
+Added "//" extractor for mission builder!
+Added help (search engine [case sensitive])!
+Added *.init/information/visual.oms editor!

*Fixed 2 bugs.

{2.5}
*Fixed a bug that caused freeze when trying to
remove a selected item from the Archive maker
listbox.
+Cleaned the code: There was an unused func-
tion in the source code of the program. App 
runs smoother.
+New functions has been implemented:
(*.oms file open-save)

{2.6}
+Added: some new icons
+Added: program restart function
+Added: program documentation!
+Added: .cfg file read/edit function
+Added: .cfg save option in the file-->save menu.
+Archive builder can be accessed from File-->
create-> *.box archive menu.

*Extract "selected file", and "extract entire archive"
inside the Extact menu now.
*Archive builder fixed!

*NET 6.0.0 is requred to use this app.

USAGE:
=
*Extracting files from *.box archive:

Just put the *.box archives to the program's
root folder.
In the program, select the "open archive option
to load the *. file. The content of the *.box file
will be loaded to the listbox. You can decide how
would you extract files: extract selected file
(only one file), or extract the whole content of a 
*.box archive file.
The files will be extracted to the ExtractedFiles
folder.

*.box archive building:
You can make *.box archives with the *.box
builder tab, but still there's an unknown error
that will make your *.box file useless.
Will be fixed and implemented in the upcoming
updates...

Image inspector:
There's a built-in image viewer tool, that lets you
display images (.bmp, .jpg, .jpeg, .png) fileform
ats are supported. you can save every image
file in these formats. Also you can rotate them, 
set the size mode and save them.
Usage: 
after you loaded a *.box archive file in the list-
Box component, select an image file format
(mentioned above) and open the Image
Inspector tab...

KNOWING ISSUES
-
/Archive builder can only open (add) files from
the Developer tools folder
/Archive buider can only save in the same folder
that the Developer tools use.
/There's an unknown problem with the archive
builder, so the game won't read the *.box files
made by Developer tools (will be solved in the
next update)...
Filtered files (image files, oms/cfg files) can not
be displayed in OMS editor. Will be fixed in the
next update.

UPCOMING FEATURES
-
+MP3 player!
+Fix Archive builder
+Update Mission builder 
+Update Image inspector
+*.opf archive extractor		
+OMS editor content to ArchiveBuilder listBox
+Research, gather info about the classes section
(.CBaseClass .CGridMember, .CUnit, 
.CWeaponUnit file formats).
+Mesh viewer
+classes converter
+3D model viewer (?)
+Add single files
+fixes

Contact
=
Moddb: https://www.moddb.com/games/the-outforce
Discord: https://discord.gg/7RbzqN9
e-mail: krisztiankispeti1990@gmail.com

Special thanks to:
Krisztian Kispeti for developing the prog.
This file was last modified on: 2023.aug.20. 22:00. PM.

This is the full v2.6 program with the source code. The .zip file contains the icons, resources etc. folders. just unzip the content of the .zip file to the folder you downloaded rest of the files of my program...

I really need all of your help to improve this tool, because i have reached the limit of my programming skills.
=
My problems are:
1.) [Fix:] The .box builder does not work properly... However the filestruct for the *.box archive is correct, it can extract the *.box archives made for the game...
2.) [Add:]I tried to add a MP3 player to the program. If you select an item from the listBox (that files are loaded from an external *.box file with their directory ) you can play on a button that will start playing the selected .MP3 file. So you do NOT need to extract any files if you just want to play an MP3 file... There's no any API that could play a MP3 file with this method. If you know how to solve these problem, please inform me!
3.) I really want to know more about the .opf file (main project file that contains the assets of the game. It has a really complicated structure as well as this non-traditional "archive" contains another fileformats that structures are unknown. The 3D model files as well as the animation files are in .CBaseClass .CGridMember .CUnit .CWeaponUnit formats...
