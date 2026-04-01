using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using TacadaApp.Models;
using TacadaApp.Data;

namespace TacadaApp.ViewModels
{
    public class MainViewModel
    {
        // Dependências
        private readonly DatabaseService _dbService;

        // Dados da tela
        public ObservableCollection<Comanda> ComandasAtivas { get; set; }

        // Comandos
        public Command CarregarComandasCommand { get; set; }
        public Command NovaComandaCommand { get; set; }
        public Command AbrirMenuFlutuanteCommand { get; set; }
        public Command<Comanda> AbrirComandaCommand { get; set; }

        // Construtor
        public MainViewModel()
        {
            _dbService = new DatabaseService();
            ComandasAtivas = new ObservableCollection<Comanda>();

            CarregarComandasCommand = new Command(async () => await CarregarComandasAsync());
            NovaComandaCommand = new Command(async () => await NovaComandaAsync());
            AbrirMenuFlutuanteCommand = new Command(async () => await AbrirMenuFlutuanteAsync());
            AbrirComandaCommand = new Command<Comanda>(async (comanda) => await AbrirComandaAsync(comanda));
        }

        // ==========================================
        // MÉTODOS
        // ==========================================

        private async Task CarregarComandasAsync()
        {
            var comandasDoBanco = await _dbService.GetComandasAtivasAsync();

            ComandasAtivas.Clear();
            foreach (var comanda in comandasDoBanco)
            {
                ComandasAtivas.Add(comanda);
            }
        }

        private async Task NovaComandaAsync()
        {
            string nomeOuMesa = await Application.Current.MainPage.DisplayPromptAsync("Nova Comanda", "Digite o nome do cliente ou o número da mesa:");

            if (string.IsNullOrWhiteSpace(nomeOuMesa)) return;

            var novaComanda = new Comanda { Identificacao = nomeOuMesa };

            await _dbService.SalvarComandaAsync(novaComanda);
            ComandasAtivas.Add(novaComanda);
        }

        private async Task AbrirMenuFlutuanteAsync()
        {
            string acao = await Application.Current.MainPage.DisplayActionSheet("Menu do Tacada", "Cancelar", null, "Histórico de Comandas", "Configurar adição rápida", "Restaurar Backup");

            if (string.IsNullOrEmpty(acao) || acao == "Cancelar") return;

            if (acao == "Histórico de Comandas")
            {
                await Application.Current.MainPage.Navigation.PushAsync(new HistoricoPage());
            }
            else if (acao == "Configurar adição rápida")
            {
                await Application.Current.MainPage.Navigation.PushAsync(new ConfiguracoesPage());
            }
            else if (acao == "Restaurar Backup")
            {
                await ProcessarRestauracaoBackupAsync();
            }
        }

        private async Task AbrirComandaAsync(Comanda comandaClicada)
        {
            if (comandaClicada == null) return;

            await Application.Current.MainPage.Navigation.PushAsync(new DetalhesComandaPage(comandaClicada, ComandasAtivas));
        }

        private async Task ProcessarRestauracaoBackupAsync()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o arquivo de Backup do Tacada"
                });

                if (result == null) return;

                var backupService = new BackupService();
                var comandas = await backupService.LerBackupJsonAsync(result.FullPath);

                if (comandas == null || comandas.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", "O arquivo selecionado está vazio ou não é um backup válido.", "OK");
                    return;
                }

                await _dbService.RestaurarBackupAsync(comandas);
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Backup restaurado com sucesso!", "OK");

                await CarregarComandasAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao restaurar: {ex.Message}", "OK");
            }
        }
    }
}