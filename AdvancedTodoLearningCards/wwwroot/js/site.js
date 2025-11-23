// Advanced Todo Learning Cards - Main JavaScript

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});

// Smooth scroll to top button
const scrollToTopBtn = document.getElementById('scrollToTop');
if (scrollToTopBtn) {
    window.addEventListener('scroll', () => {
        if (window.pageYOffset > 300) {
            scrollToTopBtn.classList.add('show');
        } else {
            scrollToTopBtn.classList.remove('show');
        }
    });

    scrollToTopBtn.addEventListener('click', () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
}

// Confirm delete actions
document.querySelectorAll('[data-confirm-delete]').forEach(element => {
    element.addEventListener('click', function (e) {
        if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
            e.preventDefault();
        }
    });
});

// Add animation class on scroll
const animateOnScroll = () => {
    const elements = document.querySelectorAll('.animate-on-scroll');
    elements.forEach(element => {
        const position = element.getBoundingClientRect();
        if (position.top < window.innerHeight && position.bottom >= 0) {
            element.classList.add('fade-in');
        }
    });
};

window.addEventListener('scroll', animateOnScroll);
window.addEventListener('load', animateOnScroll);

// Format dates to relative time
function formatRelativeTime(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;

    return date.toLocaleDateString();
}

// Apply relative time formatting to elements with data-relative-time
document.querySelectorAll('[data-relative-time]').forEach(element => {
    const dateString = element.getAttribute('data-relative-time');
    element.textContent = formatRelativeTime(dateString);
});

// Card flip animation for review
function flipCard(cardElement) {
    cardElement.classList.toggle('flipped');
}

// Initialize tooltips
const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
});

// Initialize popovers
const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
popoverTriggerList.map(function (popoverTriggerEl) {
    return new bootstrap.Popover(popoverTriggerEl);
});

// Search debounce
function debounce(func, wait) {
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

// Live search for cards
const searchInput = document.getElementById('cardSearchInput');
if (searchInput) {
    const debouncedSearch = debounce(function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const cards = document.querySelectorAll('.learning-card-item');

        cards.forEach(card => {
            const title = card.querySelector('.card-title-custom')?.textContent.toLowerCase() || '';
            const content = card.querySelector('.card-content-preview')?.textContent.toLowerCase() || '';

            if (title.includes(searchTerm) || content.includes(searchTerm)) {
                card.style.display = '';
                card.classList.add('fade-in');
            } else {
                card.style.display = 'none';
            }
        });
    }, 300);

    searchInput.addEventListener('input', debouncedSearch);
}

// Copy to clipboard functionality
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showToast('Copied to clipboard!', 'success');
    }).catch(err => {
        showToast('Failed to copy', 'error');
    });
}

// Show toast notification
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toastContainer') || createToastContainer();

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'primary'} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    toastContainer.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();

    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
}

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toastContainer';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '9999';
    document.body.appendChild(container);
    return container;
}

// Keyboard shortcuts
document.addEventListener('keydown', function (e) {
    // Ctrl/Cmd + K for quick search
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        const searchInput = document.querySelector('input[type="search"], input[name="search"]');
        if (searchInput) {
            searchInput.focus();
        }
    }

    // Escape to close modals
    if (e.key === 'Escape') {
        const modals = document.querySelectorAll('.modal.show');
        modals.forEach(modal => {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) bsModal.hide();
        });
    }
});

// Export data functionality
async function exportData(format = 'csv') {
    try {
        const response = await fetch(`/Dashboard/Export${format.toUpperCase()}`, {
            method: 'GET',
            headers: {
                'Accept': format === 'csv' ? 'text/csv' : 'application/json'
            }
        });

        if (!response.ok) throw new Error('Export failed');

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `learning-cards-export-${new Date().toISOString().split('T')[0]}.${format}`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();

        showToast('Data exported successfully!', 'success');
    } catch (error) {
        console.error('Export error:', error);
        showToast('Failed to export data', 'error');
    }
}

// Form validation helpers
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form) return false;

    let isValid = true;
    const inputs = form.querySelectorAll('input[required], textarea[required], select[required]');

    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('is-invalid');
            isValid = false;
        } else {
            input.classList.remove('is-invalid');
        }
    });

    return isValid;
}

// Auto-save draft (localStorage)
function saveDraft(formId, key) {
    const form = document.getElementById(formId);
    if (!form) return;

    const formData = new FormData(form);
    const data = Object.fromEntries(formData);
    localStorage.setItem(key, JSON.stringify(data));
}

function loadDraft(formId, key) {
    const form = document.getElementById(formId);
    if (!form) return;

    const savedData = localStorage.getItem(key);
    if (!savedData) return;

    const data = JSON.parse(savedData);
    Object.keys(data).forEach(name => {
        const input = form.querySelector(`[name="${name}"]`);
        if (input && !input.value) {
            input.value = data[name];
        }
    });
}

// Console welcome message
console.log('%c🧠 Advanced Learning Cards', 'color: #7C3AED; font-size: 24px; font-weight: bold;');
console.log('%cBuilt with ASP.NET Core MVC & Bootstrap 5', 'color: #6B7280; font-size: 14px;');
console.log('%cVersion: 1.0.0', 'color: #6B7280;');