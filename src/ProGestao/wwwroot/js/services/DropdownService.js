/**
 * =============================================================
 * PROGESTAO - DROPDOWN SERVICE PATTERN
 * Arquivo: wwwroot/js/services/DropdownService.js
 * Versão: 1.0
 * Descrição: Service pattern para gerenciamento de dropdowns
 * =============================================================
 */

'use strict';

/**
 * Dropdown Service - Gerencia todos os dropdowns da aplicação
 * Implementa Service Pattern e Observer Pattern
 */
class DropdownService {
    constructor() {
        this.dropdowns = new Map();
        this.observers = [];
        this.config = {
            zIndexBase: 1050,
            animationDuration: 300,
            mobileBreakpoint: 768,
            debug: false
        };

        this.init();
    }

    /**
     * Inicializa o serviço
     */
    init() {
        this.bindEvents();
        this.setupMutationObserver();
        this.log('DropdownService initialized');
    }

    /**
     * Registra um dropdown no serviço
     * @param {string} id - ID único do dropdown
     * @param {HTMLElement} element - Elemento do dropdown
     * @param {Object} options - Configurações específicas
     */
    register(id, element, options = {}) {
        if (this.dropdowns.has(id)) {
            this.log(`Dropdown ${id} already registered, updating...`, 'warn');
        }

        const dropdown = new DropdownInstance(id, element, {
            ...this.config,
            ...options
        }, this);

        this.dropdowns.set(id, dropdown);
        this.notifyObservers('register', { id, dropdown });
        this.log(`Dropdown ${id} registered successfully`);

        return dropdown;
    }

    /**
     * Remove um dropdown do serviço
     * @param {string} id - ID do dropdown
     */
    unregister(id) {
        const dropdown = this.dropdowns.get(id);
        if (dropdown) {
            dropdown.destroy();
            this.dropdowns.delete(id);
            this.notifyObservers('unregister', { id });
            this.log(`Dropdown ${id} unregistered`);
        }
    }

    /**
     * Fecha todos os dropdowns exceto o especificado
     * @param {string} excludeId - ID do dropdown a ser excluído
     */
    closeAll(excludeId = null) {
        this.dropdowns.forEach((dropdown, id) => {
            if (id !== excludeId && dropdown.isOpen()) {
                dropdown.close();
            }
        });
    }

    /**
     * Adiciona um observer
     * @param {Function} callback - Função de callback
     */
    addObserver(callback) {
        this.observers.push(callback);
    }

    /**
     * Remove um observer
     * @param {Function} callback - Função de callback
     */
    removeObserver(callback) {
        const index = this.observers.indexOf(callback);
        if (index > -1) {
            this.observers.splice(index, 1);
        }
    }

    /**
     * Notifica todos os observers
     * @param {string} event - Nome do evento
     * @param {Object} data - Dados do evento
     */
    notifyObservers(event, data) {
        this.observers.forEach(callback => {
            try {
                callback(event, data);
            } catch (error) {
                this.log(`Observer error: ${error.message}`, 'error');
            }
        });
    }

    /**
     * Vincula eventos globais
     */
    bindEvents() {
        // Fecha dropdowns ao clicar fora
        document.addEventListener('click', (event) => {
            this.handleGlobalClick(event);
        });

        // Fecha dropdowns com ESC
        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape') {
                this.closeAll();
            }
        });

        // Reajusta posicionamento no resize
        window.addEventListener('resize', this.debounce(() => {
            this.dropdowns.forEach(dropdown => {
                if (dropdown.isOpen()) {
                    dropdown.updatePosition();
                }
            });
        }, 250));
    }

    /**
     * Configura observer para mudanças no DOM
     */
    setupMutationObserver() {
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                mutation.addedNodes.forEach((node) => {
                    if (node.nodeType === Node.ELEMENT_NODE) {
                        this.autoRegisterDropdowns(node);
                    }
                });
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    /**
     * Auto-registra dropdowns encontrados no DOM
     * @param {HTMLElement} container - Container para buscar dropdowns
     */
    autoRegisterDropdowns(container = document) {
        const dropdowns = container.querySelectorAll('[data-dropdown-auto]');
        dropdowns.forEach(element => {
            const id = element.getAttribute('data-dropdown-id') ||
                element.id ||
                this.generateId();

            if (!this.dropdowns.has(id)) {
                this.register(id, element);
            }
        });
    }

    /**
     * Manipula cliques globais
     * @param {Event} event - Evento de clique
     */
    handleGlobalClick(event) {
        this.dropdowns.forEach((dropdown, id) => {
            if (!dropdown.element.contains(event.target) && dropdown.isOpen()) {
                dropdown.close();
            }
        });
    }

    /**
     * Debounce utility
     * @param {Function} func - Função para fazer debounce
     * @param {number} wait - Tempo de espera
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
    }

    /**
     * Gera ID único
     * @returns {string} ID único
     */
    generateId() {
        return 'dropdown_' + Math.random().toString(36).substr(2, 9);
    }

    /**
     * Log com níveis
     * @param {string} message - Mensagem
     * @param {string} level - Nível do log
     */
    log(message, level = 'info') {
        if (this.config.debug) {
            console[level](`[DropdownService] ${message}`);
        }
    }
}

/**
 * Instância individual de dropdown
 * Implementa Strategy Pattern para diferentes comportamentos
 */
class DropdownInstance {
    constructor(id, element, config, service) {
        this.id = id;
        this.element = element;
        this.config = config;
        this.service = service;
        this.isOpenState = false;

        this.toggle = null;
        this.menu = null;
        this.items = [];

        this.strategies = {
            positioning: new PositioningStrategy(this),
            animation: new AnimationStrategy(this),
            accessibility: new AccessibilityStrategy(this)
        };

        this.init();
    }

    /**
     * Inicializa a instância
     */
    init() {
        this.findElements();
        this.bindEvents();
        this.setupAccessibility();
        this.applyStyles();
        this.service.log(`Dropdown instance ${this.id} initialized`);
    }

    /**
     * Encontra elementos do dropdown
     */
    findElements() {
        this.toggle = this.element.querySelector('[data-bs-toggle="dropdown"]') ||
            this.element.querySelector('.dropdown-toggle');

        this.menu = this.element.querySelector('.dropdown-menu');

        if (this.menu) {
            this.items = Array.from(this.menu.querySelectorAll('.dropdown-item, .dropdown-period-item'));
        }

        if (!this.toggle || !this.menu) {
            throw new Error(`Invalid dropdown structure for ${this.id}`);
        }
    }

    /**
     * Vincula eventos específicos do dropdown
     */
    bindEvents() {
        // Toggle dropdown
        this.toggle.addEventListener('click', (event) => {
            event.preventDefault();
            event.stopPropagation();
            this.toggle();
        });

        // Items do dropdown
        this.items.forEach((item, index) => {
            item.addEventListener('click', (event) => {
                this.handleItemClick(event, item, index);
            });
        });

        // Bootstrap events (se disponível)
        if (window.bootstrap) {
            this.toggle.addEventListener('shown.bs.dropdown', () => {
                this.onShow();
            });

            this.toggle.addEventListener('hidden.bs.dropdown', () => {
                this.onHide();
            });
        }
    }

    /**
     * Configura acessibilidade
     */
    setupAccessibility() {
        this.strategies.accessibility.setup();
    }

    /**
     * Aplica estilos necessários
     */
    applyStyles() {
        // Z-index dinâmico
        this.element.style.zIndex = this.config.zIndexBase;

        // Classes necessárias
        this.element.classList.add('dropdown-period-container');

        if (this.toggle) {
            this.toggle.classList.add('dropdown-period-toggle');
        }
    }

    /**
     * Abre o dropdown
     */
    open() {
        if (this.isOpenState) return;

        this.service.closeAll(this.id);

        this.isOpenState = true;
        this.element.classList.add('show');
        this.menu.classList.add('show');

        this.strategies.positioning.position();
        this.strategies.animation.show();

        this.service.notifyObservers('open', { id: this.id, dropdown: this });
        this.service.log(`Dropdown ${this.id} opened`);
    }

    /**
     * Fecha o dropdown
     */
    close() {
        if (!this.isOpenState) return;

        this.isOpenState = false;
        this.element.classList.remove('show');

        this.strategies.animation.hide(() => {
            this.menu.classList.remove('show');
        });

        this.service.notifyObservers('close', { id: this.id, dropdown: this });
        this.service.log(`Dropdown ${this.id} closed`);
    }

    /**
     * Alterna o estado do dropdown
     */
    toggle() {
        if (this.isOpenState) {
            this.close();
        } else {
            this.open();
        }
    }

    /**
     * Verifica se está aberto
     * @returns {boolean} Estado do dropdown
     */
    isOpen() {
        return this.isOpenState;
    }

    /**
     * Atualiza posicionamento
     */
    updatePosition() {
        if (this.isOpenState) {
            this.strategies.positioning.position();
        }
    }

    /**
     * Manipula clique em item
     * @param {Event} event - Evento
     * @param {HTMLElement} item - Item clicado
     * @param {number} index - Índice do item
     */
    handleItemClick(event, item, index) {
        // Remove active de todos os itens
        this.items.forEach(i => i.classList.remove('active'));

        // Adiciona active no item clicado
        item.classList.add('active');

        // Notifica observers
        this.service.notifyObservers('itemClick', {
            id: this.id,
            dropdown: this,
            item,
            index,
            value: item.textContent.trim()
        });

        // Fecha o dropdown
        this.close();
    }

    /**
     * Callback quando dropdown é mostrado
     */
    onShow() {
        this.element.style.zIndex = this.config.zIndexBase + 5;
        this.menu.style.zIndex = this.config.zIndexBase + 10;
    }

    /**
     * Callback quando dropdown é escondido
     */
    onHide() {
        this.element.style.zIndex = this.config.zIndexBase;
        this.menu.style.zIndex = this.config.zIndexBase;
    }

    /**
     * Destrói a instância
     */
    destroy() {
        // Remove eventos (implementar se necessário)
        this.service.log(`Dropdown instance ${this.id} destroyed`);
    }
}

/**
 * Strategy para posicionamento
 */
class PositioningStrategy {
    constructor(dropdown) {
        this.dropdown = dropdown;
    }

    position() {
        const { menu, toggle } = this.dropdown;
        const rect = toggle.getBoundingClientRect();
        const menuRect = menu.getBoundingClientRect();
        const viewport = {
            width: window.innerWidth,
            height: window.innerHeight
        };

        // Posicionamento básico
        menu.style.position = 'absolute';
        menu.style.top = '100%';
        menu.style.left = '0';

        // Ajusta se sair da tela
        if (rect.left + menuRect.width > viewport.width) {
            menu.style.left = 'auto';
            menu.style.right = '0';
        }

        // Para mobile, centraliza
        if (viewport.width <= this.dropdown.config.mobileBreakpoint) {
            menu.style.position = 'fixed';
            menu.style.left = '1rem';
            menu.style.right = '1rem';
            menu.style.top = `${rect.bottom + 5}px`;
        }
    }
}

/**
 * Strategy para animações
 */
class AnimationStrategy {
    constructor(dropdown) {
        this.dropdown = dropdown;
    }

    show() {
        const { menu } = this.dropdown;
        menu.style.opacity = '0';
        menu.style.transform = 'translateY(-8px) scale(0.95)';

        requestAnimationFrame(() => {
            menu.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
            menu.style.opacity = '1';
            menu.style.transform = 'translateY(0) scale(1)';
        });
    }

    hide(callback) {
        const { menu } = this.dropdown;
        menu.style.transition = 'all 0.2s ease';
        menu.style.opacity = '0';
        menu.style.transform = 'translateY(-8px) scale(0.95)';

        setTimeout(() => {
            if (callback) callback();
        }, 200);
    }
}

/**
 * Strategy para acessibilidade
 */
class AccessibilityStrategy {
    constructor(dropdown) {
        this.dropdown = dropdown;
    }

    setup() {
        const { toggle, menu, items } = this.dropdown;

        // ARIA attributes
        toggle.setAttribute('aria-haspopup', 'true');
        toggle.setAttribute('aria-expanded', 'false');

        if (!menu.id) {
            menu.id = this.dropdown.id + '_menu';
        }

        toggle.setAttribute('aria-controls', menu.id);

        // Keyboard navigation
        items.forEach((item, index) => {
            item.setAttribute('tabindex', '-1');
            item.setAttribute('role', 'menuitem');
        });
    }
}

// Singleton instance
const dropdownService = new DropdownService();

// Auto-initialize
document.addEventListener('DOMContentLoaded', () => {
    dropdownService.autoRegisterDropdowns();
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { DropdownService, dropdownService };
}

// Global access
window.ProGestao = window.ProGestao || {};
window.ProGestao.DropdownService = dropdownService;