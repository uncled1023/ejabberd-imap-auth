# ejabberd-imap-auth

A .net console app that acts as an external authentication for ejabberd to an IMAP server.

## Installation

To install the external authentication, download the latest binaries from the Releases and place them anywhere that is accessible by the ejabberd service.  

Open the config of ejabberd and set the `extauth_program` variable to the path to the `Imap-Auth.exe` executable.

Run the `Imap-Auth.exe` once to generate the default config file, and edit it to point to the IMAP server you are authenticating against.