using miminal_api.Dominio.Enums;

namespace miminal_api.Dominio.ModelsViews
{
    public record AdministradorModelView
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string Perfil { get; set; } = default!;
    }
}
