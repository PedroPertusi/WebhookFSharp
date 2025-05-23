namespace Webhook

open Microsoft.AspNetCore.Http
open System.Text.Json
open System.Threading.Tasks
open Webhook.Db
open Webhook.WebhookClient
open Webhook.Validation

module WebhookHandler =

    /// Handler principal do webhook
    let webhookHandler (ctx: HttpContext) : Task<IResult> = task {
        // 1. Verifica token de segurança
        if not (validateToken ctx) then
            return Results.Unauthorized()
        else
            use reader = new System.IO.StreamReader(ctx.Request.Body)
            let! body = reader.ReadToEndAsync()

            // 3. Parse e validação do payload
            match parseAndValidate body with
            | None ->
                // 1) Extrai sempre o transaction_id (já existe no JSON)
                let txId =
                    try
                        use doc = JsonDocument.Parse(body)
                        doc.RootElement.GetProperty("transaction_id").GetString()
                    with _ ->
                        "invalid_transaction_id"

                let! _ = postCancel txId
                return Results.StatusCode 400

            | Some payload ->
                // 4. Verifica se já foi processada essa transação
                let! already = hasProcessed payload.transactionId
                if already then
                    // Conflito: já processada
                    return Results.StatusCode 409
                else
                    // 5. Marca no banco
                    let! marked = markProcessed payload.transactionId
                    if marked then
                        // 6. Notifica confirmação
                        let! _ = postConfirm payload.transactionId
                        return Results.Ok()
                    else
                        // Falha interna ao inserir
                        return Results.StatusCode 500
    }