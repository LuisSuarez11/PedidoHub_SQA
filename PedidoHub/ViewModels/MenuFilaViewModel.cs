namespace PedidoHub.ViewModels;

public record MenuFilaItem(string Texto, string Url, bool Peligro = false);

// Acciones secundarias de una fila de lista (editar, eliminar...), agrupadas en un menú.
public record MenuFilaViewModel(IReadOnlyList<MenuFilaItem> Items);
