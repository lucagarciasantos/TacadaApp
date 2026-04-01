using SQLite;

namespace TacadaApp.Models
{
    [Table("Produtos")]
    public class Produto
    {
        // Pk
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Fk
        public int ComandaId { get; set; }

        // Dados do item consumido
        public string Nome { get; set; }
        public decimal Preco { get; set; }
    }
}