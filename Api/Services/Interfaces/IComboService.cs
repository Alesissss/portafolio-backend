using Api.Dtos;

namespace Api.Services.Interfaces
{
    // Contrato de los "combos": listados mínimos (id + texto) para poblar selects.
    // Aquí se irán agregando los demás a medida que se necesiten (productos, roles, etc.).
    public interface IComboService
    {
        public Task<List<ComboDto>> GetCategoriasComboAsync();
        public Task<List<ComboDto>> GetRolesComboAsync();
    }
}
