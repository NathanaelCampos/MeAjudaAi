'use client';

import { ChangeEvent, FormEvent, useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { apiSend, ApiError } from '@/lib/api';
import { useBairros } from '@/hooks/useBairros';
import { useCidades } from '@/hooks/useCidades';
import { useEspecialidades } from '@/hooks/useEspecialidades';
import { useProfissoes } from '@/hooks/useProfissoes';
import { useAuth } from '@/providers/auth-provider';
import { useToast } from '@/providers/toast-provider';
import { TipoFormaRecebimento, UploadPortfolioResponse } from '@/types/api';

interface AreaDraft {
  id: string;
  cidadeId: string;
  bairroId: string;
  cidadeInteira: boolean;
}

interface PortfolioDraft {
  id: string;
  urlArquivo: string;
  legenda: string;
  ordem: number;
}

interface FormaRecebimentoDraft {
  tipoFormaRecebimento: TipoFormaRecebimento;
  descricao: string;
}

const FORMA_RECEBIMENTO_OPTIONS: Array<{
  tipo: TipoFormaRecebimento;
  titulo: string;
  descricao: string;
  placeholder: string;
}> = [
  {
    tipo: 1,
    titulo: 'Pix',
    descricao: 'Receba por chave Pix ou conta simplificada.',
    placeholder: 'Ex.: Chave Pix telefone, e-mail ou CPF',
  },
  {
    tipo: 2,
    titulo: 'Conta bancária',
    descricao: 'Informe banco, agência, conta ou dados equivalentes.',
    placeholder: 'Ex.: Banco XP, ag. 0001, conta 12345-6',
  },
  {
    tipo: 3,
    titulo: 'Dinheiro',
    descricao: 'Aceita recebimento presencial em espécie.',
    placeholder: 'Ex.: Pagamento no atendimento',
  },
  {
    tipo: 4,
    titulo: 'Cheque',
    descricao: 'Use apenas se esse formato fizer sentido para seu público.',
    placeholder: 'Ex.: Cheque nominal, compensação em 2 dias',
  },
];

function createAreaDraft(): AreaDraft {
  return {
    id: Math.random().toString(36).slice(2, 9),
    cidadeId: '',
    bairroId: '',
    cidadeInteira: true,
  };
}

function createPortfolioDraft(urlArquivo = '', legenda = '', ordem = 0): PortfolioDraft {
  return {
    id: Math.random().toString(36).slice(2, 9),
    urlArquivo,
    legenda,
    ordem,
  };
}

function AreaEditor({
  area,
  cidades,
  onChange,
  onRemove,
  removable,
}: {
  area: AreaDraft;
  cidades: Array<{ id: string; nome: string; uf: string }>;
  onChange: (area: AreaDraft) => void;
  onRemove: () => void;
  removable: boolean;
}) {
  const { data: bairros } = useBairros(area.cidadeId);

  return (
    <div className="rounded-[1.4rem] border border-slate-200 bg-slate-50 p-4">
      <div className="grid gap-3 sm:grid-cols-[1.2fr_1fr_auto]">
        <select
          className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
          value={area.cidadeId}
          onChange={(event) =>
            onChange({
              ...area,
              cidadeId: event.target.value,
              bairroId: '',
            })
          }
        >
          <option value="">Selecione a cidade</option>
          {cidades.map((cidade) => (
            <option key={cidade.id} value={cidade.id}>
              {cidade.nome} - {cidade.uf}
            </option>
          ))}
        </select>

        <select
          className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
          value={area.bairroId}
          disabled={area.cidadeInteira || !area.cidadeId}
          onChange={(event) =>
            onChange({
              ...area,
              bairroId: event.target.value,
            })
          }
        >
          <option value="">{area.cidadeInteira ? 'Cidade inteira' : 'Selecione o bairro'}</option>
          {(bairros ?? []).map((bairro) => (
            <option key={bairro.id} value={bairro.id}>
              {bairro.nome}
            </option>
          ))}
        </select>

        {removable ? (
          <button
            type="button"
            onClick={onRemove}
            className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm font-medium text-rose-700 transition hover:bg-rose-100"
          >
            Remover
          </button>
        ) : (
          <div />
        )}
      </div>

      <label className="mt-3 flex items-center gap-3 text-sm text-slate-700">
        <input
          type="checkbox"
          checked={area.cidadeInteira}
          onChange={(event) =>
            onChange({
              ...area,
              cidadeInteira: event.target.checked,
              bairroId: event.target.checked ? '' : area.bairroId,
            })
          }
        />
        Atendo a cidade inteira
      </label>
    </div>
  );
}

export function ProfessionalOnboardingPageClient() {
  const router = useRouter();
  const { session, hydrated } = useAuth();
  const { showToast } = useToast();
  const { data: profissoes } = useProfissoes();
  const { data: cidades } = useCidades();

  const [nomeExibicao, setNomeExibicao] = useState('');
  const [descricao, setDescricao] = useState('');
  const [whatsApp, setWhatsApp] = useState('');
  const [instagram, setInstagram] = useState('');
  const [facebook, setFacebook] = useState('');
  const [outraFormaContato, setOutraFormaContato] = useState('');
  const [aceitaContatoPeloApp, setAceitaContatoPeloApp] = useState(true);
  const [selectedProfissoes, setSelectedProfissoes] = useState<string[]>([]);
  const [selectedEspecialidades, setSelectedEspecialidades] = useState<string[]>([]);
  const [areas, setAreas] = useState<AreaDraft[]>([createAreaDraft()]);
  const [portfolio, setPortfolio] = useState<PortfolioDraft[]>([]);
  const [formasRecebimento, setFormasRecebimento] = useState<FormaRecebimentoDraft[]>([]);
  const [isSaving, setIsSaving] = useState(false);
  const [isUploadingPortfolio, setIsUploadingPortfolio] = useState(false);

  const { data: especialidades } = useEspecialidades(selectedProfissoes);

  useEffect(() => {
    if (!hydrated) {
      return;
    }

    if (!session?.token) {
      router.replace('/login?next=%2Fonboarding%2Fprofissional');
      return;
    }

    if (session.role && session.role !== 'Profissional') {
      router.replace('/explorar');
      return;
    }

    if (session.nome && !nomeExibicao) {
      setNomeExibicao(session.nome);
    }
  }, [hydrated, nomeExibicao, router, session]);

  useEffect(() => {
    setSelectedEspecialidades((current) =>
      current.filter((id) => (especialidades ?? []).some((item) => item.id === id)),
    );
  }, [especialidades]);

  const canSubmit = useMemo(() => {
    const hasValidArea = areas.some((area) => area.cidadeId && (area.cidadeInteira || area.bairroId));

    return (
      nomeExibicao.trim().length >= 3 &&
      descricao.trim().length >= 20 &&
      selectedProfissoes.length > 0 &&
      hasValidArea
    );
  }, [areas, descricao, nomeExibicao, selectedProfissoes.length]);

  function toggleSelection(list: string[], value: string) {
    return list.includes(value) ? list.filter((item) => item !== value) : [...list, value];
  }

  function toggleFormaRecebimento(tipoFormaRecebimento: TipoFormaRecebimento) {
    setFormasRecebimento((current) => {
      const exists = current.some((item) => item.tipoFormaRecebimento === tipoFormaRecebimento);

      if (exists) {
        return current.filter((item) => item.tipoFormaRecebimento !== tipoFormaRecebimento);
      }

      return [...current, { tipoFormaRecebimento, descricao: '' }];
    });
  }

  function updateFormaRecebimento(tipoFormaRecebimento: TipoFormaRecebimento, descricao: string) {
    setFormasRecebimento((current) =>
      current.map((item) =>
        item.tipoFormaRecebimento === tipoFormaRecebimento ? { ...item, descricao } : item,
      ),
    );
  }

  async function handlePortfolioUpload(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];

    if (!file) {
      return;
    }

    setIsUploadingPortfolio(true);

    try {
      const formData = new FormData();
      formData.append('arquivo', file);

      const response = await apiSend<UploadPortfolioResponse>('/api/profissionais/me/upload-portfolio', {
        method: 'POST',
        body: formData,
      });

      setPortfolio((current) => [
        ...current,
        createPortfolioDraft(response.urlArquivo, file.name.replace(/\.[^.]+$/, ''), current.length),
      ]);

      showToast({
        title: 'Imagem enviada',
        message: 'A foto foi adicionada ao seu portfólio. Ajuste a legenda antes de salvar.',
        variant: 'success',
      });
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Não foi possível enviar a imagem do portfólio.';
      showToast({
        title: 'Falha no upload',
        message,
        variant: 'error',
      });
    } finally {
      setIsUploadingPortfolio(false);
      event.target.value = '';
    }
  }

  function updatePortfolioItem(id: string, patch: Partial<PortfolioDraft>) {
    setPortfolio((current) =>
      current.map((item) => (item.id === id ? { ...item, ...patch } : item)),
    );
  }

  function removePortfolioItem(id: string) {
    setPortfolio((current) =>
      current
        .filter((item) => item.id !== id)
        .map((item, index) => ({
          ...item,
          ordem: index,
        })),
    );
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!canSubmit) {
      showToast({
        title: 'Complete o onboarding',
        message: 'Preencha o perfil, escolha ao menos uma profissão e informe uma área de atendimento.',
        variant: 'error',
      });
      return;
    }

    setIsSaving(true);

    try {
      await apiSend('/api/profissionais/me', {
        method: 'PUT',
        body: {
          nomeExibicao: nomeExibicao.trim(),
          descricao: descricao.trim(),
          whatsApp: whatsApp.trim(),
          instagram: instagram.trim(),
          facebook: facebook.trim(),
          outraFormaContato: outraFormaContato.trim(),
          aceitaContatoPeloApp,
        },
      });

      await apiSend('/api/profissionais/me/profissoes', {
        method: 'PUT',
        body: {
          profissaoIds: selectedProfissoes,
        },
      });

      await apiSend('/api/profissionais/me/especialidades', {
        method: 'PUT',
        body: {
          especialidadeIds: selectedEspecialidades,
        },
      });

      await apiSend('/api/profissionais/me/areas-atendimento', {
        method: 'PUT',
        body: {
          areas: areas
            .filter((area) => area.cidadeId && (area.cidadeInteira || area.bairroId))
            .map((area) => ({
              cidadeId: area.cidadeId,
              bairroId: area.cidadeInteira ? null : area.bairroId || null,
              cidadeInteira: area.cidadeInteira,
            })),
        },
      });

      await apiSend('/api/profissionais/me/portfolio', {
        method: 'PUT',
        body: {
          fotos: portfolio.map((item, index) => ({
            urlArquivo: item.urlArquivo,
            legenda: item.legenda.trim(),
            ordem: index,
          })),
        },
      });

      await apiSend('/api/profissionais/me/formas-recebimento', {
        method: 'PUT',
        body: {
          itens: formasRecebimento.map((item) => ({
            tipoFormaRecebimento: item.tipoFormaRecebimento,
            descricao: item.descricao.trim(),
          })),
        },
      });

      showToast({
        title: 'Onboarding concluído',
        message: 'Seu perfil profissional está pronto para aparecer melhor na busca e receber solicitações com mais contexto.',
        variant: 'success',
      });

      router.replace('/servicos/profissional');
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Não foi possível concluir o onboarding profissional.';
      showToast({
        title: 'Falha ao salvar onboarding',
        message,
        variant: 'error',
      });
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2.2rem] border border-white/80 bg-white/85 p-6 shadow-[0_28px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-500">Onboarding profissional</p>
          <h1 className="mt-2 text-3xl font-semibold text-slate-900">Monte um perfil realmente encontrável</h1>
          <p className="mt-3 max-w-2xl text-sm leading-7 text-slate-600">
            Além das informações básicas, defina profissões, especialidades e áreas onde você atende.
          </p>
        </section>

        <section className="rounded-[2rem] border border-white/80 bg-white/90 p-6 shadow-[0_24px_60px_rgba(15,23,42,0.08)]">
          <form className="space-y-6" onSubmit={handleSubmit}>
            <section className="space-y-4">
              <div>
                <p className="text-sm font-semibold text-slate-900">1. Perfil base</p>
                <p className="mt-1 text-sm text-slate-500">Essas informações aparecem primeiro para o cliente.</p>
              </div>

              <input
                className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                placeholder="Nome de exibição"
                value={nomeExibicao}
                onChange={(event) => setNomeExibicao(event.target.value)}
              />

              <textarea
                className="min-h-[140px] w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                placeholder="Descreva sua experiência, especialidades e diferencial."
                value={descricao}
                onChange={(event) => setDescricao(event.target.value)}
              />

              <div className="grid gap-4 sm:grid-cols-2">
                <input
                  className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                  placeholder="WhatsApp"
                  value={whatsApp}
                  onChange={(event) => setWhatsApp(event.target.value)}
                />
                <input
                  className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                  placeholder="Instagram"
                  value={instagram}
                  onChange={(event) => setInstagram(event.target.value)}
                />
                <input
                  className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                  placeholder="Facebook"
                  value={facebook}
                  onChange={(event) => setFacebook(event.target.value)}
                />
                <input
                  className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                  placeholder="Outro contato"
                  value={outraFormaContato}
                  onChange={(event) => setOutraFormaContato(event.target.value)}
                />
              </div>

              <label className="flex items-center gap-3 rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-700">
                <input
                  type="checkbox"
                  checked={aceitaContatoPeloApp}
                  onChange={(event) => setAceitaContatoPeloApp(event.target.checked)}
                />
                Aceitar contato pelo app
              </label>
            </section>

            <section className="space-y-4">
              <div>
                <p className="text-sm font-semibold text-slate-900">2. Profissões e especialidades</p>
                <p className="mt-1 text-sm text-slate-500">Escolha onde você quer aparecer na busca.</p>
              </div>

              <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                {(profissoes ?? []).map((profissao) => {
                  const selected = selectedProfissoes.includes(profissao.id);

                  return (
                    <button
                      key={profissao.id}
                      type="button"
                      onClick={() => setSelectedProfissoes((current) => toggleSelection(current, profissao.id))}
                      className={`rounded-[1.4rem] border px-4 py-4 text-left transition ${
                        selected
                          ? 'border-slate-900 bg-slate-900 text-white'
                          : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
                      }`}
                    >
                      <p className="text-sm font-semibold">{profissao.nome}</p>
                    </button>
                  );
                })}
              </div>

              {selectedProfissoes.length ? (
                <div className="rounded-[1.6rem] border border-slate-200 bg-slate-50 p-4">
                  <p className="text-sm font-semibold text-slate-900">Especialidades</p>
                  <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                    {(especialidades ?? []).map((especialidade) => {
                      const selected = selectedEspecialidades.includes(especialidade.id);

                      return (
                        <button
                          key={especialidade.id}
                          type="button"
                          onClick={() => setSelectedEspecialidades((current) => toggleSelection(current, especialidade.id))}
                          className={`rounded-[1.2rem] border px-4 py-3 text-left transition ${
                            selected
                              ? 'border-rose-300 bg-rose-50 text-rose-700'
                              : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
                          }`}
                        >
                          <p className="text-sm font-medium">{especialidade.nome}</p>
                        </button>
                      );
                    })}
                  </div>
                </div>
              ) : null}
            </section>

            <section className="space-y-4">
              <div className="flex items-end justify-between gap-3">
                <div>
                  <p className="text-sm font-semibold text-slate-900">3. Áreas de atendimento</p>
                  <p className="mt-1 text-sm text-slate-500">Informe onde você aceita trabalhar.</p>
                </div>

                <button
                  type="button"
                  onClick={() => setAreas((current) => [...current, createAreaDraft()])}
                  className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
                >
                  Adicionar área
                </button>
              </div>

              <div className="space-y-3">
                {areas.map((area, index) => (
                  <div key={area.id}>
                    <AreaEditor
                      area={area}
                      cidades={cidades ?? []}
                      onChange={(nextArea) =>
                        setAreas((current) => current.map((item) => (item.id === nextArea.id ? nextArea : item)))
                      }
                      onRemove={() =>
                        setAreas((current) => current.filter((item) => item.id !== area.id))
                      }
                      removable={areas.length > 1}
                    />
                  </div>
                ))}
              </div>
            </section>

            <section className="space-y-4">
              <div className="flex items-end justify-between gap-3">
                <div>
                  <p className="text-sm font-semibold text-slate-900">4. Portfólio</p>
                  <p className="mt-1 text-sm text-slate-500">
                    Adicione imagens do seu trabalho para aumentar conversão no perfil público.
                  </p>
                </div>

                <label className="inline-flex cursor-pointer items-center rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50">
                  <input
                    type="file"
                    accept="image/*"
                    className="hidden"
                    disabled={isUploadingPortfolio}
                    onChange={handlePortfolioUpload}
                  />
                  {isUploadingPortfolio ? 'Enviando imagem...' : 'Adicionar imagem'}
                </label>
              </div>

              {portfolio.length ? (
                <div className="grid gap-4 md:grid-cols-2">
                  {portfolio
                    .slice()
                    .sort((left, right) => left.ordem - right.ordem)
                    .map((item, index) => (
                      <div
                        key={item.id}
                        className="overflow-hidden rounded-[1.6rem] border border-slate-200 bg-slate-50"
                      >
                        <div className="aspect-[4/3] bg-slate-200">
                          <img
                            src={item.urlArquivo}
                            alt={item.legenda || `Imagem ${index + 1} do portfólio`}
                            className="h-full w-full object-cover"
                          />
                        </div>

                        <div className="space-y-3 p-4">
                          <div className="flex items-center justify-between gap-3">
                            <p className="text-sm font-semibold text-slate-900">Imagem {index + 1}</p>
                            <button
                              type="button"
                              onClick={() => removePortfolioItem(item.id)}
                              className="rounded-full border border-rose-200 bg-rose-50 px-3 py-1 text-xs font-medium text-rose-700 transition hover:bg-rose-100"
                            >
                              Remover
                            </button>
                          </div>

                          <input
                            className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
                            placeholder="Legenda da imagem"
                            value={item.legenda}
                            onChange={(event) => updatePortfolioItem(item.id, { legenda: event.target.value })}
                          />
                        </div>
                      </div>
                    ))}
                </div>
              ) : (
                <div className="rounded-[1.6rem] border border-dashed border-slate-300 bg-slate-50 px-4 py-5 text-sm text-slate-500">
                  Você ainda não adicionou imagens. O perfil funciona sem isso, mas um bom portfólio costuma melhorar a confiança do cliente.
                </div>
              )}
            </section>

            <section className="space-y-4">
              <div>
                <p className="text-sm font-semibold text-slate-900">5. Formas de recebimento</p>
                <p className="mt-1 text-sm text-slate-500">
                  Informe como você prefere receber para reduzir atrito na contratação.
                </p>
              </div>

              <div className="space-y-3">
                {FORMA_RECEBIMENTO_OPTIONS.map((option) => {
                  const enabled = formasRecebimento.some(
                    (item) => item.tipoFormaRecebimento === option.tipo,
                  );
                  const current = formasRecebimento.find(
                    (item) => item.tipoFormaRecebimento === option.tipo,
                  );

                  return (
                    <div
                      key={option.tipo}
                      className={`rounded-[1.4rem] border p-4 transition ${
                        enabled ? 'border-slate-900 bg-slate-50' : 'border-slate-200 bg-white'
                      }`}
                    >
                      <label className="flex cursor-pointer items-start gap-3">
                        <input
                          type="checkbox"
                          checked={enabled}
                          onChange={() => toggleFormaRecebimento(option.tipo)}
                        />

                        <div className="min-w-0 flex-1">
                          <p className="text-sm font-semibold text-slate-900">{option.titulo}</p>
                          <p className="mt-1 text-sm text-slate-500">{option.descricao}</p>
                        </div>
                      </label>

                      {enabled ? (
                        <input
                          className="mt-3 w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
                          placeholder={option.placeholder}
                          value={current?.descricao ?? ''}
                          onChange={(event) =>
                            updateFormaRecebimento(option.tipo, event.target.value)
                          }
                        />
                      ) : null}
                    </div>
                  );
                })}
              </div>
            </section>

            <button
              type="submit"
              disabled={isSaving || isUploadingPortfolio || !canSubmit}
              className="w-full rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isSaving ? 'Salvando...' : 'Concluir onboarding'}
            </button>
          </form>
        </section>
      </div>
    </main>
  );
}
