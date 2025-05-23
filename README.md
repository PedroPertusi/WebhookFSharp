# WebhookFSharp

Aluno: Pedro Pertusi

## Descrição do Projeto

WebhookFSharp é um serviço mínimo escrito em F# e ASP.NET Core (.NET 9) para receber notificações via POST em `/webhook`, validar e processar transações de pagamento, persistindo em SQLite e confirmando ou cancelando as transações em um endpoint externo.

## Funcionalidades Implementadas

- **O serviço deve verificar a integridade do payload**
- **O serviço deve implementar algum mecanismo de veracidade da transação**
- **O serviço deve cancelar a transação em caso de divergência**
- **O serviço deve confirmar a transação em caso de sucesso**
- **O serviço deve persistir a transação em um BD**

A unica funcionalidade **não** implementada foi implementar o **serviço em HTTPS.** 

## Instalação do Projeto

### Pré-requisitos 

* [.NET 9 SDK](https://dotnet.microsoft.com/download)
* F# 7.0+ (incluído no SDK)
* (Opcional) Cliente HTTP (curl, Postman etc.)

### Clonando o Repositório

```bash
git clone https://github.com/PedroPertusi/WebhookFSharp.git
cd WebhookFSharp
```

### Dependências a serem instaladas

```bash
dotnet add package DotNetEnv --version 3.1.1
dotnet add package Microsoft.Data.Sqlite --version 9.0.5
```

*O restante (ASP.NET Core, System.Text.Json) já vem com o SDK Web.*

### Configurando o Token 

Um .env com o token secreto deve ser criado na raiz do projeto para permitir sua validação. O arquivo deve conter a seguinte linha:

```env
WEBHOOK_SECRET_TOKEN="meu-token-secreto"
```

### Buildando e Executando o projeto

```bash
dotnet build
dotnet run
```

Isso irá compilar e executar o projeto, que estará disponível em `http://localhost:5000/webhook`. Além disso, quando executado o projeto irá criar um banco de dados SQLite chamado `webhook.db` na raiz do projeto, onde as transações serão armazenadas.

## Estrutura de Arquivos

```
WebhookFSharp/
│  WebhookFSharp.fsproj
│  README.md
|  .env
|  .gitignore
│
├─Payload.fs          # Model do JSON de entrada
├─Validation.fs       # Parsing e regras de negócio
├─WebhookClient.fs    # HTTP client para confirmar/cancelar
├─Db.fs               # Acesso a SQLite
├─WebhookHandler.fs   # Lógica principal do endpoint /webhook
└─Program.fs          # Configuração do ASP.NET Core +
```

## Testando o Endpoint

Envie um POST para `/webhook` incluindo o token e payload JSON:

```bash
curl -k http://localhost:5001/webhook \
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

## Roadmap Futuro

- Implementar HTTPS