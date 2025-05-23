namespace Webhook

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open Webhook.Db
open Webhook.WebhookHandler
open DotNetEnv

module Program =

  [<EntryPoint>]
  let main _ =
    Env.Load()

    let secret =
      match Environment.GetEnvironmentVariable "WEBHOOK_SECRET_TOKEN" with
      | null | "" -> failwith ".env: WEBHOOK_SECRET_TOKEN not defined"
      | s -> s

    Validation.expectedToken <- secret

    Db.ensureTable()

    let builder = WebApplication.CreateBuilder()
    let app     = builder.Build()

    app.MapPost("/webhook",Func<HttpContext, Task<IResult>>(fun ctx -> WebhookHandler.webhookHandler ctx)) |> ignore

    app.Run("http://localhost:5000")
    0
