# WebhookFSharp

Aluno: Pedro Pertusi

## Descrição do Projeto

WebhookFSharp é um serviço mínimo escrito em F# e ASP.NET Core (.NET 9) para receber notificações via POST em `/webhook`, validar e processar transações de pagamento, persistindo em SQLite e confirmando ou cancelando as transações em um endpoint externo. Suporta HTTPS em desenvolvimento usando certificados gerados pelo `dotnet dev-certs`.

### Funcionalidades

* **Validação de segurança**: verifica header `X-Webhook-Token` contra um token secreto.
* **Parsing e validação** do JSON recebido, com logs detalhados (parse + regras de negócio).
* **Persistência** em banco SQLite (`webhook.db`), garantindo idempotência.
* **Confirmação/Cancelamento**: chama external HTTP API (`/confirmar` ou `/cancelar`) via `WebhookClient`.
* **HTTPS**: servidor Kestrel configurado para ouvir em `https://localhost:5001` com certificado de desenvolvimento.

## Pré-requisitos

* [.NET 9 SDK](https://dotnet.microsoft.com/download)
* F# 7.0+ (incluído no SDK)
* (Opcional) Cliente HTTP (curl, Postman etc.)

## Dependências NuGet

```bash
dotnet add package Microsoft.Data.Sqlite --version 9.0.5
dotnet add package FSharp.Control.Tasks.V2 --version 2.2.0
```

*O restante (ASP.NET Core, System.Text.Json) já vem com o SDK Web.*

## Estrutura de Arquivos

```
WebhookFSharp/
│  WebhookFSharp.fsproj
│  README.md
│
├─Payload.fs          # Model do JSON de entrada
├─Validation.fs       # Parsing e regras de negócio
├─WebhookClient.fs    # HTTP client para confirmar/cancelar
├─Db.fs               # Acesso a SQLite (hasProcessed, markProcessed)
├─WebhookHandler.fs   # Lógica principal do endpoint /webhook
└─Program.fs          # Configuração do ASP.NET Core + Kestrel HTTPS
```

## Instalação e Execução

1. **Clone o repositório**

   ```bash
   ```

git clone <repo-url>
cd WebhookFSharp

````
2. **Instale dependências**
   ```bash
dotnet restore
````

3. **Configure certificado HTTPS (desenvolvimento)**

   ```bash
   ```

dotnet dev-certs https --trust

````
4. **Compile e execute**
   ```bash
dotnet build
dotnet run
````

O serviço ficará disponível em `https://localhost:5001`.

## Testando o Endpoint

Envie um POST para `/webhook` incluindo o token e payload JSON:

```bash
curl -k https://localhost:5001/webhook \
  -H "X-Webhook-Token: meu-token-secreto" \
  -H "Content-Type: application/json" \
  -d '{
    "event": "payment_success",
    "transaction_id": "tx123",
    "amount": 49.90,
    "currency": "BRL",
    "timestamp": "2025-05-23T15:00:00Z"
  }'
```

Resposta esperada: `200 OK` e logs no console detalhando cada etapa.

## Configurações Extras

* Token secreto em `Validation.expectedToken`.
* Nome e local do arquivo SQLite em `Db.connectionString`.
* URLs de confirmação/cancelamento em `WebhookClient.postConfirm` e `postCancel`.

## Licença

MIT © Seu Nome
