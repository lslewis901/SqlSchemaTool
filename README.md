# SqlSchemaTool
A SQL Schema and Data Comparison Tool - Its source code...

The SQL Schema Tool or SST as it is now known started as a command-line developer’s tool. As such, the tool was designed to assist developers working against a SQL server database, so as to migrate any database changes from a development environment into a test or nightly build environment.

The SST is an application for generating XML schema documents from SQL server databases, comparing those databases or the XML schema documents, and finally generating a DiffGram XML document from the compare results.

Simply put, the purpose is to make a snapshot of the current latest database schema and merge those changes into a build of the previous database schema (live or snapshot).

The tool can make snapshots of databases, DTS packages, or even the data itself, to perform offline compare(s) and create update script(s). The tool can also be used to generate schema create scripts for archival or source control purposes.

As developers, testers, or deployment people, we want to take database B, and make it look like database A.

See the Codeproject Article (http://www.codeproject.com/KB/database/sqlschematool.aspx) for additional information about the project.

<b>Dependencies</b>
There are dependencies for the following:

<UL>
<li>WIX Installer http://wix.sourceforge.net/
<li>NLog http://www.nlog-project.org/
<li>Microsoft's XMLDiff code (part of the project) http://msdn.microsoft.com/en-us/library/aa302295.aspx
<li>Microsoft's SQL Server 2000 SP4, DTS DLLs. (Interop DLLs now in source) http://support.microsoft.com/kb/839884
</UL>

Details

There is a dependency for the development project to have the Microsoft SQL Server 2000 Service Pack 4 installed or the SQL Server 2000 DTS DLLs. There are three DLLs included in the SQL Server 2000 SP4 that I should have made sure the interops were included in the reference DLLs directory: Interop.DTS.dll, Interop.DTSCustTasks.dll, and Interop.DTSPump.dll. I removed them from the ZIP when I was cleaning up the build DLLs.

There is also a dependency Microsoft's XMLDiff DLLs where the source should be in the ZIP along with a dependency on NLog which is in the reference DLLs directory and the latest WIX installer which is available from wix.sourceforge.net.

I added zip(s) file for the DTS interop DLLs on the google code site, though you should probably get them from SQL Server 2000 SP4.
