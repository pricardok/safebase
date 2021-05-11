using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.IO;
using InitDB.Client;
using System.Xml;
using System.Diagnostics;

public partial class StoredProcedures
{

    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void stpZabbixSender(SqlString Ds_Query)
    {

        try
        {

            var scriptProc = new Process
            {
                StartInfo =
                {
                    FileName = @"",
                    CreateNoWindow = true 
                }
            };

            scriptProc.Start();

        }
        catch (Exception e)
        {
            Core.Erro("Erro : " + e.Message);
        }
    }

}


