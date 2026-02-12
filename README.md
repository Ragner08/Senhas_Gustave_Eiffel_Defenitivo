# Senhas de Almoço EPGE

Sistema de Gestão de Senhas de Almoço para a Escola Gustave Eiffel.

## Descrição

Este é um sistema web desenvolvido em ASP.NET Core MVC para gestão de senhas de almoço, permitindo:
- Marcação de senhas através de um calendário interativo
- Gestão de carteira virtual
- Definição de refeições por parte dos funcionários
- Relatórios diários
- Gestão de utilizadores com diferentes escalões

## Tecnologias Utilizadas

- **Framework**: ASP.NET Core 8.0
- **Padrão**: MVC (Model-View-Controller)
- **Base de Dados**: SQL Server (LocalDB)
- **ORM**: Entity Framework Core
- **Autenticação**: ASP.NET Identity
- **Frontend**: Bootstrap 5, HTML5, CSS3, JavaScript
- **Design**: Tema personalizado com cores branco e laranja

## Requisitos

- .NET 8.0 SDK
- Visual Studio 2022 ou VS Code
- SQL Server LocalDB (incluído no Visual Studio)

## Instalação

### 1. Clonar ou extrair o projeto

```bash
cd Senhas_Gustave_Eiffel
```

### 2. Restaurar pacotes NuGet

```bash
dotnet restore
```

### 3. Instalar dependências client-side (LibMan)

```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
libman restore
```

### 4. Criar a base de dados

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Ou execute diretamente:

```bash
dotnet run
```

A base de dados será criada automaticamente na primeira execução.

## Utilizadores Padrão

O sistema cria automaticamente os seguintes utilizadores:

### Administrador
- **Email**: admin@epge.pt
- **Password**: Admin123!
- **Role**: Admin

### Funcionário
- **Email**: funcionario@epge.pt
- **Password**: Func123!
- **Role**: Funcionário

## Estrutura do Projeto

```
Senhas_Gustave_Eiffel/
├── Controllers/          # Controladores MVC
│   ├── AccountController.cs
│   ├── HomeController.cs
│   ├── CalendarController.cs
│   ├── WalletController.cs
│   └── BookingsController.cs
├── Models/               # Modelos de dados
│   ├── ApplicationUser.cs
│   ├── Meal.cs
│   ├── Booking.cs
│   ├── WalletTransaction.cs
│   └── ViewModels/
├── Views/                # Vistas Razor
│   ├── Account/
│   ├── Home/
│   ├── Calendar/
│   ├── Wallet/
│   ├── Bookings/
│   └── Shared/
├── Data/                 # Contexto da base de dados
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── wwwroot/              # Ficheiros estáticos
│   ├── css/
│   ├── js/
│   └── lib/
└── Properties/           # Configurações
```

## Funcionalidades

### Para Utilizadores

1. **Login**
   - Autenticação por email e password
   - Sessão persistente opcional

2. **Calendário**
   - Visualização mensal das refeições
   - Marcação de senhas (apenas um por dia)
   - Indicadores visuais de marcações e refeições

3. **Carteira Virtual**
   - Consulta de saldo
   - Adição de fundos (simulação)
   - Histórico de transações

4. **Minhas Senhas**
   - Lista de marcações
   - Cancelamento de marcações (com reembolso)

### Para Funcionários

1. **Gestão de Refeições**
   - Criar/editar refeições para cada dia
   - Definir sopa, prato principal, vegetariano e sobremesa
   - Configurar preços por escalão

2. **Relatórios**
   - Relatório diário de marcações
   - Estatísticas por escalão
   - Valor total recebido

### Para Administradores

1. **Gestão de Utilizadores**
   - Criar novos utilizadores (sem registro público)
   - Editar informações
   - Alterar passwords
   - Eliminar contas
   - Atribuir roles (Escalão A, B, Sem escalão, Funcionário)

2. **Gestão de Carteiras**
   - Ver saldo de todos os utilizadores
   - Adicionar fundos a qualquer utilizador
   - Ver todas as transações

## Roles do Sistema

- **Admin**: Acesso total ao sistema
- **Funcionário**: Gestão de refeições e relatórios
- **Escalão A**: Utilizador com preço reduzido (2€)
- **Escalão B**: Utilizador com preço intermédio (3€)
- **Sem escalão**: Utilizador com preço normal (4€)

## Preços por Escalão

| Escalão | Preço Padrão |
|---------|--------------|
| Escalão A | 2,00 € |
| Escalão B | 3,00 € |
| Sem escalão | 4,00 € |

*Os preços podem ser ajustados pelo funcionário ao criar a refeição.*

## Regras de Negócio

1. **Marcações**
   - Apenas uma senha por dia por utilizador
   - Não é possível marcar para dias passados
   - Requer saldo suficiente na carteira
   - O valor é descontado automaticamente

2. **Cancelamentos**
   - Possível apenas para datas futuras
   - O valor é reembolsado integralmente
   - Cria uma transação de reembolso

3. **Carteira**
   - Sistema virtual (sem pagamentos reais)
   - Apenas administradores podem adicionar fundos a outros utilizadores
   - Todas as transações são registadas

## Design

O sistema utiliza um tema personalizado com:
- **Fundo**: Branco (#FFFFFF)
- **Cor primária**: Laranja (#FF6B35)
- **Cor secundária**: Laranja escuro (#F7931E)
- **Botões**: Gradientes laranja
- **Cards**: Sombras suaves com bordas arredondadas
- **Calendário**: Design interativo com indicadores visuais

## Execução

### Visual Studio 2022
1. Abra o ficheiro `Senhas_Gustave_Eiffel.csproj`
2. Pressione `F5` ou clique em "IIS Express"

### VS Code
1. Abra a pasta do projeto
2. Execute no terminal:
```bash
dotnet run
```
3. Aceda a `https://localhost:7000` ou `http://localhost:5000`

### Linha de Comandos
```bash
dotnet run
```

## Migrações da Base de Dados

Para criar uma nova migração:
```bash
dotnet ef migrations add NomeDaMigracao
```

Para atualizar a base de dados:
```bash
dotnet ef database update
```

## Resolução de Problemas

### Erro de conexão à base de dados
- Verifique se o SQL Server LocalDB está instalado
- Execute: `sqllocaldb start MSSQLLocalDB`

### Dependências em falta
- Execute: `dotnet restore`
- Execute: `libman restore`

### Erro de permissões
- Execute o Visual Studio como Administrador
- Ou use a linha de comandos com privilégios elevados

## Suporte

Para questões ou problemas, contacte o administrador do sistema.

## Licença

Este projeto é propriedade da Escola Gustave Eiffel.

---

**Desenvolvido para**: Escola Gustave Eiffel  
**Ano**: 2024
