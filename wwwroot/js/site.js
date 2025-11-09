// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


// Write your JavaScript code.
$(function () {

    // --- Configuración Global de Toastr ---
    toastr.options = {
        "positionClass": "toast-top-right",
        "progressBar": true,
        "timeOut": "3000"
    };

    // --- SCRIPT 1: Agregar productos al carrito (El que tú pegaste) ---
    $(document).on("submit", ".js-add-cart-form", function (e) {
        e.preventDefault();

        var form = $(this);
        var url = form.attr("action");
        var formData = form.serialize(); // Esto ya incluye el anti-forgery token del formulario

        $.ajax({
            type: "POST",
            url: url,
            data: formData,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message, "¡Éxito!");
                } else {
                    toastr.error("Hubo un problema al agregar el producto.", "Error");
                }
            },
            error: function (xhr, status, error) {
                if (xhr.status === 401) {
                    toastr.warning("Debes iniciar sesión para agregar productos.", "Atención");
                    setTimeout(function () {
                        window.location.href = '/Identity/Account/Login';
                    }, 2000);
                } else {
                    toastr.error("Error: " + error, "Error de Red");
                }
            }
        });
    });

    // --- SCRIPT 2: Actualizar ( +/-/Eliminar ) productos del carrito (El nuevo) ---
    // (Aquí incluyo la corrección del Anti-Forgery Token)
    $(document).on("click", ".js-cart-change", function (e) {
        e.preventDefault();

        var $btn = $(this);
        var $row = $btn.closest("tr");
        var productId = $row.data("product-id");
        var change = parseInt($btn.data("change"));

        var $quantityInput = $row.find(".js-cart-quantity");
        var currentQuantity = parseInt($quantityInput.val());

        var newQuantity;
        if (change === 0) {
            newQuantity = 0; // "Eliminar" es cantidad 0
        } else {
            newQuantity = currentQuantity + change;
        }

        if (newQuantity < 0) {
            return; // No hacer nada si se resta de 0 o menos
        }

        // ¡IMPORTANTE! Leer el token de la página
        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            type: "POST",
            url: "/ShoppingCart/UpdateCart", // Revisa que esta URL sea correcta
            data: {
                productId: productId,
                newQuantity: newQuantity,
                __RequestVerificationToken: token // ¡IMPORTANTE! Enviar el token
            },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);

                    if (response.wasRemoved) {
                        $row.remove(); // Eliminar la fila
                    } else {
                        // Actualizar cantidad y subtotal
                        $quantityInput.val(newQuantity);
                        $row.find(".js-item-subtotal").text(response.newSubtotal);
                    }

                    // Actualizar el total general
                    $("#js-cart-total").text(response.newTotal);

                    // Opcional: actualizar un contador en el navbar
                    // updateNavbarCartCount(response.cartItemCount);

                    // Comprobar si el carrito quedó vacío
                    if ($("tbody tr").length === 0) {
                        $(".table, .text-end.mt-4").remove(); // Ocultar tabla y botón de checkout
                        $("#js-cart-empty").show(); // Mostrar mensaje de carrito vacío
                    }
                } else {
                    toastr.error(response.message, "Error");
                }
            },
            error: function (xhr, status, error) {
                // Si esto falla, REVISA LA CONSOLA (F12) para ver el error (ej. 400 Bad Request)
                toastr.error("No se pudo conectar con el servidor. Revisa la consola (F12).", "Error de Red");
            }
        });
    });

    // --- SCRIPT 3: Ocultar mensaje de carrito vacío si hay items ---
    // (Se ejecuta al cargar la página)
    if ($("tbody tr").length > 0) {
        $("#js-cart-empty").hide();
    }

}); // <-- Cierre final del $(function () { ... });