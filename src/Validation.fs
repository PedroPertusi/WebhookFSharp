namespace Webhook

open System
open System.Text.Json
open Microsoft.AspNetCore.Http

/// Módulo responsável pelo parsing e validação do payload JSON recebido no webhook.
module Validation =

    /// Tenta desserializar o JSON bruto em um Payload.
    /// <param name="body">String JSON recebida no corpo da requisição.</param>
    /// <returns>Some Payload se o parse for bem-sucedido; None em caso de erro.</returns>
    let parsePayload (body: string) : Payload option =
        try
            JsonSerializer.Deserialize<Payload>(body) |> Some
        with _ ->
            None

    /// Aplica as regras de negócio ao Payload desserializado.
    /// <param name="payload">Objeto Payload a ser validado.</param>
    /// <returns>true se atender a todos os critérios; false caso contrário.</returns>
    let validatePayload (payload: Payload) : bool =
        payload.event = "payment_success"
        && payload.amount > 0.0
        && not (String.IsNullOrWhiteSpace payload.transactionId)
        && not (String.IsNullOrWhiteSpace payload.currency)
        && payload.timestamp > DateTime.MinValue

    /// Combina parse e validação em um único passo.
    /// <param name="body">JSON bruto a ser processado.</param>
    /// <returns>Some Payload se parse e validação forem bem-sucedidos; None caso contrário.</returns>
    let parseAndValidate (body: string) : Payload option =
        let parsed = parsePayload body

        match parsed with
        | Some p ->
            let isValid = validatePayload p
            if isValid then
                Some p
            else
                None

        | None ->
            None

    /// Token esperado para validação do header "X-Webhook-Token".
    /// Deve ser configurado externamente antes de usar validateToken.
    let mutable expectedToken = ""

    /// Verifica se o header de autenticação coincide com o token esperado.
    /// <param name="ctx">Contexto HTTP da requisição.</param>
    /// <returns>true se o header X-Webhook-Token estiver presente e corresponder a expectedToken.</returns>
    let validateToken (ctx: HttpContext) : bool =
        match ctx.Request.Headers.TryGetValue "X-Webhook-Token" with
        | true, values -> values.Count > 0 && values.[0] = expectedToken
        | _ -> false
