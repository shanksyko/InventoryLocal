# 📊 Comparação de Versões

## Tabela Comparativa

| Critério | Completa | Leve |
|----------|----------|------|
| **Arquivo** | InventorySystem-v1.1.0-Complete.zip | InventorySystem-v1.1.0-Lite.zip |
| **Tamanho** | 70 MB | 6.9 MB |
| **Inclui .NET** | ✅ Sim | ❌ Requer .NET 8.0 |
| **Pré-requisitos** | Apenas Windows | Windows + .NET 8.0 |
| **Tempo Extração** | ~2 minutos | ~30 segundos |
| **Tempo 1ª Execução** | < 3 segundos | < 3 segundos |
| **Melhor para** | Distribuição/Produção | Desenvolvimento/Updates |
| **Recomendado** | ✅ Sim | ❌ Não (a menos que tenha .NET) |

---

## 🎯 Quando Usar Cada Uma?

### 📦 Versão Completa - InventorySystem-v1.1.0-Complete.zip
**Use se:**
- ✅ Não tem certeza se .NET 8.0 está instalado
- ✅ Quer instalar em múltiplos PCs
- ✅ Quer garantir que vai funcionar em qualquer máquina
- ✅ É para distribuição/produção
- ✅ Quer instalação "plug and play"

**Exemplo:**
```
Extrair → Executar → Pronto!
Sem passos extras necessários
```

### 💾 Versão Leve - InventorySystem-v1.1.0-Lite.zip
**Use se:**
- ✅ .NET 8.0 já está instalado na máquina
- ✅ Quer economizar bandwidth
- ✅ Já fez download da "Completa" antes
- ✅ Está desenvolvendo/atualizando
- ✅ Tem conexão lenta para download

**Exemplo:**
```
Verificar .NET: dotnet --version
Se retornar 8.0 → Pode usar Lite
Se não → Baixe Complete ou instale .NET
```

---

## 📥 Instruções por Versão

### Versão Completa (Recomendada)

```bash
1. Baixe: InventorySystem-v1.1.0-Complete.zip (70 MB)
2. Extraia a pasta
3. Abra InventorySystem.exe
4. Pronto! Configure banco de dados na primeira execução
```

✅ **Vantagens:**
- Não precisa instalar nada
- Funciona imediatamente
- Ideal para produção

❌ **Desvantagens:**
- Arquivo maior (70 MB)
- Download mais lento

---

### Versão Leve

```bash
1. Verificar .NET:
   Abra PowerShell/CMD e digite:
   dotnet --version
   
   Se retornar "8.x.x" → Continue
   Se não encontrar → Instale .NET 8.0

2. Instalar .NET 8.0 (se necessário):
   https://dotnet.microsoft.com/download/dotnet/8.0
   
3. Baixe: InventorySystem-v1.1.0-Lite.zip (6.9 MB)

4. Extraia a pasta

5. Abra InventorySystem.exe

6. Configure banco de dados na primeira execução
```

✅ **Vantagens:**
- Arquivo pequeno (6.8 MB)
- Download rápido
- Usar se .NET já instalado

❌ **Desvantagens:**
- Requer .NET 8.0 instalado
- Passos extras se não tiver .NET

---

## 🔍 Como Verificar .NET Instalado?

### Windows - PowerShell/CMD:
```powershell
dotnet --version
```

**Se retornar algo como:**
```
8.0.0
8.0.1
8.0.2
```
→ ✅ Tem .NET 8.0, pode usar Lite

**Se retornar:**
```
'dotnet' is not recognized
```
→ ❌ Não tem .NET, use Completa ou instale

---

## 📊 Conteúdo do ZIP Completa

```
InventorySystem-v1.1.0-Complete.zip (70 MB)
├── InventorySystem.exe (175 MB descomprimido)
├── InventarioSistem.Core.dll
├── InventarioSistem.Access.dll
├── Microsoft.Data.SqlClient.dll
├── BCrypt.Net-Next.dll
├── ... (todas as dependências .NET incluídas)
└── [.NET 8.0 Runtime embutido]
```

---

## 📊 Conteúdo do ZIP Leve

```
InventorySystem-v1.1.0-Lite.zip (6.9 MB)
├── InventorySystem.exe
├── InventarioSistem.Core.dll
├── InventarioSistem.Access.dll
├── Microsoft.Data.SqlClient.dll
├── BCrypt.Net-Next.dll
└── [Dependências do aplicativo, sem .NET]
```

---

## 🎯 Recomendação Final

### Para 99% dos casos:
**👉 Use a Versão Completa (70 MB)**

Motivos:
- Funciona em qualquer máquina
- Instalação simples
- Sem surpresas
- Ideal para distribuição
- "Plug and Play"

### Exceção (Use Leve se):
- Sabe que tem .NET 8.0 instalado
- Quer economizar 63 MB de download
- Está em ambiente de desenvolvimento

---

## ❓ FAQ

**P: Qual versão escolho?**
R: Dúvida? Escolha a Completa (70 MB). Funciona garantido.

**P: Posso deletar o arquivo ZIP após extrair?**
R: Sim, após extrair, pode deletar o ZIP.

**P: Posso usar a Leve se não souber se tem .NET?**
R: Não recomendo. Use a Completa para garantir.

**P: Posso instalar ambas?**
R: Sim, mas não é necessário. Escolha uma.

**P: Qual é mais rápida?**
R: Mesma velocidade após extração. Leve é mais rápida de baixar.

**P: Qual tem melhor suporte?**
R: Ambas são idênticas em funcionalidade e suporte.

---

**Versão**: 1.1.0  
**Data**: 12 de Dezembro de 2025  
**Status**: ✅ Pronto para Produção
