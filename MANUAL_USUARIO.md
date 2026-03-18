# Manual de Usuario

## Place Store Invoice

Este manual explica el flujo de uso del sistema actual para operar clientes, productos, compras, ventas, tesorería, reportes y configuración.

## 1. Acceso al sistema

1. Abra la aplicacion en el navegador.
2. En la pantalla de inicio de sesión escriba:
   Usuario: `admin`
   Contraseña: `Admin123*`
3. Presione `Entrar al sistema`.

Notas:
- La sesión caduca luego de 15 minutos sin uso.
- Si ya tiene una sesión activa, el sistema puede entrar directo al inicio.
- Para salir, use el botón `Salir` en la parte superior.

## 2. Configuración inicial recomendada

Antes de facturar, se recomienda completar estos pasos:

1. Ir a `Configuración > Empresa`.
2. Registrar o actualizar:
   Razón social
   RNC o cédula
   Dirección
   Teléfono
   Correo
   Logo URL si aplica
   Moneda
3. Guardar cambios.

Importante:
- Estos datos aparecen como cabecera en los reportes.
- Estos datos también aparecen en el PDF de las facturas y cotizaciones.

## 3. Inicio

La pantalla de inicio muestra:
- Cantidad de clientes
- Cantidad de proveedores
- Cantidad de productos
- Cotizaciones del dia
- Facturas del dia
- Ventas del dia
- Compras del dia
- Balance de caja y bancos
- Alertas rápidas
- Accesos directos a procesos frecuentes

## 4. Maestros

Los maestros deben registrarse antes de vender o comprar.

### 4.1 Clientes

Ruta:
`Maestros > Clientes`

Para crear un cliente:
1. Presione `Nuevo cliente`.
2. Complete:
   Nombre
   RNC o cédula
   Dirección
   Teléfono
   Forma de pago
3. Presione `Guardar`.

Para editar un cliente:
1. Entre a `Maestros > Clientes`.
2. En el listado presione `Editar`.
3. Modifique los datos necesarios.
4. Presione `Guardar cambios`.

Uso:
- Los clientes se usan en cotizaciones, facturas, cobros y reportes.

### 4.2 Proveedores

Ruta:
`Maestros > Proveedores`

Para crear un proveedor:
1. Presione `Nuevo proveedor`.
2. Complete:
   Nombre
   RNC o cédula
   Dirección
   Contacto
   Teléfono
   Correo
3. Presione `Guardar`.

Uso:
- Los proveedores se usan en compras, facturas de proveedor y pagos a proveedores.

### 4.3 Productos / Servicios

Ruta:
`Maestros > Productos / Servicios`

Para crear un producto o servicio:
1. Presione `Nuevo producto o servicio`.
2. Complete:
   Nombre
   Unidad
   Precio de venta
3. Presione `Guardar`.

Importante:
- Este mismo catalogo se usa en ventas y compras.
- Cuando registra una compra, el precio de venta del producto puede actualizarse con base en el costo y la ganancia fija indicada en esa compra.

## 5. Compras

## 5.1 Registro de compras

Ruta:
`Compras > Registro de compras`

Para registrar una compra:
1. Presione `Nueva compra`.
2. Complete:
   Numero
   Fecha
   Proveedor
   Forma de pago
3. En la linea del detalle complete:
   Producto o servicio
   Cantidad
   Costo
   Ganancia fija
4. Revise el campo `Venta`, que muestra el precio de venta calculado.
5. Presione `Guardar compra`.

Formula usada:

```text
Precio de venta = Costo + Ganancia fija
```

Que ocurre al guardar:
- Se registra la compra.
- Se calcula el total de la compra.
- El producto seleccionado actualiza su precio de venta segun el costo y la ganancia fija indicada.

Importante:
- Actualmente el sistema actualiza el precio de venta del producto.
- Aun no existe un modulo completo de inventario con existencias, entradas y salidas acumuladas.

## 5.2 Facturas de proveedor

Ruta:
`Compras > Facturas de proveedor`

Para registrar una factura de proveedor:
1. Presione `Nueva factura proveedor`.
2. Seleccione:
   Numero
   Fecha
   Compra relacionada
3. Presione `Guardar factura`.

Uso:
- Sirve como respaldo documental de una compra ya registrada.

## 6. Ventas

## 6.1 Cotizaciones

Ruta:
`Ventas > Cotizaciones`

Para crear una cotización:
1. Presione `Nueva cotizacion`.
2. Complete:
   Número
   Fecha
   Cliente
   Estado
   Forma de pago
3. En la linea del detalle complete:
   Producto o servicio
   Cantidad
   Precio
4. Verifique el total.
5. Presione `Guardar cotizacion`.

Uso:
- Sirve como documento previo a la factura.

Flujo disponible desde el listado:
1. En `Ventas > Cotizaciones` verá todas las cotizaciones registradas.
2. Puede usar `PDF` para abrir e imprimir la cotización.
3. Puede usar `Facturar` para convertir la cotización en una factura.
4. Al facturar, el sistema crea una factura nueva con el mismo cliente, forma de pago, productos, cantidades y precios de la cotización.
5. Luego del proceso, el sistema abre la pantalla de factura emitida para imprimirla o registrar su cobro.

## 6.2 Facturas

Ruta:
`Ventas > Facturas`

Para crear una factura:
1. Presione `Nueva factura`.
2. Complete:
   Número
   Cliente
   Estado
   Forma de pago
3. La fecha de creación la asigna el sistema automáticamente.
4. En el detalle puede trabajar con varias líneas.
5. En cada linea complete:
   Producto o servicio
   Cantidad
   Precio
6. Use `Agregar linea` si necesita más productos.
7. Puede quitar lineas con el botón `Quitar`.
8. Presione `Guardar e imprimir factura`.
9. El sistema muestra una pantalla posterior con:
   vista previa del PDF
   botón para reimprimir
   botón para registrar cobro
   botón para crear otra factura
   botón para volver al listado

Importante:
- La fecha de creación de la factura no se puede modificar manualmente.
- El sistema la guarda en el momento real de la creacion.
- Luego de guardar la factura, el flujo principal abre el PDF inline para impresion directa.
- El cliente y los productos admiten autocompletado por nombre mientras escribe.

### Editar una factura

Desde el listado de facturas:
1. Presione `Editar`.
2. Puede cambiar:
   Cliente
   Estado
   Forma de pago
   Detalle
3. No puede cambiar la fecha de creación.
4. Presione `Guardar cambios`.

Restricción importante:
- Si la factura ya tiene cobros registrados o notas de credito, el sistema bloquea su edicion para proteger el saldo y la auditoria.

### Ver factura en PDF

Desde el listado de facturas:
1. Presione `PDF`.
2. La factura se abre en el navegador.
3. Desde ahi puede imprimir o guardar manualmente.

## 6.3 Notas de credito

Ruta:
`Ventas > Notas de credito`

Para crear una nota de credito:
1. Presione `Nueva nota de credito`.
2. Complete:
   Numero
   Fecha
   Factura relacionada
   Estado
3. En el detalle puede usar varias lineas.
4. Complete producto, cantidad y precio en cada linea.
5. Presione `Guardar nota de credito`.

Uso:
- Sirve para anular o ajustar parcialmente una factura emitida.

Comportamiento actual:
- La nota de credito reduce el saldo pendiente de la factura relacionada.
- Si la nota consume todo el saldo, la factura queda cancelada.
- Si reduce solo una parte, la factura queda con saldo parcial.

## 7. Tesorería

## 7.1 Cobros

Ruta:
`Tesorería > Cobros`

Para registrar un cobro:
1. Presione `Nuevo cobro`.
2. Complete:
   Fecha
   Factura
   Método
   Monto
   Referencia
3. Presione `Guardar cobro`.

Detalle del campo `Referencia`:
- Se usa para guardar el dato que identifica el pago.
- Puede ser número de transferencia, número de cheque, número de recibo, autorización o una nota corta como `Pago en efectivo`.

Que ocurre al guardar:
- Se registra el cobro.
- El cliente se toma de la factura seleccionada.
- El sistema valida que el monto no exceda el saldo pendiente.
- La factura baja su saldo pendiente.
- Si el saldo llega a cero, la factura queda pagada.
- Si el saldo no llega a cero, la factura queda parcialmente pagada.
- Se genera un movimiento positivo en caja o bancos.
- El sistema muestra un mensaje de confirmación en pantalla.

Flujo recomendado:
1. Crear la factura o convertir una cotización en factura.
2. Desde la pantalla de factura emitida o desde el listado de facturas, presione `Registrar cobro` o `Cobrar`.
3. Verifique la factura, el saldo pendiente y el monto.
4. Guarde el cobro.

## 7.2 Pagos a proveedores

Ruta:
`Tesorería > Pagos a proveedores`

Para registrar un pago:
1. Presione `Nuevo pago`.
2. Complete:
   Fecha
   Proveedor
   Método
   Monto
   Referencia
3. Presione `Guardar pago`.

Que ocurre al guardar:
- Se registra el pago.
- Se genera un movimiento negativo en caja o bancos.
- El sistema muestra un mensaje de confirmación en pantalla.

## 7.3 Caja / Bancos

Ruta:
`Tesoreria > Caja / Bancos`

Aquí puede:
- Ver cuentas registradas
- Ver el balance de cada cuenta
- Crear movimientos manuales

Para crear un movimiento manual:
1. Presione `Nuevo movimiento`.
2. Complete:
   Fecha
   Cuenta
   Descripción
   Monto
3. Presione `Guardar movimiento`.

Notas:
- Si el monto es positivo, suma al balance.
- Si el monto es negativo, resta al balance.

## 8. Reportes

Todos los reportes:
- cargan en pantalla
- permiten filtrar por fecha
- tienen campo de búsqueda
- permiten autocompletar nombres al escribir en la busqueda cuando aplica
- se pueden ordenar y paginar
- incluyen botón para ver PDF en el navegador

Adicionalmente:
- Algunos reportes permiten filtrar por cliente
- Algunos permiten filtrar por proveedor
- Los reportes de ventas y compras también admiten filtro por estado

## 8.1 Reporte de ventas

Ruta:
`Reportes > Ventas`

Permite filtrar por:
- fecha
- cliente
- estado
- numero de factura

Tambien muestra el saldo pendiente de cada factura en el detalle del reporte.

## 8.2 Reporte de compras

Ruta:
`Reportes > Compras`

Permite ver:
- proveedor
- producto
- cantidad
- total comprado

Tambien puede filtrar por proveedor y estado.

## 8.3 Cuentas por cobrar

Ruta:
`Reportes > Cuentas por cobrar`

Permite ver facturas pendientes por cliente y rango de antiguedad.

Importante:
- Este reporte ahora usa el saldo pendiente real de cada factura.
- Los cobros y notas de credito afectan directamente este reporte.

## 8.4 Cuentas por pagar

Ruta:
`Reportes > Cuentas por pagar`

Permite ver facturas de proveedor pendientes y su antiguedad.

## 8.5 Reporte de notas de credito

Ruta:
`Reportes > Notas de credito`

Permite filtrar por:
- fecha
- nombre del cliente
- numero de la nota
- numero de la factura relacionada

Tambien permite filtrar por cliente desde el selector del reporte.

## 8.6 Libro de ventas

Ruta:
`Reportes > Libro de ventas`

Muestra resumen de facturas emitidas dentro del rango indicado.

## 8.7 Libro de compras

Ruta:
`Reportes > Libro de compras`

Muestra resumen de compras dentro del rango indicado.

## 8.8 Flujo de caja

Ruta:
`Reportes > Flujo de caja`

Muestra ingresos y egresos dentro del rango indicado.

## 9. Usuarios y roles

Ruta:
`Configuracion > Usuarios / Roles`

Para crear un usuario:
1. Presione `Nuevo usuario`.
2. Complete:
   Nombre completo
   Usuario
   Clave
   Rol
   Activo
3. Presione `Guardar usuario`.

Nota:
- El rol `Administrador` tiene acceso total.
- El rol `Operador` puede trabajar maestros, ventas, compras, tesoreria y reportes.
- El rol `Consulta` solo trabaja dashboard y reportes.
- Configuracion queda reservada para `Administrador`.

## 10. Flujo operativo recomendado

Este es el flujo recomendado para usar el sistema desde cero:

1. Iniciar sesion.
2. Configurar datos de la empresa.
3. Crear clientes.
4. Crear proveedores.
5. Crear productos y servicios.
6. Registrar compras y definir ganancia fija por compra cuando aplique.
7. Emitir cotizaciones si el cliente requiere aprobación previa.
8. Imprimir la cotización o convertirla en factura desde el listado.
9. Emitir facturas directas cuando no se necesite cotización previa.
10. Registrar notas de crédito cuando sea necesario ajustar una venta.
11. Registrar cobros de clientes.
12. Registrar pagos a proveedores.
12. Revisar caja y bancos.
13. Consultar reportes y abrir PDF en el navegador.

Flujo resumido de cliente a cobro:
1. Crear el cliente en `Maestros > Clientes`.
2. Crear una cotización en `Ventas > Cotizaciones` si aplica.
3. Imprimir la cotización o convertirla en factura.
4. Emitir la factura.
5. Registrar el cobro desde la factura o desde `Tesorería > Cobros`.

## 11. Observaciones importantes del sistema actual

- Las facturas tienen fecha de creacion automatica y no editable.
- Los reportes PDF, las facturas PDF y las cotizaciones PDF se abren en el navegador.
- El precio de venta puede actualizarse desde compras usando ganancia fija por linea.
- Los cobros ahora se registran contra una factura especifica.
- Las notas de credito reducen el saldo pendiente de la factura relacionada.
- Existen campos de auditoria basicos para creacion y actualizacion interna de registros.
- El sistema aun no maneja inventario completo con existencia disponible, kardex o costo promedio.
- El login puede entrar directo al dashboard si la sesion sigue activa.

## 12. Soporte

Pie de página del sistema:
`Desarrollado por DevTechRD 829-966-1111`

Tambien puede usar el botón de WhatsApp visible en el sistema.
