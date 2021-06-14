

/*

SF Download – Integrating Salesforce Downloads to SQL Server

Copyright (C) 2021 Kevin Chadney


This program is free software: you can redistribute it and/or modify

it under the terms of the GNU General Public License as published by

the Free Software Foundation, either version 3 of the License, or

(at your option) any later version.


This program is distributed in the hope that it will be useful,

but WITHOUT ANY WARRANTY; without even the implied warranty of

MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the

GNU General Public License for more details.


You should have received a copy of the GNU General Public License

along with this program.  If not, see <https://www.gnu.org/licenses/>.

*/

using CommandLine;

[Verb("init", HelpText = "Setup a new SF download database.")]
class InitOptions
{   //Options for initial download 
    //SQL options in uppercase
    //SF options in lowercase
    //Other options in lowercase

    [Option('S', "sqlServer", Required = true, HelpText = "The SQL server name to download to.")]
    public string SqlServer { get; set; }

    [Option('D', "sqlDatabase", Required = true, HelpText = "The SQL server database name to download to.")]
    public string SqlDatabase { get; set; }

    [Option('u', "sfUsername", Required = true, HelpText = "The SF username to connect with.")]
    public string SfUsername { get; set; }

    [Option('p', "sfPassword", Required = true, HelpText = "The SF password to connect with.")]
    public string SfPassword { get; set; }

    [Option('t', "sfToken", Required = true, HelpText = "The SF security token to connect with.")]
    public string SfToken { get; set; }
    
    [Option('n', "downloadNew", Required = false, HelpText = "Always download data from objects as they are found using the default options.")]
    public bool DownloadNew { get; set; }

    [Option('m', "downloadMethod", Required = false, HelpText = "Specify the method by which data will be downloaded from Salesforce: SOAP | Bulk | Batched")]
    public string DownloadMethod { get; set; }

    [Option('i', "integrationMethod", Required = false, HelpText = "Specify the method by which data will be saved to SQL: Incremental | Delete")]
    public string IntegrationMethod { get; set; }


}


[Verb("download", HelpText = "Download to an existing SF database.")]
class DownloadOptions
{
    [Option('S', "sqlServer", Required = true, HelpText = "The SQL server name to download to.")]
    public string SqlServer { get; set; }

    [Option('D', "sqlDatabase", Required = true, HelpText = "The SQL server database name to download to.")]
    public string SqlDatabase { get; set; }
}



[Verb("objects", HelpText = "Set the download options for SF objects.")]
class ObjectOptions
{
    [Option('S', "sqlServer", Required = true, HelpText = "The SQL server name to download to.")]
    public string SqlServer { get; set; }

    [Option('D', "sqlDatabase", Required = true, HelpText = "The SQL server database name to download to.")]
    public string SqlDatabase { get; set; }

    [Option('o', "object", Required = true, HelpText = "The SF object API name.")]
    public string SfObject { get; set; }

    [Option('d', "download", Required = false, HelpText = "Set whether the download should be run for an object.")]
    public string Download { get; set; }

    [Option('m', "downloadMethod", Required = false, HelpText = "Specify the method by which data will be downloaded from the object: SOAP | Bulk | Batched")]
    public string DownloadMethod { get; set; }

    [Option('i', "integrationMethod", Required = false, HelpText = "Specify the method by which data will be saved to SQL: Incremental | Delete")]
    public string IntegrationMethod { get; set; }
}


[Verb("fields", HelpText = "Set the download options for SF fields.")]
class FieldOptions
{
    [Option('S', "sqlServer", Required = true, HelpText = "The SQL server name to download to.")]
    public string SqlServer { get; set; }

    [Option('D', "sqlDatabase", Required = true, HelpText = "The SQL server database name to download to.")]
    public string SqlDatabase { get; set; }

    [Option('o', "object", Required = true, HelpText = "The SF object API name.")]
    public string SfObject { get; set; }

    [Option('f', "fieldName", Required = false, HelpText = "The field name to change download status for. Leaving blank will prompt the user to set the status of every field on the object.")]
    public string FieldName { get; set; }

    [Option('d', "download", Required = false, HelpText = "Specify whether the field should be included in the object download (Y/N).")]
    public string Download { get; set; }

}