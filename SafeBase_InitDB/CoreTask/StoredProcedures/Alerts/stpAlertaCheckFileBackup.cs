using System;
using System.Collections.Generic;
using System.Text;

namespace InitDB.Client
{
    class stpAlertaCheckFileBackup
    {
        public static string Query(string type)
        {
            return
            //@"insert into [dbo].[Testedb] ([Nome],[DateTest]) values ('Teste da ferramenta DB - stpcheckFileBackup',GETDATE())";
             
            @"

            SET NOCOUNT ON

            ---- Recupera os parametros base
            DECLARE @Id_AlertaParametro INT = (SELECT Id_AlertaParametro FROM [dbo].AlertaParametro (NOLOCK) WHERE Nm_Alerta = 'Alerta File DB' AND Ativo = 1)
            DECLARE @Ds_Caminho_Base VARCHAR(100) = (SELECT Ds_Caminho FROM [dbo].AlertaParametro (NOLOCK) WHERE Nm_Alerta = 'CheckList')
            DECLARE @Telegram INT = (select Id_AlertaParametro from AlertaParametro WHERE Nm_Alerta = 'Envia Telegram')
            DECLARE @Teams INT = (select Id_AlertaParametro from AlertaParametro WHERE Nm_Alerta = 'Envia Teams')

            ---- Recupera os parametros do Alerta
            DECLARE @File_Backup_Parametro INT, @EmailDestination VARCHAR(200), @TextRel1 VARCHAR(max), @TextRel2 VARCHAR(4000), 
                @NomeRel VARCHAR(300),@MntMsg VARCHAR(200), @TLMsg VARCHAR(200), @SendMail VARCHAR(200), @ProfileDBMail VARCHAR(50), 
                @BodyFormatMail VARCHAR(20), @CaminhoPath VARCHAR(50), @CaminhoFim VARCHAR(50), @Ass VARCHAR(4000),@HTML VARCHAR(MAX), 
                @Query VARCHAR(MAX), @Importance AS VARCHAR(6), @Subject VARCHAR(600), @Ds_Email_Assunto_alerta VARCHAR (600), 
                @Ds_Email_Assunto_solucao VARCHAR (600), @Ds_Email_Texto_alerta VARCHAR (max), @Ds_Email_Texto_solucao VARCHAR (600), 
                @Ds_Menssageiro_01 VARCHAR (30), @Ds_Menssageiro_02 VARCHAR (30), @Ds_Menssageiro_03 VARCHAR (30),@Fl_Tipo TINYINT,
	            @File NVARCHAR(10)
  
            ---- Email, Parametro, Id Telegram, Caminho dos reports, Profile DB Mail, Body Format Mail 
            SELECT @NomeRel = Nm_Alerta, 
                @File_Backup_Parametro = Vl_Parametro, 
                @EmailDestination = Ds_Email, 
                @TLMsg = Ds_MSG,
	            @Ds_Menssageiro_01 = A.Ds_Menssageiro_01,
	            @Ds_Menssageiro_02 = A.Ds_Menssageiro_02,
                @Ds_Menssageiro_03 = A.Ds_Menssageiro_03,
                @CaminhoPath = Ds_Caminho_Log, 
                @ProfileDBMail = Ds_ProfileDBMail, 
                @BodyFormatMail = Ds_BodyFormatMail,
                @importance = Ds_TipoMail,
                @Ds_Email_Assunto_solucao = B.SubjectSolution,
                @Ds_Email_Texto_solucao = B.MailTextSolution,
                @Ds_Email_Assunto_alerta = B.SubjectProblem,
                @Ds_Email_Texto_alerta = B.MailTextProblem,
                @Ass = C.Assinatura
            FROM [dbo].[AlertaParametro] A
            INNER JOIN [dbo].[AlertaParametroMenssage] B ON A.Id_AlertaParametro = B.IdAlertaParametro
            INNER JOIN [dbo].[MailAssinatura] C ON C.Id = A.IdMailAssinatura
            WHERE [Id_AlertaParametro] = @Id_AlertaParametro  

            DECLARE @CanalTelegram VARCHAR(100) = (SELECT A.canal FROM [dbo].[AlertaMsgToken] A
                    INNER JOIN [dbo].AlertaParametro B ON A.Id = B.Ds_Menssageiro_01 where b.Ds_Menssageiro_01 = @Ds_Menssageiro_01 AND B.Id_AlertaParametro = @Telegram AND B.Ativo = 1) 

            -- Cria a tabela que ira armazenar os dados dos processos
            IF(OBJECT_ID('tempdb..#tb_bkp') IS NOT NULL)
	            DROP TABLE #tb_bkp;
            CREATE TABLE #tb_bkp
	            ([sqldb] NVARCHAR(400),
	            [dir] NVARCHAR(900));

            -- Declara valores de contexto
            DECLARE @Lines TABLE (Line NVARCHAR(MAX)) ;
            DECLARE @FullText NVARCHAR(MAX) = '' ;
            DECLARE @M VARCHAR(200), @X VARCHAR(MAX)
            DECLARE @BX NVARCHAR(1000) = (
								            SELECT TOP 1   
									            ParametersXML.value('(/Customer/BackupFull/BackupPath)[1]', 'varchar(max)')
									            +CASE WHEN RIGHT((ParametersXML.value('(/Customer/BackupFull/BackupPath)[1]', 'varchar(max)')), 1) = '\\' THEN '' ELSE '\\' END	+ @@ServerName + '\\' + 
									            +CASE WHEN RIGHT(@@ServerName, LEN(@@ServiceName)) = @@ServiceName THEN '' ELSE @@ServiceName + '\\' END AS BackupPath                                                                 
								            FROM [dbo].[ConfigDB]
								          )
	
            SET @File = '"+ type + @"'

            DECLARE @F NVARCHAR(767)
            DECLARE cursor_d CURSOR

            FOR 

	            SELECT RTRIM(name) name 
	            FROM sys.databases sd
	            INNER JOIN fncListarDiretorio (''+@BX+'', '*') ld ON sd.name = ld.arquivo
	            WHERE sd.state_desc not in ('OFFLINE','RESTORING','RECOVERY_PENDING') and sd.is_in_standby = 0 and sd.is_read_only = 0 and sd.database_id > 4 and [name] not like 'SafeBase'
						AND sd.NAME NOT IN (SELECT 
												ADC.database_name                               
											FROM sys.availability_groups_cluster as AGC                                                                            
											JOIN sys.dm_hadr_availability_replica_cluster_states as RCS ON AGC.group_id = RCS.group_id                             
											JOIN sys.dm_hadr_availability_replica_states as ARS ON RCS.replica_id = ARS.replica_id and RCS.group_id = ARS.group_id 
											JOIN sys.availability_databases_cluster as ADC ON AGC.group_id = ADC.group_id                                          
											WHERE ARS.is_local = 1
											AND ARS.role_desc LIKE 'SECONDARY')
	
            OPEN cursor_d;
            FETCH NEXT FROM cursor_d INTO @F
            WHILE @@FETCH_STATUS = 0

                BEGIN 

		            DECLARE @TypeBackup VARCHAR(30) = CASE @File
										                WHEN 'D' THEN '.bak'
										                WHEN 'I' THEN '.dif'
										                WHEN 'L' THEN '.trn'
										                else '.bak'
									                    END 

		            IF NOT EXISTS (	SELECT Arquivo 
						            FROM dbo.fncListarDiretorio (''+@BX + @F+'', '*') A
						            WHERE Extensao like @TypeBackup 
						            AND @F in (
									            SELECT RTRIM(Banco) 
									            FROM vwcheckbackup 
									            WHERE banco LIKE @F -- AND DataLog is not null -- AND DataDiff is not null
									            )
						            AND ((@TypeBackup = '.bak')
							            OR (@TypeBackup = '.dif' AND CONVERT(char(10), DataCriacao,126)  = CONVERT(char(10), GetDate(),126))
							            OR (@TypeBackup = '.trn' AND CONVERT(char(10), DataCriacao,126)  = CONVERT(char(10), GetDate(),126)
														            AND @F IN (SELECT RTRIM(Banco) FROM vwcheckbackup where DataLog is not null)
							            )
						            ))
		            BEGIN

			            insert #tb_bkp ([sqldb],[dir])
			            select @F,+@BX + @F

			            insert @Lines select @F+'; '

			            SELECT @M = 'Alerta: Arquivo de #Backup do banco: #'+@F+@TypeBackup+' Não Encontrado, na instância ' +@@servername + '.'
			            PRINT @M

		            END
		            /* DESCOMENTE PRA VER TODOS OS BANCOS OK
		            ELSE
		            BEGIN

			            PRINT 'AQUIVO DE BACKUP: '+@F+' - TIPO: '+@TypeBackup+' - OK' 

		            END
		            */
			
                    FETCH NEXT FROM cursor_d into @F
			
                END;

	            select @FullText = @FullText + Char(13) +  Line from @Lines ;
	            select @X = ' - Arquivos de #Backup dos bancos não encontrados na instância ' +@@servername + ': ' + @FullText

            CLOSE cursor_d;
            DEALLOCATE cursor_d;


            IF EXISTS(select [sqldb] from #tb_bkp where [sqldb] <> '' )
            BEGIN

			            BEGIN
				
				            /*******************************************************************************************************************************
		                    --	CRIA O EMAIL - ALERTA
		                    *******************************************************************************************************************************/			
				            -- Parametros do Alerta
			                SET @Subject =  @Ds_Email_Assunto_alerta + @@SERVERNAME
			                SET @TextRel1 =  @Ds_Email_Texto_alerta +'<p style=color:red;> Tipo de Arquivo faltante: '+@TypeBackup + '</p><BR /><BR />USE [SafeBase] <BR /><BR />GO <BR /> <BR />DECLARE @DB NVARCHAR(500) = ''NOME DO BANCO'' -- INFORME O NOME DO BANCO <BR />DECLARE @BX NVARCHAR(1000) = (SELECT TOP 1 <BR />ParametersXML.value(''(/Customer/BackupFull/BackupPath)[1]'', ''varchar(max)'') <BR />+CASE WHEN RIGHT((ParametersXML.value(''(/Customer/BackupFull/BackupPath)[1]'', ''varchar(max)'')), 1) = ''\\'' THEN '''' ELSE ''\\'' END	+ @@ServerName + ''\\'' + <BR />+CASE WHEN RIGHT(@@ServerName, LEN(@@ServiceName)) = @@ServiceName THEN '''' ELSE @@ServiceName + ''\\'' END AS BackupPath <BR />FROM [dbo].[ConfigDB]) <BR /><BR />SELECT * FROM dbo.fncListarDiretorio (''''+@BX + @DB+'''', ''*'')'	
			                SET @CaminhoFim = @Ds_Caminho_Base + @CaminhoPath + @NomeRel +'.html'
			 
			                -- Gera Primeiro bloco de HTML
			                SET @Query = 'select [sqldb] as Banco, [dir] as Diretorio from #tb_bkp order by sqldb'
			                SET @HTML = dbo.fncExportaMultiHTML(@Query, @TextRel1, 2, 1)
			                -- Gera Segundo bloco de HTML
			                select @HTML = @HTML + @Ass
			                -- Salva Arquivo HTML de Envio
			                EXEC dbo.stpWriteFile 
				                @Ds_Texto = @HTML, -- nvarchar(max)
				                @Ds_Caminho = @CaminhoFim, -- nvarchar(max)
				                @Ds_Codificacao = N'UTF-8', -- nvarchar(max)
				                @Ds_Formato_Quebra_Linha = N'windows', -- nvarchar(max)
				                @Fl_Append = 0 -- bit

				            /*******************************************************************************************************************************
		                    --	ALERTA - ENVIA O EMAIL - ENVIA TELEGRAM
		                    *******************************************************************************************************************************/	
                            IF EXISTS  (SELECT B.Ativo from AlertaParametro A 
			                                INNER JOIN [dbo].[AlertaEnvio] B ON B.IdAlertaParametro = A.Id_AlertaParametro
			                                WHERE B.Ativo = 1
			                                AND B.Des LIKE '%Email'
			                                AND [Id_AlertaParametro] = @Id_AlertaParametro
			                                )
                                BEGIN

                                     EXEC [msdb].[dbo].[sp_send_dbmail]
                                            @profile_name = @ProfileDBMail,
                                            @recipients = @EmailDestination,
                                            @body_format = @BodyFormatMail,
                                            @subject = @Subject,
                                            @importance = @Importance,
                                            @body = @HTML;

                                END
			 
	                        -- Parametro Menssageiro
                            SET @MntMsg = @Subject+', Verifique os detalhes no *E-Mail*'

                            IF EXISTS  (SELECT B.Ativo from AlertaParametro A 
			                                INNER JOIN [dbo].[AlertaEnvio] B ON B.IdAlertaParametro = A.Id_AlertaParametro
			                                WHERE B.Ativo = 1
			                                AND B.Des LIKE '%Telegram'
			                                AND [Id_AlertaParametro] = @Id_AlertaParametro
			                                )
                                BEGIN
						            DECLARE @TLF NVARCHAR(MAX)
						            SET @TLF = @MntMsg + @X
                                    -- Envio do Telegram    
                                    EXEC dbo.StpSendMsgTelegram 
                                          @Destino = @CanalTelegram,
                                          @Msg = @TLF
                                END

                            IF EXISTS  (SELECT B.Ativo from AlertaParametro A 
			                                INNER JOIN [dbo].[AlertaEnvio] B ON B.IdAlertaParametro = A.Id_AlertaParametro
			                                WHERE B.Ativo = 1
			                                AND B.Des LIKE '%Teams'
			                                AND [Id_AlertaParametro] = @Id_AlertaParametro
			                                )
                                BEGIN
                                    -- MS TEAMS
                                    SET @MntMsg = (select replace (@MntMsg, '\', '-'))
                                    EXEC [dbo].[stpSendMsgTeams]
	                                    @msg = @MntMsg,
	                                    @channel = @Ds_Menssageiro_02,
                                        @ap = @Teams
                                END	

				            INSERT INTO [dbo].[Alerta] ( [Id_AlertaParametro], [Ds_Mensagem], [Fl_Tipo] )
				            SELECT @Id_AlertaParametro, @Subject, 1	

            END

            END

            ";

        }
    }
}
