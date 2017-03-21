# SqlSchemaTool
A SQL Schema and Data Comparison Tool - Its source code...

The SQL Schema Tool or SST as it is now known started as a command-line developer’s tool. As such, the tool was designed to assist developers working against a SQL server database, so as to migrate any database changes from a development environment into a test or nightly build environment.

The SST is an application for generating XML schema documents from SQL server databases, comparing those databases or the XML schema documents, and finally generating a DiffGram XML document from the compare results.

Simply put, the purpose is to make a snapshot of the current latest database schema and merge those changes into a build of the previous database schema (live or snapshot).

The tool can make snapshots of databases, DTS packages, or even the data itself, to perform offline compare(s) and create update script(s). The tool can also be used to generate schema create scripts for archival or source control purposes.

As developers, testers, or deployment people, we want to take database B, and make it look like database A.

See the Codeproject Article (http://www.codeproject.com/KB/database/sqlschematool.aspx) for additional information about the project.
