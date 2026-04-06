export interface BackgroundJobAdminItemResponse {
  jobId: string;
  nome: string;
  intervaloSegundos?: number;
  habilitado: boolean;
  emExecucao: boolean;
  ultimoStatus: string;
  ultimaExecucaoIniciadaEm?: string | null;
  ultimaExecucaoFinalizadaEm?: string | null;
  ultimosRegistrosProcessados?: number | null;
  totalExecucoes: number;
  totalSucessos: number;
  totalFalhas: number;
  ultimaMensagemErro?: string;
}

export interface BackgroundJobFilaItemResponse {
  execucaoId: string;
  jobId: string;
  nomeJob: string;
  origem: string;
  solicitadoPorAdminUsuarioId?: string | null;
  status: string;
  tentativasProcessamento: number;
  registrosProcessados: number;
  processarAposUtc?: string | null;
  dataInicioProcessamento?: string | null;
  dataFinalizacao?: string | null;
  mensagemResultado?: string;
  dataCriacao: string;
}

export interface BackgroundJobFilaMetricasResponse {
  totalPendentes: number;
  totalProcessando: number;
  totalSucesso: number;
  totalFalhas: number;
  totalCancelados: number;
  tempoMedioFilaSegundos: number;
  tempoMedioProcessamentoSegundos: number;
  tempoMedioFalhaSegundos: number;
  porJob: Record<string, number>;
}

export interface BackgroundJobFilaAlertaResponse {
  jobId: string;
  nivelAlerta: string;
  mensagem: string;
  cor: string;
  totalPendentes: number;
  totalFalhas: number;
  tempoMedioFilaSegundos: number;
  tempoMedioProcessamentoSegundos: number;
}

export interface BackgroundJobFilaAlertasHistoricoResponse {
  jobId: string;
  data: string;
  tempoMedioFilaSegundos: number;
  tempoMedioProcessamentoSegundos: number;
  totalAlertas: number;
  totalPendentes: number;
  totalFalhas: number;
}

export interface BackgroundJobRetryLogResponse {
  execucaoId: string;
  jobId: string;
  tipo: string;
  mensagem: string;
  dataCriacao: string;
}

export interface ProcessarFilaBackgroundJobAdminResponse {
  execucoesProcessadas: number;
  processadoEm: string;
}

export interface PaginacaoResponse<T> {
  paginaAtual: number;
  tamanhoPagina: number;
  totalRegistros: number;
  totalPaginas: number;
  itens: T[];
}

export interface ProfissaoResumoResponse {
  id: string;
  nome: string;
}

export interface EspecialidadeResumoResponse {
  id: string;
  nome: string;
}

export interface AreaAtendimentoResumoResponse {
  cidadeId: string;
  cidadeNome: string;
  uf: string;
  bairroId?: string | null;
  bairroNome?: string | null;
  cidadeInteira: boolean;
}

export interface ProfissaoResponse {
  id: string;
  nome: string;
  slug: string;
}

export interface CidadeResponse {
  id: string;
  estadoId: string;
  nome: string;
  uf: string;
  codigoIbge: string;
}

export interface BairroResponse {
  id: string;
  cidadeId: string;
  nome: string;
}

export interface EspecialidadeResponse {
  id: string;
  profissaoId: string;
  nome: string;
}

export interface PortfolioFotoResponse {
  id: string;
  urlArquivo: string;
  legenda: string;
  ordem: number;
}

export interface UploadPortfolioResponse {
  nomeArquivo: string;
  urlArquivo: string;
}

export type TipoFormaRecebimento = 1 | 2 | 3 | 4;

export interface FormaRecebimentoResponse {
  id: string;
  tipoFormaRecebimento: TipoFormaRecebimento | string;
  descricao: string;
}

export type TipoNotificacao = 1 | 2 | 3 | 4 | 5 | 6;

export interface PreferenciaNotificacaoResponse {
  tipo: TipoNotificacao | string;
  ativoInterno: boolean;
  ativoEmail: boolean;
}

export type NotaAvaliacao = 1 | 2 | 3 | 4 | 5;
export type StatusModeracaoComentario = 1 | 2 | 3 | 4 | string;

export interface CriarAvaliacaoRequest {
  servicoId: string;
  notaAtendimento: NotaAvaliacao;
  notaServico: NotaAvaliacao;
  notaPreco: NotaAvaliacao;
  comentario: string;
}

export interface AvaliacaoResponse {
  id: string;
  clienteId: string;
  profissionalId: string;
  nomeCliente: string;
  notaAtendimento: NotaAvaliacao | string;
  notaServico: NotaAvaliacao | string;
  notaPreco: NotaAvaliacao | string;
  comentario: string;
  statusModeracaoComentario: StatusModeracaoComentario;
  dataCriacao: string;
}

export interface ProfissionalResumoResponse {
  id: string;
  usuarioId: string;
  nomeExibicao: string;
  descricao: string;
  aceitaContatoPeloApp: boolean;
  perfilVerificado: boolean;
  estaImpulsionado: boolean;
  notaMediaAtendimento?: number | null;
  notaMediaServico?: number | null;
  notaMediaPreco?: number | null;
  profissoes: ProfissaoResumoResponse[];
  especialidades: EspecialidadeResumoResponse[];
  areasAtendimento: AreaAtendimentoResumoResponse[];
}

export interface ProfissionalDetalhesResponse {
  id: string;
  usuarioId: string;
  nomeExibicao: string;
  descricao: string;
  whatsApp: string;
  instagram: string;
  facebook: string;
  outraFormaContato: string;
  aceitaContatoPeloApp: boolean;
  perfilVerificado: boolean;
  notaMediaAtendimento?: number | null;
  notaMediaServico?: number | null;
  notaMediaPreco?: number | null;
  estaImpulsionado: boolean;
  profissoes: ProfissaoResumoResponse[];
  especialidades: EspecialidadeResumoResponse[];
  areasAtendimento: AreaAtendimentoResumoResponse[];
  portfolio: PortfolioFotoResponse[];
  formasRecebimento: FormaRecebimentoResponse[];
}

export interface CriarServicoRequest {
  profissionalId: string;
  profissaoId?: string | null;
  especialidadeId?: string | null;
  cidadeId: string;
  bairroId?: string | null;
  titulo: string;
  descricao: string;
  valorCombinado?: number | null;
}

export interface ServicoResponse {
  id: string;
  clienteId: string;
  profissionalId: string;
  nomeCliente: string;
  nomeProfissional: string;
  profissaoId?: string | null;
  nomeProfissao?: string | null;
  especialidadeId?: string | null;
  nomeEspecialidade?: string | null;
  cidadeId: string;
  cidadeNome: string;
  uf: string;
  bairroId?: string | null;
  bairroNome?: string | null;
  titulo: string;
  descricao: string;
  valorCombinado?: number | null;
  status: number | string;
  dataCriacao: string;
  dataAceite?: string | null;
  dataInicio?: string | null;
  dataConclusao?: string | null;
  dataCancelamento?: string | null;
}
