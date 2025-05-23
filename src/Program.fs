namespace Webhook

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open Webhook.Db
open Webhook.WebhookHandler

module Program =

  [<EntryPoint>]
  let main _ =
    Db.ensureTable()

    let builder = WebApplication.CreateBuilder()
    let app     = builder.Build()

    app.MapPost("/webhook",Func<HttpContext, Task<IResult>>(fun ctx -> WebhookHandler.webhookHandler ctx)) |> ignore

    app.Run("http://localhost:5000")
    0
