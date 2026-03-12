using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeAjudaAi.Api.Swagger;

public class SwaggerExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath?.TrimEnd('/') ?? string.Empty;
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;

        if (method == "POST" && relativePath == "api/auth/registrar")
        {
            operation.Summary = "Registra cliente ou profissional";
            operation.Description = "Cria um novo usuario e retorna token JWT para uso imediato na API.";
            operation.RequestBody = CriarRequestBodyExemplo(
                "{\n  \"nome\": \"Profissional Swagger\",\n  \"email\": \"profissional.swagger@teste.local\",\n  \"telefone\": \"11999999999\",\n  \"senha\": \"Senha@123\",\n  \"tipoPerfil\": 2\n}");
            operation.Responses["200"] = CriarResponseComExemplo(
                "Usuario registrado com sucesso.",
                CriarAuthResponseExemplo());
            operation.Responses["400"] = CriarResponseComExemplo(
                "Erro de validacao.",
                CriarErroValidacaoExemplo());
        }

        if (method == "POST" && relativePath == "api/auth/login")
        {
            operation.Summary = "Autentica usuario";
            operation.Description = "Valida credenciais e retorna token JWT com expiracao.";
            operation.RequestBody = CriarRequestBodyExemplo(
                "{\n  \"email\": \"admin@meajudaai.local\",\n  \"senha\": \"Admin@123\"\n}");
            operation.Responses["200"] = CriarResponseComExemplo(
                "Login realizado com sucesso.",
                CriarAuthResponseExemplo());
            operation.Responses["401"] = CriarResponseComExemplo(
                "Credenciais invalidas.",
                CriarMensagemExemplo("Email ou senha inválidos."));
        }

        if (method == "POST" && relativePath == "api/servicos")
        {
            operation.Summary = "Cria um servico";
            operation.Description = "Fluxo iniciado pelo cliente para contratar um profissional.";
            operation.RequestBody = CriarRequestBodyExemplo(
                "{\n  \"profissionalId\": \"210b3492-6be1-4737-bea4-530be424e206\",\n  \"profissaoId\": \"f1c1a36c-fde8-44bf-8f4b-efc2a2a6d001\",\n  \"especialidadeId\": \"f1c1a36c-fde8-44bf-8f4b-efc2a2a6d002\",\n  \"cidadeId\": \"d1b364f6-144d-4a79-b61e-687e2c44fcf7\",\n  \"bairroId\": \"d1b364f6-144d-4a79-b61e-687e2c44fcf8\",\n  \"titulo\": \"Troca de tomada da cozinha\",\n  \"descricao\": \"Preciso trocar uma tomada que esta com mau contato\",\n  \"valorCombinado\": 120.00\n}");
            operation.Responses["200"] = CriarResponseComExemplo(
                "Servico criado com sucesso.",
                CriarServicoExemplo(1));
        }

        if (method == "GET" && relativePath == "api/servicos/{servicoId:guid}")
        {
            operation.Summary = "Consulta servico por id";
            operation.Description = "Retorna os dados completos do servico para cliente ou profissional participante.";
            operation.Responses["200"] = CriarResponseComExemplo(
                "Servico encontrado.",
                CriarServicoExemplo(4));
            operation.Responses["404"] = CriarResponseComExemplo(
                "Servico nao encontrado.",
                CriarMensagemExemplo("Serviço não encontrado."));
        }

        if (method == "PUT" && relativePath == "api/servicos/{servicoId:guid}/aceitar")
        {
            operation.Summary = "Aceita um servico";
            operation.Description = "Transicao do servico para aceito pelo profissional.";
            operation.Responses["200"] = CriarResponseComExemplo(
                "Servico aceito.",
                CriarServicoExemplo(2));
        }

        if (method == "PUT" && relativePath == "api/servicos/{servicoId:guid}/iniciar")
        {
            operation.Summary = "Inicia um servico";
            operation.Description = "Transicao do servico para em andamento.";
            operation.Responses["200"] = CriarResponseComExemplo(
                "Servico iniciado.",
                CriarServicoExemplo(3));
        }

        if (method == "PUT" && relativePath == "api/servicos/{servicoId:guid}/concluir")
        {
            operation.Summary = "Conclui um servico";
            operation.Description = "Transicao do servico para concluido, liberando a avaliacao pelo cliente.";
            operation.Responses["200"] = CriarResponseComExemplo(
                "Servico concluido.",
                CriarServicoExemplo(4));
        }

        if (method == "POST" && relativePath == "api/avaliacoes")
        {
            operation.Summary = "Cria uma avaliacao";
            operation.Description = "Permite ao cliente avaliar um servico concluido.";
            operation.RequestBody = CriarRequestBodyExemplo(
                "{\n  \"servicoId\": \"8dce1a1b-b1e7-4d1e-a81d-b97d707d1001\",\n  \"notaAtendimento\": 5,\n  \"notaServico\": 5,\n  \"notaPreco\": 4,\n  \"comentario\": \"Servico muito bom e profissional pontual\"\n}");
            operation.Responses["200"] = CriarResponseComExemplo(
                "Avaliacao criada com sucesso.",
                CriarAvaliacaoExemplo(0));
            operation.Responses["400"] = CriarResponseComExemplo(
                "Erro de regra de negocio ou validacao.",
                CriarMensagemExemplo("O serviço ainda não foi concluído."));
        }

        if (method == "GET" && relativePath == "api/avaliacoes/profissional/{profissionalId:guid}")
        {
            operation.Summary = "Lista avaliacoes publicas do profissional";
            operation.Description = "Retorna apenas comentarios aprovados para exibicao publica.";
            operation.Responses["200"] = CriarResponseComExemplo(
                "Lista de avaliacoes aprovadas.",
                OpenApiAnyFactory.CreateFromJson(
                    $$"""
                    [
                      {{CriarAvaliacaoExemploJson(1)}}
                    ]
                    """));
        }

        if (method == "GET" && relativePath == "api/avaliacoes/pendentes")
        {
            operation.Summary = "Lista avaliacoes pendentes de moderacao";
            operation.Description = "Endpoint administrativo para fila de comentarios aguardando aprovacao ou rejeicao.";
            operation.Responses["200"] = CriarResponseComExemplo(
                "Lista de avaliacoes pendentes.",
                OpenApiAnyFactory.CreateFromJson(
                    $$"""
                    [
                      {{CriarAvaliacaoExemploJson(0)}}
                    ]
                    """));
        }

        if (method == "PUT" && relativePath == "api/avaliacoes/{avaliacaoId:guid}/moderar")
        {
            operation.Summary = "Modera uma avaliacao";
            operation.Description = "Permite aprovar ou rejeitar comentario pelo administrador.";
            operation.RequestBody = CriarRequestBodyExemplo(
                "{\n  \"acao\": 1\n}");
            operation.Responses["200"] = CriarResponseComExemplo(
                "Avaliacao moderada com sucesso.",
                CriarAvaliacaoExemplo(1));
        }

        if (method == "POST" && relativePath == "api/webhooks/pagamentos/impulsionamentos")
        {
            operation.Summary = "Recebe webhook de pagamento do provedor padrao";
            operation.Description = "Valida assinatura HMAC no header X-Webhook-Signature e processa o evento de pagamento por codigo de referencia.";
            operation.RequestBody = CriarRequestBodyExemplo(
                "{\n  \"codigoReferenciaPagamento\": \"ref-001\",\n  \"statusPagamento\": \"pago\",\n  \"eventoExternoId\": \"evt-001\"\n}");
            operation.Responses["200"] = CriarResponseComExemplo(
                "Webhook processado com sucesso.",
                CriarWebhookProcessadoExemplo("padrao"));
            operation.Responses["401"] = CriarResponseComExemplo(
                "Webhook nao autorizado.",
                CriarMensagemExemplo("Webhook não autorizado."));
        }

        if (method == "POST" && relativePath == "api/webhooks/pagamentos/{provedor}/impulsionamentos")
        {
            operation.Summary = "Recebe webhook de pagamento por provedor";
            operation.Description = "Resolve o adaptador do provedor, valida a assinatura no header configurado e normaliza o payload antes do processamento.";
            operation.Parameters ??= new List<OpenApiParameter>();

            var provedorParameter = operation.Parameters.FirstOrDefault(x => x.Name == "provedor");
            if (provedorParameter is not null)
            {
                provedorParameter.Description = "Nome do provedor configurado, por exemplo: asaas.";
                provedorParameter.Example = new OpenApiString("asaas");
            }

            operation.RequestBody = CriarRequestBodyExemplo(
                "{\n  \"id\": \"evt-asaas-001\",\n  \"event\": \"PAYMENT_RECEIVED\",\n  \"payment\": {\n    \"externalReference\": \"ref-asaas-001\",\n    \"status\": \"RECEIVED\"\n  }\n}");
            operation.Responses["200"] = CriarResponseComExemplo(
                "Webhook processado com sucesso.",
                CriarWebhookProcessadoExemplo("asaas"));
            operation.Responses["400"] = CriarResponseComExemplo(
                "Payload invalido.",
                CriarPayloadInvalidoExemplo());
            operation.Responses["401"] = CriarResponseComExemplo(
                "Webhook nao autorizado.",
                CriarMensagemExemplo("Webhook não autorizado."));
        }

        if (method == "GET" && relativePath == "api/impulsionamentos/webhooks")
        {
            operation.Summary = "Consulta historico de webhooks de pagamento";
            operation.Description = "Endpoint administrativo para suporte operacional, com filtros por evento externo, codigo de referencia e provedor.";
            operation.Responses["200"] = CriarResponseComExemplo(
                "Historico paginado de eventos de webhook.",
                CriarHistoricoWebhooksExemplo());
        }

        if (method == "GET" && relativePath == "api/impulsionamentos/webhooks/metricas")
        {
            operation.Summary = "Consulta snapshot de metricas de webhook";
            operation.Description = "Retorna contagens em memoria por provedor, resultado e status recebido desde o boot da aplicacao.";
            operation.Responses["200"] = CriarResponseComExemplo(
                "Snapshot atual de metricas.",
                CriarMetricasWebhooksExemplo());
        }
    }

    private static OpenApiRequestBody CriarRequestBodyExemplo(string rawJson)
    {
        return new OpenApiRequestBody
        {
            Required = true,
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = OpenApiAnyFactory.CreateFromJson(rawJson)
                }
            }
        };
    }

    private static OpenApiResponse CriarResponseComExemplo(string description, IOpenApiAny exemplo)
    {
        return new OpenApiResponse
        {
            Description = description,
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = exemplo
                }
            }
        };
    }

    private static IOpenApiAny CriarWebhookProcessadoExemplo(string provedor)
    {
        return OpenApiAnyFactory.CreateFromJson(
            $$"""
            {
              "provedor": "{{provedor}}",
              "mensagem": "Webhook processado.",
              "eventoExternoId": "evt-001",
              "statusRecebido": "pago",
              "duplicado": false,
              "impulsionamento": {
                "id": "2775b4de-315f-448c-a457-68edd1350ba5",
                "profissionalId": "210b3492-6be1-4737-bea4-530be424e206",
                "planoImpulsionamentoId": "935d7e9c-2bf4-4c57-9daa-2d4df032f6c7",
                "nomePlano": "Impulso 1 Dia",
                "dataInicio": "2026-03-12T16:56:33.88553Z",
                "dataFim": "2026-03-13T16:56:33.88553Z",
                "status": 2,
                "valorPago": 9.90,
                "codigoReferenciaPagamento": "ref-001"
              }
            }
            """);
    }

    private static IOpenApiAny CriarAuthResponseExemplo()
    {
        return OpenApiAnyFactory.CreateFromJson(
            """
            {
              "usuarioId": "210b3492-6be1-4737-bea4-530be424e206",
              "nome": "Profissional Swagger",
              "email": "profissional.swagger@teste.local",
              "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.exemplo",
              "expiraEmUtc": "2026-03-12T21:00:00Z"
            }
            """);
    }

    private static IOpenApiAny CriarServicoExemplo(int status)
    {
        return OpenApiAnyFactory.CreateFromJson(CriarServicoExemploJson(status));
    }

    private static string CriarServicoExemploJson(int status)
    {
        return $$"""
               {
                 "id": "8dce1a1b-b1e7-4d1e-a81d-b97d707d1001",
                 "clienteId": "8dce1a1b-b1e7-4d1e-a81d-b97d707d1002",
                 "profissionalId": "210b3492-6be1-4737-bea4-530be424e206",
                 "nomeCliente": "Cliente Swagger",
                 "nomeProfissional": "Profissional Swagger",
                 "profissaoId": "f1c1a36c-fde8-44bf-8f4b-efc2a2a6d001",
                 "nomeProfissao": "Eletricista",
                 "especialidadeId": "f1c1a36c-fde8-44bf-8f4b-efc2a2a6d002",
                 "nomeEspecialidade": "Instalacao residencial",
                 "cidadeId": "d1b364f6-144d-4a79-b61e-687e2c44fcf7",
                 "cidadeNome": "Sao Paulo",
                 "uf": "SP",
                 "bairroId": "d1b364f6-144d-4a79-b61e-687e2c44fcf8",
                 "bairroNome": "Vila Mariana",
                 "titulo": "Troca de tomada da cozinha",
                 "descricao": "Preciso trocar uma tomada que esta com mau contato",
                 "valorCombinado": 120.00,
                 "status": {{status}},
                 "dataCriacao": "2026-03-12T18:00:00Z",
                 "dataAceite": "2026-03-12T18:05:00Z",
                 "dataInicio": "2026-03-12T18:10:00Z",
                 "dataConclusao": "2026-03-12T18:20:00Z",
                 "dataCancelamento": null
               }
               """;
    }

    private static IOpenApiAny CriarAvaliacaoExemplo(int statusModeracao)
    {
        return OpenApiAnyFactory.CreateFromJson(CriarAvaliacaoExemploJson(statusModeracao));
    }

    private static string CriarAvaliacaoExemploJson(int statusModeracao)
    {
        return $$"""
               {
                 "id": "4d1e8dd0-b127-4c61-9f3c-f97f2b9d0001",
                 "clienteId": "8dce1a1b-b1e7-4d1e-a81d-b97d707d1002",
                 "profissionalId": "210b3492-6be1-4737-bea4-530be424e206",
                 "nomeCliente": "Cliente Swagger",
                 "notaAtendimento": 5,
                 "notaServico": 5,
                 "notaPreco": 4,
                 "comentario": "Servico muito bom e profissional pontual",
                 "statusModeracaoComentario": {{statusModeracao}},
                 "dataCriacao": "2026-03-12T18:30:00Z"
               }
               """;
    }

    private static IOpenApiAny CriarHistoricoWebhooksExemplo()
    {
        return OpenApiAnyFactory.CreateFromJson(
            """
            {
              "paginaAtual": 1,
              "tamanhoPagina": 20,
              "totalRegistros": 1,
              "totalPaginas": 1,
              "itens": [
                {
                  "id": "11111111-2222-3333-4444-555555555555",
                  "provedor": "asaas",
                  "eventoExternoId": "evt-asaas-001",
                  "codigoReferenciaPagamento": "ref-asaas-001",
                  "statusPagamento": "pago",
                  "processadoComSucesso": true,
                  "mensagemResultado": "Webhook processado.",
                  "ipOrigem": "::1",
                  "requestId": "0HNK0BTU7K3J4:00000001",
                  "userAgent": "manual-audit-check/1.0",
                  "impulsionamentoProfissionalId": "2775b4de-315f-448c-a457-68edd1350ba5",
                  "statusImpulsionamentoResultado": 2,
                  "dataCriacao": "2026-03-12T18:00:00Z"
                }
              ]
            }
            """);
    }

    private static IOpenApiAny CriarMetricasWebhooksExemplo()
    {
        return OpenApiAnyFactory.CreateFromJson(
            """
            {
              "itens": [
                {
                  "provedor": "padrao",
                  "resultado": "recebido",
                  "statusRecebido": "pago",
                  "quantidade": 3
                },
                {
                  "provedor": "padrao",
                  "resultado": "processado",
                  "statusRecebido": "pago",
                  "quantidade": 2
                },
                {
                  "provedor": "asaas",
                  "resultado": "duplicado",
                  "statusRecebido": "pago",
                  "quantidade": 1
                }
              ]
            }
            """);
    }

    private static IOpenApiAny CriarErroValidacaoExemplo()
    {
        return OpenApiAnyFactory.CreateFromJson(
            """
            {
              "mensagem": "Erro de validação.",
              "erros": [
                {
                  "campo": "Email",
                  "mensagens": [
                    "Email inválido."
                  ]
                },
                {
                  "campo": "Senha",
                  "mensagens": [
                    "Senha deve possuir no mínimo 6 caracteres."
                  ]
                }
              ]
            }
            """);
    }

    private static IOpenApiAny CriarMensagemExemplo(string mensagem)
    {
        return OpenApiAnyFactory.CreateFromJson($$"""{"mensagem":"{{mensagem}}"}""");
    }

    private static IOpenApiAny CriarPayloadInvalidoExemplo()
    {
        return OpenApiAnyFactory.CreateFromJson(
            """
            {
              "mensagem": "Payload inválido.",
              "erros": [
                "Código de referência do pagamento é obrigatório.",
                "Status do pagamento é obrigatório."
              ]
            }
            """);
    }
}
