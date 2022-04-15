![Preview](Preview.png?raw=true "Preview")

# NetTap
TCP packet logger and forwarder.

This utility is used as a troubleshooting tool to review and analyze packet data being passed between TCP systems. It listens and captures the data from source TCP system, then stores a copy of the data to a log file, then relays the data to the destination TCP system.

What does the "CCC" plug-in do?

ESRI GeoEvent service was built for modern data communication. It has no native support for legacy serial data communication over TCP/IP that utilizes the STX & ETX ASCII control codes. With the "CCC" plug-in, the utility cleanses the inconsistent incoming ANIALI serial data. Basically, it takes the raw ANIALI data and makes it clean before the GeoEvent server can digest the ANIALI data for plotting on a map.

The utility performs the following: 

1) Recieves packet data from source system.

2) Strips LINE-FEED (LF) ASCII control code in between the START-OF-TEXT (STX) and the END-OF-TEXT (ETX) ASCII control codes. LN in the data causes issues for GeoEvent service parsing process.

3) Strips the BLOCK-CHECK-CHARACTER (BCC) that comes after the ETX. The computed BCC value could be another ASCII control code that causes issues for GeoEvent service parsing process.

4) Converts COMMA character to SPACE character. The COMMA character is a field separator used by GeoEvent service parsing.  The process cannot tell if the COMMA character is being used as a delimiter or input data.

5) Adds CARRAGIE-RETURN + LINE-FEED (CRLN) at the record end to minimize overlapping of received records causing extra processing for GeoEvent service.  GeoEvent service uses the ETX as record separator but sometimes receives short status messages like "STXE99ETX". The GeoEvent parsing process is setup to receive ANIALI as one long 512+ character field per record.  The required components are parsed from this long single field via fixed positioning.

6) Stores the "cleansed" data in a log file.

7) Sends modified data to destination system.

## Stack:

Windows 10 Professional\
Visual Studio 2019 CE\
C#\
.NET Core 6

## Usage:

1. Open solution.
2. Compile and publish project.
3. Excecute command in console/terminal.

## Syntax:

NetTap Destination-IP Destination-Port [Listen-Port]

Default listen port is 35263. An optional custom [Listen-Port] can be specified.
