namespace API_PROJECT_BEAUTY.DTOs
{
    // Una venta del historial de compras
    public class VentaResponseDto
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = null!;
        public List<VentaDetalleResponseDto> Detalle { get; set; } = new();
    }

    public class VentaDetalleResponseDto
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}