# 🧪 Teste de Performance - InventorySystem

## 📋 Descrição

Ferramenta de teste de stress para validar a performance do sistema de inventário com grande volume de dados.

## 🚀 Como Executar

### Método 1: Via dotnet run
```bash
cd tests
dotnet run
```

### Método 2: Via executável
```bash
cd tests\bin\Debug\net8.0
.\PerformanceTest.exe
```

## 📊 Funcionalidades

### Menu Principal

```
Escolha um teste:
1 - Inserir 100 computadores
2 - Inserir 100 tablets
3 - Inserir 100 celulares
4 - Inserir 100 monitores
5 - Inserir 100 nobreaks
6 - Inserir 50 de cada tipo (550 total)
7 - Teste de leitura (listar todos)
8 - Limpar todos os dados
0 - Sair
```

## 🧪 Cenários de Teste

### Teste 1: Inserção Massiva de Computadores
- **Quantidade**: 100 dispositivos
- **Objetivo**: Validar velocidade de inserção
- **Métricas**: Tempo total, média por item

### Teste 2-5: Inserção por Tipo
- **Tablets**: 100 dispositivos com IMEIs
- **Celulares**: 100 dispositivos com dados completos
- **Monitores**: 100 dispositivos com vinculação
- **Nobreaks**: 100 dispositivos com IPs

### Teste 6: Stress Test Completo
- **Quantidade**: 550 dispositivos (50 de cada tipo)
- **Tipos incluídos**:
  - 50 Computadores
  - 50 Tablets
  - 50 Celulares
  - 50 Monitores
  - 50 Nobreaks
  - 50 Coletores
  - 50 Impressoras
  - 50 DECTs
  - 50 Telefones Cisco
  - 50 Televisores
  - 50 Relógios de Ponto
- **Objetivo**: Testar sistema com volume real de dados

### Teste 7: Performance de Leitura
- **Operação**: ListAsync() + CountByTypeAsync()
- **Objetivo**: Validar velocidade de consulta
- **Métricas**: 
  - Tempo de leitura de todos os registros
  - Contagem por tipo

### Teste 8: Limpeza de Dados
- **Operação**: DeleteAsync() em loop
- **Objetivo**: Limpar banco após testes
- **Confirmação**: Requer confirmação (S/N)

## 📈 Métricas Coletadas

Para cada teste de inserção:
- ✅ Tempo total de execução
- ✅ Média de tempo por item
- ✅ Progresso em tempo real (a cada 10 itens)

Para teste de leitura:
- ✅ Tempo de consulta
- ✅ Quantidade total de registros
- ✅ Distribuição por tipo

## ⚙️ Configuração

### Pré-requisitos
1. SQL Server Express instalado e rodando
2. Arquivo `sqlserver.config.json` configurado:

```json
{
  "ConnectionString": "Server=localhost\\SQLEXPRESS;Database=InventoryDB;Integrated Security=true;TrustServerCertificate=true;"
}
```

### Localização do Config
- `tests/bin/Debug/net8.0/sqlserver.config.json`

## 🎯 Objetivos do Teste

### Validar Performance
- [ ] Sistema suporta 100+ inserções sem travar
- [ ] Sistema suporta 500+ dispositivos no total
- [ ] Leitura de todos os registros < 1 segundo
- [ ] UI não trava durante operações pesadas

### Validar Integridade
- [ ] Dados inseridos corretamente
- [ ] Relacionamentos preservados
- [ ] Sem perda de dados em volume

### Validar Escalabilidade
- [ ] Performance linear com crescimento de dados
- [ ] Memória estável durante operações
- [ ] Sem vazamento de conexões

## 📝 Exemplo de Saída

```
=== TESTE DE PERFORMANCE - INVENTÁRIO ===

✅ Conectado ao banco de dados

Escolha um teste:
...
Opção: 6

Inserindo 50 de cada tipo...
Inserindo 50 computadores...
Progresso: 50/50 (100%)
✅ 50 computadores inseridos em 245ms
   Média: 4ms por item

Inserindo 50 tablets...
Progresso: 50/50 (100%)
✅ 50 tablets inseridos em 198ms
   Média: 3ms por item

...

✅ 550 dispositivos inseridos em 2.35 segundos
```

## 🐛 Troubleshooting

### Erro: Connection string não configurada
**Solução**: Criar arquivo `sqlserver.config.json` no diretório do executável

### Erro: SQL Server não responde
**Solução**: Verificar se SQL Server Express está rodando:
```powershell
Get-Service MSSQL$SQLEXPRESS
```

### Performance baixa
**Possíveis causas**:
- Antivírus escaneando banco de dados
- SQL Server em modo de recuperação
- Disco lento (HDD vs SSD)

## 🎓 Boas Práticas

1. **Limpar dados** antes de cada teste para resultados consistentes
2. **Executar múltiplas vezes** para validar estabilidade
3. **Monitorar Task Manager** durante testes para validar uso de recursos
4. **Testar WinForms** após testes massivos para validar UI responsiva

## 📊 Benchmark Esperado (Referência)

| Operação | Tempo Esperado | Observações |
|----------|----------------|-------------|
| Inserir 100 itens | < 500ms | ~5ms por item |
| Inserir 550 itens | < 3s | Todos os tipos |
| Listar todos (550) | < 100ms | Query otimizada |
| Deletar 100 itens | < 1s | ~10ms por item |

**Hardware de referência**: CPU i5+, 8GB RAM, SSD

---

## 🔄 Próximos Passos

Após validar performance:
1. Testar com WinForms aberto
2. Validar dashboard com muitos dados
3. Testar exportação XLSX com 500+ itens
4. Validar filtros com grandes volumes
