# Artefatos da pipeline de Observabilidade

Esses artefatos foram gerados pelo workflow `backend-ci.yml` (run 23616933293) após corrigirmos a geração de métricas e o script `run-admin-jobs-tests.sh`.

## Lista de artefatos
- `backend-test-results.zip`: contém os `test-results.trx` de `MeAjudaAi.UnitTests` e `MeAjudaAi.IntegrationTests`.
- `admin-jobs-tests-log.zip`: guarda o log completo do `scripts/run-admin-jobs-tests.sh`.

## Como reproduzir o download
```bash
GH_RUN_DOWNLOAD_EXTRACT=false gh run download 23616933293 --name backend-test-results --dir <destino>
GH_RUN_DOWNLOAD_EXTRACT=false gh run download 23616933293 --name admin-jobs-tests-log --dir <destino>
```
Substitua `<destino>` pelo caminho local desejado. Como os arquivos não são extraídos automaticamente, você pode descompactar o `zip` depois com `unzip` e revisar o `.trx`/`.log` antes de anexar ao relatório.

## Observação
- Os artefatos podem ser consultados online em: https://github.com/NathanaelCampos/MeAjudaAi/actions/runs/23616933293.
- O comando `gh run download` acima foi executado duas vezes aqui, e os ZIPs estão disponíveis em `/tmp/backend-test-results` e `/tmp/admin-jobs-tests-log` neste ambiente.
