/**
 * Currency Mask for Dormitory Management System
 * Handles Turkish-style formatting (dot for thousands, comma for decimals)
 * and cleans data before submission for en-US backend.
 */

document.addEventListener("DOMContentLoaded", function () {
    const currencyInputs = document.querySelectorAll('.currency-input');

    currencyInputs.forEach(input => {
        // Initial formatting
        formatDisplay(input);

        input.addEventListener('input', function (e) {
            // Get current cursor position
            let cursorPosition = this.selectionStart;
            let originalLength = this.value.length;

            // Remove all non-numeric characters except comma
            let value = this.value.replace(/[^\d,]/g, '');

            // Ensure only one comma exists
            const parts = value.split(',');
            if (parts.length > 2) {
                value = parts[0] + ',' + parts.slice(1).join('');
            }

            // Limit to 2 decimal places
            if (parts.length === 2 && parts[1].length > 2) {
                value = parts[0] + ',' + parts[1].substring(0, 2);
            }

            // Separate integer and decimal parts
            let [integerPart, decimalPart] = value.split(',');

            // Add thousands separators (.) to integer part
            if (integerPart) {
                integerPart = parseInt(integerPart, 10).toLocaleString('tr-TR');
            }

            // Join parts
            this.value = decimalPart !== undefined ? integerPart + ',' + decimalPart : integerPart;

            // Adjust cursor position
            let newLength = this.value.length;
            cursorPosition = cursorPosition + (newLength - originalLength);
            this.setSelectionRange(cursorPosition, cursorPosition);
        });

        input.addEventListener('blur', function() {
            formatDisplay(this);
        });
    });

    // Intercept form submissions to clean the data
    // The controller now parses tr-TR format directly (14.500,00), so no conversion needed.
    // We only strip the thousands separator dots so the backend gets a clean value like "14500,00".
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const inputs = this.querySelectorAll('.currency-input');
            inputs.forEach(input => {
                // Remove thousands separator dots only; keep comma as decimal separator
                // e.g. "14.500,00" → "14500,00" (tr-TR compatible)
                input.value = input.value.replace(/\./g, '');
            });
        });
    });

    function formatDisplay(input) {
        let value = input.value;
        if (!value || value === "0" || value === "0,00") {
            input.value = "";
            return;
        }

        // Strip everything except digits and comma/dot
        // If it came from server, it might have a dot as decimal
        value = value.replace(/[^\d.,]/g, '');
        
        // Convert dot to comma if it's likely a decimal from server (e.g. 12500.00)
        if (value.includes('.') && !value.includes(',')) {
            // Check if it looks like a standard decimal (e.g. 12500.00)
            const parts = value.split('.');
            if (parts.length === 2 && parts[1].length <= 2) {
                value = parts[0] + ',' + parts[1];
            } else {
                // If multiple dots, probably thousands from a previous failed pass
                value = value.replace(/\./g, '');
            }
        }

        let [integerPart, decimalPart] = value.split(',');
        if (integerPart) {
            integerPart = parseInt(integerPart.replace(/\D/g, ''), 10).toLocaleString('tr-TR');
        }
        
        if (decimalPart !== undefined) {
            decimalPart = decimalPart.padEnd(2, '0').substring(0, 2);
            input.value = integerPart + ',' + decimalPart;
        } else {
            input.value = integerPart + ',00';
        }
    }
});
