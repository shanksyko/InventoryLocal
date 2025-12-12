# v1.1.7 – Correções de criação do MDF e logs detalhados

Esta versão melhora a confiabilidade na criação do banco `.mdf` e amplia os logs para facilitar suporte e diagnóstico.

## Destaques
- Correção: não assumir banco existente apenas por `db_id(@name)` quando os arquivos físicos `.mdf/.ldf` não existem.
- Fluxo robusto: se DB consta na instância sem arquivos, o banco é removido e recriado do zero.
- Criação explícita: `CREATE DATABASE ... ON (FILENAME=...)` para MDF/LDF com verificação pós-criação.
- Garantia de schema/admin via `Database={dbName}` durante criação (sem Attach); conexão final retorna `AttachDbFileName`.

## Logs ampliados
- Caminhos completos de MDF/LDF.
- Nome do banco (`dbName`).
- Decisões: reutilizar vs. dropar/recriar.
- Connection strings usadas (controle, garantia, final Attach).
- Confirmação visual na UI: caminhos completos dos arquivos e connection string final.

## Distribuição
- Artefato: `InventorySystem_v1.1.7_SingleFile.zip` (executável único self-contained, WinForms, sem trimming).
