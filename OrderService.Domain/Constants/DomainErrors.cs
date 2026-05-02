namespace OrderService.Domain.Constants;

public static class DomainErrors
{
  public static class Order
  {
    public const string CannotPlaceEmptyOrder = "Não é possível fechar um pedido sem itens.";
    public const string CannotAddItemsWhenNotDraft = "Não é possível adicionar itens a um pedido que não está em rascunho.";
    public const string OnlyDraftOrdersCanBePlaced = "Apenas pedidos em rascunho podem ser abertos.";
    public const string OnlyPlacedOrdersCanBeConfirmed = "Apenas pedidos abertos (Placed) podem ser confirmados.";
    public const string CannotCancelInCurrentStatus = "Este pedido não pode ser cancelado no status atual.";
    public const string CannotCreateOrderWithoutItems = "Não é possível criar um pedido sem itens.";
    public const string NotFound = "Pedido não encontrado.";
  }

  public static class OrderItem
  {
    public const string InvalidItemQuantity = "A quantidade do item deve ser maior que zero.";
    public const string InvalidItemPrice = "O preço do item deve ser maior que zero.";
  }

  public static class Product
  {
    public const string InvalidPrice = "Preço deve ser maior que zero.";
    public const string InvalidInitialQuantity = "Quantidade inicial não pode ser negativa.";
    public const string InvalidDecreaseQuantity = "A quantidade a ser baixada deve ser maior que zero.";
    public const string InvalidIncreaseQuantity = "A quantidade a ser reposta deve ser maior que zero.";
    public static string NotFound(Guid id) => $"Produto com ID {id} não encontrado.";
    public static string InsufficientStockForOrder(Guid id, int available) => $"Estoque insuficiente para o produto {id}. Disponivel: {available}";
    public static string InsufficientStock(int available) => $"Estoque insuficiente. Disponivel: {available}";
  }
}