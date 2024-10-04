using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using miminal_api.Dominio.DTOs;
using miminal_api.Dominio.Entidades;

namespace miminal_api.Dominio.Interfaces
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        Administrador? Salvar(Administrador administrador);

        Administrador? BuscarPorId(int Id);
        List<Administrador> Todos(int? pagina);

    }
}