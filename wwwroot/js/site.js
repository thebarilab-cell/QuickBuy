// QuickBuy E-commerce Website JavaScript

$(document).ready(function () {
    // Initialize all components
    initSite();
});

function initSite() {
    // Initialize tooltips
    initTooltips();

    // Initialize popovers
    initPopovers();

    // Initialize form validation
    initFormValidation();

    // Initialize cart functionality
    initCart();

    // Initialize product interactions
    initProductInteractions();

    // Initialize checkout process
    initCheckout();

    // Initialize admin components
    initAdminComponents();

    // Initialize responsive behavior
    initResponsive();
}

// ===== TOOLTIPS =====
function initTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// ===== POPOVERS =====
function initPopovers() {
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// ===== FORM VALIDATION =====
function initFormValidation() {
    // Enable Bootstrap validation
    var forms = document.querySelectorAll('.needs-validation');
    Array.prototype.slice.call(forms).forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });

    // Real-time password validation
    $('input[type="password"]').on('input', function () {
        validatePasswordStrength($(this).val(), $(this).attr('id'));
    });

    // Email validation
    $('input[type="email"]').on('blur', function () {
        validateEmail($(this).val(), $(this));
    });
}

function validatePasswordStrength(password, fieldId) {
    var strength = 0;
    var feedback = '';

    if (password.length >= 8) strength++;
    if (password.match(/[a-z]/) && password.match(/[A-Z]/)) strength++;
    if (password.match(/\d/)) strength++;
    if (password.match(/[^a-zA-Z\d]/)) strength++;

    var strengthElement = $('#' + fieldId + '-strength');
    if (strengthElement.length) {
        if (strength < 2) {
            strengthElement.html('<span class="text-danger">Weak password</span>');
        } else if (strength === 2) {
            strengthElement.html('<span class="text-warning">Medium strength</span>');
        } else if (strength === 3) {
            strengthElement.html('<span class="text-info">Good password</span>');
        } else {
            strengthElement.html('<span class="text-success">Strong password</span>');
        }
    }
}

function validateEmail(email, element) {
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        element.addClass('is-invalid');
        element.next('.invalid-feedback').remove();
        element.after('<div class="invalid-feedback">Please enter a valid email address</div>');
        return false;
    } else {
        element.removeClass('is-invalid');
        element.next('.invalid-feedback').remove();
        return true;
    }
}

// ===== CART FUNCTIONALITY =====
function initCart() {
    // Update cart count on page load
    updateCartCount();

    // Cart item quantity controls
    $('.quantity-btn').click(function () {
        var btn = $(this);
        var input = btn.siblings('.quantity-input');
        var currentVal = parseInt(input.val());

        if (btn.hasClass('quantity-up')) {
            input.val(currentVal + 1);
        } else if (btn.hasClass('quantity-down') && currentVal > 1) {
            input.val(currentVal - 1);
        }

        updateCartItem(btn.closest('.cart-item').data('item-id'), input.val());
    });

    // Remove item from cart
    $('.remove-item-btn').click(function () {
        var itemId = $(this).closest('.cart-item').data('item-id');
        removeCartItem(itemId);
    });

    // Apply promo code
    $('#applyPromoBtn').click(function () {
        applyPromoCode($('#promoCode').val());
    });
}

function updateCartCount() {
    $.ajax({
        url: '/Cart/GetCartCount',
        type: 'GET',
        success: function (data) {
            $('#cartCount').text(data.count || 0);
        },
        error: function () {
            console.log('Failed to update cart count');
        }
    });
}

function updateCartItem(itemId, quantity) {
    $.ajax({
        url: '/Cart/UpdateQuantity',
        type: 'POST',
        data: {
            cartItemId: itemId,
            quantity: quantity
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                location.reload();
            } else {
                showToast('Error', 'Failed to update cart item', 'danger');
            }
        },
        error: function () {
            showToast('Error', 'An error occurred', 'danger');
        }
    });
}

function removeCartItem(itemId) {
    if (confirm('Are you sure you want to remove this item?')) {
        $.ajax({
            url: '/Cart/RemoveItem',
            type: 'POST',
            data: { cartItemId: itemId },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    $('.cart-item[data-item-id="' + itemId + '"]').fadeOut(300, function () {
                        $(this).remove();
                        updateCartCount();
                        updateCartTotals();
                    });
                    showToast('Success', 'Item removed from cart', 'success');
                } else {
                    showToast('Error', 'Failed to remove item', 'danger');
                }
            },
            error: function () {
                showToast('Error', 'An error occurred', 'danger');
            }
        });
    }
}

function applyPromoCode(code) {
    if (!code.trim()) {
        showToast('Error', 'Please enter a promo code', 'warning');
        return;
    }

    $.ajax({
        url: '/Cart/ApplyPromoCode',
        type: 'POST',
        data: { promoCode: code },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                showToast('Success', 'Promo code applied successfully!', 'success');
                updateCartTotals();
            } else {
                showToast('Error', response.message || 'Invalid promo code', 'danger');
            }
        },
        error: function () {
            showToast('Error', 'An error occurred', 'danger');
        }
    });
}

function updateCartTotals() {
    $.ajax({
        url: '/Cart/GetCartTotal',
        type: 'GET',
        success: function (data) {
            $('.cart-subtotal').text('$' + data.subtotal.toFixed(2));
            $('.cart-tax').text('$' + data.tax.toFixed(2));
            $('.cart-shipping').text(data.shipping === 0 ? 'FREE' : '$' + data.shipping.toFixed(2));
            $('.cart-total').text('$' + data.total.toFixed(2));
        }
    });
}

// ===== PRODUCT INTERACTIONS =====
function initProductInteractions() {
    // Add to cart buttons
    $('.add-to-cart-btn').click(function (e) {
        e.preventDefault();
        var btn = $(this);
        var productId = btn.data('product-id');
        var quantity = btn.data('quantity') || 1;

        addToCart(productId, quantity, btn);
    });

    // Quick view modal
    $('.quick-view-btn').click(function () {
        var productId = $(this).data('product-id');
        showQuickView(productId);
    });

    // Product image zoom
    $('.product-image-zoom').hover(function () {
        $(this).addClass('zoomed');
    }, function () {
        $(this).removeClass('zoomed');
    });

    // Product rating stars
    $('.rating-stars .star').click(function () {
        var rating = $(this).data('rating');
        setProductRating(rating);
    });
}

function addToCart(productId, quantity, button) {
    // Show loading state
    var originalText = button.html();
    button.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i> Adding...');

    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: {
            productId: productId,
            quantity: quantity
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                showToast('Success', 'Product added to cart!', 'success');
                updateCartCount();

                // Update button state
                setTimeout(function () {
                    button.prop('disabled', false).html('<i class="fas fa-check me-2"></i> Added to Cart');
                    setTimeout(function () {
                        button.html(originalText);
                    }, 2000);
                }, 500);
            } else {
                showToast('Error', response.message || 'Failed to add to cart', 'danger');
                button.prop('disabled', false).html(originalText);
            }
        },
        error: function () {
            showToast('Error', 'An error occurred', 'danger');
            button.prop('disabled', false).html(originalText);
        }
    });
}

function showQuickView(productId) {
    $.ajax({
        url: '/Product/QuickView/' + productId,
        type: 'GET',
        success: function (data) {
            $('#quickViewModal .modal-body').html(data);
            $('#quickViewModal').modal('show');
        },
        error: function () {
            showToast('Error', 'Failed to load product details', 'danger');
        }
    });
}

function setProductRating(rating) {
    $('.rating-stars .star').each(function () {
        if ($(this).data('rating') <= rating) {
            $(this).addClass('active');
        } else {
            $(this).removeClass('active');
        }
    });

    // Submit rating
    $.ajax({
        url: '/Product/SubmitRating',
        type: 'POST',
        data: { rating: rating },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        }
    });
}

// ===== CHECKOUT PROCESS =====
function initCheckout() {
    // Shipping method selection
    $('input[name="shippingMethod"]').change(function () {
        updateShippingCost($(this).val());
    });

    // Payment method selection
    $('input[name="paymentMethod"]').change(function () {
        updatePaymentForm($(this).val());
    });

    // Address validation
    $('#shippingZipCode').on('blur', function () {
        validateZipCode($(this).val());
    });

    // Order summary update
    $('.checkout-item-quantity').change(function () {
        updateCheckoutItem($(this).data('item-id'), $(this).val());
    });
}

function updateShippingCost(method) {
    var subtotal = parseFloat($('#checkoutSubtotal').data('value'));
    var shippingCost = method === 'express' ? 9.99 : (subtotal > 50 ? 0 : 5.99);

    $('#shippingCost').text(shippingCost === 0 ? 'FREE' : '$' + shippingCost.toFixed(2));
    updateCheckoutTotal(subtotal, shippingCost);
}

function updatePaymentForm(method) {
    $('.payment-form').hide();
    $('#' + method + 'PaymentForm').show();
}

function validateZipCode(zipCode) {
    if (zipCode.length >= 5) {
        $.ajax({
            url: '/Checkout/ValidateZipCode',
            type: 'POST',
            data: { zipCode: zipCode },
            success: function (response) {
                if (response.valid) {
                    $('#shippingZipCode').removeClass('is-invalid').addClass('is-valid');
                    $('#zipCodeFeedback').html('<span class="text-success">Valid zip code</span>');
                } else {
                    $('#shippingZipCode').addClass('is-invalid');
                    $('#zipCodeFeedback').html('<span class="text-danger">Invalid zip code</span>');
                }
            }
        });
    }
}

function updateCheckoutItem(itemId, quantity) {
    $.ajax({
        url: '/Checkout/UpdateItem',
        type: 'POST',
        data: {
            itemId: itemId,
            quantity: quantity
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                $('#itemTotal-' + itemId).text('$' + response.itemTotal.toFixed(2));
                updateCheckoutTotal(response.subtotal, response.shipping);
            }
        }
    });
}

function updateCheckoutTotal(subtotal, shipping) {
    var tax = subtotal * 0.1;
    var total = subtotal + tax + shipping;

    $('#checkoutSubtotal').text('$' + subtotal.toFixed(2));
    $('#checkoutTax').text('$' + tax.toFixed(2));
    $('#checkoutShipping').text(shipping === 0 ? 'FREE' : '$' + shipping.toFixed(2));
    $('#checkoutTotal').text('$' + total.toFixed(2));
}

// ===== ADMIN COMPONENTS =====
function initAdminComponents() {
    // DataTables initialization
    if ($.fn.DataTable) {
        $('.data-table').DataTable({
            pageLength: 10,
            responsive: true,
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Search..."
            }
        });
    }

    // Chart initialization
    initCharts();

    // Date range picker
    $('.date-range-picker').daterangepicker({
        opens: 'left',
        locale: {
            format: 'YYYY-MM-DD'
        }
    });

    // Product bulk actions
    $('#bulkActionSelect').change(function () {
        var action = $(this).val();
        if (action) {
            executeBulkAction(action);
        }
    });

    // Order status updates
    $('.order-status-select').change(function () {
        var orderId = $(this).data('order-id');
        var status = $(this).val();
        updateOrderStatus(orderId, status);
    });
}

function initCharts() {
    // Revenue chart
    var revenueCtx = document.getElementById('revenueChart');
    if (revenueCtx) {
        var revenueChart = new Chart(revenueCtx, {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                datasets: [{
                    label: 'Revenue',
                    data: [12000, 19000, 15000, 25000, 22000, 30000, 28000, 32000, 35000, 40000, 38000, 42000],
                    borderColor: '#4e73df',
                    backgroundColor: 'rgba(78, 115, 223, 0.05)'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });
    }

    // Sales by category chart
    var categoryCtx = document.getElementById('categoryChart');
    if (categoryCtx) {
        var categoryChart = new Chart(categoryCtx, {
            type: 'doughnut',
            data: {
                labels: ['Electronics', 'Fashion', 'Home & Kitchen', 'Books', 'Others'],
                datasets: [{
                    data: [30, 25, 20, 15, 10],
                    backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b']
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });
    }
}

function executeBulkAction(action) {
    var selectedItems = [];
    $('.bulk-select:checked').each(function () {
        selectedItems.push($(this).val());
    });

    if (selectedItems.length === 0) {
        showToast('Warning', 'Please select items first', 'warning');
        return;
    }

    if (confirm('Are you sure you want to ' + action + ' ' + selectedItems.length + ' item(s)?')) {
        $.ajax({
            url: '/Admin/BulkAction',
            type: 'POST',
            data: {
                action: action,
                items: selectedItems
            },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    showToast('Success', 'Bulk action completed', 'success');
                    location.reload();
                } else {
                    showToast('Error', response.message, 'danger');
                }
            },
            error: function () {
                showToast('Error', 'An error occurred', 'danger');
            }
        });
    }
}

function updateOrderStatus(orderId, status) {
    $.ajax({
        url: '/Admin/UpdateOrderStatus',
        type: 'POST',
        data: {
            orderId: orderId,
            status: status
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                showToast('Success', 'Order status updated', 'success');
            } else {
                showToast('Error', 'Failed to update order status', 'danger');
            }
        },
        error: function () {
            showToast('Error', 'An error occurred', 'danger');
        }
    });
}

// ===== RESPONSIVE BEHAVIOR =====
function initResponsive() {
    // Mobile menu toggle
    $('#mobileMenuToggle').click(function () {
        $('#mobileMenu').toggleClass('show');
    });

    // Search toggle for mobile
    $('#searchToggle').click(function () {
        $('#searchBar').toggleClass('show');
        if ($('#searchBar').hasClass('show')) {
            $('#searchInput').focus();
        }
    });

    // Adjust product grid on resize
    $(window).resize(function () {
        adjustProductGrid();
    });

    // Initialize on load
    adjustProductGrid();
}

function adjustProductGrid() {
    var width = $(window).width();
    var productCards = $('.product-card');

    if (width < 768) {
        productCards.addClass('mobile-view');
    } else {
        productCards.removeClass('mobile-view');
    }
}

// ===== UTILITY FUNCTIONS =====
function showToast(title, message, type) {
    // Create toast element
    var toastId = 'toast-' + Date.now();
    var toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}:</strong> ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    // Add to toast container
    $('#toastContainer').append(toastHtml);

    // Show toast
    var toastElement = document.getElementById(toastId);
    var toast = new bootstrap.Toast(toastElement);
    toast.show();

    // Remove after hide
    toastElement.addEventListener('hidden.bs.toast', function () {
        $(this).remove();
    });
}

function showLoading() {
    if ($('#loadingOverlay').length === 0) {
        $('body').append('<div id="loadingOverlay" class="loading-overlay"><div class="spinner"></div></div>');
    } else {
        $('#loadingOverlay').show();
    }
}

function hideLoading() {
    $('#loadingOverlay').fadeOut();
}

function formatCurrency(amount) {
    return '$' + parseFloat(amount).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
}

function formatDate(dateString) {
    var date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

// ===== GLOBAL EVENT HANDLERS =====
$(document).on('click', '.ajax-link', function (e) {
    e.preventDefault();
    var url = $(this).attr('href');
    showLoading();

    $.ajax({
        url: url,
        type: 'GET',
        success: function (data) {
            $('#contentArea').html(data);
            hideLoading();
        },
        error: function () {
            hideLoading();
            showToast('Error', 'Failed to load content', 'danger');
        }
    });
});

$(document).on('submit', '.ajax-form', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method') || 'POST';
    var data = form.serialize();

    showLoading();

    $.ajax({
        url: url,
        type: method,
        data: data,
        success: function (response) {
            hideLoading();
            if (response.success) {
                showToast('Success', response.message || 'Action completed successfully', 'success');
                if (response.redirect) {
                    setTimeout(function () {
                        window.location.href = response.redirect;
                    }, 1500);
                }
            } else {
                showToast('Error', response.message || 'Action failed', 'danger');
            }
        },
        error: function () {
            hideLoading();
            showToast('Error', 'An error occurred', 'danger');
        }
    });
});

// ===== PAGE SPECIFIC SCRIPTS =====
// Check if we're on specific pages and run page-specific scripts
if ($('.product-detail-page').length) {
    initProductDetailPage();
}

if ($('.checkout-page').length) {
    initCheckoutPage();
}

if ($('.admin-dashboard').length) {
    initAdminDashboard();
}

function initProductDetailPage() {
    // Image gallery
    $('.thumbnail-img').click(function () {
        var mainImage = $(this).data('main-image');
        $('#mainProductImage').attr('src', mainImage);
        $('.thumbnail-img').removeClass('active');
        $(this).addClass('active');
    });

    // Quantity controls
    $('.quantity-minus').click(function () {
        var input = $(this).siblings('.quantity-input');
        var value = parseInt(input.val());
        if (value > 1) {
            input.val(value - 1);
        }
    });

    $('.quantity-plus').click(function () {
        var input = $(this).siblings('.quantity-input');
        var value = parseInt(input.val());
        var max = input.data('max') || 999;
        if (value < max) {
            input.val(value + 1);
        }
    });

    // Tab switching
    $('.product-tab-link').click(function (e) {
        e.preventDefault();
        var tabId = $(this).attr('href');
        $('.product-tab-link').removeClass('active');
        $(this).addClass('active');
        $('.product-tab-content').removeClass('active');
        $(tabId).addClass('active');
    });
}

function initCheckoutPage() {
    // Address same as billing
    $('#sameAsBilling').change(function () {
        if ($(this).is(':checked')) {
            $('#shippingAddress').slideUp();
            copyBillingToShipping();
        } else {
            $('#shippingAddress').slideDown();
        }
    });

    // Payment method tabs
    $('.payment-method-tab').click(function () {
        var method = $(this).data('method');
        $('.payment-method-tab').removeClass('active');
        $(this).addClass('active');
        $('.payment-form').removeClass('active');
        $('#' + method + 'Form').addClass('active');
    });

    // Place order button
    $('#placeOrderBtn').click(function () {
        var btn = $(this);
        var originalText = btn.html();
        btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i> Processing...');

        // Validate form
        if (!validateCheckoutForm()) {
            btn.prop('disabled', false).html(originalText);
            return;
        }

        // Submit form
        $('#checkoutForm').submit();
    });
}

function initAdminDashboard() {
    // Refresh dashboard stats
    $('#refreshStats').click(function () {
        var btn = $(this);
        var originalText = btn.html();
        btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i> Refreshing...');

        $.ajax({
            url: '/Admin/GetDashboardStats',
            type: 'GET',
            success: function (data) {
                // Update stats
                $('#totalRevenue').text(data.totalRevenue);
                $('#totalOrders').text(data.totalOrders);
                $('#totalProducts').text(data.totalProducts);
                $('#totalUsers').text(data.totalUsers);

                btn.prop('disabled', false).html(originalText);
                showToast('Success', 'Stats refreshed', 'success');
            },
            error: function () {
                btn.prop('disabled', false).html(originalText);
                showToast('Error', 'Failed to refresh stats', 'danger');
            }
        });
    });

    // Quick stats date range
    $('#quickStatsRange').change(function () {
        var range = $(this).val();
        updateQuickStats(range);
    });
}

function copyBillingToShipping() {
    $('#shippingFirstName').val($('#billingFirstName').val());
    $('#shippingLastName').val($('#billingLastName').val());
    $('#shippingAddress1').val($('#billingAddress1').val());
    $('#shippingAddress2').val($('#billingAddress2').val());
    $('#shippingCity').val($('#billingCity').val());
    $('#shippingState').val($('#billingState').val());
    $('#shippingZipCode').val($('#billingZipCode').val());
}

function validateCheckoutForm() {
    var isValid = true;

    // Validate required fields
    $('.checkout-required').each(function () {
        if (!$(this).val().trim()) {
            $(this).addClass('is-invalid');
            isValid = false;
        } else {
            $(this).removeClass('is-invalid');
        }
    });

    // Validate email
    var email = $('#email').val();
    if (!isValidEmail(email)) {
        $('#email').addClass('is-invalid');
        isValid = false;
    }

    // Validate credit card
    if ($('#paymentMethodCard').is(':checked')) {
        var cardNumber = $('#cardNumber').val();
        var expiry = $('#cardExpiry').val();
        var cvv = $('#cardCVV').val();

        if (!isValidCardNumber(cardNumber)) {
            $('#cardNumber').addClass('is-invalid');
            isValid = false;
        }

        if (!isValidExpiry(expiry)) {
            $('#cardExpiry').addClass('is-invalid');
            isValid = false;
        }

        if (!isValidCVV(cvv)) {
            $('#cardCVV').addClass('is-invalid');
            isValid = false;
        }
    }

    if (!isValid) {
        showToast('Error', 'Please fill all required fields correctly', 'danger');
        $('html, body').animate({
            scrollTop: $('.is-invalid').first().offset().top - 100
        }, 500);
    }

    return isValid;
}

function isValidEmail(email) {
    var re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

function isValidCardNumber(number) {
    // Simple validation - in production use proper validation
    var re = /^\d{16}$/;
    return re.test(number.replace(/\s/g, ''));
}

function isValidExpiry(expiry) {
    var re = /^(0[1-9]|1[0-2])\/?([0-9]{2})$/;
    return re.test(expiry);
}

function isValidCVV(cvv) {
    var re = /^\d{3,4}$/;
    return re.test(cvv);
}

function updateQuickStats(range) {
    showLoading();

    $.ajax({
        url: '/Admin/GetQuickStats',
        type: 'GET',
        data: { range: range },
        success: function (data) {
            // Update quick stats cards
            // This would update the specific elements with new data
            hideLoading();
        },
        error: function () {
            hideLoading();
            showToast('Error', 'Failed to update stats', 'danger');
        }
    });
}

// ===== INITIALIZATION =====
// Run initialization when DOM is ready
$(document).ready(initSite);

// Initialize when page loads with turbolinks (if using)
$(document).on('turbolinks:load', initSite);