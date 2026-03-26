export interface BackgroundJobAdminItemResponse {
  jobId: string;
  nome: string;
  intervaloSegundos?: number;
  habilitado: boolean;
}

export interface BackgroundJobFilaItemResponse {
  execucaoId: string;
  jobId: string;
  nomeJob: string;
  status: string;
  tentativasProcessamento: number;
  registrosProcessados: number;
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
