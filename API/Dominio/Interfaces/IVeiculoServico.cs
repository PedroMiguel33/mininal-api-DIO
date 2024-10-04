using miminal_api.Dominio.DTOs;
using miminal_api.Dominio.Entidades;

namespace miminal_api.Dominio.Interfaces
{
    public interface IVeiculoServico
    {

        List<Veiculo>? Todos(int? pagina = 1, string? nome = null, string? marca = null);
        Veiculo? BuscarPorId(int id);
        void Salvar(Veiculo veiculo);
        void Atualizar(Veiculo veiculo);
        void Apagar(Veiculo veiculo);
    }
}