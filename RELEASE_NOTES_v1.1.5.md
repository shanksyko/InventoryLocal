# Release v1.1.5 - Garantia física do MDF/LDF

## Correções
- Verificação ativa pós `CREATE DATABASE`: aguarda até 10s pelos arquivos `.mdf` e `.ldf` com retry (300ms); erro claro se não forem criados.
- Fluxo de inicialização ajustado: usa `Database={dbName}` para criar schema/admin e retorna `AttachDbFileName` para compatibilidade.

## Impacto
- Elimina casos onde o MDF parecia não ser criado.
- Mensagens de log mais claras: "✅ Arquivos físicos confirmados (.mdf/.ldf)".

## Binários
- Pacote completo (runtime .NET 8.0 incluído): 182 MB (ZIP ~74 MB).

## Download
Extraia e execute `InventorySystem.exe`.
