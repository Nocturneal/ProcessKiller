# ProcessKiller
Small utility to automagically kill a defined app.

Used to end processes that get stuck in a production environment and won't allow you to get to TaskManager.

This app uses the Process.Kill() method in .Net 4.x

The program is interactive when given no command-line aruments, however by creating a shortcut to the app, one can predefine the program they want to kill and use "Run as Administrator" on the shortcut.

Command-line parameters are as follows:

ProcessKiller.exe <ProcessName:string> <ShouldKillLoop:bool>

Where ProcessName is the name of the executable or process you want to kill (do not add .exe to the image name)

ShouldKillLoop is used for a shortcut, to repeat the kill command for 'loops' (build from source to add more than 1 extra loop)
