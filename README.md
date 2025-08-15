# Desafio Back-End - API de Apostas

Este projeto é uma API RESTful desenvolvida em ASP.NET Core 6 como parte do desafio técnico. A aplicação simula funcionalidades básicas de apostas online, incluindo gerenciamento de usuários, autenticação, carteira digital e um sistema de apostas.

## Features

- **Cadastro e Autenticação de Usuários**: Sistema de registro e login com autenticação baseada em JSON Web Tokens (JWT).
- **Carteira Digital**: Cada usuário possui uma carteira com múltiplos saldos.
- **Sistema de Apostas**: Os usuários podem realizar apostas, com a lógica de ganho ou perda sendo processada pela aplicação.
- **Histórico de Transações**: Todas as movimentações financeiras (depósitos, saques, apostas) são registradas.
- **Segurança**: Senhas são hasheadas e salvas de forma segura. Rotas protegidas que exigem autenticação.
- **Tratamento de Concorrência**: Implementado bloqueio otimista para garantir a integridade dos dados em operações concorrentes na carteira do usuário.

## Arquitetura e Tecnologias

Estruturei o projeto com padrão que hoje tenho usado, é uma arquitetura de 3 camadas.

- **`Models`**: Camada das Entidades. Contém as entidades do banco de dados (POCOs), os DTOs (Data Transfer Objects) para comunicação com a API e as interfaces de modelo.
- **`Engine`**: Camada de Regra de Negócios e Acesso a Dados. Utiliza o Entity Framework Core para o ORM e contém os `Services` que orquestram as regras de negócio da aplicação. As Migrations do banco de dados também estão aqui.
- **`WebApp`**: Camada de controllers. É o projeto ASP.NET Core que expõe os endpoints da API, gerencia a configuração, injeção de dependência e o pipeline de requisições HTTP.

### Tecnologias Utilizadas

- **.NET 6** e **ASP.NET Core 6**
- **Entity Framework Core 6**: Para o mapeamento objeto-relacional (ORM).
- **PostgreSQL**: O banco de dados de destino (hospedado no Neon).
- **JWT (JSON Web Tokens)**: Para autenticação.
- **Swagger (OpenAPI)**: Para documentação e teste interativo da API.

---

## Como Executar o Projeto

Siga os passos abaixo para configurar e executar a aplicação localmente.

### Pré-requisitos

- [.NET 6 SDK](https://dotnet.microsoft.com/pt-br/download/dotnet/6.0)
- Um editor de código como [Visual Studio 2022](https://visualstudio.microsoft.com/pt-br/vs/) ou [VS Code](https://code.visualstudio.com/).
- PostgreSQL (o appsettings com os environments enviarei em anexo no email).
- Por motivos de rapidez, acabei deixando o banco de dados no [Neon](https://neon.com/)

### 1. Configure o Banco de Dados

Coloque o appsettings na pasta WebApp
Se precisar, ajuste a string para a sua instância do banco.

### 2. Aplique as Migrações (Criar o Banco de Dados)

As Migrations do Entity Framework criarão o banco de dados e todas as tabelas necessárias.
Serão geradas automaticamente ao rodar a aplicação.
Também, será rodado uma seed de moedas, por padronização.

### 3. Execute a Aplicação

Ao abrir a solução utilizando o Visual Studio, add o WebApp como projeto de inicialização e execute (F5 ou Ctrl+F5).
A aplicação estará disponível em `https://localhost:7218` (ou outra porta, dependendo da configuração local).

---

## Como Testar a API

Após iniciar a aplicação, acesse a URL do Swagger para visualizar a documentação e interagir com os endpoints:

**URL do Swagger:** [https://localhost:7218/swagger](https://localhost:7218/swagger)

### Fluxo de Teste Recomendado

1. **Crie um Usuário**:
    - Use o endpoint `POST /api/SignUp`.
    - Forneça `username`, `email` `password`, `document` e `phoneNumber`.

2. **Faça Login para Obter um Token**:
    - Use o endpoint `POST /api/SignIn`.
    - Forneça o `username` e `password` criados.
    - A resposta incluirá um `token` JWT. Copie este token.
    - Também será retornado informações da carteira do usuário.

3. **Autorize as Requisições**:
    - No Swagger, clique no botão "Authorize" no canto superior direito.
    - Na janela que abrir, cole o token no formato: `Bearer SEU_TOKEN_AQUI`.
    - Clique em "Authorize". Agora você pode acessar os endpoints protegidos.

4. **Teste os Endpoints Protegidos**:

   ## User Endpoints

    - **`GET /api/User/{userId}`**: Veja os detalhes do seu usuário.
    - **`GET /api/User/{userId}/wallet`**: Veja os detalhes da sua carteira.

   ## Bet Endpoints

    - **`GET /api/Bet/{betId}/execute`**: Execute a aposta que esta pendente.
    - **`GET /api/Bet/{betId}`**: Veja os detalhes da sua aposta.
    - **`GET /api/Bet/byUser/{userId}`**: Veja todas as suas apostas.
    - **`GET /api/Bet/all`**: Veja todas as apostas.
    - **`POST /api/Bet`**: Crie uma nova aposta, fornecendo `amount` e `autoPlayOnCreate`, o que permitirá que a aposta seja executada após criar.
    - **`PUT /api/Bet/{betId}/cancel`**: Cancele uma aposta existente, se a mesma ainda estiver como pendente e estiver valor disponivel no "bloqueio" da carteira.

   ## Transaction Endpoints

    - **`GET /api/Transaction/byUser/{userId}`**: Veja o histórico de transações do usuário.
    - **`GET /api/Transaction/all`**: Veja todas as transações.
    - **`POST /api/Transaction/movement`**: Crie um novo movimento na carteira, sendo ele um Deposito ou uma Retirada.

---

## Lógica de Negócio (Services e Controllers)

### Controllers (`WebApp/Controllers`)

- **Responsabilidade**: Receber requisições HTTP, validar os dados de entrada (DTOs), chamar o `Service` correspondente para executar a lógica de negócio e retornar uma resposta HTTP.
- **`SignUpController` e `SignInController`**: Lidam exclusivamente com o fluxo de criação de conta e autenticação, retornando o token JWT no sucesso.
- **`ProtectedController`**: Controller base que aplica o atributo `[Authorize]`, garantindo que todos os controllers que herdam dele exijam um token JWT válido.
- **`BetController`, `UserController`, `TransactionController`**: Contêm os endpoints relacionados às suas respectivas entidades, seguindo os princípios REST.

### Services (`Engine/Services`)

- **Responsabilidade**: Implementar as regras de negócio centrais da aplicação. Eles são agnósticos ao protocolo HTTP e focam puramente na lógica.
- **`UserService`**: Gerencia a criação de usuários, validação de credenciais e hashing de senhas. Ao criar um usuário, ele também inicializa uma carteira (`Wallet`) com saldos zerados para as moedas padrão.
- **`WalletService`**: Lida com as operações na carteira do usuário. A lógica de débito e crédito é centralizada aqui para garantir consistência.
- **`BetService`**: Contém a lógica do jogo.
    1. Ao receber uma aposta, ele primeiro verifica se o usuário tem saldo suficiente na carteira.
    2. Se o saldo for suficiente, um débito é realizado na carteira.
    3. A lógica de "sorteio" é executada (`Engine\Helpers\WinRandomHelper.cs`).
    4. Se o usuário ganhar, o dobro do valor apostado é creditado de volta na carteira.
    5. Uma nova `Transaction` é registrada para auditar a operação (seja ganho ou perda).
    6. Se a aposta for perdida, o valor é simplesmente debitado porem se for o 5º jogo perdido consecutivamente, o usuário recebe um bônus de consolação com o valor de 10% das mesmas 5 apostas.
- **`TransactionService`**: Responsável por consultar e retornar o histórico de transações de um usuário e criar novos movimentos na carteira.
