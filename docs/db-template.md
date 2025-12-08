# Template de banco Access (obsoleto)

O InventarioSistem não depende mais de um arquivo `InventarioTemplate.accdb`.
A criação de novos bancos agora é feita via PowerShell/ADOX, gerando o arquivo
vazio no caminho solicitado e criando as tabelas padrão programaticamente.

Para detalhes sobre o novo fluxo, consulte
[`docs/db-creation-powershell.md`](db-creation-powershell.md).
