# AppManager

## !Attention:
At the moment all apps are closed with force exit. This can mess up things in some apps.\
Make sure you have saved your work, and that the apps to close are not installing or updating anything.\
Also note that all apps starting with the chosen name will be closed, even though the process counter only count proceess with the exact name.

## Main purpose: 
Closes all chosen process.\
\
The AppManager is not a replacement for Windows TaskManager.\
AppManager will not show all apps or background apps running on the user pc.\
AppManager will not scan the computer for apps.\
\
AppManager will only check if the apps on the user's list are running.

## History:
This app started out as a batch file to close Skype as it could not be closed at the time. It kept reopening in the background.\
Another app needed to be closed often, so a list begang to form.\
Some apps can not close because another app keep reopenning it. \
These apps names would start the same. Example only: skype and skypetray.\
So instead of adding all hidden apps to the list, I made use of the Windows feature, of closing apps, that close all apps beginning with the given name.\
Since not all apps on the list needed closing in all situations, I began making the AppManager, so I could deselect some apps before closing.\
This was acceptable for me for years.\
Late 2023 I began to build the app to be used by others and to have some nicer features.\
Late 2024, so this project obviously got on a standby. I hope to get back to it soon.

## Description:
Add process names to the list of apps you would like to manage, or change the names already there.\
Clear the name field to remove a row from the list (happens when saving).\
The list saves when closing AppManager.\
The left selection box let's you select the apps you want included when clicking "Close apps" button.\
\
Your list will be saved in your documents folder for now.\
\
Every few seconds AppManager will count how many instances is running of each app.\
The number is shown next to the still inactive selectboxes in each row.\
\
Names are case-insensitive.\
\
AppManager have a default starter list, as an example.
