using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace TacadaApp.Models
{
    [Table("Comandas")]
    public class Comanda : INotifyPropertyChanged
    {
        // Colunas do banco
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Identificacao { get; set; }
        public bool IsFechada { get; set; }
        public DateTime DataFechamento { get; set; }

        // Listas em memória
        [Ignore]
        public ObservableCollection<Produto> ItensConsumidos { get; set; }

        // Cálculos dinâmicos

        // Saldo final
        [Ignore]
        public decimal TotalConta
        {
            get
            {
                if (ItensConsumidos == null) return 0;

                decimal total = 0;
                foreach (var produto in ItensConsumidos)
                {
                    total += produto.Preco;
                }
                return total;
            }
        }

        // Soma de consumo (preços > 0)
        [Ignore]
        public decimal TotalConsumido
        {
            get
            {
                if (ItensConsumidos == null) return 0;

                decimal total = 0;
                foreach (var produto in ItensConsumidos)
                {
                    if (produto.Preco > 0)
                    {
                        total += produto.Preco;
                    }
                }
                return total;
            }
        }

        // Soma de pagamentos (preços < 0 convertidos para positivo)
        [Ignore]
        public decimal TotalPago
        {
            get
            {
                if (ItensConsumidos == null) return 0;

                decimal total = 0;
                foreach (var produto in ItensConsumidos)
                {
                    if (produto.Preco < 0)
                    {
                        total += Math.Abs(produto.Preco);
                    }
                }
                return total;
            }
        }

        // Data formatada para visualização
        [Ignore]
        public string DataFormatada => DataFechamento.ToString("dd/MM/yyyy 'às' HH:mm");

        // Texto agrupado para recibos e compartilhamento
        [Ignore]
        public string ResumoProdutos
        {
            get
            {
                if (ItensConsumidos == null || ItensConsumidos.Count == 0) return "Nenhum item consumido";

                var contagem = new Dictionary<string, int>();

                foreach (var produto in ItensConsumidos)
                {
                    if (produto.Preco > 0)
                    {
                        if (contagem.ContainsKey(produto.Nome))
                            contagem[produto.Nome]++;
                        else
                            contagem[produto.Nome] = 1;
                    }
                }

                if (contagem.Count == 0) return "Nenhum item consumido";

                var textos = new List<string>();
                foreach (var item in contagem)
                {
                    textos.Add($"{item.Value}x {item.Key}");
                }

                return string.Join(", ", textos);
            }
        }

        // MVVM
        public event PropertyChangedEventHandler PropertyChanged;

        public Comanda()
        {
            ItensConsumidos = new ObservableCollection<Produto>();

            // Atualiza propriedades calculadas na tela ao alterar a lista
            ItensConsumidos.CollectionChanged += (sender, e) =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalConta)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalConsumido)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalPago)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResumoProdutos)));
            };
        }
    }
}