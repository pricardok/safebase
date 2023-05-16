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

                    var connectionString = @"Data Source=TIRADOR;Initial Catalog=testes;User ID=s_coleta_dw;Password=0w4iprxw0D";

                    foreach (var avaliacao in result.dados)
                    {
                        SqlConnection sqlConn = new SqlConnection();
                        sqlConn.ConnectionString = connectionString;

                        var protocolo = avaliacao.bzpcodigoreferencia;
                        var setor = ObterSetor(protocolo, sigla, serviceKey);
                        var operador = ObterOperador(protocolo, sigla, serviceKey);

                        string solicitacaoAtendida;
                        if (avaliacao.solicitacaoatendida == 0) solicitacaoAtendida = "N/I";
                        else if (avaliacao.solicitacaoatendida == 1) solicitacaoAtendida = "Sim";
                        else solicitacaoAtendida = "Não";

                        var nota = avaliacao.avaliacao;
                        var comentario = avaliacao.comentario;
                        var solicitante = ObterSolicitante(protocolo, sigla, serviceKey);
                        var dataRegistro = avaliacao.bzpdata;

                        if (sqlConn.State == ConnectionState.Closed)
                            sqlConn.Open();
                        
                        var sqlQuery = @"INSERT INTO [StagingPB] 
           ([Protocolo]
           ,[Setor]
           ,[Operador]
           ,[SolicitacaoAtendida]
           ,[Nota]
           ,[Comentario]
           ,[Solicitante]
           ,[DataRegistro])
     VALUES
           (@Protocolo
           ,@Setor
           ,@Operador
           ,@SolicitacaoAtendida
           ,@Nota
           ,@Comentario
           ,@Solicitante
           ,@DataRegistro)";

                        var data = Convert.ToDateTime(dataRegistro).ToString("dd-MM-yyyy HH:mm:ss");

                        SqlCommand SQLScriptToRun = new SqlCommand(sqlQuery, sqlConn);
                        SQLScriptToRun.Parameters.AddWithValue("Protocolo", protocolo);
                        SQLScriptToRun.Parameters.AddWithValue("Setor", setor);
                        SQLScriptToRun.Parameters.AddWithValue("Operador", operador);
                        SQLScriptToRun.Parameters.AddWithValue("SolicitacaoAtendida", solicitacaoAtendida);
                        SQLScriptToRun.Parameters.AddWithValue("Nota", nota);
                        SQLScriptToRun.Parameters.AddWithValue("Comentario", comentario);
                        SQLScriptToRun.Parameters.AddWithValue("Solicitante", solicitante);
                        SQLScriptToRun.Parameters.AddWithValue("DataRegistro", data);

                        rows += SQLScriptToRun.ExecuteNonQuery();

                        sqlConn.Close();
                        SQLScriptToRun.Dispose();
                    }
                }

                return "Feito. Linhas afetadas: " + rows.ToString();
            }
            catch (Exception ex)
            {
                return "Erro ao enviar os dados. " + ex.Message;
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

        public static string ObterSolicitante(string protocolo, string sigla, string serviceKey)
        {
            var servico = new AtendimentoOnlineBrazip.AtendimentoOnlinePortClient();

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

            var result = servico.ObterConversaAtendimento(request);

            int inicioNome = result.dados[0].mensagensHtml.IndexOf(">") + 1;
            int finalNome = result.dados[0].mensagensHtml.IndexOf("</") - inicioNome;

            if (finalNome < 0) return "Não identificado";

            return result.dados[0].mensagensHtml.Substring(inicioNome, finalNome);
        }

        public static string ObterSetor(string protocolo, string sigla, string serviceKey)
        {
            var servico = new AtendimentoOnlineBrazip.AtendimentoOnlinePortClient();

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

            var result = servico.ObterConversaAtendimento(request);

            var setor = "Setor: <b>";
            int inicioSetor = result.dados[0].mensagensHtml.IndexOf(setor) + setor.Length;
            int finalSetor = result.dados[0].mensagensHtml.IndexOf("</b>", inicioSetor) - inicioSetor;

            if (finalSetor < 0) return "Não identificado";

            return result.dados[0].mensagensHtml.Substring(inicioSetor, finalSetor);
        }

        public static string ObterOperador(string protocolo, string sigla, string serviceKey)
        {
            var servico = new AtendimentoOnlineBrazip.AtendimentoOnlinePortClient();

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

            var result = servico.ObterConversaAtendimento(request);

            var operador = "<font color='#0000FF'>";
            int inicioOperador = result.dados[0].mensagensHtml.IndexOf(operador) + operador.Length;
            int finalOperador = result.dados[0].mensagensHtml.IndexOf(":</font></b>", inicioOperador) - inicioOperador;

            if (finalOperador < 0) return "Não identificado";

            var retorno = result.dados[0].mensagensHtml.Substring(inicioOperador, finalOperador);

            if (retorno.Contains("Protocolo:")) return "Não identificado";

            return retorno;
        }
    }
}
