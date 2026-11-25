namespace API_PROJECT_BEAUTY.DTOs
{
    // Un producto dentro del carrito
    public class VentaItemDto
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
    }

    // Datos de tarjeta: SOLO se validan de formato, nunca se guardan en la BD
    public class DatosTarjetaDto
    {
        public string? Titular { get; set; }
        public string? Numero { get; set; }
        public string? Vencimiento { get; set; } // formato MM/AA
        public string? Cvv { get; set; }
    }

    // Lo que el frontend envía para crear una venta
    public class VentaCreateDto
    {
        public int IdMetodoPago { get; set; }
        public List<VentaItemDto> Items { get; set; } = new();
        public string? Comprobante { get; set; } // transferencia: número del comprobante (opcional)
        public DatosTarjetaDto? Tarjeta { get; set; } // solo si paga con tarjeta
    }
}