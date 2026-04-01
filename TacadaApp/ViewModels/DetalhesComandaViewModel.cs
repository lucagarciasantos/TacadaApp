using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using TacadaApp.Models;
using TacadaApp.Data;

namespace TacadaApp.ViewModels
{
    public class DetalhesComandaViewModel : INotifyPropertyChanged
    {
        // Dependências
        private readonly DatabaseService _dbService;
        private ObservableCollection<Comanda> _listaPrincipal;

        // Dados da tela
        public Comanda ComandaAtual { get; set; }
        public ObservableCollection<ProdutoCardapio> BotoesCardapio { get; set; }
        public ObservableCollection<ProdutoAgrupado> ItensAgrupados { get; set; }

        // Totais visuais
        private string _textoTotalConsumido;
        public string TextoTotalConsumido
        {
            get => _textoTotalConsumido;
            set { _textoTotalConsumido = value; OnPropertyChanged(); }
        }

        private string _textoTotalPago;
        public string TextoTotalPago
        {
            get => _textoTotalPago;
            set { _textoTotalPago = value; OnPropertyChanged(); }
        }

        private string _textoFaltaPagar;
        public string TextoFaltaPagar
        {
            get => _textoFaltaPagar;
            set { _textoFaltaPagar = value; OnPropertyChanged(); }
        }

        // Comandos
        public Command CarregarCardapioCommand { get; set; }
        public Command<ProdutoCardapio> AdicionarItemCardapioCommand { get; set; }
        public Command AddOutroCommand { get; set; }
        public Command<ProdutoAgrupado> RemoverItemCommand { get; set; }
        public Command FecharContaCommand { get; set; }
        public Command PagamentoParcialCommand { get; set; }
        public Command CompartilharWhatsAppCommand { get; set; }

        // Construtor
        public DetalhesComandaViewModel(Comanda comandaSelecionada, ObservableCollection<Comanda> listaPrincipal)
        {
            _dbService = new DatabaseService();

            ComandaAtual = comandaSelecionada;
            _listaPrincipal = listaPrincipal;

            BotoesCardapio = new ObservableCollection<ProdutoCardapio>();
            ItensAgrupados = new ObservableCollection<ProdutoAgrupado>();

            CarregarCardapioCommand = new Command(async () => await CarregarCardapioAsync());
            AdicionarItemCardapioCommand = new Command<ProdutoCardapio>(async (item) => await AdicionarItemCardapioAsync(item));
            AddOutroCommand = new Command(async () => await AddOutroAsync());
            RemoverItemCommand = new Command<ProdutoAgrupado>(async (grupo) => await RemoverItemAsync(grupo));
            FecharContaCommand = new Command(async () => await FecharContaAsync());
            PagamentoParcialCommand = new Command(async () => await PagamentoParcialAsync());
            CompartilharWhatsAppCommand = new Command(async () => await CompartilharWhatsAppAsync());

            AtualizarResumoNaTela();
        }

        // ==========================================
        // MÉTODOS
        // ==========================================

        private void AtualizarResumoNaTela()
        {
            TextoTotalConsumido = $"R$ {ComandaAtual.TotalConsumido:F2}";
            TextoTotalPago = $"R$ {ComandaAtual.TotalPago:F2}";
            TextoFaltaPagar = $"R$ {ComandaAtual.TotalConta:F2}";

            var listaAgrupada = ComandaAtual.ItensConsumidos
                .GroupBy(p => p.Nome)
                .Select(grupo => new ProdutoAgrupado
                {
                    Nome = grupo.Key,
                    Quantidade = grupo.Count(),
                    PrecoUnitario = grupo.First().Preco,
                    PrecoTotal = grupo.Sum(p => p.Preco)
                }).ToList();

            ItensAgrupados.Clear();
            foreach (var item in listaAgrupada)
            {
                ItensAgrupados.Add(item);
            }
        }

        private async Task CarregarCardapioAsync()
        {
            var itensBanco = await _dbService.GetCardapioAsync();
            BotoesCardapio.Clear();
            foreach (var item in itensBanco)
            {
                BotoesCardapio.Add(item);
            }
        }

        private async Task AdicionarItemCardapioAsync(ProdutoCardapio produtoCardapio)
        {
            if (produtoCardapio == null) return;

            string result = await Application.Current.MainPage.DisplayPromptAsync("Quantidade", $"Quantas unidades de {produtoCardapio.Nome}?", keyboard: Keyboard.Numeric);

            if (int.TryParse(result, out int quantidade) && quantidade > 0)
            {
                for (int i = 0; i < quantidade; i++)
                {
                    var novoItemConsumido = new Produto
                    {
                        Nome = produtoCardapio.Nome,
                        Preco = produtoCardapio.Preco,
                        ComandaId = ComandaAtual.Id
                    };

                    await _dbService.SalvarProdutoAsync(novoItemConsumido);
                    ComandaAtual.ItensConsumidos.Add(novoItemConsumido);
                }

                AtualizarResumoNaTela();
            }
        }

        private async Task AddOutroAsync()
        {
            string nome = await Application.Current.MainPage.DisplayPromptAsync("Outro Produto", "Digite o nome do item consumido:");
            if (string.IsNullOrWhiteSpace(nome)) return;

            string precoStr = await Application.Current.MainPage.DisplayPromptAsync("Preço", $"Digite o valor unitário para {nome} (ex: 15,50):", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(precoStr)) return;

            if (decimal.TryParse(precoStr, out decimal preco))
            {
                string qtdStr = await Application.Current.MainPage.DisplayPromptAsync("Quantidade", $"Quantas unidades de {nome}?", keyboard: Keyboard.Numeric);
                if (string.IsNullOrWhiteSpace(qtdStr)) return;

                if (int.TryParse(qtdStr, out int quantidade) && quantidade > 0)
                {
                    for (int i = 0; i < quantidade; i++)
                    {
                        var produto = new Produto
                        {
                            Nome = nome,
                            Preco = preco,
                            ComandaId = ComandaAtual.Id
                        };
                        await _dbService.SalvarProdutoAsync(produto);
                        ComandaAtual.ItensConsumidos.Add(produto);
                    }
                    AtualizarResumoNaTela();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Quantidade inválida. Por favor, digite um número inteiro maior que zero.", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Valor inválido. Use apenas números e vírgula.", "OK");
            }
        }

        private async Task RemoverItemAsync(ProdutoAgrupado grupoSelecionado)
        {
            if (grupoSelecionado == null) return;

            int quantidadeParaRemover = 0;

            if (grupoSelecionado.Quantidade == 1)
            {
                quantidadeParaRemover = 1;
            }
            else
            {
                string acao = await Application.Current.MainPage.DisplayActionSheet(
                    $"Remover {grupoSelecionado.Nome}?", "Cancelar", null,
                    "Remover 1 unidade", $"Remover TODOS ({grupoSelecionado.Quantidade})", "Digitar quantidade");

                if (string.IsNullOrEmpty(acao) || acao == "Cancelar") return;

                if (acao == "Remover 1 unidade")
                {
                    quantidadeParaRemover = 1;
                }
                else if (acao == $"Remover TODOS ({grupoSelecionado.Quantidade})")
                {
                    quantidadeParaRemover = grupoSelecionado.Quantidade;
                }
                else if (acao == "Digitar quantidade")
                {
                    string qtdStr = await Application.Current.MainPage.DisplayPromptAsync("Quantidade", $"Quantos remover? (Máximo: {grupoSelecionado.Quantidade})", keyboard: Keyboard.Numeric);
                    if (string.IsNullOrWhiteSpace(qtdStr)) return;

                    if (int.TryParse(qtdStr, out int qtdDigitada))
                    {
                        if (qtdDigitada <= 0) return;
                        if (qtdDigitada > grupoSelecionado.Quantidade)
                        {
                            await Application.Current.MainPage.DisplayAlert("Erro", $"Você não pode remover {qtdDigitada}.", "OK");
                            return;
                        }
                        quantidadeParaRemover = qtdDigitada;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (quantidadeParaRemover > 0)
            {
                var itensDesseGrupo = ComandaAtual.ItensConsumidos
                    .Where(p => p.Nome == grupoSelecionado.Nome && p.Preco == grupoSelecionado.PrecoUnitario)
                    .ToList();

                var itensParaDeletar = itensDesseGrupo.Take(quantidadeParaRemover).ToList();

                foreach (var item in itensParaDeletar)
                {
                    await _dbService.DeletarProdutoAsync(item);
                    ComandaAtual.ItensConsumidos.Remove(item);
                }
                AtualizarResumoNaTela();
            }
        }

        private async Task FecharContaAsync()
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert("Fechar Conta", $"Deseja fechar a conta no valor de R$ {ComandaAtual.TotalConta:F2}?", "Sim, Fechar", "Cancelar");

            if (confirmar)
            {
                ComandaAtual.IsFechada = true;
                ComandaAtual.DataFechamento = DateTime.Now;

                await _dbService.SalvarComandaAsync(ComandaAtual);

                _listaPrincipal.Remove(ComandaAtual);
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        private async Task PagamentoParcialAsync()
        {
            string valorStr = await Application.Current.MainPage.DisplayPromptAsync("Pagamento", $"Total atual: R$ {ComandaAtual.TotalConta:F2}\nQuanto o cliente está pagando?", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(valorStr)) return;

            if (decimal.TryParse(valorStr, out decimal valorPago))
            {
                if (valorPago <= 0) return;
                if (valorPago > ComandaAtual.TotalConta)
                {
                    await Application.Current.MainPage.DisplayAlert("Aviso", "Valor maior do que o total da conta!", "OK");
                    return;
                }

                var produtoPagamento = new Produto
                {
                    Nome = "Pagamento Parcial",
                    Preco = -valorPago,
                    ComandaId = ComandaAtual.Id
                };

                await _dbService.SalvarProdutoAsync(produtoPagamento);
                ComandaAtual.ItensConsumidos.Add(produtoPagamento);

                AtualizarResumoNaTela();
            }
        }

        private async Task CompartilharWhatsAppAsync()
        {
            if (ComandaAtual.ItensConsumidos == null || ComandaAtual.ItensConsumidos.Count == 0) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🎱 *TACADA SNOOKER BAR* 🎱");
            sb.AppendLine($"Comanda: *{ComandaAtual.Identificacao}*");
            sb.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy} às {DateTime.Now:HH:mm}");
            sb.AppendLine("-----------------------------------");
            sb.AppendLine("*ITENS CONSUMIDOS:*");

            foreach (var item in ItensAgrupados)
            {
                sb.AppendLine($"{item.Quantidade}x {item.Nome} - R$ {item.PrecoTotal:F2}");
            }

            sb.AppendLine("-----------------------------------");
            sb.AppendLine($"Subtotal: R$ {ComandaAtual.TotalConsumido:F2}");

            if (ComandaAtual.TotalPago > 0)
            {
                sb.AppendLine($"Valor já pago: R$ {ComandaAtual.TotalPago:F2}");
            }

            sb.AppendLine($"*TOTAL A PAGAR: R$ {ComandaAtual.TotalConta:F2}*");
            sb.AppendLine("-----------------------------------");
            sb.AppendLine("CHAVE PIX: 84703431904");
            sb.AppendLine("-----------------------------------");
            sb.AppendLine("Obrigado pela preferência e volte sempre! 🍻");

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = sb.ToString(),
                Title = "Enviar Comanda"
            });
        }

        // ==========================================
        // EVENTOS INOTIFYPROPERTYCHANGED
        // ==========================================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}