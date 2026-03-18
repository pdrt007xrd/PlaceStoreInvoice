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

    $(document).on("input", ".cost-input, .profit-input", function () {
        const row = $(this).closest(".item-row");
        const cost = Number(row.find(".cost-input").val() || 0);
        const profit = Number(row.find(".profit-input").val() || 0);
        row.find(".sale-price-input").val((cost + profit).toFixed(2));
    });

    $(document).on("click", "#sidebarMenu .menu-item", function () {
        if (window.innerWidth >= 992) {
            return;
        }

        const sidebarMenu = document.getElementById("sidebarMenu");
        if (!sidebarMenu || !sidebarMenu.classList.contains("show")) {
            return;
        }

        const collapseInstance = bootstrap.Collapse.getOrCreateInstance(sidebarMenu);
        collapseInstance.hide();
    });
});
