using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using TacadaApp.Models;
using TacadaApp.Data;

namespace TacadaApp
{
    public partial class ConfiguracoesPage : ContentPage
    {
        private DatabaseService _dbService;
        public ObservableCollection<ProdutoCardapio> ProdutosCardapio { get; set; }

        public ConfiguracoesPage()
        {
            InitializeComponent();

            _dbService = new DatabaseService();
            ProdutosCardapio = new ObservableCollection<ProdutoCardapio>();
            ListaCardapio.ItemsSource = ProdutosCardapio;
        }

        // Carrega o cardápio salvo no banco ao abrir a tela
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var itensBanco = await _dbService.GetCardapioAsync();

            ProdutosCardapio.Clear();
            foreach (var item in itensBanco)
            {
                ProdutosCardapio.Add(item);
            }
        }

        // Fluxo de criação e salvamento de um novo botão no cardápio
        private async void OnAdicionarProdutoClicked(object sender, EventArgs e)
        {
            string nome = await DisplayPromptAsync("Novo Botão", "Digite o nome do produto (ex: Porção de Fritas):");
            if (string.IsNullOrWhiteSpace(nome)) return;

            string precoStr = await DisplayPromptAsync("Preço", $"Digite o valor para {nome} (ex: 25,50):", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(precoStr)) return;

            if (decimal.TryParse(precoStr, out decimal preco))
            {
                string corEscolhida = await DisplayActionSheet("Escolha a cor do botão", "Cancelar", null, "Azul", "Laranja", "Vermelho", "Roxo", "Verde", "Cinza");

                if (corEscolhida == "Cancelar" || string.IsNullOrEmpty(corEscolhida)) return;

                string corHex = "#2196F3"; // Azul padrão
                if (corEscolhida == "Laranja") corHex = "#FF9800";
                else if (corEscolhida == "Vermelho") corHex = "#F44336";
                else if (corEscolhida == "Roxo") corHex = "#9C27B0";
                else if (corEscolhida == "Verde") corHex = "#4CAF50";
                else if (corEscolhida == "Cinza") corHex = "#607D8B";

                var novoItem = new ProdutoCardapio
                {
                    Nome = nome,
                    Preco = preco,
                    CorHexadecimal = corHex
                };

                await _dbService.SalvarProdutoCardapioAsync(novoItem);
                ProdutosCardapio.Add(novoItem);
            }
            else
            {
                await DisplayAlert("Erro", "Valor inválido. Digite apenas números e vírgula.", "OK");
            }
        }

        // Exclui o item selecionado do banco e da interface
        private async void OnRemoverProdutoClicked(object sender, EventArgs e)
        {
            var botao = (Button)sender;
            var itemParaRemover = (ProdutoCardapio)botao.CommandParameter;

            bool confirmar = await DisplayAlert("Remover", $"Deseja apagar '{itemParaRemover.Nome}' do cardápio?", "Sim", "Cancelar");

            if (confirmar)
            {
                await _dbService.DeletarProdutoCardapioAsync(itemParaRemover);
                ProdutosCardapio.Remove(itemParaRemover);
            }
        }
    }
}