using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using TacadaApp.Models;
using TacadaApp.ViewModels;

namespace TacadaApp;

public partial class DetalhesComandaPage : ContentPage
{
    // Dependência (ViewModel)
    private DetalhesComandaViewModel _viewModel;

    // Construtor
    public DetalhesComandaPage(Comanda comandaSelecionada, ObservableCollection<Comanda> listaPrincipal)
    {
        InitializeComponent();

        // Inicialização e vinculação de dados (Data Binding)
        _viewModel = new DetalhesComandaViewModel(comandaSelecionada, listaPrincipal);
        BindingContext = _viewModel;
    }

    // Evento: Carregar dados ao exibir a tela
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.CarregarCardapioCommand != null)
        {
            _viewModel.CarregarCardapioCommand.Execute(null);
        }
    }
}