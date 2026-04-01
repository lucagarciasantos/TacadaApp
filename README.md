# 🎱 TacadaApp

Sistema de gerenciamento de comandas para o **Tacada Snooker Bar**, desenvolvido em **C# com .NET MAUI**. Roda diretamente no celular Android/iOS, funciona 100% offline com banco de dados SQLite local.

---

## Sobre o projeto

O TacadaApp nasceu da necessidade real de controlar as comandas do bar de forma simples e rápida. O garçom abre uma comanda por mesa ou cliente, registra os itens consumidos através de botões rápidos do cardápio, registra pagamentos parciais, fecha a conta e exporta o histórico como backup JSON.

---

## Funcionalidades

- Abrir e gerenciar múltiplas comandas simultaneamente
- Cardápio rápido com botões coloridos configuráveis
- Adição manual de itens com nome, preço e quantidade
- Pagamentos parciais com cálculo automático do saldo devedor
- Fechamento de conta com confirmação
- Compartilhamento da comanda via WhatsApp ou qualquer app do celular
- Histórico completo de comandas fechadas
- Exportação e restauração de backup em formato JSON

---

## Tecnologias utilizadas

| Tecnologia | Uso |
|---|---|
| C# / .NET MAUI | Framework principal, UI multiplataforma |
| SQLite-net-pcl | Banco de dados local no dispositivo |
| System.Text.Json | Serialização/desserialização do backup |
| MVVM Pattern | Arquitetura de separação de responsabilidades |

---

## Arquitetura

O projeto segue o padrão **MVVM (Model — View — ViewModel)**, separando dados, lógica e interface visual em camadas independentes.

```
TacadaApp/
├── Models/
│   ├── Comanda.cs              # Entidade principal — mesa/cliente
│   ├── Produto.cs              # Item consumido ou pagamento parcial
│   ├── ProdutoAgrupado.cs      # Modelo de exibição (apenas memória)
│   └── ProdutoCardapio.cs      # Botão do cardápio rápido
│
├── Data/
│   ├── DatabaseService.cs      # Toda comunicação com o SQLite
│   └── BackupService.cs        # Exportação e importação JSON
│
├── ViewModels/
│   ├── MainViewModel.cs        # Lógica da tela principal
│   └── DetalhesComandaViewModel.cs  # Lógica da tela de detalhes
│
└── Views/
    ├── MainPage.xaml/.cs           # Tela principal — lista de comandas
    ├── DetalhesComandaPage.xaml/.cs # Tela de detalhes da comanda
    ├── HistoricoPage.xaml/.cs       # Histórico de comandas fechadas
    └── ConfiguracoesPage.xaml/.cs   # Configuração do cardápio rápido
```

---

## Como o MVVM está aplicado

**Model** — classes puras de dados (`Comanda`, `Produto`). Não sabem que a tela existe. Contêm apenas propriedades, cálculos sobre si mesmas e atributos de banco.

**ViewModel** — contém toda a lógica de negócio. Expõe `ObservableCollection` para as listas e `Command` para os botões via Data Binding. Notifica a tela de mudanças através do `INotifyPropertyChanged`.

**View** — code-behind enxuto. Instancia o ViewModel, define o `BindingContext` e chama comandos no `OnAppearing()`. Sem lógica de negócio.

---

## Decisões de design relevantes

**Pagamento parcial como produto negativo**
Pagamentos parciais são salvos como `Produto` com `Preco` negativo (ex: `-30.00`). As propriedades calculadas da `Comanda` distinguem automaticamente consumo de pagamento pelo sinal do valor, sem necessidade de uma tabela separada.

```csharp
// TotalConsumido soma só valores positivos (itens reais)
if (produto.Preco > 0) total += produto.Preco;

// TotalPago soma os absolutos dos negativos (pagamentos)
if (produto.Preco < 0) total += Math.Abs(produto.Preco);

// TotalConta = soma de tudo = saldo devedor
```

**Inicialização preguiçosa do banco**
O `DatabaseService` só cria a conexão e as tabelas na primeira operação, não ao abrir o app. O método `Init()` é `private` e chamado internamente por todos os métodos públicos.

**Lista compartilhada entre telas**
A `DetalhesComandaPage` recebe por parâmetro a mesma referência da `ObservableCollection` da `MainPage`. Ao fechar uma conta, o ViewModel remove a comanda dessa lista — o card some da tela principal sem recarregar dados.

---

## Dívidas técnicas conhecidas

- `HistoricoPage` e `ConfiguracoesPage` acessam o `DatabaseService` diretamente, sem ViewModel — violam o MVVM e têm um `HistoricoViewModel.cs` vazio aguardando implementação
- O `DatabaseService` é instanciado com `new` em cada classe — o ideal seria injeção de dependência (Singleton via `MauiProgram.cs`)
- Ausência de `try/catch` nas operações de banco dentro das Views
- A chave PIX está hardcoded no `DetalhesComandaViewModel` — deveria vir de uma tela de configurações

---

## Pré-requisitos para rodar

- .NET 8 SDK ou superior
- .NET MAUI instalado (`dotnet workload install maui`)
- Android SDK (para emulador ou dispositivo físico Android)
- Visual Studio 2022 ou JetBrains Rider com suporte a MAUI

---

## Pacotes NuGet utilizados

```xml
<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.6" />
```
