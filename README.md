# Dafny Server Redesign
This porject is a redesign of the [original Dafny language server and VSCode plugin that brings support for Dafny for Visual Studio Code](https://github.com/DafnyVSCode/Dafny-VSCode). 

## Overview
The project has two main parts. 
On the one hand, there is the Visual Studio Code Plugin. You can find [it in this seperate git repository](https://gitlab.dev.ifs.hsr.ch/dafny-ba/dafny-vscode-plugin). 
It just contains a very basic level of logic and is mainly responsible for displaying information to the user. 
On the other hand, is the Visual Studio Solution “Dafny Server”. The server is placed in this git repository. 
It delivers the needed information to the Visual Studio Code Plugin over the Language Server Protocol (LSP). 
The Dafny Server itself does not directly evaluate Dafny code. It uses the Dafny-Lang project.
It uses this project directly and not over an JSON pared request like the bachelor theses before thought.
Therefore it can offer more features in the future since there are now no limitation trough a parsing process. 

### VS Code Plugin
Open the folder `VSCodePlugin` in your VS Code. Find and open the file `src/extension.ts` and press `F5`. 
Now VS Code loads the plugin in a virtual test environment. 
It can be used now like it would be installed and you can set breakpoints and debug the code as well. Make sure that you setup your Dafny Language Server before running your Plugin. 

### Dafny Language Server
First you have to open the Dafny Solution in your Visual Studio and build all subsolution. 
Then you can build the main Dafny solution with depends on the subsolutions. Now you are good to go.

## PDF Developer Documentation
You will finde more information how to setup this project on your local development environment in the developer documentation document. 
There are some helpfull links to getting into Visuel Studio Code Plugins as well. 

You can [download the PDF here](https://wuza.ch/specials/SA/Entwicklerdokumentation.pdf).
For future adaptations of the documentation, the Word file will be made available via Git after our Bachelor thesis. 
For the current processing an alternative Word document synchronization is more suitable (SharePoint).

## VS Code Plugin Features
If you would like to know which features are supported by the Visual Studio Code Plugin, please checkout the [VSCodePlugin-ReadMe in the seperate git repository](https://gitlab.dev.ifs.hsr.ch/dafny-ba/dafny-vscode-plugin/-/blob/master/README.md). 

## Server Launch Arguments

* **/log:[relativePath]** Relative path to the logfile. The default is `Logs/LanguageServerLog.txt`
* **/stream:[relativePath]**  Relative path to the stream output. The default is `Logs/LanguageServerStreamRedirection.txt`
* **/loglevel:[int]**   The minimum loglevel you desire. Options are:
  * 0: trace
  * 1: debug
  * 2: information
  * 3: warning
  * 4: error (default)
  * 5: fatal
  * 6: none
* **/synckind**:[incremental|full] How the client is requested to send textdocument changes. Incremental will only send changes, while full will always send the full document. Full reuqires less CPU, but more bandwidth. Try switching this options, if your language server is slow. The options are:
  * incremental (default)
  * full


## Server Config
You can edit the file Config/LanguageServerConfig.json with the same Information as above. The launch arguments have priority over the json config file.