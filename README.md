![Preview](Preview.png?raw=true "Preview")

# TCPF
TCP Forwarder.

What does TCPF do? 

This utility was used as a troubleshooting tool to review\analyze what data is being passed via TCP between systems.  It captures the data stream and writes it to log files (_Raw.log & _TimeStamp.log).

What does the "CCC" optional parameter do?

ESRI GeoEvent service was built for modern data communication.  It has no native support for legacy serial data communication over TCP.  With the "CCC" option, the utility has been modified to adopt to the inconsistent ANIALI data coming from the vendors.  Basically, it takes the raw ANIALI data from the vendors (CARRIERS/PROVIDERS, INTRADO, CENTRAL SQAURE) and makes it clean before the GeoEvent server can digest the ANIALI data for plotting on a map.

Utility does the following: 

1) Strips end-of-line (LN) control code in between the start of text (STX) and the end of text (ETX) control codes. LN in the actual data stream causes issues for GeoEvent service parsing.

2) Strips the computed checksum (BCC) that comes after the ETX. Sometimes the calculated BCC is a control code which is more frequent with the new phone system for whatever reason.  The BCC control code causes issues for GeoEvent service parsing.

3) Converts all "COMMA" to "SPACE" (GeoEvent service parsing uses "COMMA" as field separator.  Parsing process is setup to receive ANIALI as one long 512+ character field per record.  It parses the needed components from the long field via fixed positioning.) 

4) Adds a "CRLN" to minimize received records from being overlapped causing extra processing for GeoEvent service.  GeoEvent service uses the ETX as record separator but sometimes short status messages like "STXE99ETX" come over from the ALI servers.

5) The "cleansed" version of the data is also logged in the _CCC.log file.

## Stack:

Windows 10 Professional\
Visual Studio 2019 CE\
C#\
.NET Core 3.1

## Usage:

1. Open solution.
2. Compile project.
3. Excecute command in console/terminal.

## Syntax:

TCPF Local-IP Local-Port Destination-IP Destination-Port [CCC]




























