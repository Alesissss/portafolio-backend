namespace Api.Dtos;

// DTO genérico para poblar <select> del frontend. Ambos campos son string porque el value de
// un select siempre es texto (para ids numéricos se usa .ToString()). Los nombres Value/Label
// calzan con lo que espera Mantine, así el front los usa sin transformar.
public record ComboDto(
    string Value,
    string Label
);

// Dto con más datos
public record ProductoComboDto(
    string Value,
    string Label,
    decimal Precio,
    decimal Stock
);