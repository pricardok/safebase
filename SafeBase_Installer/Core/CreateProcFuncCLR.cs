using System;
using System.Collections.Generic;
using System.Text;
using SafeBase_Installer.Core;

namespace SafeBase_Installer
{
    class CreateProcFuncCLR
    {
        public static string Query(string use)
        {
            return
            @"
            USE "+ use + @"
            GO
            CREATE FUNCTION [dbo].[fncExportaMultiHTML]
            (@Ds_Query NVARCHAR (MAX), @Ds_Mensagem_Mail NVARCHAR (MAX), @Fl_Estilo INT, @Fl_Html_Completo BIT)
            RETURNS NVARCHAR (MAX)
            AS
             EXTERNAL NAME [" + use + @"].[UserDefinedFunctions].[fncExportaMultiHTML]
            GO
            CREATE FUNCTION [dbo].[fncResolveHttpRequest](@requestMethod [nvarchar](max), @url [nvarchar](max), @parameters [nvarchar](max), @headers [nvarchar](max), @timeout [int], @autoDecompress [bit], @convertResponseToBas64 [bit])
            RETURNS [xml] WITH EXECUTE AS CALLER
            AS 
            EXTERNAL NAME [" + use + @"].[UserDefinedFunctions].[fncResolveHttpRequest]
            GO
            CREATE FUNCTION [dbo].[fncLerArquivoRetornaString]
            (@Ds_Caminho NVARCHAR (MAX))
            RETURNS NVARCHAR (MAX)
            AS
             EXTERNAL NAME [" + use + @"].[UserDefinedFunctions].[fncLerArquivoRetornaString]
            GO
            CREATE FUNCTION [dbo].[fncLerArquivo]
            (@Ds_Caminho NVARCHAR (MAX))
            RETURNS 
                 TABLE (
                    [Nr_Linha] INT            NULL,
                    [Ds_Texto] NVARCHAR (MAX) NULL)
            AS
             EXTERNAL NAME [" + use + @"].[UserDefinedFunctions].[fncLerArquivo]
            GO
            CREATE FUNCTION [dbo].[fncListarDiretorio]
            (@Ds_Diretorio NVARCHAR (MAX), @Ds_Filtro NVARCHAR (MAX))
            RETURNS 
                 TABLE (
                    [Linha]              INT            NULL,
                    [Tipo]               NVARCHAR (50)  NULL,
                    [Arquivo]            NVARCHAR (500) NULL,
                    [ArquivoSemExtensao] NVARCHAR (500) NULL,
                    [Diretorio]          NVARCHAR (500) NULL,
                    [Extensao]           NVARCHAR (20)  NULL,
                    [CaminhoCompleto]    NVARCHAR (500) NULL,
                    [QuantidadeTamanho]  BIGINT         NULL,
                    [SomenteLeitura]     BIT            NULL,
                    [DataCriacao]        DATETIME       NULL,
                    [DataUltimoAcesso]   DATETIME       NULL,
                    [DataModificacao]    DATETIME       NULL)
            AS
             EXTERNAL NAME [" + use + @"].[UserDefinedFunctions].[fncListarDiretorio]
            GO
            CREATE PROCEDURE [dbo].[Help]
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[Help]
            GO
            CREATE PROCEDURE [dbo].[stpCopyFile]
            @Origem NVARCHAR (MAX), @Destino NVARCHAR (MAX), @Sobrescrever BIT
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpCopyFile]
            GO
            CREATE PROCEDURE [dbo].[stpDeleteFile]
            @caminho NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpDeleteFile]
            GO
            CREATE PROCEDURE [dbo].[stpExportaQueryArquivo]
            @Query NVARCHAR (MAX), @Separador NVARCHAR (MAX), @Caminho NVARCHAR (MAX), @Coluna INT
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpExportaQueryArquivo]
            GO
            CREATE PROCEDURE [dbo].[stpImportCSV]
            @Ds_Caminho_Arquivo NVARCHAR (MAX), @Ds_Separador NVARCHAR (MAX), @Fl_Primeira_Linha_Cabecalho BIT, @Nr_Linha_Inicio INT, @Nr_Linhas_Retirar_Final INT, @Ds_Tabela_Destino NVARCHAR (MAX), @Ds_Codificacao NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpImportCSV]
            GO
            CREATE PROCEDURE [dbo].[stpMoveArquivo]
            @ArquivoOrigem NVARCHAR (MAX), @PastaDestino NVARCHAR (MAX), @Sobrescrever BIT
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpMoveArquivo]
            GO
            CREATE PROCEDURE [dbo].[stpRenameFile]
            @CaminhoOrigem NVARCHAR (MAX), @CaminhoDestino NVARCHAR (MAX), @Sobrescrever BIT
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpRenameFile]
            GO
            CREATE PROCEDURE [dbo].[stpSendMsgCurl]
            @d NVARCHAR (MAX), @url NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpSendMsgCurl]
            GO
            CREATE PROCEDURE [dbo].[stpSendMsgTeams]
            @msg NVARCHAR (MAX), @channel NVARCHAR (MAX), @ap INT
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpSendMsgTeams]
            GO
            CREATE PROCEDURE [dbo].[stpSendMsgTelegram]
            @Destino NVARCHAR (MAX), @Msg NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpSendMsgTelegram]
            GO
            CREATE PROCEDURE [dbo].[stpServerAlert]
            @ServerAlert NVARCHAR (MAX),
	        @what [int] = 0
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpServerAlert]
            GO
            CREATE PROCEDURE [dbo].[stpServerJob]
            @ServerTask NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpServerJob]
            GO
            CREATE PROCEDURE [dbo].[stpServerLoads]
            @ServerLoad NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpServerLoads]
            GO
            CREATE PROCEDURE [dbo].[stpWriteFile]
            @Ds_Texto NVARCHAR (MAX), @Ds_Caminho NVARCHAR (MAX), @Ds_Codificacao NVARCHAR (MAX), @Ds_Formato_Quebra_Linha NVARCHAR (MAX), @Fl_Append BIT
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpWriteFile]
            GO
            CREATE PROCEDURE [dbo].[stpToZabbix]
            @job INT, @name NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpToZabbix]
            GO
            --CREATE PROCEDURE [dbo].[stpStartBackup]
            --@BackupType NVARCHAR (MAX)
            --AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpStartBackup]
            --GO
            CREATE PROCEDURE [dbo].[stpStartDeleteOldBackups]
            @BackupType NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpStartDeleteOldBackups]
            GO
            CREATE PROCEDURE [dbo].[stpStartUpdateStats]
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpStartUpdateStats]
            GO
            CREATE PROCEDURE [dbo].[stpStartShrinkingLogFiles]
            @Shrinkfile INT
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpStartShrinkingLogFiles]
            GO
            CREATE PROCEDURE [dbo].[stpStartCheckDB]
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpStartCheckDB]
            GO
            CREATE PROCEDURE [dbo].[stpStartDefraging]
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpStartDefraging]
            GO
            CREATE PROCEDURE [dbo].[stpGetInfo]
            @What NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpGetInfo]
            GO
            CREATE PROCEDURE [dbo].[stpStartBackupCustom]
            @BackupType NVARCHAR (MAX), @db NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpStartBackupCustom]
            GO
            CREATE PROCEDURE [dbo].[stpStartDeleteBackupsCustom]
            @BackupType NVARCHAR (MAX), @db NVARCHAR (MAX)
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpStartDeleteBackupsCustom]
            GO
            CREATE PROCEDURE [dbo].[stpStartBackupDB]
            @BackupType NVARCHAR (MAX), @type INT = 1
            AS EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpStartBackupDB]
            GO
            CREATE PROCEDURE [dbo].[stpSendNotification]
	            @Id_AlertaParametro [int],
	            @ProfileDBMail [nvarchar](max),
	            @mailDestination [nvarchar](max),
	            @BodyFormatMail [nvarchar](max),
	            @Subject [nvarchar](max),
	            @Importance [nvarchar](max),
	            @HTML [nvarchar](max),
	            @MntMsg [nvarchar](max),
	            @CanalTelegram [nvarchar](max),
	            @Ds_Menssageiro_02 [nvarchar](max),
	            @Teams [nvarchar](max)
            WITH EXECUTE AS CALLER
            AS
            EXTERNAL NAME [" + use + @"].[StoredProcedures].[stpSendNotification]
            GO
            
             /****** Object:  StoredProcedure [dbo].[stpZabbixSender]    Script Date: 08/06/2021 15:09:47 ******/
            SET ANSI_NULLS OFF
            GO

            SET QUOTED_IDENTIFIER OFF
            GO

            CREATE PROCEDURE [dbo].[stpZabbixSender]
            	@Ds_caminho [nvarchar](max),
            	@Ds_ZabbixServer [nvarchar](max),
            	@Ds_ZabbixLocalServer [nvarchar](max),
            	@ZabbixAlertName [nvarchar](max),
            	@valor [bigint]
            WITH EXECUTE AS CALLER
            AS
            EXTERNAL NAME [SafebaseZabbix].[StoredProcedures].[stpZabbixSender]
            GO
            
            /****** Object:  StoredProcedure [dbo].[stpJobExecuteSSIS]    Script Date: 08/06/2021 15:09:59 ******/
            SET ANSI_NULLS OFF
            GO
            
            SET QUOTED_IDENTIFIER OFF
            GO
            
            CREATE PROCEDURE [dbo].[stpJobExecuteSSIS]
            	@package_name_INPUT [nvarchar](max),
            	@folder_name_INPUT [nvarchar](max),
            	@project_name_INPUT [nvarchar](max)
            WITH EXECUTE AS CALLER
            AS
            EXTERNAL NAME [SafebaseZabbix].[StoredProcedures].[stpJobExecuteSSIS]
            GO
            USE [SafeBase]
            GO
            
            /****** Object:  StoredProcedure [dbo].[stpJobGravaHistorico]    Script Date: 08/06/2021 15:10:08 ******/
            SET ANSI_NULLS OFF
            GO
            
            SET QUOTED_IDENTIFIER OFF
            GO
            
            CREATE PROCEDURE [dbo].[stpJobGravaHistorico]
            	@JobId [int],
            	@TimeExecIni [datetime],
            	@TimeExecFim [datetime],
            	@SegundosExec [int],
            	@Error [int],
            	@ErrorMessage [nvarchar](max)
            WITH EXECUTE AS CALLER
            AS
            EXTERNAL NAME [SafebaseZabbix].[StoredProcedures].[stpJobGravaHistorico]
            GO
            
            /****** Object:  StoredProcedure [dbo].[stpJobSchedule]    Script Date: 08/06/2021 15:10:19 ******/
            SET ANSI_NULLS OFF
            GO
            
            SET QUOTED_IDENTIFIER OFF
            GO
            
            CREATE PROCEDURE [dbo].[stpJobSchedule]
            	@Force_freq [int] = 0
            WITH EXECUTE AS CALLER
            AS
            EXTERNAL NAME [SafebaseZabbix].[StoredProcedures].[stpJobSchedule]
            GO

             ";

        }
    }
}
