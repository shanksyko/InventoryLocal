# v1.1.6 – Single-file distribution e melhorias na criação do MDF

Esta versão foca em reduzir a quantidade de arquivos distribuídos e consolidar correções para garantir a criação confiável do banco `.mdf` no LocalDB.

## Destaques

- Single-file self-contained para WinForms: executável único, sem necessidade de múltiplos DLLs (sem trimming, compatível com WinForms).
- Garantia física dos arquivos `.mdf` e `.ldf`: verificação com timeout e fallback de criação.
- Validação de attach: tenta `FOR ATTACH` quando necessário e confirma disponibilidade do DB.
- Operações de schema e admin com `Database={dbName}` para evitar inconsistências de login.

## Correções e melhorias técnicas

- `LocalDbManager.CreateMdfDatabase` reestruturado:
  - Remove resíduos de `.mdf/.ldf` antigos de forma segura quando apropriado.
  - Primeiro caminho: `CREATE DATABASE ...` normal; fallback: `CREATE DATABASE ... ON (FILENAME=...)` quando necessário.
  - Polling com timeout para confirmar criação física dos arquivos.
  - Verificação e attach do DB quando não encontrado.
  - Usa `Database={dbName}` para `EnsureSchemaAndAdmin`, retornando `AttachDbFileName` para runtime.
- Timeouts configurados para evitar travamentos e cargas infinitas.
- Fluxo de UI mantém responsividade (trabalho em background) e logs mais claros.

## Distribuição

- Artefato: `InventorySystem_v1.1.6_SingleFile.zip` (~70 MB) contendo um único executável self-contained.
- Recomendado para ambientes que preferem menos arquivos na instalação.

## Observações

- O single-file não utiliza trimming para preservar compatibilidade completa com WinForms.
- Caso o ambiente possua restrições de permissões, os logs e validações foram ampliados para apontar claramente o ponto de falha.
