namespace Webhook

open Microsoft.AspNetCore.Http
open System.Text.Json
open System.Threading.Tasks
open Webhook.Db
open Webhook.WebhookClient
open Webhook.Validation

module WebhookHandler =
    let webhookHandler (ctx: HttpContext) : Task<IResult> = task {
        if not (validateToken ctx) then
            return Results.Unauthorized()
        else
            use reader = new System.IO.StreamReader(ctx.Request.Body)
            let! body = reader.ReadToEndAsync()

            match parseAndValidate body with
            | None ->
                let txId =
                    try
                        use doc = JsonDocument.Parse(body)
                        doc.RootElement.GetProperty("transaction_id").GetString()
                    with _ ->
                        "invalid_transaction_id"

                let! _ = postCancel txId
                return Results.StatusCode 400

            | Some payload ->
                let! already = hasProcessed payload.transactionId
                if already then
                    return Results.StatusCode 409
                else
                    let! marked = markProcessed payload.transactionId
                    if marked then
                        let! _ = postConfirm payload.transactionId
                        return Results.Ok()
                    else
                        return Results.StatusCode 500
    }