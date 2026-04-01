using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using TacadaApp.Models;

namespace TacadaApp.Data
{
    public class BackupService
    {
        public async Task<string> GerarBackupJsonAsync(object dadosParaSalvar, string nomeArquivo)
        {
            try
            {
                // Converte os dados para JSON
                var opcoes = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(dadosParaSalvar, opcoes);

                // Define o caminho e salva o arquivo
                string caminhoPasta = FileSystem.CacheDirectory;
                string caminhoCompleto = Path.Combine(caminhoPasta, $"{nomeArquivo}_{DateTime.Now:dd-MM-yyyy}.json");
                await File.WriteAllTextAsync(caminhoCompleto, jsonString);

                return caminhoCompleto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Comanda>> LerBackupJsonAsync(string caminhoArquivo)
        {
            try
            {
                // Lê o conteúdo do arquivo
                string jsonString = await File.ReadAllTextAsync(caminhoArquivo);

                // Converte o JSON de volta para lista
                var comandas = JsonSerializer.Deserialize<List<Comanda>>(jsonString);

                return comandas;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}