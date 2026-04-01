using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using TacadaApp.Models;
using TacadaApp.Data;

namespace TacadaApp;

public partial class HistoricoPage : ContentPage
{
    // Dependências
    private DatabaseService _dbService;

    // Dados da tela
    public ObservableCollection<Comanda> HistoricoComandas { get; set; }

    // Construtor
    public HistoricoPage()
    {
        InitializeComponent();

        _dbService = new DatabaseService();
        HistoricoComandas = new ObservableCollection<Comanda>();
        ListaHistorico.ItemsSource = HistoricoComandas;
    }

    // Evento: Carrega o histórico de comandas fechadas ao abrir a tela
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var comandasFechadas = await _dbService.GetComandasFechadasAsync();

        HistoricoComandas.Clear();
        foreach (var c in comandasFechadas)
        {
            HistoricoComandas.Add(c);
        }
    }

    // Evento: Geração e compartilhamento do arquivo de backup (JSON)
    private async void OnExportarBackupClicked(object sender, EventArgs e)
    {
        var comandasFechadas = await _dbService.GetComandasFechadasAsync();

        // Validação de segurança
        if (comandasFechadas.Count == 0)
        {
            await DisplayAlert("Aviso", "Não há comandas fechadas para exportar.", "OK");
            return;
        }

        // Processamento do backup
        var backupService = new BackupService();
        string caminhoArquivo = await backupService.GerarBackupJsonAsync(comandasFechadas, "Backup_Tacada");

        // Compartilhamento nativo ou tratamento de erro
        if (caminhoArquivo != null)
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Backup do Caixa - Tacada",
                File = new ShareFile(caminhoArquivo)
            });
        }
        else
        {
            await DisplayAlert("Erro", "Ocorreu um problema ao gerar o arquivo de backup.", "OK");
        }
    }
}