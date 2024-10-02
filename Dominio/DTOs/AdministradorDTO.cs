using miminal_api.Dominio.Enums;

namespace miminal_api.Dominio.DTOs
{
    public class AdministradorDTO
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
        public Perfil? Pefil { get; set; } = default!;
    }
}
