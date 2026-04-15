/**
 * Premium Searchable Select component
 * Handles custom dropdown behavior for room selection.
 */

class PremiumSelect {
    constructor(inputId, dropdownId, hiddenId) {
        this.input = document.getElementById(inputId);
        this.dropdown = document.getElementById(dropdownId);
        this.hidden = document.getElementById(hiddenId);
        this.options = this.dropdown.querySelectorAll('.custom-select-option');
        
        if (this.input && this.dropdown) {
            this.init();
        }
    }

    init() {
        // Show dropdown on focus
        this.input.addEventListener('focus', () => {
            this.showDropdown();
            this.filterOptions();
        });

        // Search options as typing
        this.input.addEventListener('input', () => {
            this.showDropdown();
            this.filterOptions();
        });

        // Selection logic
        this.options.forEach(option => {
            option.addEventListener('click', () => {
                const id = option.getAttribute('data-id');
                const val = option.getAttribute('data-value');
                this.selectOption(id, val);
            });
        });

        // Close on blur (with delay to allow click event to fire)
        this.input.addEventListener('blur', () => {
            setTimeout(() => this.hideDropdown(), 200);
        });
        
        // Prevent dropdown from closing when clicking inside it
        this.dropdown.addEventListener('mousedown', (e) => {
            e.preventDefault();
        });
    }

    showDropdown() {
        this.dropdown.classList.add('show');
    }

    hideDropdown() {
        this.dropdown.classList.remove('show');
    }

    filterOptions() {
        const query = this.input.value.toLowerCase().trim();
        let visibleCount = 0;

        this.options.forEach(option => {
            const text = option.textContent.toLowerCase();
            const value = option.getAttribute('data-value').toLowerCase();
            
            if (text.includes(query) || value.includes(query)) {
                option.style.display = 'block';
                visibleCount++;
            } else {
                option.style.display = 'none';
            }
        });

        // Handle "No results"
        let noResults = this.dropdown.querySelector('.no-results');
        if (visibleCount === 0) {
            if (!noResults) {
                noResults = document.createElement('div');
                noResults.className = 'no-results';
                noResults.textContent = 'No matching rooms found';
                this.dropdown.appendChild(noResults);
            }
        } else if (noResults) {
            noResults.remove();
        }
    }

    selectOption(id, value) {
        this.input.value = value;
        this.hidden.value = id;
        this.filterOptions(); // Reset visibility
        this.hideDropdown();
        
        // Trigger generic "change" event for other listeners
        this.input.dispatchEvent(new Event('change'));
    }
}

// Export for use
window.PremiumSelect = PremiumSelect;
