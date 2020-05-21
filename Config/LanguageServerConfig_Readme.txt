Logging -> Log
Relative Path to Language Server Logfile.

Logging -> Stream
Relative Path to Language Server Stream Output.

Logging -> Loglevel
The minimum loglevel you desire. Options are:
0: trace
1: debug
2: information
3: warning
4: error
5: fatal
6: none

syncKind
How the client is requested to send textdocument changes. Incremental will only send changes, while full will always send the full document. Full reuqires less CPU, but more bandwidth. Try switching this options, if your language server is slow. The options are:
incremental
full
