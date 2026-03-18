$(function () {
    $(".datatable").DataTable({
        language: {
            search: "Buscar:",
            lengthMenu: "Mostrar _MENU_ registros",
            info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
            paginate: {
                previous: "Anterior",
                next: "Siguiente"
            },
            zeroRecords: "No se encontraron registros"
        },
        order: []
    });

    $(document).on("input", ".qty-input, .price-input, .cost-input", function () {
        const row = $(this).closest(".item-row");
        const qty = Number(row.find(".qty-input").val() || 0);
        const price = Number(row.find(".price-input, .cost-input").val() || 0);
        row.find(".total-input").val((qty * price).toFixed(2));
    });
});
