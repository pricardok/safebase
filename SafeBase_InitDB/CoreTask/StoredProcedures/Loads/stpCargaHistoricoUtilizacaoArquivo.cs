﻿using System;
using System.Collections.Generic;
using System.Text;

namespace InitDB.Client
{
    class stpCargaHistoricoUtilizacaoArquivo
    {
        public static string Query()
        {
            return
            // @"insert into [dbo].[Testedb] ([Nome],[DateTest]) values ('Teste da ferramenta DB - stpCargaHistoricoUtilizacaoArquivo',GETDATE())";
            @"
                SET NOCOUNT ON
                	INSERT INTO dbo.HistoricoUtilizacaoArquivo
	                SELECT DB_NAME(database_id) AS [Database Name]
			                , file_id 
			                , io_stall_read_ms
			                , num_of_reads
			                , CAST(io_stall_read_ms/(1.0 + num_of_reads) AS NUMERIC(10,1)) AS [avg_read_stall_ms]
			                , io_stall_write_ms
			                , num_of_writes
			                , CAST(io_stall_write_ms/(1.0+num_of_writes) AS NUMERIC(10,1)) AS [avg_write_stall_ms]
			                , io_stall_read_ms + io_stall_write_ms AS [io_stalls]
			                , num_of_reads + num_of_writes AS [total_io]
			                , CAST((io_stall_read_ms + io_stall_write_ms)/(1.0 + num_of_reads + num_of_writes) AS NUMERIC(10,1)) AS [avg_io_stall_ms]
			                , GETDATE() as [Dt_Registro]
	                FROM sys.dm_io_virtual_file_stats(null,null)
	            ";
        }
    }
}
