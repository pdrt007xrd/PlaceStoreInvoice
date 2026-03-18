# Manual de Usuario

## Place Store Invoice

Este manual explica el flujo de uso del sistema actual para operar clientes, productos, compras, ventas, tesoreria, reportes y configuracion.

## 1. Acceso al sistema

1. Abra la aplicacion en el navegador.
2. En la pantalla de inicio de sesion escriba:
   Usuario: `admin`
   Contrasena: `Admin123*`
3. Presione `Entrar al sistema`.

Notas:
- La sesion caduca luego de 15 minutos sin uso.
- Si ya tiene una sesion activa, el sistema puede entrar directo al dashboard.
- Para salir, use el boton `Salir` en la parte superior.

## 2. Configuracion inicial recomendada

Antes de facturar, se recomienda completar estos pasos:

1. Ir a `Configuracion > Empresa`.
2. Registrar o actualizar:
   Razon social
   RNC o cedula
   Direccion
   Telefono
   Correo
   Logo URL si aplica
   Moneda
3. Guardar cambios.

Importante:
- Estos datos aparecen como cabecera en los reportes.
- Estos datos tambien aparecen en el PDF de las facturas.

## 3. Dashboard

El `Dashboard` muestra:
- Cantidad de clientes
- Cantidad de proveedores
- Cantidad de productos
- Cotizaciones del dia
- Facturas del dia
- Ventas del dia
- Compras del dia
- Balance de caja y bancos
- Alertas rapidas
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
   RNC o cedula
   Direccion
   Forma de pago
3. Presione `Guardar`.

Uso:
- Los clientes se usan en cotizaciones, facturas, cobros y reportes.

### 4.2 Proveedores

Ruta:
`Maestros > Proveedores`

Para crear un proveedor:
1. Presione `Nuevo proveedor`.
2. Complete:
   Nombre
   RNC o cedula
   Direccion
   Contacto
   Telefono
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

Para crear una cotizacion:
1. Presione `Nueva cotizacion`.
2. Complete:
   Numero
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

## 6.2 Facturas

Ruta:
`Ventas > Facturas`

Para crear una factura:
1. Presione `Nueva factura`.
2. Complete:
   Numero
   Cliente
   Estado
   Forma de pago
3. La fecha de creacion la asigna el sistema automaticamente.
4. En el detalle puede trabajar con varias lineas.
5. En cada linea complete:
   Producto o servicio
   Cantidad
   Precio
6. Use `Agregar linea` si necesita mas productos.
7. Puede quitar lineas con el boton `Quitar`.
8. Presione `Guardar e imprimir factura`.
9. El sistema muestra una pantalla posterior con:
   vista previa del PDF
   boton para reimprimir
   boton para registrar cobro
   boton para crear otra factura
   boton para volver al listado

Importante:
- La fecha de creacion de la factura no se puede modificar manualmente.
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
3. No puede cambiar la fecha de creacion.
4. Presione `Guardar cambios`.

Restriccion importante:
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

## 7. Tesoreria

## 7.1 Cobros

Ruta:
`Tesoreria > Cobros`

Para registrar un cobro:
1. Presione `Nuevo cobro`.
2. Complete:
   Fecha
   Factura
   Metodo
   Monto
   Referencia
3. Presione `Guardar cobro`.

Que ocurre al guardar:
- Se registra el cobro.
- El cliente se toma de la factura seleccionada.
- El sistema valida que el monto no exceda el saldo pendiente.
- La factura baja su saldo pendiente.
- Si el saldo llega a cero, la factura queda pagada.
- Si el saldo no llega a cero, la factura queda parcialmente pagada.
- Se genera un movimiento positivo en caja o bancos.

## 7.2 Pagos a proveedores

Ruta:
`Tesoreria > Pagos a proveedores`

Para registrar un pago:
1. Presione `Nuevo pago`.
2. Complete:
   Fecha
   Proveedor
   Metodo
   Monto
   Referencia
3. Presione `Guardar pago`.

Que ocurre al guardar:
- Se registra el pago.
- Se genera un movimiento negativo en caja o bancos.

## 7.3 Caja / Bancos

Ruta:
`Tesoreria > Caja / Bancos`

Aqui puede:
- Ver cuentas registradas
- Ver el balance de cada cuenta
- Crear movimientos manuales

Para crear un movimiento manual:
1. Presione `Nuevo movimiento`.
2. Complete:
   Fecha
   Cuenta
   Descripcion
   Monto
3. Presione `Guardar movimiento`.

Notas:
- Si el monto es positivo, suma al balance.
- Si el monto es negativo, resta al balance.

## 8. Reportes

Todos los reportes:
- cargan en pantalla
- permiten filtrar por fecha
- tienen campo de busqueda
- permiten autocompletar nombres al escribir en la busqueda cuando aplica
- se pueden ordenar y paginar
- incluyen boton para ver PDF en el navegador

Adicionalmente:
- Algunos reportes permiten filtrar por cliente
- Algunos permiten filtrar por proveedor
- Los reportes de ventas y compras tambien admiten filtro por estado

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
7. Emitir cotizaciones si el cliente requiere aprobacion previa.
8. Emitir facturas.
9. Registrar notas de credito cuando sea necesario ajustar una venta.
10. Registrar cobros de clientes.
11. Registrar pagos a proveedores.
12. Revisar caja y bancos.
13. Consultar reportes y abrir PDF en el navegador.

## 11. Observaciones importantes del sistema actual

- Las facturas tienen fecha de creacion automatica y no editable.
- Los reportes PDF y las facturas PDF se abren en el navegador.
- El precio de venta puede actualizarse desde compras usando ganancia fija por linea.
- Los cobros ahora se registran contra una factura especifica.
- Las notas de credito reducen el saldo pendiente de la factura relacionada.
- Existen campos de auditoria basicos para creacion y actualizacion interna de registros.
- El sistema aun no maneja inventario completo con existencia disponible, kardex o costo promedio.
- El login puede entrar directo al dashboard si la sesion sigue activa.

## 12. Soporte

Pie de pagina del sistema:
`Desarrollado por DevTechRD 829-966-1111`

Tambien puede usar el boton de WhatsApp visible en el sistema.
