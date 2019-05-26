# Project-X

Project X is an MMORPG server cluster with supporting client software which allows delivery of 
reporting output via a database.

It’s comprised of the Game Server, Login Server and Synchronization Server, as well as a 
Client application. The reporting output uses log data captured by client connections to the 
cluster and informs the analyst on the best course of action using KPI’s to monitor performance 
on important strategic metrics. 

The client has been developed outside of GitHub in Unity and the exe can be found here: 

Mac:
https://1drv.ms/u/s!AhRTeukcFugmgcR8fWGq68VMAtc_5w
(Please note that the client was developed primarily for Windows and therefore there are some errors that I was unable to fix during production due to not owning a Mac)

Windows:
https://1drv.ms/u/s!AhRTeukcFugmgcR7JWVwVl7PImVPSQ

The reporting output can be found here:
https://1drv.ms/x/s!AhRTeukcFugmgYt9lpe83doeQQo9zg

Please note that the following ports need to be open in order for the client to connect correctly:
TCP Ports 5600, 5601, 5602, 5603
UDP Port 5604

The server cluster itself is hosted on an Amazon EC-2 Windows remote server and the database is hosted on Amazon AWS MySQL.

If you have any questions regarding this then please message or email me and I'll endeavour to get back to you ASAP.

James
