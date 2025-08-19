// ProGestao - Scripts 

document.addEventListener('DOMContentLoaded', function () {

    // Inicializar tooltips do Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Auto-hide alerts após 5 segundos
    setTimeout(function () {
        var alerts = document.querySelectorAll('.alert');
        alerts.forEach(function (alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);

    // Confirmação de exclusão
    document.querySelectorAll('[data-confirm]').forEach(function (element) {
        element.addEventListener('click', function (e) {
            var message = this.getAttribute('data-confirm');
            if (!confirm(message)) {
                e.preventDefault();
            }
        });
    });

    // Loading state para formulários
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function () {
            var submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.classList.add('loading');
                submitBtn.disabled = true;
            }
        });
    });

    // Máscara para campos de data
    document.querySelectorAll('input[type="date"]').forEach(function (input) {
        if (!input.value) {
            input.value = new Date().toISOString().split('T')[0];
        }
    });

    // Validação de formulários em tempo real
    document.querySelectorAll('.form-control, .form-select').forEach(function (field) {
        field.addEventListener('blur', function () {
            validateField(this);
        });
    });

    // Busca em tempo real (debounced)
    var searchInputs = document.querySelectorAll('input[type="search"], .search-input');
    searchInputs.forEach(function (input) {
        var timeout;
        input.addEventListener('input', function () {
            clearTimeout(timeout);
            timeout = setTimeout(function () {
                // Implementar busca
                console.log('Searching for:', input.value);
            }, 500);
        });
    });
});

// Função para validar campos
function validateField(field) {
    var isValid = field.checkValidity();
    var feedback = field.parentNode.querySelector('.invalid-feedback');

    if (isValid) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
    } else {
        field.classList.remove('is-valid');
        field.classList.add('is-invalid');

        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            field.parentNode.appendChild(feedback);
        }
        feedback.textContent = field.validationMessage;
    }
}

// Função para exportar dados
function exportarDados(tipo, dados) {
    if (tipo === 'csv') {
        var csv = convertToCSV(dados);
        downloadFile(csv, 'dados.csv', 'text/csv');
    } else if (tipo === 'json') {
        var json = JSON.stringify(dados, null, 2);
        downloadFile(json, 'dados.json', 'application/json');
    }
}

function convertToCSV(data) {
    if (!data.length) return '';

    var headers = Object.keys(data[0]);
    var csvContent = headers.join(',') + '\n';

    data.forEach(function (row) {
        var values = headers.map(function (header) {
            var value = row[header] || '';
            return '"' + value.toString().replace(/"/g, '""') + '"';
        });
        csvContent += values.join(',') + '\n';
    });

    return csvContent;
}

function downloadFile(content, filename, contentType) {
    var blob = new Blob([content], { type: contentType });
    var url = window.URL.createObjectURL(blob);
    var link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
}

// Função para mostrar notificações
function showNotification(message, type = 'info') {
    var alertClass = 'alert-' + type;
    var alert = document.createElement('div');
    alert.className = 'alert ' + alertClass + ' alert-dismissible fade show position-fixed';
    alert.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    alert.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(alert);

    setTimeout(function () {
        if (alert.parentNode) {
            alert.parentNode.removeChild(alert);
        }
    }, 5000);
}

// Utility functions
window.ProGestao = {
    validateField: validateField,
    exportarDados: exportarDados,
    showNotification: showNotification
};