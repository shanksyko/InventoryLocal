# InventarioSistem Access schema

A base Access utiliza uma tabela única para todos os dispositivos. O `AccessInventoryStore` cria a tabela automaticamente quando ela não existir.

## Tabela `Devices`

| Coluna                 | Tipo             | Descrição |
| ---------------------- | ---------------- | --------- |
| `Id`                   | `COUNTER`        | Chave primária gerada automaticamente. |
| `Tipo`                 | `TEXT(50)`       | Nome do tipo do dispositivo (`Computer`, `Tablet`, `ColetorAndroid`, `Celular`). |
| `Patrimonio`           | `TEXT(100)`      | Identificador do patrimônio ou etiqueta interna. |
| `Marca`                | `TEXT(100)`      | Marca do equipamento. |
| `Modelo`               | `TEXT(100)`      | Modelo do equipamento. |
| `NumeroSerie`          | `TEXT(100)`      | Número de série. |
| `Imei`                 | `TEXT(30)`       | IMEI normalizado (15 dígitos) quando aplicável. |
| `SistemaOperacional`   | `TEXT(100)`      | Sistema operacional para computadores. |
| `Processador`          | `TEXT(100)`      | Processador (computador). |
| `MemoriaRamGb`         | `INTEGER`        | Quantidade de memória RAM em GB (computador). |
| `ArmazenamentoGb`      | `INTEGER`        | Armazenamento em GB (computador). |
| `VersaoAndroid`        | `TEXT(50)`       | Versão do Android (tablet, coletor e celular). |
| `LinhaTelefonica`      | `TEXT(50)`       | Linha telefônica associada (tablet/celular). |
| `Responsavel`          | `TEXT(100)`      | Usuário responsável. |
| `Localizacao`          | `TEXT(100)`      | Local ou setor em que o equipamento está. |
| `Observacoes`          | `MEMO`           | Observações livres. |
| `AtualizadoEm`         | `DATETIME`       | Data da última atualização de cadastro. |
| `FabricanteScanner`    | `TEXT(100)`      | Fabricante/linha do scanner (coletor). |
| `PossuiCarregadorBase` | `YESNO`          | Indica se o coletor possui base/carregador. |
| `PossuiTeclado`        | `YESNO`          | Sinaliza a existência de teclado para tablets. |
| `Corporativo`          | `YESNO`          | Linha corporativa (celular). |

## Índices sugeridos

- Índice composto em `Tipo` + `Patrimonio` para acelerar listagens filtradas.
- Índice em `Localizacao` para relatórios por setor.
- Índice em `Imei` para buscas de dispositivos móveis.

A criação da base e da tabela é feita pelo método `EnsureSchemaAsync` do `AccessInventoryStore`. Para bases existentes, as colunas devem corresponder ao layout acima para que as operações CRUD funcionem corretamente.
