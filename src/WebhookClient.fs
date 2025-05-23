namespace Webhook

open System
open System.Net.Http
open System.Text
open System.Text.Json
open System.Threading.Tasks


/// Módulo responsável por enviar confirmações ou cancelamentos de transações
/// a um endpoint externo, via HTTP POST com payload JSON.
module WebhookClient = 

    /// Cliente HTTP reutilizável para chamadas externas.
    let client = new HttpClient()

    /// Envia um objeto serializado como JSON para a URL especificada.
    /// <param name="url">Endpoint completo para envio do JSON.</param>
    /// <param name="payload">Objeto que será serializado em JSON no corpo da requisição.</param>
    /// <returns>Task<HttpResponseMessage> representando a resposta HTTP.</returns>
    let private postJsonAsync (url: string) (payload: obj) : Task<HttpResponseMessage> =
        let json = JsonSerializer.Serialize(payload)
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        client.PostAsync(url, content)

    /// Envia confirmação de transação ao endpoint `/confirmar`.
    /// <param name="transactionId">ID da transação a ser confirmada.</param>
    /// <returns>Task<HttpResponseMessage> com o resultado do POST.</returns>
    let postConfirm (transactionId: string) : Task<HttpResponseMessage> =
        postJsonAsync "http://localhost:5001/confirmar" {| transaction_id = transactionId |}

    /// Envia cancelamento de transação ao endpoint `/cancelar`.
    /// <param name="transactionId">ID da transação a ser cancelada.</param>
    /// <returns>Task<HttpResponseMessage> com o resultado do POST.</returns>
    let postCancel (transactionId: string) : Task<HttpResponseMessage> =
        postJsonAsync "http://localhost:5001/cancelar" {| transaction_id = transactionId |}