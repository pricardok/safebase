using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brazip_ExtracaoDadosAutomaticamente
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("Extrair dados da Avaliação CSAT do Brazip");
            Console.WriteLine("---------------------------------------------");

            var resultado = EnviarDados();
            Console.WriteLine(resultado);
        }

        static readonly AtendimentoOnlineBrazip.AtendimentoOnlinePortClient ServicoAtendimentoOnline = new AtendimentoOnlineBrazip.AtendimentoOnlinePortClient();
        private static string ProtocoloEx { get; set; }

        public static string EnviarDados()
        {
            //Enviar os dados para o banco
            Console.WriteLine("Enviando Dados...");

            var sigla = "safw";
            var senhaService = "495592629ec386a5b9e69029be6aa2e1";

            var time = DateTime.Now.ToString("yyyy/MM/dd").Replace("/", "");
            var serviceKey = CreateMD5(sigla + senhaService + time);

            try
            {
                int rows = 0;
                //
                //
                //MUDAR O MÁXIMO DE DIAS
                //
                //
                for (int i = -1; i >= -15; i--)
                {
                    FormularioAvaliacaoAOBrazip.ObterListaResult result;
                    do
                    {
                        var dados = new FormularioAvaliacaoAOBrazip.ObterListaRequestDados()
                        {
                            bzpdata = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd 00:00:00")
                        };

                        var servico = new FormularioAvaliacaoAOBrazip.FormularioAvaliacaoAOPortClient();

                        var request = new FormularioAvaliacaoAOBrazip.ObterListaRequest()
                        {
                            sigla = sigla,
                            servicekey = serviceKey,
                            dados = dados
                        };

                        result = servico.obterLista(request);
                    } while (result.statusRetorno == "consultaMenorQueLimiteDe45Segundos");

                    if (result.statusRetorno == "vazio") continue;

                    //
                    //
                    //MUDAR A STRING DE CONEXÃO
                    //
                    //
                    var connectionString = @"Data Source=TIRADOR;Initial Catalog=StagingPB;User ID=s_coleta_dw;Password=0w4iprxw0D";

                    foreach (var avaliacao in result.dados)
                    {
                        var protocolo = avaliacao.bzpcodigoreferencia;
                        ProtocoloEx = protocolo;

                        var dados = ObterDados(protocolo, sigla, serviceKey);

                        string solicitacaoAtendida;
                        if (avaliacao.solicitacaoatendida == 0) solicitacaoAtendida = "N/I";
                        else if (avaliacao.solicitacaoatendida == 1) solicitacaoAtendida = "Sim";
                        else solicitacaoAtendida = "Não";

                        var nota = avaliacao.avaliacao;
                        var comentario = avaliacao.comentario;
                        var dataRegistro = avaliacao.bzpdata;

                        SqlConnection sqlConn = new SqlConnection(connectionString);

                        if (sqlConn.State == ConnectionState.Closed)
                            sqlConn.Open();
                        //
                        //
                        //MUDAR TABELA
                        //
                        //
                        var sqlQuery = @"INSERT INTO [Staging].[SurveysBrazip] 
           ([DataRegistro]
           ,[Protocolo]
           ,[Setor]
           ,[AtendimentoDuracao]
           ,[EntrouFila]
           ,[AtendimentoInicio]
           ,[AtendimentoFinal]
           ,[Operador]
           ,[SolicitacaoAtendida]
           ,[Nota]
           ,[Comentario]
           ,[Solicitante]
           ,[CnpjCpf]
           ,[Email]
           ,[Cidade]
           ,[AC]
           ,[AR]
           ,[ProtocoloCD]
           ,[Modelo]
           ,[Tipo]
           ,[Certificado]
           ,[Atendimento]
           ,[TipoEmissao]
           ,[Duvida]
           ,[MotivoContato]
           ,[DescricaoProblema]
           ,[Posicionamento]
           ,[Ambiente]
           ,[Sistema]
           ,[IP]
           ,[Browser]
           ,[Plataforma]
           ,[LogErro])
     VALUES
           (@DataRegistro
           ,@Protocolo
           ,@Setor
           ,@AtendimentoDuracao
           ,@EntrouFila
           ,@AtendimentoInicio
           ,@AtendimentoFinal
           ,@Operador
           ,@SolicitacaoAtendida
           ,@Nota
           ,@Comentario
           ,@Solicitante
           ,@CnpjCpf
           ,@Email
           ,@Cidade
           ,@AC
           ,@AR
           ,@ProtocoloCD
           ,@Modelo
           ,@Tipo
           ,@Certificado
           ,@Atendimento
           ,@TipoEmissao
           ,@Duvida
           ,@MotivoContato
           ,@DescricaoProblema
           ,@Posicionamento
           ,@Ambiente
           ,@Sistema
           ,@IP
           ,@Browser
           ,@Plataforma
           ,@LogErro)";

                        var data = Convert.ToDateTime(dataRegistro).ToString("dd-MM-yyyy HH:mm:ss");

                        SqlCommand SQLScriptToRun = new SqlCommand(sqlQuery, sqlConn);

                        try
                        {
                            SQLScriptToRun.Parameters.AddWithValue("DataRegistro", data);
                            SQLScriptToRun.Parameters.AddWithValue("Protocolo", protocolo);
                            SQLScriptToRun.Parameters.AddWithValue("Setor", dados["Setor"]);
                            SQLScriptToRun.Parameters.AddWithValue("AtendimentoDuracao", dados["AtendimentoDuracao"]);
                            SQLScriptToRun.Parameters.AddWithValue("EntrouFila", dados["EntrouFila"]);
                            SQLScriptToRun.Parameters.AddWithValue("AtendimentoInicio", dados["AtendimentoInicio"]);
                            SQLScriptToRun.Parameters.AddWithValue("AtendimentoFinal", dados["AtendimentoFinal"]);
                            SQLScriptToRun.Parameters.AddWithValue("Operador", dados["Operador"]);
                            SQLScriptToRun.Parameters.AddWithValue("SolicitacaoAtendida", solicitacaoAtendida);
                            SQLScriptToRun.Parameters.AddWithValue("Nota", nota);
                            SQLScriptToRun.Parameters.AddWithValue("Comentario", comentario);
                            SQLScriptToRun.Parameters.AddWithValue("Solicitante", dados["Solicitante"]);
                            SQLScriptToRun.Parameters.AddWithValue("CnpjCpf", dados["CnpjCpf"]);
                            SQLScriptToRun.Parameters.AddWithValue("Email", dados["Email"]);
                            SQLScriptToRun.Parameters.AddWithValue("Cidade", dados["Cidade"]);
                            SQLScriptToRun.Parameters.AddWithValue("AC", dados["AC"]);
                            SQLScriptToRun.Parameters.AddWithValue("AR", dados["AR"]);
                            SQLScriptToRun.Parameters.AddWithValue("ProtocoloCD", dados["ProtocoloCD"]);
                            SQLScriptToRun.Parameters.AddWithValue("Modelo", dados["Modelo"]);
                            SQLScriptToRun.Parameters.AddWithValue("Tipo", dados["Tipo"]);
                            SQLScriptToRun.Parameters.AddWithValue("Certificado", dados["Certificado"]);
                            SQLScriptToRun.Parameters.AddWithValue("Atendimento", dados["Atendimento"]);
                            SQLScriptToRun.Parameters.AddWithValue("TipoEmissao", dados["TipoEmissao"]);
                            SQLScriptToRun.Parameters.AddWithValue("Duvida", dados["Duvida"]);
                            SQLScriptToRun.Parameters.AddWithValue("MotivoContato", dados["MotivoContato"]);
                            SQLScriptToRun.Parameters.AddWithValue("DescricaoProblema", dados["DescricaoProblema"]);
                            SQLScriptToRun.Parameters.AddWithValue("Posicionamento", dados["Posicionamento"]);
                            SQLScriptToRun.Parameters.AddWithValue("Ambiente", dados["Ambiente"]);
                            SQLScriptToRun.Parameters.AddWithValue("Sistema", dados["Sistema"]);
                            SQLScriptToRun.Parameters.AddWithValue("IP", dados["IP"]);
                            SQLScriptToRun.Parameters.AddWithValue("Browser", dados["Browser"]);
                            SQLScriptToRun.Parameters.AddWithValue("Plataforma", dados["Plataforma"]);
                            SQLScriptToRun.Parameters.AddWithValue("LogErro", dados["LogErro"]);

                            rows += SQLScriptToRun.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            SQLScriptToRun.Parameters.Clear();

                            SQLScriptToRun.Parameters.AddWithValue("DataRegistro", data);
                            SQLScriptToRun.Parameters.AddWithValue("Protocolo", protocolo);
                            SQLScriptToRun.Parameters.AddWithValue("Setor", dados["Setor"]);
                            SQLScriptToRun.Parameters.AddWithValue("AtendimentoDuracao", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("EntrouFila", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("AtendimentoInicio", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("AtendimentoFinal", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Operador", dados["Operador"]);
                            SQLScriptToRun.Parameters.AddWithValue("SolicitacaoAtendida", solicitacaoAtendida);
                            SQLScriptToRun.Parameters.AddWithValue("Nota", nota);
                            SQLScriptToRun.Parameters.AddWithValue("Comentario", comentario);
                            SQLScriptToRun.Parameters.AddWithValue("Solicitante", dados["Solicitante"]);
                            SQLScriptToRun.Parameters.AddWithValue("CnpjCpf", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Email", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Cidade", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("AC", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("AR", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("ProtocoloCD", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Modelo", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Tipo", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Certificado", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Atendimento", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("TipoEmissao", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Duvida", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("MotivoContato", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("DescricaoProblema", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Posicionamento", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Ambiente", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Sistema", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("IP", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Browser", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("Plataforma", DBNull.Value);
                            SQLScriptToRun.Parameters.AddWithValue("LogErro", ex.Message);

                            rows += SQLScriptToRun.ExecuteNonQuery();
                        }

                        sqlConn.Close();
                        SQLScriptToRun.Dispose();
                        ProtocoloEx = "";
                    }
                }

                return "Feito. Linhas afetadas: " + rows.ToString();
            }
            catch (Exception ex)
            {
                return "Erro ao enviar os dados. " + ex.Message + "\nProtocolo (caso tenha): " + ProtocoloEx;
            }
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }

        public static Dictionary<string, dynamic> ObterDados(string protocolo, string sigla, string serviceKey)
        {
            string solicitante;
            string setor;
            string operador;
            string email;
            dynamic protocoloCD;
            dynamic autoridadeRegistro;
            dynamic cidade;
            dynamic autoridadeCertificadora;
            dynamic cnpjCpf;
            dynamic ip;
            dynamic browser;
            dynamic plataforma;
            dynamic posicionamento;
            dynamic duvida;
            dynamic dataHorario;
            dynamic descricaoProblema;
            dynamic modelo;
            dynamic tipoEmissao;
            dynamic tipo;
            dynamic ambiente;
            dynamic sistema;
            dynamic motivoContato;
            dynamic certificado;
            dynamic atendimento;
            dynamic entrouFila;
            dynamic atendimentoInicio;
            dynamic atendimentoFinal;
            dynamic atendimentoDuracao;

            var dados = new AtendimentoOnlineBrazip.ObterConversaAtendimentoRequestDados()
            {
                idatendimento = protocolo
            };

            var request = new AtendimentoOnlineBrazip.ObterConversaAtendimentoRequest()
            {
                sigla = sigla,
                servicekey = serviceKey,
                dados = dados
            };

            try
            {
                var result = ServicoAtendimentoOnline.ObterConversaAtendimento(request);

                int inicio, final;
                string index;

                //Solicitante
                inicio = result.dados[0].mensagensHtml.IndexOf(">") + 1;
                final = result.dados[0].mensagensHtml.IndexOf("</") - inicio;

                if (final < 0) solicitante = "Não identificado";
                else solicitante = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Setor
                index = "Setor: <b>";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0) setor = "Não identificado";
                else setor = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Operador
                index = "<font color='#0000FF'>";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf(":</font></b>", inicio) - inicio;

                if (final < 0) operador = "Não identificado";
                else
                {
                    var retorno = result.dados[0].mensagensHtml.Substring(inicio, final);
                    if (retorno.Contains("Protocolo:")) operador = "Não identificado";
                    else operador = retorno;
                }

                //Email
                index = "</b><br>Email: <b>";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b><br>", inicio) - inicio;

                if (final < 0) email = "Não identificado";
                else email = result.dados[0].mensagensHtml.Substring(inicio, final);

                //ProtocoloCD
                index = "</b><br><b>Protocolo: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b><br>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) protocoloCD = DBNull.Value;
                else protocoloCD = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Autoridade de Registro
                if (result.dados[0].mensagensHtml.Contains("Setor: <b>Suporte documental interno"))
                {
                    index = "AR: ";
                    inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                    final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                    if (final < 0 || inicio <= index.Length) autoridadeRegistro = DBNull.Value;
                    else autoridadeRegistro = result.dados[0].mensagensHtml.Substring(inicio, final);
                }
                else
                {
                    index = "<b>Autoridade de Registro: ";
                    inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                    final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                    if (final < 0 || inicio <= index.Length) autoridadeRegistro = DBNull.Value;
                    else autoridadeRegistro = result.dados[0].mensagensHtml.Substring(inicio, final);
                }

                //Cidade
                index = "<br><b>Cidade: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) cidade = DBNull.Value;
                else cidade = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Autoridade Certificadora
                index = "<b>AC: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) autoridadeCertificadora = DBNull.Value;
                else autoridadeCertificadora = result.dados[0].mensagensHtml.Substring(inicio, final);

                //CNPJ/CPF
                index = "<b>CNPJ/CPF: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) cnpjCpf = DBNull.Value;
                else cnpjCpf = result.dados[0].mensagensHtml.Substring(inicio, final);

                //IP
                index = "<br>IP: <b>";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) ip = DBNull.Value;
                else ip = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Browser
                index = "<br>Browser: <b>";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) browser = DBNull.Value;
                else browser = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Plataforma
                index = "<br>Plataforma: <b>";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) plataforma = DBNull.Value;
                else plataforma = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Posicionamento
                index = "<b>Posicionamento: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) posicionamento = DBNull.Value;
                else posicionamento = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Dúvida
                if (result.dados[0].mensagensHtml.Contains("Setor: <b>Suporte documental interno"))
                {
                    index = "<b>Dúvida: ";
                    inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                    final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                    if (final < 0 || inicio <= index.Length) duvida = DBNull.Value;
                    else
                    {
                        var resultado = result.dados[0].mensagensHtml.Substring(inicio, final);

                        if (resultado.Contains("Protocolo:"))
                        {
                            index = "\nDúvida:";
                            inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                            final = result.dados[0].mensagensHtml.IndexOf("\n", inicio) - inicio;

                            if (final < 0 || inicio <= index.Length) duvida = DBNull.Value;
                            else duvida = result.dados[0].mensagensHtml.Substring(inicio, final);
                        }
                        else duvida = resultado;
                    }
                }
                else
                {
                    index = "<b>Qual a sua Dúvida:";
                    inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                    final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                    if (final < 0 || inicio <= index.Length) duvida = DBNull.Value;
                    else
                    {
                        var resultado = result.dados[0].mensagensHtml.Substring(inicio, final);

                        if (resultado.Contains("Protocolo:")) duvida = DBNull.Value;
                        else duvida = resultado;
                    }
                }

                //Data/horário
                index = "<b>Data/horário: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) dataHorario = DBNull.Value;
                else dataHorario = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Descrição do Problema
                index = "<b>Descrição do Problema: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) descricaoProblema = DBNull.Value;
                else descricaoProblema = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Modelo
                index = "<b>Modelo: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) modelo = DBNull.Value;
                else
                {
                    var numero = result.dados[0].mensagensHtml.Substring(inicio, final);

                    if (numero == "1") numero = "e-CPF";
                    else if (numero == "2") numero = "e-CNPJ";

                    modelo = numero;
                }

                //Tipo de Emissão
                index = "<b>Tipo de Emissão: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) tipoEmissao = DBNull.Value;
                else
                {
                    var numero = result.dados[0].mensagensHtml.Substring(inicio, final);

                    if (numero == "1") numero = "Presencial";
                    else if (numero == "2") numero = "Videoconferência";
                    else if (numero == "3") numero = "Renovação online";

                    tipoEmissao = numero;
                }

                //Tipo
                index = "<b>Tipo: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) tipo = DBNull.Value;
                else
                {
                    var numero = result.dados[0].mensagensHtml.Substring(inicio, final);

                    if (numero == "1") numero = "A1";
                    else if (numero == "2") numero = "A3";

                    tipo = numero;
                }

                //Ambiente
                index = "<b>Ambiente: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) ambiente = DBNull.Value;
                else
                {
                    var numero = result.dados[0].mensagensHtml.Substring(inicio, final);

                    if (result.dados[0].mensagensHtml.Contains("Setor: <b>Suporte AC"))
                    {
                        if (numero == "1") numero = "Produção";
                        else if (numero == "2") numero = "Treinamento";

                        ambiente = numero;
                    }
                    else if (result.dados[0].mensagensHtml.Contains("Setor: <b>Suporte PSS"))
                    {
                        if (numero == "1") numero = "Produção";
                        else if (numero == "2") numero = "Homologação";

                        ambiente = numero;
                    }
                    else ambiente = DBNull.Value;
                }

                //Sistema
                index = "<b>Sistema: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) sistema = DBNull.Value;
                else
                {
                    var numero = result.dados[0].mensagensHtml.Substring(inicio, final);

                    if (result.dados[0].mensagensHtml.Contains("Setor: <b>Suporte AC"))
                    {
                        if (numero == "1") numero = "GEDAR AVP";
                        else if (numero == "2") numero = "GEDAR ACI";
                        else if (numero == "3") numero = "HOPE";
                        else if (numero == "4") numero = "Gerenciamento AC";
                        else if (numero == "5") numero = "Gestão AC";
                        else if (numero == "6") numero = "Outros";

                        sistema = numero;
                    }
                    else if (result.dados[0].mensagensHtml.Contains("Setor: <b>Suporte PSS"))
                    {
                        if (numero == "1") numero = "GEDAR AVP";
                        else if (numero == "2") numero = "GEDAR ACI";
                        else if (numero == "3") numero = "Gerenciamento AC";
                        else if (numero == "4") numero = "Gestão AC";
                        else if (numero == "5") numero = "HOPE";
                        else if (numero == "6") numero = "Monitor de Revogação";
                        else if (numero == "7") numero = "CD em Nuvem";
                        else if (numero == "8") numero = "Outros";

                        sistema = numero;
                    }
                    else if (result.dados[0].mensagensHtml.Contains("Setor: <b>Suporte Técnico ao Cliente"))
                    {
                        if (numero == "1") numero = "NF-e";
                        else if (numero == "2") numero = "CT-e";
                        else if (numero == "3") numero = "NFS-e";
                        else if (numero == "4") numero = "CT-OS";
                        else if (numero == "5") numero = "MDF-e";
                        else if (numero == "6") numero = "NFC-e";
                        else if (numero == "7") numero = "eNota E";
                        else if (numero == "8") numero = "eNota G";
                        else if (numero == "9") numero = "eNota C";
                        else if (numero == "10") numero = "Safe Agro";
                        else if (numero == "11") numero = "eNota R";
                        else if (numero == "12") numero = "Outros";

                        sistema = numero;
                    }
                    else sistema = DBNull.Value;
                }

                //Motivo do Contato
                if (result.dados[0].mensagensHtml.Contains("Setor: <b>Suporte AC"))
                {
                    index = "<b>Motivo do seu Contato: ";
                    inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                    final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                    if (final < 0 || inicio <= index.Length) motivoContato = DBNull.Value;
                    else
                    {
                        var numero = result.dados[0].mensagensHtml.Substring(inicio, final);

                        if (numero == "1") numero = "Erro";
                        else if (numero == "2") numero = "Dúvida";
                        else if (numero == "3") numero = "Solicitação de serviço";

                        motivoContato = numero;
                    }
                }
                else if (result.dados[0].mensagensHtml.Contains("Setor: <b>Suporte PSS"))
                {
                    index = "<b>Motivo do Contato: ";
                    inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                    final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                    if (final < 0 || inicio <= index.Length) motivoContato = DBNull.Value;
                    else
                    {
                        var numero = result.dados[0].mensagensHtml.Substring(inicio, final);

                        if (numero == "1") numero = "Erro";
                        else if (numero == "2") numero = "Solicitação de Serviço";
                        else if (numero == "3") numero = "Solicitação de Melhoria";

                        motivoContato = numero;
                    }
                }
                else motivoContato = DBNull.Value;

                //Certificado
                index = "<b>Certificado: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) certificado = DBNull.Value;
                else
                {
                    var numero = result.dados[0].mensagensHtml.Substring(inicio, final);

                    if (numero == "1") numero = "A1";
                    else if (numero == "2") numero = "A3";

                    certificado = numero;
                }

                //Atendimento
                index = "<b>Atendimento: ";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) atendimento = DBNull.Value;
                else
                {
                    var numero = result.dados[0].mensagensHtml.Substring(inicio, final);

                    if (numero == "1") numero = "Presencial";
                    else if (numero == "2") numero = "Videoconferência";

                    atendimento = numero;
                }

                //Entrou na Fila
                index = "Entrou na fila: <b>";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</b>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) entrouFila = DBNull.Value;
                else entrouFila = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Atendimento Iniciou
                index = "<br><br><font color='#808080'>";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) + index.Length;
                final = result.dados[0].mensagensHtml.IndexOf("</font>", inicio) - inicio;

                if (final < 0 || inicio <= index.Length) atendimentoInicio = DBNull.Value;
                else atendimentoInicio = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Atendimento Finalizou
                index = "</font> <b> <font color='#FF0000'>AVISO: </font></b>";
                inicio = result.dados[0].mensagensHtml.IndexOf(index) - 5;
                final = 5;

                if (final < 0 || inicio <= index.Length) atendimentoFinal = DBNull.Value;
                else atendimentoFinal = result.dados[0].mensagensHtml.Substring(inicio, final);

                //Duração Atendimento
                if (atendimentoFinal.ToString() == DBNull.Value.ToString() || atendimentoInicio.ToString() == DBNull.Value.ToString()) atendimentoDuracao = DBNull.Value;
                else atendimentoDuracao = Convert.ToDateTime(atendimentoFinal) - Convert.ToDateTime(atendimentoInicio);

                return new Dictionary<string, dynamic>
                {
                    { "Solicitante", solicitante },
                    { "Setor", setor },
                    { "Operador", operador },
                    { "Email", email },
                    { "ProtocoloCD", protocoloCD },
                    { "AR", autoridadeRegistro },
                    { "Cidade", cidade },
                    { "AC", autoridadeCertificadora },
                    { "CnpjCpf", cnpjCpf },
                    { "IP", ip },
                    { "Browser", browser },
                    { "Plataforma", plataforma },
                    { "Posicionamento", posicionamento },
                    { "Duvida", duvida },
                    { "DataHorario", dataHorario },
                    { "DescricaoProblema", descricaoProblema },
                    { "Modelo", modelo },
                    { "TipoEmissao", tipoEmissao },
                    { "Tipo", tipo },
                    { "Ambiente", ambiente },
                    { "Sistema", sistema },
                    { "MotivoContato", motivoContato },
                    { "Certificado", certificado },
                    { "Atendimento", atendimento },
                    { "EntrouFila", entrouFila },
                    { "AtendimentoInicio", atendimentoInicio },
                    { "AtendimentoFinal", atendimentoFinal },
                    { "AtendimentoDuracao", atendimentoDuracao },
                    { "LogErro", DBNull.Value }
                };
            }
            catch (Exception)
            {
                return new Dictionary<string, dynamic>
                {
                    { "Solicitante", DBNull.Value },
                    { "Setor", DBNull.Value },
                    { "Operador", DBNull.Value },
                    { "Email", DBNull.Value },
                    { "ProtocoloCD", DBNull.Value },
                    { "AR", DBNull.Value },
                    { "Cidade", DBNull.Value },
                    { "AC", DBNull.Value },
                    { "CnpjCpf", DBNull.Value },
                    { "IP", DBNull.Value },
                    { "Browser", DBNull.Value },
                    { "Plataforma", DBNull.Value },
                    { "Posicionamento", DBNull.Value },
                    { "Duvida", DBNull.Value },
                    { "DataHorario", DBNull.Value },
                    { "DescricaoProblema", DBNull.Value },
                    { "Modelo", DBNull.Value },
                    { "TipoEmissao", DBNull.Value },
                    { "Tipo", DBNull.Value },
                    { "Ambiente", DBNull.Value },
                    { "Sistema", DBNull.Value },
                    { "MotivoContato", DBNull.Value },
                    { "Certificado", DBNull.Value },
                    { "Atendimento", DBNull.Value },
                    { "EntrouFila", DBNull.Value },
                    { "AtendimentoInicio", DBNull.Value },
                    { "AtendimentoFinal", DBNull.Value },
                    { "AtendimentoDuracao", DBNull.Value },
                    { "LogErro", "Erro na API" }
                };
            }
        }
    }
}
