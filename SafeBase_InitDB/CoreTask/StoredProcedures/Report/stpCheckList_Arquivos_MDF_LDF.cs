﻿using System;
using System.Collections.Generic;
using System.Text;

namespace InitDB.Client
{
    class stpCheckList_Arquivos_MDF_LDF 
    {
        public static string Query()
        {
            return
            //@"insert into [dbo].[Testedb] ([Nome],[DateTest]) values ('Teste da ferramenta DB - stpCheckList_Arquivos_MDF_LDF',GETDATE())";
            @"  SET NOCOUNT ON
                
	            -- COLETA DE INFORMAÇÕES SOBRE ARQUIVOS MDF
	            IF (OBJECT_ID('tempdb..##MDFs_Sizes') IS NOT NULL)
		            DROP TABLE ##MDFs_Sizes

	            CREATE TABLE ##MDFs_Sizes (
		            [Server]			VARCHAR(50),
		            [Nm_Database]		VARCHAR(100),
		            [Logical_Name]		VARCHAR(100),
		            [Size]				NUMERIC(15,2),
		            [Total_Utilizado]	NUMERIC(15,2),
		            [Espaco_Livre_MB] NUMERIC(15,2),
		            [Percent_Free] NUMERIC(15,2)
	            )

	            EXEC sp_MSforeachdb '
		            Use [?]

			            ;WITH cte_datafiles AS 
			            (
			              SELECT name, size = size/128.0 FROM sys.database_files
			            ),
			            cte_datainfo AS
			            (
			              SELECT	name, CAST(size as numeric(15,2)) as size, 
						            CAST( (CONVERT(INT,FILEPROPERTY(name,''SpaceUsed''))/128.0) as numeric(15,2)) as used, 
						            free = CAST( (size - (CONVERT(INT,FILEPROPERTY(name,''SpaceUsed''))/128.0)) as numeric(15,2))
			              FROM cte_datafiles
			            )

			            INSERT INTO ##MDFs_Sizes
			            SELECT	@@SERVERNAME, DB_NAME(), name as [Logical_Name], size, used, free,
					            percent_free = case when size <> 0 then cast((free * 100.0 / size) as numeric(15,2)) else 0 end
			            FROM cte_datainfo	
	            '
	
	            -- LIMPA DADOS	
	            TRUNCATE TABLE [dbo].[CheckArquivosDados]
	            TRUNCATE TABLE [dbo].[CheckArquivosLog]
	
	            -- Arquivos de Dados (MDF e NDF)
	            INSERT INTO [dbo].[CheckArquivosDados] ([Server], [Nm_Database], [Logical_Name], [FileName], [Total_Reservado], [Total_Utilizado], [Espaco_Livre_MB], [Espaco_Livre_Perc], [MaxSize], [Growth] )
	            SELECT	@@SERVERNAME AS [Server],
			            DB_NAME(A.database_id) AS [Nm_Database],
			            [name] AS [Logical_Name],
			            A.[physical_name] AS [Filename],
			            B.[Size] AS [Total_Reservado],
			            B.[Total_Utilizado],
			            B.[Espaco_Livre_MB] AS [Espaco_Livre (MB)],
			            B.[Percent_Free] AS [Espaco_Livre (%)],
			            CASE WHEN A.[Max_Size] = -1 THEN -1 ELSE (A.[Max_Size] / 1024) * 8 END AS [MaxSize(MB)], 
			            CASE WHEN [is_percent_growth] = 1 
				            THEN CAST(A.[Growth] AS VARCHAR) + ' %'
				            ELSE CAST(CAST((A.[Growth] * 8 ) / 1024.00 AS NUMERIC(15, 2)) AS VARCHAR) + ' MB'
			            END AS [Growth]
	            FROM [sys].[master_files] A WITH(NOLOCK)	
		            JOIN ##MDFs_Sizes B ON DB_NAME(A.[database_id]) = B.[Nm_Database] and A.[name] = B.[Logical_Name]
	            WHERE	A.[type_desc] <> 'FULLTEXT'
			            and A.type = 0	-- Arquivos de Dados (MDF e NDF)

	            IF ( @@ROWCOUNT = 0 )
	            BEGIN
		            INSERT INTO [dbo].[CheckArquivosDados] (	[Server], [Nm_Database], [Logical_Name], [FileName], [Total_Reservado], [Total_Utilizado], 
														            [Espaco_Livre_MB], [Espaco_Livre_Perc], [MaxSize], [Growth], [NextSize], [Fl_Situacao] )
		            SELECT	NULL, 'Sem registro dos Arquivos de Dados (MDF e NDF)', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
	            END

	            -- Arquivos de Log (LDF)
	            INSERT INTO [dbo].[CheckArquivosLog] (	[Server], [Nm_Database], [Logical_Name], [FileName], [Total_Reservado], [Total_Utilizado], 
													            [Espaco_Livre_MB], [Espaco_Livre_Perc], [MaxSize], [Growth] )
	            SELECT	@@SERVERNAME AS [Server],
			            DB_NAME(A.database_id) AS [Nm_Database],
			            [name] AS [Logical_Name],
			            A.[physical_name] AS [Filename],
			            B.[Size] AS [Total_Reservado],
			            B.[Total_Utilizado],
			            B.[Espaco_Livre_MB] AS [Espaco_Livre (MB)],
			            B.[Percent_Free] AS [Espaco_Livre (%)],
			            CASE WHEN A.[Max_Size] = -1 THEN -1 ELSE (A.[Max_Size] / 1024) * 8 END AS [MaxSize(MB)], 
			            CASE WHEN [is_percent_growth] = 1 
				            THEN CAST(A.[Growth] AS VARCHAR) + ' %'
				            ELSE CAST(CAST((A.[Growth] * 8 ) / 1024.00 AS NUMERIC(15, 2)) AS VARCHAR) + ' MB'
			            END AS [Growth]
	            FROM [sys].[master_files] A WITH(NOLOCK)	
		            JOIN ##MDFs_Sizes B ON DB_NAME(A.[database_id]) = B.[Nm_Database] and A.[name] = B.[Logical_Name]
	            WHERE	A.[type_desc] <> 'FULLTEXT'
			            and A.type = 1	-- Arquivos de Log (LDF)
	
	            IF ( @@ROWCOUNT = 0 )
	            BEGIN
		            INSERT INTO [dbo].[CheckArquivosLog] (	[Server], [Nm_Database], [Logical_Name], [FileName], [Total_Reservado], [Total_Utilizado], 
														            [Espaco_Livre_MB], [Espaco_Livre_Perc], [MaxSize], [Growth], [NextSize], [Fl_Situacao] )
		            SELECT	NULL, 'Sem registro dos Arquivos de Log (LDF)', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
	            END
                ";

        }
    }
}
