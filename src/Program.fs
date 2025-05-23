namespace Webhook

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open Webhook.Db
open Webhook.WebhookHandler
open DotNetEnv

/// Ponto de entrada da aplicação WebhookFSharp.
/// Carrega variáveis de ambiente, configura o token secreto,
/// prepara o banco de dados e expõe o endpoint `/webhook`.
module Program =

  [<EntryPoint>]
  let main _ =
    // Carrega variáveis definidas em .env na raíz do projeto
    Env.Load()

    // Lê o token secreto do ambiente para validação de segurança
    let secret =
      match Environment.GetEnvironmentVariable "WEBHOOK_SECRET_TOKEN" with
      | null | "" -> failwith ".env: WEBHOOK_SECRET_TOKEN not defined"
      | s -> s

    // Injeta o token no módulo de validação
    Validation.expectedToken <- secret

    // Garante que a tabela de transações processadas exista antes de iniciar
    Db.ensureTable()

    // Cria e configura o servidor web (Kestrel) usando as definições padrão
    let builder = WebApplication.CreateBuilder()
    let app = builder.Build()

    /// Mapeia POST /webhook para o handler principal, que:
    /// 1) Valida token,
    /// 2) Lê e valida o payload,
    /// 3) Checa idempotência no DB,
    /// 4) Chama confirmação ou cancelamento externo.
    app.MapPost("/webhook",Func<HttpContext, Task<IResult>>(fun ctx -> WebhookHandler.webhookHandler ctx)) |> ignore

    // Inicia o servidor na porta 5000 (HTTP)
    app.Run("http://localhost:5000")
    0
