# blazor-viacep-net
pratica-sala-de-aula

## O que foi solicitado

| Páginas | Etapa do material | Implementação |
| --- | --- | --- |
| 16-24 | Criar e executar um projeto WebAssembly chamado BlazorViaCep | `BlazorViaCep.csproj`, `Program.cs` e estrutura do template .NET 10 |
| 25-27 | Criar um modelo que represente o JSON | `Models/Endereco.cs`, com atributos `JsonPropertyName` |
| 28-37 | Implementar a lógica na Home, injetar HttpClient, validar o CEP e consultar a API | `Pages/Home.razor`, método assíncrono `ConsultarCep` |
| 38-41 | Criar campo de CEP, botão, tabela e mensagens de erro | Interface da página inicial, com Bootstrap local |
| 42-45 | Executar e testar, inclusive CEP inválido e 30130000 | Verificações no navegador e testes automatizados |

A implementação mantém a lógica principal em `Home.razor`, como na aula.
As páginas Counter e Weather do template foram preservadas, pois aparecem
nas capturas do material. Weather usa dados locais de exemplo; não é uma API
meteorológica. Não foram implementadas as etapas posteriores à página 45.

## Requisitos e execução

- SDK **.NET 10**; ambiente utilizado: `10.0.400`.
- Navegador moderno com WebAssembly.
- Internet para restaurar pacotes na primeira compilação e consultar o ViaCEP.

No PowerShell:

```powershell
Set-Location 'C:\Users\lucas\OneDrive\Documentos\vs code uni\pratica blazor viacep'
dotnet restore BlazorViaCep.csproj
dotnet run --project BlazorViaCep.csproj
```

Abra **http://localhost:5085**. Digite `30130000` ou `30130-000` e clique em
**Consultar**. O resultado deve apresentar os dados retornados pelo serviço.
Para encerrar o servidor de desenvolvimento, pressione `Ctrl+C` no terminal.

O primeiro carregamento pode levar alguns segundos porque o navegador baixa
o runtime WebAssembly e os arquivos da aplicação. Não abra `index.html` com
duplo clique: execute o servidor com `dotnet run`.

Para compilar separadamente:

```powershell
dotnet build BlazorViaCep.csproj
```

Durante a edição, também é possível usar:

```powershell
dotnet watch --project BlazorViaCep.csproj run
```

O fluxo verificado nesta entrega foi `dotnet run`. No VS Code, abra a pasta
com `code .`. Há recomendações de extensões e tarefas para compilar e executar
em `.vscode/`. Em **Terminal > Run Task**, escolha **executar BlazorViaCep**.
Também é possível abrir o `.csproj` no Visual Studio com suporte ao .NET 10.

## Estrutura dos arquivos

```text
pratica blazor viacep/
├── BlazorViaCep.csproj
├── Program.cs
├── App.razor
├── _Imports.razor
├── Models/
│   └── Endereco.cs
├── Pages/
│   ├── Home.razor
│   ├── Home.razor.css
│   ├── Counter.razor
│   └── Weather.razor
├── Layout/                         # Layout e menu lateral
├── Properties/launchSettings.json  # Porta 5085 e perfil HTTP local
├── wwwroot/                       # HTML, Bootstrap, CSS e dados do template
├── .vscode/                       # Tarefas e extensões recomendadas
├── .gitignore
├── tests/BlazorViaCep.Tests/       # Testes xUnit e bUnit
└── README.md
```

## Como os códigos funcionam

### Inicialização e WebAssembly

`Program.cs` cria o `WebAssemblyHostBuilder`, registra `App` no elemento HTML
`#app` e configura o `HeadOutlet` para títulos e metadados. O `HttpClient` é
registrado para injeção de dependência e tem timeout de **15 segundos**.

O código C# da interface executa no navegador. Este é um projeto WebAssembly
standalone, portanto **não usa `@rendermode InteractiveServer`**. A chamada
HTTPS ao ViaCEP parte diretamente do navegador. Não há backend próprio,
banco de dados, chave de API ou armazenamento permanente dos CEPs.

`App.razor` controla as rotas; `Layout/MainLayout.razor` monta o layout;
`Layout/NavMenu.razor` fornece a navegação; `wwwroot/index.html` carrega o
runtime e os estilos. O Bootstrap é incluído localmente no projeto.

### Modelo Endereco

`JsonPropertyName` associa o nome recebido no JSON ao nome da propriedade C#:

| Campo JSON | Propriedade C# |
| --- | --- |
| cep | Cep |
| logradouro | Rua |
| complemento | Complemento |
| bairro | Bairro |
| localidade | Cidade |
| uf | Uf |
| ibge | Ibge |
| gia | Gia |
| ddd | Ddd |
| siafi | Siafi |

As propriedades de texto aceitam `null`, pois uma resposta de erro não contém
um endereço. `Erro` guarda o indicador da API como `JsonElement`, permitindo
tanto `{"erro": true}` quanto `{"erro": "true"}`. A propriedade calculada
`NaoEncontrado` interpreta as duas formas. A segunda foi observada na consulta
real ao serviço durante a implementação.

### Consulta na Home

A página usa `@inject HttpClient Http`, o modelo `Endereco` e estas variáveis:

- `cep`: texto informado pelo usuário.
- `viaCepResponse`: endereço obtido, ou `null` quando não há resultado válido.
- `errorMessage`: mensagem de validação ou de falha.
- `consultando`: indica uma requisição em andamento.

O método `ConsultarCep` segue este fluxo:

1. Impede uma segunda consulta enquanto a primeira está em andamento.
2. Limpa o resultado e a mensagem anteriores.
3. Rejeita o campo vazio com “Por favor, digite um CEP.”.
4. Remove os caracteres que não são dígitos ASCII, conforme a normalização
   apresentada na aula, e exige exatamente oito dígitos.
5. Monta a URL `https://viacep.com.br/ws/{cepFormatado}/json/`.
6. Usa `await Http.GetFromJsonAsync<Endereco>(...)` para consultar e desserializar.
7. Verifica resposta nula, o indicador de CEP inexistente e a presença de CEP.
8. Exibe o endereço ou uma mensagem de erro e libera os controles no `finally`.

`@bind:event="oninput"` atualiza o texto a cada edição.
`@bind:after="LimparResultado"` remove o endereço antigo quando o usuário muda
o campo. O botão e o campo ficam desabilitados durante a requisição, com a
mensagem “Consultando o ViaCEP. Aguarde...”.

### Resultado e tratamento de erros

A tabela apresenta os sete campos da aula: **CEP, Logradouro, Complemento,
Bairro, Localidade, UF e DDD**. Quando um campo opcional vem vazio, a interface
mostra “Não informado”, sem inventar um valor.

O código diferencia:

- campo vazio e quantidade inválida de dígitos, sem fazer uma requisição;
- CEP inexistente, indicado pelo ViaCEP;
- resposta nula, incompleta ou JSON malformado;
- falha de conexão e status HTTP de erro;
- tempo limite excedido;
- erro inesperado durante a consulta.

Ao sair da página, `Dispose` cancela a requisição pendente. Esse cancelamento
de navegação não é apresentado como uma falha de conexão.

## Ajustes em relação aos trechos dos slides

- Removida a validação de comprimento repetida na captura da página 37.
- Inicializadas as variáveis e usados tipos anuláveis para evitar acesso a
  `viaCepResponse.Cep` quando a resposta é nula.
- Interpretado explicitamente o campo `erro` da API, inclusive quando é texto.
- Acrescentados timeout, estado de carregamento e cancelamento de navegação.
- Adicionada limpeza do resultado ao editar o CEP e mensagens sem detalhes
  técnicos de exceções na interface.
- Mantido o HTTP apenas para o servidor local de desenvolvimento; a consulta
  ao ViaCEP utiliza HTTPS.

## Testes

Os testes automatizados usam **xUnit e bUnit**, com respostas HTTP simuladas.
Não fazem consultas à API pública. Execute a partir da raiz:

```powershell
dotnet test .\tests\BlazorViaCep.Tests\BlazorViaCep.Tests.csproj
```

Foram aprovados **14 casos**, cobrindo validação sem acesso à rede, normalização,
exibição dos sete campos, campo opcional vazio, limpeza ao editar, erro booleano
e textual, resposta nula, JSON inválido, HTTP 503, falha de rede, timeout e
bloqueio/liberação dos controles durante a requisição.

O projeto principal exclui `tests/**` de seus itens. Assim, xUnit e bUnit são
dependências apenas dos testes e não entram na aplicação WebAssembly.

Também foram realizados testes no navegador com o ViaCEP real:

| Entrada | Resultado observado |
| --- | --- |
| Campo vazio | Mensagem solicitando um CEP |
| 111111111111 | Mensagem exigindo 8 dígitos |
| 99999999 | CEP não encontrado ou inválido |
| 30130000 | Avenida Afonso Pena, Centro, Belo Horizonte/MG, DDD 31 |
| 30130-000 | Mesmo endereço, confirmando a normalização do hífen |

Os dados reais podem mudar conforme a base do ViaCEP. Os endereços exibidos
na aplicação sempre vêm da API; os dados fixos de exemplo ficam somente nos testes.

## Problemas comuns

- **Porta ocupada:** encerre uma execução anterior ou use
  `dotnet run --urls http://localhost:5086` e acesse a nova porta.
- **Falha ao restaurar pacotes:** confira a conexão com `api.nuget.org` e execute
  `dotnet restore` novamente.
- **Erro na consulta:** confira o CEP, a conexão e a disponibilidade do ViaCEP.
  O timeout permite tentar novamente sem deixar a tela travada.
- **Tela desatualizada:** após compilar sem watcher, recarregue a página.
- **Sem resultado em campo opcional:** alguns CEPs não têm complemento,
  logradouro ou bairro; a interface exibe “Não informado”.

## Referências

- Material fornecido: `blazor viacep.pdf`, páginas 16 a 45.
- [Documentação oficial do ViaCEP](https://viacep.com.br/).
