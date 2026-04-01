using System;
using Microsoft.Maui.Controls;
using TacadaApp.ViewModels;

namespace TacadaApp;

public partial class MainPage : ContentPage
{
    // Dependência (ViewModel)
    private MainViewModel _viewModel;

    // Construtor
    public MainPage()
    {
        InitializeComponent();

        // Inicialização e vinculação de dados (Data Binding)
        _viewModel = new MainViewModel();
        BindingContext = _viewModel;
    }

    // Evento: Carrega a lista de comandas ativas ao exibir a tela
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.CarregarComandasCommand != null)
        {
            _viewModel.CarregarComandasCommand.Execute(null);
        }
    }
}