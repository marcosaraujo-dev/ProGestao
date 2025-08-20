/**
 * PROGESTAO - JAVASCRIPT OTIMIZADO
 * Arquivo: wwwroot/js/site.js
 * Versão: 2.0 - Performance e UX melhorados
 */

(function () {
    'use strict';

    // =============================================================
    // 1. CONSTANTES E CONFIGURAÇÕES
    // =============================================================
    const CONFIG = {
        ALERT_AUTO_HIDE_DELAY: 5000,
        FORM_SUBMIT_DELAY: 300,
        TOAST_DELAY: 3000,
        DEBOUNCE_DELAY: 300
    };

    const SELECTORS = {
        TOOLTIPS: '[data-bs-toggle="tooltip"]',
        CONFIRMABLE: '[data-confirm]',
        FORMS: 'form',
        SUBMIT_BUTTONS: 'button[type="submit"]',
        DATE_INPUTS: 'input[type="date"]',
        LOADING_OVERLAY: '#loadingOverlay',
        TOAST: '#liveToast'
    };

    // =============================================================
    // 2. UTILITÁRIOS
    // =============================================================
    const Utils = {
        /**
         * Debounce function para otimizar performance
         */
        debounce(func, wait) {
            let timeout;
            return function executedFunction(...args) {
                const later = () => {
                    clearTimeout(timeout);
                    func(...args);
                };
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
            };
        },

        /**
         * Mostra loading overlay
         */
        showLoading() {
            const overlay = document.querySelector(SELECTORS.LOADING_OVERLAY);
            if (overlay) {
                overlay.style.display = 'flex';
            }
        },

        /**
         * Esconde loading overlay
         */
        hideLoading() {
            const overlay = document.querySelector(SELECTORS.LOADING_OVERLAY);
            if (overlay) {
                overlay.style.display = 'none';
            }
        },

        /**
         * Mostra toast notification
         */
        showToast(message, type = 'info') {
            const toast = document.querySelector(SELECTORS.TOAST);
            if (!toast) return;

            const toastBody = toast.querySelector('.toast-body');
            const toastHeader = toast.querySelector('.toast-header strong');

            if (toastBody) {
                toastBody.innerHTML = `<i class="fas fa-info-circle me-2"></i>${message}`;
            }

            // Adicionar classe de cor baseada no tipo
            toast.className = `toast ${type === 'success' ? 'text-bg-success' : type === 'error' ? 'text-bg-danger' : 'text-bg-info'}`;

            const bsToast = new bootstrap.Toast(toast, {
                delay: CONFIG.TOAST_DELAY
            });
            bsToast.show();
        },

        /**
         * Formata data para input date
         */
        formatDateForInput(date) {
            if (!date) return '';
            return date.toISOString().split('T')[0];
        },

        /**
         * Valida email
         */
        isValidEmail(email) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return emailRegex.test(email);
        }
    };

    // =============================================================
    // 3. INICIALIZAÇÃO PRINCIPAL
    // =============================================================
    function initializeApp() {
        initializeTooltips();
        initializeAlerts();
        initializeConfirmations();
        initializeForms();
        initializeDateInputs();
        initializeFormValidation();
        initializeConditionalFields();
        initializeKeyboardShortcuts();

        console.log('ProGestao: Aplicação inicializada com sucesso');
    }

    // =============================================================
    // 4. TOOLTIPS E POPOVERS
    // =============================================================
    function initializeTooltips() {
        const tooltipTriggerList = document.querySelectorAll(SELECTORS.TOOLTIPS);
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => {
            return new bootstrap.Tooltip(tooltipTriggerEl, {
                trigger: 'hover focus',
                delay: { show: 300, hide: 100 }
            });
        });

        console.log(`ProGestao: ${tooltipList.length} tooltips inicializados`);
    }

    // =============================================================
    // 5. GERENCIAMENTO DE ALERTS
    // =============================================================
    function initializeAlerts() {
        // Auto-hide alerts após delay configurado
        setTimeout(() => {
            const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
            alerts.forEach(alert => {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
                bsAlert.close();
            });
        }, CONFIG.ALERT_AUTO_HIDE_DELAY);
    }

    // =============================================================
    // 6. CONFIRMAÇÕES DE AÇÃO
    // =============================================================
    function initializeConfirmations() {
        document.addEventListener('click', (e) => {
            const element = e.target.closest(SELECTORS.CONFIRMABLE);
            if (!element) return;

            const message = element.getAttribute('data-confirm');
            const title = element.getAttribute('data-confirm-title') || 'Confirmar ação';

            if (!confirm(`${title}\n\n${message}`)) {
                e.preventDefault();
                e.stopPropagation();
            }
        });
    }

    // =============================================================
    // 7. GERENCIAMENTO DE FORMULÁRIOS
    // =============================================================
    function initializeForms() {
        const forms = document.querySelectorAll(SELECTORS.FORMS);

        forms.forEach(form => {
            form.addEventListener('submit', handleFormSubmit);
        });
    }

    function handleFormSubmit(e) {
        const form = e.target;
        const submitBtn = form.querySelector(SELECTORS.SUBMIT_BUTTONS);

        if (submitBtn && !form.classList.contains('no-loading')) {
            // Prevenir duplo submit
            if (submitBtn.classList.contains('btn-loading')) {
                e.preventDefault();
                return;
            }

            // Mostrar loading state
            setTimeout(() => {
                submitBtn.classList.add('btn-loading');
                submitBtn.disabled = true;

                // Backup do texto original
                if (!submitBtn.dataset.originalText) {
                    submitBtn.dataset.originalText = submitBtn.innerHTML;
                }
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processando...';

                Utils.showLoading();
            }, CONFIG.FORM_SUBMIT_DELAY);
        }
    }

    // =============================================================
    // 8. CAMPOS DE DATA
    // =============================================================
    function initializeDateInputs() {
        const dateInputs = document.querySelectorAll(SELECTORS.DATE_INPUTS);

        dateInputs.forEach(input => {
            // Definir data padrão se não tiver valor
            if (!input.value && input.hasAttribute('data-default-today')) {
                input.value = Utils.formatDateForInput(new Date());
            }

            // Validação de datas
            input.addEventListener('change', validateDateInput);
        });
    }

    function validateDateInput(e) {
        const input = e.target;
        const form = input.closest('form');
        if (!form) return;

        // Validar data fim não anterior à data início
        const dataInicio = form.querySelector('input[name*="DataInicio"]');
        const dataFim = form.querySelector('input[name*="DataFim"]');

        if (dataInicio && dataFim && dataInicio.value && dataFim.value) {
            if (new Date(dataFim.value) < new Date(dataInicio.value)) {
                dataFim.setCustomValidity('A data de fim não pode ser anterior à data de início');
                dataFim.classList.add('is-invalid');
            } else {
                dataFim.setCustomValidity('');
                dataFim.classList.remove('is-invalid');
            }
        }
    }

    // =============================================================
    // 9. VALIDAÇÃO DE FORMULÁRIOS EM TEMPO REAL
    // =============================================================
    function initializeFormValidation() {
        // Validação de email
        const emailInputs = document.querySelectorAll('input[type="email"]');
        emailInputs.forEach(input => {
            input.addEventListener('blur', validateEmailInput);
        });

        // Validação de campos obrigatórios
        const requiredInputs = document.querySelectorAll('input[required], select[required], textarea[required]');
        requiredInputs.forEach(input => {
            input.addEventListener('blur', validateRequiredInput);
        });

        // Validação de números
        const numberInputs = document.querySelectorAll('input[type="number"]');
        numberInputs.forEach(input => {
            input.addEventListener('input', validateNumberInput);
        });
    }

    function validateEmailInput(e) {
        const input = e.target;
        if (input.value && !Utils.isValidEmail(input.value)) {
            input.setCustomValidity('Por favor, insira um email válido');
            input.classList.add('is-invalid');
        } else {
            input.setCustomValidity('');
            input.classList.remove('is-invalid');
        }
    }

    function validateRequiredInput(e) {
        const input = e.target;
        if (input.hasAttribute('required') && !input.value.trim()) {
            input.classList.add('is-invalid');
        } else {
            input.classList.remove('is-invalid');
        }
    }

    function validateNumberInput(e) {
        const input = e.target;
        const value = parseFloat(input.value);
        const min = parseFloat(input.min);
        const max = parseFloat(input.max);

        if (input.value && (isNaN(value) || (min && value < min) || (max && value > max))) {
            input.classList.add('is-invalid');
        } else {
            input.classList.remove('is-invalid');
        }
    }

    // =============================================================
    // 10. CAMPOS CONDICIONAIS
    // =============================================================
    function initializeConditionalFields() {
        // Mostrar/esconder exemplos baseado na seleção de projeto
        const projetoSelect = document.getElementById('selectProjeto');
        const exemplosSemProjeto = document.getElementById('exemplosSemProjeto');

        if (projetoSelect && exemplosSemProjeto) {
            projetoSelect.addEventListener('change', () => {
                if (projetoSelect.value === '') {
                    exemplosSemProjeto.classList.add('show');
                } else {
                    exemplosSemProjeto.classList.remove('show');
                }
            });

            // Verificar estado inicial
            if (projetoSelect.value === '') {
                exemplosSemProjeto.classList.add('show');
            }
        }

        // Lógica para campos condicionais baseados em status
        const statusSelects = document.querySelectorAll('select[name*="Status"]');
        statusSelects.forEach(select => {
            select.addEventListener('change', handleStatusChange);
        });
    }

    function handleStatusChange(e) {
        const select = e.target;
        const selectedOption = select.options[select.selectedIndex];
        const statusText = selectedOption.text.toLowerCase();
        const form = select.closest('form');

        if (!form) return;

        // Mostrar/esconder campo de data de conclusão baseado no status
        const dataFimRealField = form.querySelector('input[name*="DataFimReal"]');
        const dataFimRealGroup = dataFimRealField?.closest('.form-group, .mb-3');

        if (dataFimRealGroup) {
            if (statusText.includes('concluída') || statusText.includes('finalizada')) {
                dataFimRealGroup.style.display = 'block';
                dataFimRealField.setAttribute('required', '');
            } else {
                dataFimRealGroup.style.display = 'none';
                dataFimRealField.removeAttribute('required');
                dataFimRealField.value = '';
            }
        }
    }

    // =============================================================
    // 11. ATALHOS DE TECLADO
    // =============================================================
    function initializeKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl + S para salvar formulário
            if (e.ctrlKey && e.key === 's') {
                e.preventDefault();
                const form = document.querySelector('form');
                if (form) {
                    const submitBtn = form.querySelector(SELECTORS.SUBMIT_BUTTONS);
                    if (submitBtn && !submitBtn.disabled) {
                        submitBtn.click();
                    }
                }
            }

            // Esc para fechar modais ou voltar
            if (e.key === 'Escape') {
                const modal = document.querySelector('.modal.show');
                if (modal) {
                    const bsModal = bootstrap.Modal.getInstance(modal);
                    if (bsModal) bsModal.hide();
                } else {
                    const backButton = document.querySelector('.btn-secondary[href], .btn-outline-secondary[href]');
                    if (backButton) {
                        backButton.click();
                    }
                }
            }
        });
    }

    // =============================================================
    // 12. FUNCIONALIDADES ESPECÍFICAS DO PROGESTAO
    // =============================================================

    /**
     * Atualiza preview de cores em seletores de tipo/status
     */
    function initializeColorPreviews() {
        const colorSelects = document.querySelectorAll('select[data-color-preview]');
        colorSelects.forEach(select => {
            select.addEventListener('change', (e) => {
                const selectedOption = e.target.options[e.target.selectedIndex];
                const color = selectedOption.getAttribute('data-color');
                if (color) {
                    e.target.style.borderLeftColor = color;
                    e.target.style.borderLeftWidth = '4px';
                }
            });
        });
    }

    /**
     * Auto-save para formulários longos (draft)
     */
    function initializeAutoSave() {
        const forms = document.querySelectorAll('form[data-autosave]');
        forms.forEach(form => {
            const formId = form.getAttribute('data-autosave');
            const inputs = form.querySelectorAll('input, select, textarea');

            // Restaurar dados salvos
            const savedData = localStorage.getItem(`progestao_draft_${formId}`);
            if (savedData) {
                try {
                    const data = JSON.parse(savedData);
                    Object.keys(data).forEach(name => {
                        const input = form.querySelector(`[name="${name}"]`);
                        if (input && !input.value) {
                            input.value = data[name];
                        }
                    });
                } catch (e) {
                    console.warn('Erro ao restaurar draft:', e);
                }
            }

            // Salvar mudanças
            const saveData = Utils.debounce(() => {
                const data = {};
                inputs.forEach(input => {
                    if (input.name && input.value) {
                        data[input.name] = input.value;
                    }
                });
                localStorage.setItem(`progestao_draft_${formId}`, JSON.stringify(data));
            }, CONFIG.DEBOUNCE_DELAY);

            inputs.forEach(input => {
                input.addEventListener('input', saveData);
            });

            // Limpar draft ao submeter
            form.addEventListener('submit', () => {
                localStorage.removeItem(`progestao_draft_${formId}`);
            });
        });
    }

    // =============================================================
    // 13. TRATAMENTO DE ERROS E FEEDBACK
    // =============================================================
    window.addEventListener('unhandledrejection', (e) => {
        console.error('Erro não tratado:', e.reason);
        Utils.showToast('Ocorreu um erro inesperado. Tente novamente.', 'error');
        Utils.hideLoading();
    });

    window.addEventListener('error', (e) => {
        console.error('Erro JavaScript:', e.error);
        Utils.hideLoading();
    });

    // =============================================================
    // 14. API PÚBLICA
    // =============================================================
    window.ProGestao = {
        Utils,
        showToast: Utils.showToast,
        showLoading: Utils.showLoading,
        hideLoading: Utils.hideLoading,
        version: '2.0'
    };

    // =============================================================
    // 15. INICIALIZAÇÃO
    // =============================================================
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeApp);
    } else {
        initializeApp();
    }

    // Inicializar funcionalidades específicas quando disponíveis
    setTimeout(() => {
        initializeColorPreviews();
        initializeAutoSave();
    }, 100);

})();