
// Form submission handler
document.getElementById('send-quote-btn')?.addEventListener('click', async function (e) {
    e.preventDefault();

    // Validate contact fields before submitting
    const isNameValid = validateName();
    const isEmailValid = validateEmail();
    const isPhoneValid = validatePhone();

    if (!isNameValid || !isEmailValid || !isPhoneValid) {
        // Scroll to first validation error
        const firstError = document.querySelector('.is-invalid');
        if (firstError) {
            firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
            firstError.focus();
        }
        return; // Don't submit if validation fails
    }

    // Collect all form data
    const formData = new FormData(document.getElementById('quote-form'));

    // Get all form values (including display text for dropdowns)
    const materialSelect = document.getElementById('material');
    const finishSelect = document.getElementById('finish');
    const cuttingDieSelect = document.getElementById('cutting-die');

    // Get Printing (ColorCode) from active button
    let printingValue = '';
    let printingText = '';
    let printingSection = null;
    Array.from(document.querySelectorAll('.form-section')).forEach(section => {
        const label = section.querySelector('.form-section-label');
        if (label && label.textContent.trim() === 'Printing') {
            printingSection = section;
        }
    });
    if (printingSection) {
        const activePrintingBtn = printingSection.querySelector('#printing-filter button.active');
        if (activePrintingBtn) {
            printingValue = activePrintingBtn.getAttribute('data-printing-id') || '';
            printingText = activePrintingBtn.getAttribute('data-printing-text') || activePrintingBtn.textContent.trim();
        }
    }

    const getSelectedText = (selectElement) => {
        if (!selectElement || !selectElement.value) return '';
        const selectedOption = selectElement.options[selectElement.selectedIndex];
        return selectedOption ? selectedOption.text : '';
    };

    const getApplicationMethodText = () => {
        const selected = document.querySelector('input[name="application-method"]:checked');
        if (!selected) return '';
        const label = selected.closest('label');
        return label ? label.textContent.trim().replace(/\s*\([^)]*\)/, '') : '';
    };

    const getUnwindDirectionText = () => {
        const selected = document.querySelector('input[name="unwind-direction"]:checked');
        if (!selected) return '';
        const label = selected.closest('label');
        return label ? label.textContent.trim() : '';
    };

    const getArtworkOptionText = () => {
        const selected = document.querySelector('input[name="artwork-option"]:checked');
        if (!selected) return '';
        const value = selected.value;
        switch (value) {
            case 'upload-now': return 'Upload artwork now';
            case 'artwork-not-ready': return 'Artwork is not ready';
            case 'upload-later': return 'Upload artwork later';
            default: return value;
        }
    };

    const getShapeText = () => {
        const selected = document.querySelector('input[name="shape"]:checked');
        if (!selected) return '';
        const value = selected.value;
        return value.charAt(0).toUpperCase() + value.slice(1);
    };

    // Get actual form values (for restoration)
    const shapeValue = document.querySelector('input[name="shape"]:checked')?.value || '';
    const cornersValue = document.querySelector('input[name="corners"]:checked')?.value || '';
    const applicationMethodValue = document.querySelector('input[name="application-method"]:checked')?.value || '';
    const unwindDirectionValue = document.querySelector('input[name="unwind-direction"]:checked')?.value || '';
    const artworkOptionValue = document.querySelector('input[name="artwork-option"]:checked')?.value || '';
    const materialValue = materialSelect?.value || '';
    const finishValue = finishSelect?.value || '';
    // Determine cuttingDieValue using rawRef if available so server gets original reference
    let cuttingDieValue = '';
    if (cuttingDieSelect && cuttingDieSelect.selectedIndex >= 0) {
        const selOpt = cuttingDieSelect.options[cuttingDieSelect.selectedIndex];
        cuttingDieValue = selOpt ? (selOpt.dataset.rawRef || selOpt.value) : '';
    }

    // Printing value is already set above from the active button

    const quoteData = {
        // Contact information
        name: document.getElementById('contact-name')?.value.trim() || '',
        email: document.getElementById('contact-email')?.value.trim() || '',
        phone: document.getElementById('contact-phone')?.value.trim() || '',

        // Display values (for confirmation page)
        description: document.getElementById('description')?.value || '',
        shape: getShapeText(),
        labelWidth: document.getElementById('label-width')?.value || '',
        labelHeight: document.getElementById('label-height')?.value || '',
        diameter: document.getElementById('diameter')?.value || '',
        corners: cornersValue ? cornersValue.charAt(0).toUpperCase() + cornersValue.slice(1) : '',
        cuttingDie: getSelectedText(cuttingDieSelect),
        printing: getSelectedPrinting() || printingText,
        material: getSelectedText(materialSelect),
        finish: getSelectedText(finishSelect),
        applicationMethod: getApplicationMethodText(),
        unwindDirection: getUnwindDirectionText(),
        totalQuantity: document.getElementById('total-quantity')?.value || '',
        artworkOption: getArtworkOptionText(),

        // Form values (for restoration)
        shapeValue: shapeValue,
        cornersValue: cornersValue,
        materialValue: materialValue,
        finishValue: finishValue,
        applicationMethodValue: applicationMethodValue,
        unwindDirectionValue: unwindDirectionValue,
        artworkOptionValue: artworkOptionValue,
        cuttingDieValue: cuttingDieValue,
        printingValue: printingValue || printingText
    };

    // Build payload for external API - send the same structured data as JSON
    const externalPayload = {
        // You may need to adjust property names to match the CERM API contract.
        // For now we send the constructed quoteData as-is.
        ...quoteData
    };

    // Output payload to console
    console.log('Posting payload to external API:', externalPayload);

    // POST to external CERM quick-quote price endpoint (note: CORS/auth may block direct client requests)
    try {
        const externalResponse = await fetch('https://brandmark-api.cerm.be/quote-api/v1/calculations/narrow-web/quick-quote/price', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(externalPayload)
        });

        const externalText = await externalResponse.text();
        console.log('External API response status:', externalResponse.status, externalResponse.statusText);
        console.log('External API response body:', externalText);
    } catch (err) {
        console.error('Error sending payload to external API:', err);
    }

    // Submit form via POST to Index page handler (preserve original behavior)
    const submitForm = document.createElement('form');
    submitForm.method = 'POST';
    submitForm.action = '/Index?handler=Submit';

    // Add anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!token) {
        // Create token if it doesn't exist (ASP.NET Core will generate it)
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        submitForm.appendChild(tokenInput);
    } else {
        submitForm.appendChild(token.cloneNode(true));
    }

    // Add all quote data as hidden inputs (using camelCase to match model)
    Object.keys(quoteData).forEach(key => {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = key;
        input.value = quoteData[key] || '';
        submitForm.appendChild(input);
    });

    document.body.appendChild(submitForm);
    submitForm.submit();
});