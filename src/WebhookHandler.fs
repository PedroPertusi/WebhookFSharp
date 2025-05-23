namespace Webhook

open Microsoft.AspNetCore.Http
open System.Text.Json
open System.Threading.Tasks
open Webhook.Db
open Webhook.WebhookClient
open Webhook.Validation

/// Módulo que agrupa o handler principal para processar chamadas ao endpoint `/webhook`.
/// Realiza validação de token, parsing e validação de payload, controle de idempotência
/// via banco de dados e notificação de confirmação ou cancelamento externos.
module WebhookHandler =
    /// Handler assíncrono para requisições POST em `/webhook`.
    /// <param name="ctx">Contexto HTTP da requisição recebida.</param>
    /// <returns>
    ///   Task<IResult> que produz:
    ///   - 401 Unauthorized se o token for inválido,
    ///   - 400 Bad Request se o payload falhar na validação (envia cancelamento),
    ///   - 409 Conflict se a transação já tiver sido processada,
    ///   - 200 OK se confirmar com sucesso,
    ///   - 500 Internal Server Error em caso de falha de persistência.
    /// </returns>
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