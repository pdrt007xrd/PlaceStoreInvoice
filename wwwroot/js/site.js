$(function () {
    function reindexItemLines(container) {
        container.find(".item-row").each(function (index) {
            $(this).find("[name], [id], [data-hidden-target], [data-price-target]").each(function () {
                ["name", "id", "data-hidden-target", "data-price-target"].forEach((attr) => {
                    const current = $(this).attr(attr);
                    if (!current) {
                        return;
                    }

                    const updated = current
                        .replace(/Items\[\d+\]/g, `Items[${index}]`)
                        .replace(/Items_\d+__/g, `Items_${index}__`)
                        .replace(/CreditItems_\d+__/g, `CreditItems_${index}__`)
                        .replace(/Items\[\_\_INDEX\_\_\]/g, `Items[${index}]`);

                    $(this).attr(attr, updated);
                });
            });
        });
        container.data("next-index", container.find(".item-row").length);
    }

    $(".datatable").DataTable({
        language: {
            search: "Buscar:",
            lengthMenu: "Mostrar _MENU_ registros",
            info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
            infoEmpty: "Mostrando 0 a 0 de 0 registros",
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

    $(document).on("input", ".autocomplete-input", function () {
        const input = this;
        const listId = input.getAttribute("list");
        const hiddenTargetId = input.dataset.hiddenTarget;
        if (!listId || !hiddenTargetId) {
            return;
        }

        const hidden = document.getElementById(hiddenTargetId);
        const option = $(`#${listId} option`).filter(function () {
            return this.value === input.value;
        }).first();

        if (hidden) {
            hidden.value = option.length ? option.data("id") : "";
        }

        const priceTargetId = input.dataset.priceTarget;
        if (priceTargetId && option.length) {
            const priceTarget = document.getElementById(priceTargetId);
            if (priceTarget && (!priceTarget.value || Number(priceTarget.value) === 0)) {
                priceTarget.value = Number(option.data("price") || 0).toFixed(2);
                $(priceTarget).trigger("input");
            }
        }
    });

    $(document).on("click", ".add-item-row", function () {
        const target = $($(this).data("target"));
        const template = $($(this).data("template")).html();
        const index = Number(target.data("next-index") || 0);
        target.append(template.replaceAll("__INDEX__", index));
        reindexItemLines(target);
    });

    $(document).on("click", ".remove-item-row", function () {
        const container = $(this).closest(".item-lines");
        if (container.find(".item-row").length <= 1) {
            return;
        }

        $(this).closest(".item-row").remove();
        reindexItemLines(container);
    });

    $(document).on("change", ".invoice-selector", function () {
        const invoiceId = $(this).val();
        const url = new URL(window.location.href);
        if (invoiceId) {
            url.searchParams.set("invoiceId", invoiceId);
        } else {
            url.searchParams.delete("invoiceId");
        }

        window.location.href = url.toString();
    });

    const toast = document.querySelector("[data-toast]");
    if (toast) {
        requestAnimationFrame(() => {
            toast.classList.add("is-visible");
        });

        window.setTimeout(() => {
            toast.classList.remove("is-visible");
            window.setTimeout(() => toast.parentElement?.remove(), 220);
        }, 2600);
    }
});
