using SQLite;
using TacadaApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TacadaApp.Data
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _db;
        private readonly string dbPath = Path.Combine(FileSystem.AppDataDirectory, "tacada.db3");

        // Inicializa o banco e cria as tabelas
        private async Task Init()
        {
            if (_db != null) return;

            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<Comanda>();
            await _db.CreateTableAsync<Produto>();
            await _db.CreateTableAsync<ProdutoCardapio>();
        }

        // Retorna comandas abertas com seus itens
        public async Task<List<Comanda>> GetComandasAtivasAsync()
        {
            await Init();

            var comandasAbertas = await _db.Table<Comanda>().Where(c => c.IsFechada == false).ToListAsync();

            foreach (var comanda in comandasAbertas)
            {
                var produtosDestaComanda = await _db.Table<Produto>().Where(p => p.ComandaId == comanda.Id).ToListAsync();

                foreach (var produto in produtosDestaComanda)
                {
                    comanda.ItensConsumidos.Add(produto);
                }
            }

            return comandasAbertas;
        }

        // Retorna comandas fechadas ordenadas por data
        public async Task<List<Comanda>> GetComandasFechadasAsync()
        {
            await Init();

            var comandasFechadas = await _db.Table<Comanda>()
                                            .Where(c => c.IsFechada == true)
                                            .OrderByDescending(c => c.DataFechamento)
                                            .ToListAsync();

            foreach (var comanda in comandasFechadas)
            {
                var produtosDestaComanda = await _db.Table<Produto>().Where(p => p.ComandaId == comanda.Id).ToListAsync();

                foreach (var produto in produtosDestaComanda)
                {
                    comanda.ItensConsumidos.Add(produto);
                }
            }

            return comandasFechadas;
        }

        // Insere nova comanda ou atualiza existente
        public async Task SalvarComandaAsync(Comanda comanda)
        {
            await Init();

            if (comanda.Id != 0)
            {
                await _db.UpdateAsync(comanda);
            }
            else
            {
                await _db.InsertAsync(comanda);
            }
        }

        // IMPLEMENTAÇÃO FUTURA APÓS VALIDAÇÕES COM CLIENTE
        // Deleta a comanda e seus itens associados
        //public async Task DeletarComandaAsync(Comanda comanda)
        //{
            //await Init();

            //var produtosDestaComanda = await _db.Table<Produto>().Where(p => p.ComandaId == comanda.Id).ToListAsync();

            //foreach (var produto in produtosDestaComanda)
            //{
                //await _db.DeleteAsync(produto);
            //}

            //await _db.DeleteAsync(comanda);
        //}

        // ==========================================
        // CRUD DE PRODUTOS E CARDÁPIO
        // ==========================================

        // Salva item consumido na comanda
        public async Task SalvarProdutoAsync(Produto produto)
        {
            await Init();
            await _db.InsertAsync(produto);
        }

        // Remove item consumido da comanda
        public async Task DeletarProdutoAsync(Produto produto)
        {
            await Init();
            await _db.DeleteAsync(produto);
        }

        // Lista itens configurados no cardápio rápido
        public async Task<List<ProdutoCardapio>> GetCardapioAsync()
        {
            await Init();
            return await _db.Table<ProdutoCardapio>().ToListAsync();
        }

        // Salva ou atualiza item no cardápio
        public async Task SalvarProdutoCardapioAsync(ProdutoCardapio item)
        {
            await Init();

            if (item.Id != 0)
            {
                await _db.UpdateAsync(item);
            }
            else
            {
                await _db.InsertAsync(item);
            }
        }

        // Remove item do cardápio
        public async Task DeletarProdutoCardapioAsync(ProdutoCardapio item)
        {
            await Init();
            await _db.DeleteAsync(item);
        }

        // Restaura dados do backup evitando duplicidade
        public async Task RestaurarBackupAsync(List<Comanda> comandasRestauradas)
        {
            await Init();

            foreach (var comanda in comandasRestauradas)
            {
                await _db.InsertOrReplaceAsync(comanda);

                if (comanda.ItensConsumidos != null)
                {
                    foreach (var item in comanda.ItensConsumidos)
                    {
                        await _db.InsertOrReplaceAsync(item);
                    }
                }
            }
        }
    }
}
