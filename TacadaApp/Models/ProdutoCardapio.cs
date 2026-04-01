using SQLite;

namespace TacadaApp.Models
{
    [Table("Cardapio")]
    public class ProdutoCardapio
    {
        // Pk
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Dados da adic. rapida
        public string Nome { get; set; }
        public decimal Preco { get; set; }

        // Cor do botão
        public string CorHexadecimal { get; set; }
    }
}