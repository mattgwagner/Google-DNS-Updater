Google-DNS-Updater
====================

A small Windows service to retrieve and update dynamic DNS records via the dyndns protocol for Google Domains.

Building
====================

This project uses the PSake build system. To build the service, double-click `build.bat` in the project root. It will
install anything it needs to complete the build.

Installing
====================

From a command line, run `build.bat Start`, which will build the service, open notepad to accept configuration values, and then start the service.