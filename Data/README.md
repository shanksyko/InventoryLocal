# Data

Esta pasta deve conter o arquivo de banco de dados Access base do sistema.

Convenção:
- Arquivo principal de modelo: InventarioSistem.accdb
- Este arquivo é criado e mantido manualmente no Microsoft Access.
- A estrutura de tabelas (Computadores, Tablets, ColetoresAndroid, Celulares)
  é definida neste modelo.

Em tempo de execução:
- O aplicativo pode:
  - Usar um banco existente em qualquer caminho (selecionado pelo usuário).
  - Criar um novo banco copiando este modelo para outro local.
- O caminho do banco "ativo" é armazenado em config.json ao lado do executável.
