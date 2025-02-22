﻿using System;
using System.Collections.Generic;
using System.Text;

namespace InitDB.Client
{
    class stpCargaLogAlwaysOn
    {
        public static string Query()
        {
      
            return
            // @"insert into [dbo].[Testedb] ([Nome],[DateTest]) values ('Teste da ferramenta DB - stpCargaAlteracaoDB',GETDATE())";
            @"
                SET NOCOUNT ON
 
                DECLARE @IsHadrEnabled as sql_variant  
                SET @IsHadrEnabled = (select SERVERPROPERTY('IsHadrEnabled'))  

                IF @IsHadrEnabled = 1

                BEGIN 

                    DECLARE @Data datetime
                    SET @Data = getdate()

                    INSERT INTO dbo.[HistoricoAlwaysOn]
                    SELECT 
                        @Data,
                        ar.replica_server_name, 
                        adc.database_name, 
                        ag.name AS ag_name, 
                        drs.is_local, 
                        drs.is_primary_replica, 
                        drs.synchronization_state_desc, 
                        drs.is_commit_participant, 
                        drs.synchronization_health_desc, 
                        drs.recovery_lsn, 
                        drs.truncation_lsn, 
                        drs.last_sent_lsn, 
                        drs.last_sent_time, 
                        drs.last_received_lsn, 
                        drs.last_received_time, 
                        drs.last_hardened_lsn, 
                        drs.last_hardened_time, 
                        drs.last_redone_lsn, 
                        drs.last_redone_time, 
                        drs.log_send_queue_size, 
                        drs.log_send_rate, 
                        drs.redo_queue_size, 
                        drs.redo_rate, 
                        drs.filestream_send_rate, 
                        drs.end_of_log_lsn, 
                        drs.last_commit_lsn, 
                        drs.last_commit_time
                    FROM sys.dm_hadr_database_replica_states AS drs
                    INNER JOIN sys.availability_databases_cluster AS adc 
                        ON drs.group_id = adc.group_id AND 
                        drs.group_database_id = adc.group_database_id
                    INNER JOIN sys.availability_groups AS ag
                        ON ag.group_id = drs.group_id
                    INNER JOIN sys.availability_replicas AS ar 
                        ON drs.group_id = ar.group_id AND 
                        drs.replica_id = ar.replica_id

                END

                ELSE

                BEGIN
                    PRINT 'ALWAYSON NAO HABILITADO'
                END

               ";

        }
    }
}
