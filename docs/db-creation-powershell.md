# Criação de banco Access via PowerShell

O InventarioSistem consegue criar um novo banco Access (.accdb) do zero usando
PowerShell + ADOX, sem COMReference no projeto .NET.

## Como funciona

- A classe `AccessDatabaseCreator` executa:
  - `powershell.exe -NoProfile -NonInteractive -Command -`
- E envia um script que:
  - Cria um objeto COM `ADOX.Catalog`.
  - Chama `Create()` com uma connection string ACE (`Microsoft.ACE.OLEDB.12.0`).
  - Gera o arquivo `.accdb` no caminho informado.

Em seguida:

- `AccessDatabaseManager.CreateNewDatabase(path)`:
  1. Chama `AccessDatabaseCreator.CreateEmptyDatabase(path)`.
  2. Define este arquivo como banco ativo (`DatabasePath` em config.json por usuário).
  3. Chama `AccessSchemaManager.EnsureRequiredTables()` para criar tabelas:
     - Computadores
     - Tablets
     - ColetoresAndroid
     - Celulares

## Caminhos suportados

- Caminhos locais:
  - `C:\Inventarios\SetorTI.accdb`
- Outras unidades:
  - `D:\Dados\Inventario.accdb`
- Caminhos de rede (UNC):
  - `\\servidor\inventario\MeuInventario.accdb`

O programa não exige que o arquivo esteja dentro do repositório ou da pasta do executável.
Apenas precisa de:
- Permissão de escrita no caminho informado.
- Provider ACE instalado no Windows (Microsoft Access Database Engine).
