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

Background:

Login Server:
This controls all access to the game world and registration, it has direct access to read and write to the database and effectively manages traffic attempting to connect to the game server, on successful login the IP address of the connecting client is sent to the game server to be whitelisted.

Game Server:
The Game server controls all in-world spawns, health, combat, movement, etc as well as cascading all changes to all connected and in-game clients via UDP data packets. This server has a direct connection to the Synchronization Server and constantly sends updates to the player coordinates, experience and health.

Synchronization Server:
This essentially takes rapid changes to the databases dataset from the game server in real-time and continuously checks for changes and bulk-updates the database with these changes to avoid database record locking and database network management.

Database:
Hosted in MySQL this holds all non-real time data including character information, NPC's, collectables, health, experience and a host of other information vital to the game world.

The project was begun in January 2019 and finished in May, taking 4 months and 400+ hours of development working alone.

If you have any questions regarding this then please message or email me and I'll endeavour to get back to you ASAP.

James
