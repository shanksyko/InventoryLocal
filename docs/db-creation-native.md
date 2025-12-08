# Criação nativa de banco Access (.accdb)

O InventarioSistem é capaz de criar um novo banco Access (.accdb) diretamente,
sem depender de arquivos template, utilizando ADOX.

## Requisitos

- Provider ACE instalado:
  - Ex.: Microsoft Access Database Engine 2010/2016/Redistributable.
- Windows (ADOX é COM e funciona apenas em Windows).

## Fluxo de criação

- A criação é feita via:
  - AccessDatabaseCreator.CreateEmptyDatabase(path)
  - AccessDatabaseManager.CreateNewDatabase(path)
- Passos:
  1. Cria um .accdb vazio no caminho desejado.
  2. Define este caminho como "banco ativo" (salvo em config.json por usuário).
  3. Chama AccessSchemaManager.EnsureRequiredTables() para criar:
     - Computadores
     - Tablets
     - ColetoresAndroid
     - Celulares

## Uso no CLI

- No modo CLI:
  - Opção 9 → "Gerenciar banco Access"
  - Subopção "2 - Criar novo banco a partir do zero":
    - Pergunta caminho do novo .accdb.
    - Cria o arquivo.
    - Cria as tabelas padrão.
    - Define o banco como ativo.
    - Opcionalmente mostra o resumo do banco.

