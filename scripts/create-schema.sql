-- ========================================
-- INVENTORY SYSTEM - SQL SERVER SCHEMA
-- ========================================
-- Este arquivo cria o schema completo para o Inventory System
-- Compatível com SQL Server Express 2019+
-- Data: Dezembro 2024
-- ========================================

-- ========================================
-- 1. TABELA: COMPUTADORES
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Computadores')
CREATE TABLE [Computadores] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Host] VARCHAR(100),
    [SerialNumber] VARCHAR(100),
    [Proprietario] VARCHAR(100),
    [Departamento] VARCHAR(100),
    [Matricula] VARCHAR(50),
    [CreatedAt] DATETIME,
    INDEX [IX_Host] ([Host]),
    INDEX [IX_SerialNumber] ([SerialNumber])
);

-- ========================================
-- 2. TABELA: TABLETS
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tablets')
CREATE TABLE [Tablets] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Host] VARCHAR(100),
    [SerialNumber] VARCHAR(100),
    [Local] VARCHAR(100),
    [Responsavel] VARCHAR(100),
    [Imeis] VARCHAR(MAX),
    [CreatedAt] DATETIME,
    INDEX [IX_SerialNumber] ([SerialNumber]),
    INDEX [IX_Responsavel] ([Responsavel])
);

-- ========================================
-- 3. TABELA: COLETORES ANDROID
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ColetoresAndroid')
CREATE TABLE [ColetoresAndroid] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Host] VARCHAR(100),
    [SerialNumber] VARCHAR(100),
    [MacAddress] VARCHAR(50),
    [IpAddress] VARCHAR(50),
    [Local] VARCHAR(100),
    [CreatedAt] DATETIME,
    [AppGwsFg] BIT,
    [AppGwsRm] BIT,
    [AppInspection] BIT,
    [AppCuringTbr] BIT,
    [AppCuringPcr] BIT,
    [AppInspectionTbr] BIT,
    [AppQuimico] BIT,
    [AppBuildingTbr] BIT,
    [AppBuildingPcr] BIT,
    [OsWinCe] BIT,
    [OsAndroid81] BIT,
    [OsAndroid10] BIT,
    INDEX [IX_SerialNumber] ([SerialNumber]),
    INDEX [IX_MacAddress] ([MacAddress])
);

-- ========================================
-- 4. TABELA: CELULARES
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Celulares')
CREATE TABLE [Celulares] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [CellName] VARCHAR(100),
    [Imei1] VARCHAR(50),
    [Imei2] VARCHAR(50),
    [Modelo] VARCHAR(100),
    [Numero] VARCHAR(50),
    [Roaming] BIT,
    [Usuario] VARCHAR(100),
    [Matricula] VARCHAR(50),
    [Cargo] VARCHAR(100),
    [Setor] VARCHAR(100),
    [Email] VARCHAR(150),
    [Senha] VARCHAR(150),
    [CreatedAt] DATETIME,
    INDEX [IX_Imei1] ([Imei1]),
    INDEX [IX_Usuario] ([Usuario]),
    INDEX [IX_Matricula] ([Matricula])
);

-- ========================================
-- 5. TABELA: IMPRESSORAS
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Impressoras')
CREATE TABLE [Impressoras] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Nome] VARCHAR(200),
    [TipoModelo] VARCHAR(100),
    [SerialNumber] VARCHAR(100),
    [LocalAtual] VARCHAR(100),
    [LocalAnterior] VARCHAR(100),
    [CreatedAt] DATETIME,
    INDEX [IX_SerialNumber] ([SerialNumber]),
    INDEX [IX_LocalAtual] ([LocalAtual])
);

-- ========================================
-- 6. TABELA: DECTS (TELEFONES DECT)
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Dects')
CREATE TABLE [Dects] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Responsavel] VARCHAR(100),
    [Ipei] VARCHAR(100),
    [MacAddress] VARCHAR(50),
    [Numero] VARCHAR(50),
    [Local] VARCHAR(100),
    [Modelo] VARCHAR(100),
    [CreatedAt] DATETIME,
    INDEX [IX_MacAddress] ([MacAddress]),
    INDEX [IX_Numero] ([Numero])
);

-- ========================================
-- 7. TABELA: TELEFONES CISCO
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TelefonesCisco')
CREATE TABLE [TelefonesCisco] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Responsavel] VARCHAR(100),
    [MacAddress] VARCHAR(50),
    [Numero] VARCHAR(50),
    [Local] VARCHAR(100),
    [IpAddress] VARCHAR(50),
    [Serial] VARCHAR(100),
    [CreatedAt] DATETIME,
    INDEX [IX_MacAddress] ([MacAddress]),
    INDEX [IX_IpAddress] ([IpAddress])
);

-- ========================================
-- 8. TABELA: TELEVISORES
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Televisores')
CREATE TABLE [Televisores] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Modelo] VARCHAR(100),
    [SerialNumber] VARCHAR(100),
    [Local] VARCHAR(100),
    [CreatedAt] DATETIME,
    INDEX [IX_SerialNumber] ([SerialNumber]),
    INDEX [IX_Local] ([Local])
);

-- ========================================
-- 9. TABELA: RELÓGIOS DE PONTO
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RelogiosPonto')
CREATE TABLE [RelogiosPonto] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Modelo] VARCHAR(100),
    [SerialNumber] VARCHAR(100),
    [Local] VARCHAR(100),
    [Ip] VARCHAR(50),
    [DataBateria] DATETIME,
    [DataNobreak] DATETIME,
    [ProximasVerificacoes] DATETIME,
    [CreatedAt] DATETIME,
    INDEX [IX_SerialNumber] ([SerialNumber]),
    INDEX [IX_Ip] ([Ip])
);

-- ========================================
-- 10. TABELA: MONITORES
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Monitores')
CREATE TABLE [Monitores] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Modelo] VARCHAR(100),
    [SerialNumber] VARCHAR(100),
    [Local] VARCHAR(100),
    [Responsavel] VARCHAR(100),
    [ComputadorVinculado] VARCHAR(100),
    [CreatedAt] DATETIME,
    INDEX [IX_SerialNumber] ([SerialNumber]),
    INDEX [IX_Responsavel] ([Responsavel])
);

-- ========================================
-- 11. TABELA: NOBREAKS
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Nobreaks')
CREATE TABLE [Nobreaks] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Hostname] VARCHAR(100),
    [Local] VARCHAR(100),
    [IpAddress] VARCHAR(50),
    [Modelo] VARCHAR(100),
    [Status] VARCHAR(50),
    [SerialNumber] VARCHAR(100),
    [CreatedAt] DATETIME,
    INDEX [IX_IpAddress] ([IpAddress]),
    INDEX [IX_SerialNumber] ([SerialNumber])
);

-- ========================================
-- 12. TABELA: USUÁRIOS (AUTENTICAÇÃO)
-- ========================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
CREATE TABLE [Users] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Username] VARCHAR(100) UNIQUE NOT NULL,
    [PasswordHash] VARCHAR(MAX) NOT NULL,
    [FullName] VARCHAR(200),
    [Role] VARCHAR(50) NOT NULL,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [LastLogin] DATETIME,
    [LastPasswordChange] DATETIME,
    INDEX [IX_Username] ([Username]),
    INDEX [IX_IsActive] ([IsActive])
);

-- ========================================
-- INSERIR USUÁRIO PADRÃO (ADMIN)
-- ========================================
-- Senha: L9l337643k#$
-- Hash: $2a$12$HASH_BCRYPT_AQUI
-- ========================================

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'admin')
INSERT INTO [Users] ([Username], [PasswordHash], [FullName], [Role], [IsActive], [CreatedAt])
VALUES (
    'admin',
    '$2a$12$Yd7V/K2Qn.Y5W6/5X8Z9C.Uk/7N2L3M4N5O6P7Q8R9S0T1U2V3W4X5Y6',
    'Administrador',
    'Admin',
    1,
    GETUTCDATE()
);

-- ========================================
-- CRIAR ÍNDICES ADICIONAIS PARA PERFORMANCE
-- ========================================

-- Índices compostos para buscas frequentes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Active_Username')
CREATE INDEX [IX_Users_Active_Username] ON [Users] ([IsActive], [Username]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Celulares_Usuario_Matricula')
CREATE INDEX [IX_Celulares_Usuario_Matricula] ON [Celulares] ([Usuario], [Matricula]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ColetoresAndroid_Local_Serial')
CREATE INDEX [IX_ColetoresAndroid_Local_Serial] ON [ColetoresAndroid] ([Local], [SerialNumber]);

-- ========================================
-- CRIAR VIEWS ÚTEIS
-- ========================================

-- View: Resumo de Equipamentos por Tipo
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'vw_EquipamentosPorTipo')
CREATE VIEW [vw_EquipamentosPorTipo] AS
SELECT 
    'Computadores' AS Tipo, COUNT(*) AS Total
FROM [Computadores]
UNION ALL
SELECT 'Tablets', COUNT(*) FROM [Tablets]
UNION ALL
SELECT 'Coletores Android', COUNT(*) FROM [ColetoresAndroid]
UNION ALL
SELECT 'Celulares', COUNT(*) FROM [Celulares]
UNION ALL
SELECT 'Impressoras', COUNT(*) FROM [Impressoras]
UNION ALL
SELECT 'DECTs', COUNT(*) FROM [Dects]
UNION ALL
SELECT 'Telefones Cisco', COUNT(*) FROM [TelefonesCisco]
UNION ALL
SELECT 'Televisores', COUNT(*) FROM [Televisores]
UNION ALL
SELECT 'Relógios de Ponto', COUNT(*) FROM [RelogiosPonto]
UNION ALL
SELECT 'Monitores', COUNT(*) FROM [Monitores]
UNION ALL
SELECT 'Nobreaks', COUNT(*) FROM [Nobreaks];

-- View: Usuários Ativos
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'vw_UsuariosAtivos')
CREATE VIEW [vw_UsuariosAtivos] AS
SELECT 
    [Id],
    [Username],
    [FullName],
    [Role],
    [LastLogin],
    [CreatedAt]
FROM [Users]
WHERE [IsActive] = 1;

-- ========================================
-- FIM DO SCRIPT
-- ========================================
PRINT 'Schema do Inventory System criado com sucesso!';
