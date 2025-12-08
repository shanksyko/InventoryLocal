# Template de banco Access

Para que o aplicativo possa criar novos bancos Access (.accdb) automaticamente,
é necessário fornecer um arquivo template vazio.

## Convenção

- Nome do arquivo de template: `InventarioTemplate.accdb`
- Localização em tempo de execução:
  - Mesma pasta do executável (`AppDomain.CurrentDomain.BaseDirectory`).

## Como criar o template

1. Abra o Microsoft Access.
2. Crie um novo banco de dados em branco.
3. Não é necessário criar nenhuma tabela (o aplicativo cuidará disso).
4. Salve o arquivo como `InventarioTemplate.accdb`.
5. Copie o arquivo para a pasta onde o executável do InventarioSistem está.
   - Em desenvolvimento: normalmente `bin\Debug\net8.0` ou similar.

Quando o usuário escolher "Criar novo banco" no CLI (opção 9 → opção 2),
o aplicativo irá:

- Copiar `InventarioTemplate.accdb` para o caminho informado.
- Definir este novo arquivo como banco ativo.
- Criar as tabelas padrão (Computadores, Tablets, ColetoresAndroid, Celulares)
  usando AccessSchemaManager.EnsureRequiredTables().
