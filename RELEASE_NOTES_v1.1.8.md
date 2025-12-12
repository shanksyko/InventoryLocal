# v1.1.8 – Criação de MDF em qualquer diretório

Esta versão resolve o problema onde a criação de MDF funcionava apenas em "Documentos" e não em outros diretórios com restrições de permissão.

## Melhorias

- **Validação robusta de permissões**: Testa permissão de escrita em qualquer diretório antes de criar o MDF.
- **Retry automático**: Se falhar ao criar diretório, tenta novamente com pequena pausa (útil para problemas temporários de lock).
- **Feedback detalhado**: Logs explicam exatamente por que falhou e sugerem soluções:
  - Executar como Administrador
  - Escolher pasta com permissão (Documentos, Desktop)
  - Verificar se pasta está bloqueada por antivírus
- **Suporte a caminhos especiais**: Agora cria MDF em paths UNC (rede) e diretórios com restrições.

## Distribuição

- Artefato: `InventorySystem_v1.1.8_SingleFile.zip` (executável único self-contained, WinForms, sem trimming).
