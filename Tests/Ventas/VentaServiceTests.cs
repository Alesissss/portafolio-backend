using Api.Dtos;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tests.Infra;

namespace Tests.Ventas
{
    // Convención: una clase de tests por clase probada, con el sufijo "Tests".
    // Cada test sigue el patrón AAA: Arrange (preparar) -> Act (ejecutar) -> Assert (verificar).
    // Nombre del test: Metodo_Escenario_ResultadoEsperado. Debe leerse como una frase.
    public class VentaServiceTests
    {
        // Helper para no repetir la creación de productos en cada Arrange.
        private static Producto NuevoProducto(int id, decimal precio, decimal stock) =>
            new()
            {
                IdProducto = id,
                Nombre = $"Producto {id}",
                Descripcion = "de prueba",
                IdCategoria = "TEC",
                Precio = precio,
                Stock = stock,
            };

        [Fact]
        public async Task RegistrarVenta_ConProductosValidos_CalculaTotalesYGuardaEnBorrador()
        {
            // Arrange: una BD aislada para este test, con dos productos.
            var db = Guid.NewGuid().ToString();
            using (var ctx = TestDb.Nuevo(db))
            {
                ctx.Productos.Add(NuevoProducto(1, precio: 100m, stock: 10));
                ctx.Productos.Add(NuevoProducto(2, precio: 50m, stock: 10));
                await ctx.SaveChangesAsync();
            }

            var dto = new RegistrarRequestVentaDto(Guid.NewGuid(), new List<RegistrarDetalleVentaDto>
            {
                new(1, 2, null),   // 100 * 2 = 200
                new(2, 1, null),   //  50 * 1 =  50
            });

            // Act: uso un DbContext NUEVO (misma BD) para que el service actúe como en producción,
            // sin que el change tracker del Arrange le "sople" las respuestas.
            VentaResultType resultado;
            using (var ctx = TestDb.Nuevo(db))
            {
                var service = new VentaService(ctx, new FakeFileService());
                resultado = await service.RegistrarVentaAsync(dto);
            }

            // Assert: reviso el resultado Y lo que quedó realmente en la BD (con otro contexto limpio).
            Assert.Equal(VentaResultType.Ok, resultado);
            using (var ctx = TestDb.Nuevo(db))
            {
                var venta = ctx.Ventas.Include(v => v.Detalles).Single();
                Assert.Equal("BO", venta.IdEstadoVenta);
                Assert.Equal(250m, venta.Subtotal);
                Assert.Equal(45m, venta.Igv);     // 250 * 0.18
                Assert.Equal(295m, venta.Total);
                Assert.Equal(2, venta.Detalles.Count);
                // El precio se LEE del producto, no viene del cliente:
                Assert.Equal(100m, venta.Detalles.Single(d => d.IdProducto == 1).PrecioVenta);
            }
        }

        [Fact]
        public async Task RegistrarVenta_ConProductoInexistente_DevuelveProductoNoExisteYNoGuarda()
        {
            // Arrange: BD sin productos; el detalle apunta a un id que no existe.
            var db = Guid.NewGuid().ToString();
            var dto = new RegistrarRequestVentaDto(Guid.NewGuid(), new List<RegistrarDetalleVentaDto>
            {
                new(99, 1, null),
            });

            // Act
            VentaResultType resultado;
            using (var ctx = TestDb.Nuevo(db))
                resultado = await new VentaService(ctx, new FakeFileService()).RegistrarVentaAsync(dto);

            // Assert: devuelve el estado correcto y NO deja ninguna venta a medias.
            Assert.Equal(VentaResultType.ProductoNoExiste, resultado);
            using (var ctx = TestDb.Nuevo(db))
                Assert.Empty(ctx.Ventas);
        }

        [Fact]
        public async Task GenerarVenta_ConStockSuficiente_DescuentaStockYPasaAGenerada()
        {
            // Arrange: producto con stock 10 y una venta en Borrador que pide 3.
            var db = Guid.NewGuid().ToString();
            var idVenta = Guid.NewGuid();
            using (var ctx = TestDb.Nuevo(db))
            {
                ctx.Productos.Add(NuevoProducto(1, precio: 100m, stock: 10));
                ctx.Ventas.Add(new Venta
                {
                    IdVenta = idVenta,
                    IdVendedor = Guid.NewGuid(),
                    IdEstadoVenta = "BO",
                    Detalles = new List<DetalleVenta>
                    {
                        new() { IdProducto = 1, PrecioVenta = 100m, Cantidad = 3 },
                    },
                });
                await ctx.SaveChangesAsync();
            }

            // Act
            VentaResultType resultado;
            using (var ctx = TestDb.Nuevo(db))
                resultado = await new VentaService(ctx, new FakeFileService()).GenerarVentaAsync(idVenta);

            // Assert
            Assert.Equal(VentaResultType.Ok, resultado);
            using (var ctx = TestDb.Nuevo(db))
            {
                Assert.Equal("GEN", ctx.Ventas.Single().IdEstadoVenta);
                Assert.Equal(7m, ctx.Productos.Single().Stock);   // 10 - 3
            }
        }

        [Fact]
        public async Task GenerarVenta_SinStockSuficiente_NoDescuentaNiCambiaEstado()
        {
            // Arrange: stock 2, pero la venta pide 5. Debe fallar SIN efectos (todo o nada).
            var db = Guid.NewGuid().ToString();
            var idVenta = Guid.NewGuid();
            using (var ctx = TestDb.Nuevo(db))
            {
                ctx.Productos.Add(NuevoProducto(1, precio: 100m, stock: 2));
                ctx.Ventas.Add(new Venta
                {
                    IdVenta = idVenta,
                    IdVendedor = Guid.NewGuid(),
                    IdEstadoVenta = "BO",
                    Detalles = new List<DetalleVenta>
                    {
                        new() { IdProducto = 1, PrecioVenta = 100m, Cantidad = 5 },
                    },
                });
                await ctx.SaveChangesAsync();
            }

            // Act
            VentaResultType resultado;
            using (var ctx = TestDb.Nuevo(db))
                resultado = await new VentaService(ctx, new FakeFileService()).GenerarVentaAsync(idVenta);

            // Assert: el estado sigue en Borrador y el stock quedó intacto.
            Assert.Equal(VentaResultType.StockInsuficiente, resultado);
            using (var ctx = TestDb.Nuevo(db))
            {
                Assert.Equal("BO", ctx.Ventas.Single().IdEstadoVenta);
                Assert.Equal(2m, ctx.Productos.Single().Stock);
            }
        }

        [Fact]
        public async Task PagarVenta_SinComprobante_DevuelveArchivoRequeridoYNoTocaElFileService()
        {
            // Arrange: venta Generada, pero llamamos a pagar sin archivo.
            var db = Guid.NewGuid().ToString();
            var idVenta = Guid.NewGuid();
            using (var ctx = TestDb.Nuevo(db))
            {
                ctx.Ventas.Add(new Venta { IdVenta = idVenta, IdVendedor = Guid.NewGuid(), IdEstadoVenta = "GEN" });
                await ctx.SaveChangesAsync();
            }

            var fake = new FakeFileService();

            // Act
            VentaResultType resultado;
            using (var ctx = TestDb.Nuevo(db))
                resultado = await new VentaService(ctx, fake).PagarVentaAsync(idVenta, comprobante: null!);

            // Assert: rechaza antes de intentar guardar nada en disco.
            Assert.Equal(VentaResultType.ArchivoRequerido, resultado);
            Assert.False(fake.FueLlamado);
        }

        [Fact]
        public async Task PagarVenta_ConComprobanteValido_GuardaLaRutaYPasaAPagada()
        {
            // Arrange: venta Generada + un archivo falso en memoria.
            var db = Guid.NewGuid().ToString();
            var idVenta = Guid.NewGuid();
            using (var ctx = TestDb.Nuevo(db))
            {
                ctx.Ventas.Add(new Venta { IdVenta = idVenta, IdVendedor = Guid.NewGuid(), IdEstadoVenta = "GEN" });
                await ctx.SaveChangesAsync();
            }

            var fake = new FakeFileService { RutaADevolver = "comprobantes/abc.pdf" };
            var bytes = new byte[] { 1, 2, 3 };
            var archivo = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "comprobante", "recibo.pdf");

            // Act
            VentaResultType resultado;
            using (var ctx = TestDb.Nuevo(db))
                resultado = await new VentaService(ctx, fake).PagarVentaAsync(idVenta, archivo);

            // Assert: guardó la ruta que devolvió el FileService y cerró la venta.
            Assert.Equal(VentaResultType.Ok, resultado);
            Assert.True(fake.FueLlamado);
            using (var ctx = TestDb.Nuevo(db))
            {
                var venta = ctx.Ventas.Single();
                Assert.Equal("PAG", venta.IdEstadoVenta);
                Assert.Equal("comprobantes/abc.pdf", venta.ArchivoPago);
            }
        }
    }
}
