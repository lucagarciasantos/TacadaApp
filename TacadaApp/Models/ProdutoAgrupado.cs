namespace TacadaApp.Models
{
    public class ProdutoAgrupado
    {
        // Dados do agrup.
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal PrecoTotal { get; set; }

        // Texto formatado
        public string DescricaoTela => $"{Quantidade}x {Nome}";
    }
}