using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;


namespace API_PROJECT_BEAUTY.Models;

public partial class ProjectBeautyContext : DbContext
{
    public ProjectBeautyContext()
    {
    }

    public ProjectBeautyContext(DbContextOptions<ProjectBeautyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoriaProducto> CategoriaProductos { get; set; }

    public virtual DbSet<CategoriaServicio> CategoriaServicios { get; set; }

    public virtual DbSet<CitaServicio> CitaServicios { get; set; }

    public virtual DbSet<Citum> Cita { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<DetalleVentum> DetalleVenta { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<MetodoPago> MetodoPagos { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<PagoVentum> PagoVenta { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<TipoIdentificacion> TipoIdentificacions { get; set; }
    public virtual DbSet<UsuarioToken> Usuario_Token { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }
    public object Usuario { get; internal set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=projectbeauty;user=root;password=2108", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.45-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaProducto).HasName("PRIMARY");

            entity.ToTable("categoria_producto");

            entity.HasIndex(e => e.NombreCategoria, "NOMBRE_CATEGORIA").IsUnique();

            entity.Property(e => e.IdCategoriaProducto).HasColumnName("ID_CATEGORIA_PRODUCTO");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(50)
                .HasColumnName("NOMBRE_CATEGORIA");
        });

        modelBuilder.Entity<CategoriaServicio>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaServicio).HasName("PRIMARY");

            entity.ToTable("categoria_servicio");

            entity.HasIndex(e => e.NombreCategoria, "NOMBRE_CATEGORIA").IsUnique();

            entity.Property(e => e.IdCategoriaServicio).HasColumnName("ID_CATEGORIA_SERVICIO");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(50)
                .HasColumnName("NOMBRE_CATEGORIA");
        });

        modelBuilder.Entity<CitaServicio>(entity =>
        {
            entity.HasKey(e => e.IdCitaServicio).HasName("PRIMARY");

            entity.ToTable("cita_servicio");

            entity.HasIndex(e => e.IdCita, "FK_CS_CITA");

            entity.HasIndex(e => e.IdServicio, "FK_CS_SERVICIO");

            entity.Property(e => e.IdCitaServicio).HasColumnName("ID_CITA_SERVICIO");
            entity.Property(e => e.IdCita).HasColumnName("ID_CITA");
            entity.Property(e => e.IdServicio).HasColumnName("ID_SERVICIO");
            entity.Property(e => e.PrecioAplicado)
                .HasPrecision(10, 2)
                .HasColumnName("PRECIO_APLICADO");

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.CitaServicios)
                .HasForeignKey(d => d.IdCita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CS_CITA");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.CitaServicios)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CS_SERVICIO");
        });

        modelBuilder.Entity<Citum>(entity =>
        {
            entity.HasKey(e => e.IdCita).HasName("PRIMARY");

            entity.ToTable("cita");

            entity.HasIndex(e => e.IdCliente, "FK_CITA_CLIENTE");

            entity.HasIndex(e => e.IdEstado, "FK_CITA_ESTADO");

            entity.HasIndex(e => e.IdUsuario, "FK_CITA_USUARIO");

            entity.Property(e => e.IdCita).HasColumnName("ID_CITA");
            entity.Property(e => e.FechaHora)
                .HasColumnType("datetime")
                .HasColumnName("FECHA_HORA");
            entity.Property(e => e.IdCliente).HasColumnName("ID_CLIENTE");
            entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_USUARIO");
            entity.Property(e => e.Notas)
                .HasMaxLength(255)
                .HasColumnName("NOTAS");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CITA_CLIENTE");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CITA_ESTADO");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CITA_USUARIO");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PRIMARY");

            entity.ToTable("cliente");

            entity.HasIndex(e => e.Correo, "CORREO").IsUnique();

            entity.HasIndex(e => e.IdTipoIdentificacion, "FK_CLIENTE_TIPO_IDENTIFICACION");

            entity.Property(e => e.IdCliente).HasColumnName("ID_CLIENTE");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .HasColumnName("APELLIDOS");
            entity.Property(e => e.Correo)
                .HasMaxLength(50)
                .HasColumnName("CORREO");
            entity.Property(e => e.IdTipoIdentificacion).HasColumnName("ID_TIPO_IDENTIFICACION");
            entity.Property(e => e.Nombres)
                .HasMaxLength(50)
                .HasColumnName("NOMBRES");
            entity.Property(e => e.NumeroIdentificacion)
                .HasMaxLength(10)
                .HasColumnName("NUMERO_IDENTIFICACION");
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .HasColumnName("TELEFONO");

            entity.HasOne(d => d.IdTipoIdentificacionNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdTipoIdentificacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CLIENTE_TIPO_IDENTIFICACION");
        });

        modelBuilder.Entity<DetalleVentum>(entity =>
        {
            entity.HasKey(e => e.IdDetalleVenta).HasName("PRIMARY");

            entity.ToTable("detalle_venta");

            entity.HasIndex(e => e.IdProducto, "ID_PRODUCTO");

            entity.HasIndex(e => e.IdVenta, "ID_VENTA");

            entity.Property(e => e.IdDetalleVenta).HasColumnName("ID_DETALLE_VENTA");
            entity.Property(e => e.Cantidad).HasColumnName("CANTIDAD");
            entity.Property(e => e.IdProducto).HasColumnName("ID_PRODUCTO");
            entity.Property(e => e.IdVenta).HasColumnName("ID_VENTA");
            entity.Property(e => e.PrecioUnitario)
                .HasPrecision(10, 2)
                .HasColumnName("PRECIO_UNITARIO");
            entity.Property(e => e.Subtotal)
                .HasPrecision(10, 2)
                .HasColumnName("SUBTOTAL");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detalle_venta_ibfk_2");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detalle_venta_ibfk_1");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PRIMARY");

            entity.ToTable("estado");

            entity.HasIndex(e => e.Nombre, "NOMBRE").IsUnique();

            entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("NOMBRE");
        });

        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago).HasName("PRIMARY");

            entity.ToTable("metodo_pago");

            entity.Property(e => e.IdMetodoPago).HasColumnName("ID_METODO_PAGO");
            entity.Property(e => e.NombreMetodo)
                .HasMaxLength(50)
                .HasColumnName("NOMBRE_METODO");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PRIMARY");

            entity.ToTable("pago");

            entity.HasIndex(e => e.IdCita, "FK_PAGO_CITA");

            entity.HasIndex(e => e.IdEstado, "FK_PAGO_ESTADO");

            entity.HasIndex(e => e.IdMetodoPago, "FK_PAGO_METODO");

            entity.Property(e => e.IdPago).HasColumnName("ID_PAGO");
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("FECHA_PAGO");
            entity.Property(e => e.IdCita).HasColumnName("ID_CITA");
            entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
            entity.Property(e => e.IdMetodoPago).HasColumnName("ID_METODO_PAGO");
            entity.Property(e => e.MontoTotal)
                .HasPrecision(10, 2)
                .HasColumnName("MONTO_TOTAL");

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdCita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PAGO_CITA");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PAGO_ESTADO");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PAGO_METODO");
        });

        modelBuilder.Entity<PagoVentum>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PRIMARY");

            entity.ToTable("pago_venta");

            entity.HasIndex(e => e.IdMetodoPago, "ID_METODO_PAGO");

            entity.HasIndex(e => e.IdVenta, "ID_VENTA").IsUnique();

            entity.Property(e => e.IdPago).HasColumnName("ID_PAGO");
            entity.Property(e => e.FechaPago)
                .HasColumnType("datetime")
                .HasColumnName("FECHA_PAGO");
            entity.Property(e => e.IdMetodoPago).HasColumnName("ID_METODO_PAGO");
            entity.Property(e => e.IdVenta).HasColumnName("ID_VENTA");
            entity.Property(e => e.Monto)
                .HasPrecision(10, 2)
                .HasColumnName("MONTO");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.PagoVenta)
                .HasForeignKey(d => d.IdMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pago_venta_ibfk_2");

            entity.HasOne(d => d.IdVentaNavigation).WithOne(p => p.PagoVentum)
                .HasForeignKey<PagoVentum>(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pago_venta_ibfk_1");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            //Aqui agregaremos la configuracion de la logica de Stock en Producto
            entity.Property(e => e.Stock).HasColumnName("STOCK"); //Primero seleccionamos la propiedad stock que esta en producto. Ademas, mapea la columna STOCK en la bd
            //--Aqui agregamos la config de Estado--//
            entity.Property(e => e.Estado)// Seleccionamos el Enum para EstadoProducto
                .HasColumnName("Estado")// Se crea la columna ESTADO en producto en la bd
                .HasConversion<string>();//Le indica que antes de guardarlo en la bd,lo guarda en forma de string.

            entity.HasKey(e => e.IdProducto).HasName("PRIMARY");

            entity.ToTable("producto");

            entity.HasIndex(e => e.IdCategoriaProducto, "ID_CATEGORIA_PRODUCTO");

            entity.Property(e => e.IdProducto).HasColumnName("ID_PRODUCTO");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("DESCRIPCION");
            entity.Property(e => e.IdCategoriaProducto).HasColumnName("ID_CATEGORIA_PRODUCTO");
            entity.Property(e => e.Medida)
                .HasMaxLength(50)
                .HasColumnName("MEDIDA");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.Precio)
                .HasPrecision(10, 2)
                .HasColumnName("PRECIO");
            entity.Property(e => e.Stock).HasColumnName("STOCK");

            entity.HasOne(d => d.IdCategoriaProductoNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoriaProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("producto_ibfk_1");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PRIMARY");

            entity.ToTable("rol");

            entity.HasIndex(e => e.Nombre, "NOMBRE").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("ID_ROL");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("NOMBRE");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.IdServicios).HasName("PRIMARY");

            entity.ToTable("servicio");

            entity.HasIndex(e => e.IdCategoriaServicio, "FK_SERVICIO_CATEGORIA");

            entity.HasIndex(e => e.IdEstado, "FK_SERVICIO_ESTADO");

            entity.HasIndex(e => e.Nombre, "NOMBRE").IsUnique();

            entity.Property(e => e.IdServicios).HasColumnName("ID_SERVICIOS");
            entity.Property(e => e.Costo)
                .HasPrecision(10, 2)
                .HasColumnName("COSTO");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(225)
                .HasColumnName("DESCRIPCION");
            entity.Property(e => e.DuracionMinutos).HasColumnName("DURACION_MINUTOS");
            entity.Property(e => e.IdCategoriaServicio).HasColumnName("ID_CATEGORIA_SERVICIO");
            entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
            entity.Property(e => e.Insumos)
                .HasPrecision(10, 2)
                .HasColumnName("INSUMOS");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("NOMBRE");

            entity.HasOne(d => d.IdCategoriaServicioNavigation).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdCategoriaServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SERVICIO_CATEGORIA");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SERVICIO_ESTADO");
        });

        modelBuilder.Entity<TipoIdentificacion>(entity =>
        {
            entity.HasKey(e => e.IdTipoIdentificacion).HasName("PRIMARY");

            entity.ToTable("tipo_identificacion");

            entity.HasIndex(e => e.Nombre, "NOMBRE").IsUnique();

            entity.Property(e => e.IdTipoIdentificacion).HasColumnName("ID_TIPO_IDENTIFICACION");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("NOMBRE");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.IdEstado, "FK_USUARIO_ESTADO");

            entity.HasIndex(e => e.IdRol, "FK_USUARIO_ROL");

            entity.HasIndex(e => e.Nombre, "NOMBRE").IsUnique();

            entity.HasIndex(e => e.IdCliente, "UNQ_CLIENTE").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("ID_USUARIO");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(225)
                .HasColumnName("CONTRASENA");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("FECHA_REGISTRO");
            entity.Property(e => e.IdCliente).HasColumnName("ID_CLIENTE");
            entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
            entity.Property(e => e.IdRol).HasColumnName("ID_ROL");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("NOMBRE");

            entity.HasOne(d => d.IdClienteNavigation).WithOne(p => p.Usuario)
                .HasForeignKey<Usuario>(d => d.IdCliente)
                .HasConstraintName("FK_USUARIO_CLIENTE");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USUARIO_ESTADO");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USUARIO_ROL");
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PRIMARY");

            entity.ToTable("venta");

            entity.HasIndex(e => e.IdCliente, "ID_CLIENTE");

            entity.Property(e => e.IdVenta).HasColumnName("ID_VENTA");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("FECHA");
            entity.Property(e => e.IdCliente).HasColumnName("ID_CLIENTE");
            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .HasColumnName("TOTAL");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("venta_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
