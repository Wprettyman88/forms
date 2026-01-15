// Load finishing options from server-side proxy endpoint and populate #finish select
// This function is called from quote.js when a printing option is selected
// printingText: The text of the selected printing option (used for filtering)
async function loadFinishingOptions(printingText) {
    const finishSelect = document.getElementById('finish');
    const finishLoading = document.getElementById('finish-loading');
    const finishError = document.getElementById('finish-error');

    if (!finishSelect) {
        console.debug('Finish select (#finish) not found - skipping finishing types load.');
        return;
    }

    if (finishLoading) finishLoading.style.display = 'block';
    if (finishError) finishError.style.display = 'none';
    finishSelect.style.opacity = '0.5';
    finishSelect.disabled = true;
    finishSelect.innerHTML = '<option value="">Loading finishes...</option>';

    try {
        const resp = await fetch('/Api/FinishingTypes');
        if (!resp.ok) {
            console.warn('Failed to load finishing types:', resp.status, resp.statusText);
            if (finishError) {
                finishError.textContent = 'Unable to load finishes';
                finishError.style.display = 'block';
            }
            finishSelect.innerHTML = '<option value="">Error loading finishes</option>';
            return;
        }

        const data = await resp.json();
        console.debug('Finishing types data received:', data);

        // Clear existing options
        finishSelect.innerHTML = '';

        // The API returns items where each item may include a Descriptions array:
        // item.Descriptions = [{ ISOLanguageCode: 'en-US', Description: 'Gloss Laminate' }, ...]
        const items = Array.isArray(data) ? data : (Object.values(data).find(v => Array.isArray(v)) || []);

        if (!Array.isArray(items) || items.length === 0) {
            console.warn('Finishing types response has no array payload:', data);
            const placeholder = document.createElement('option');
            placeholder.value = '';
            placeholder.textContent = 'No finishes available';
            finishSelect.appendChild(placeholder);
            return;
        }

        // Add a blank placeholder option
        const placeholder = document.createElement('option');
        placeholder.value = '';
        placeholder.textContent = 'Select finish';
        finishSelect.appendChild(placeholder);

        // Determine filter criteria based on printing text
        const printingLower = (printingText || '').toLowerCase();
        const isFlexo = printingLower.includes('flexo');
        
        // Filter items based on printing selection
        const filteredItems = items.filter(item => {
            // Get the description text for filtering
            let text = '';
            if (Array.isArray(item?.Descriptions)) {
                const enDesc = item.Descriptions.find(d => (d?.ISOLanguageCode || '').toLowerCase() === 'en-us' || (d?.ISOLanguageCode || '').toLowerCase() === 'en');
                if (enDesc && (enDesc.Description || enDesc.description)) {
                    text = enDesc.Description || enDesc.description;
                }
            }
            
            // Fallback to other text fields
            if (!text) {
                text = item?.Description ?? item?.description ?? item?.Name ?? item?.name ?? '';
            }
            
            const textLower = text.toLowerCase();
            
            if (isFlexo) {
                // If printing is flexo, only show finishes that contain "flexo"
                return textLower.includes('flexo');
            } else {
                // Otherwise, show finishes that contain "Digital" or "Blank"
                return textLower.includes('digital') || textLower.includes('blank');
            }
        });

        // Populate options with filtered items
        filteredItems.forEach(item => {
            const opt = document.createElement('option');

            // Prefer the Descriptions array entry where ISOLanguageCode === 'en-US'
            let text = '';
            if (Array.isArray(item?.Descriptions)) {
                const enDesc = item.Descriptions.find(d => (d?.ISOLanguageCode || '').toLowerCase() === 'en-us' || (d?.ISOLanguageCode || '').toLowerCase() === 'en');
                if (enDesc && (enDesc.Description || enDesc.description)) {
                    text = enDesc.Description || enDesc.description;
                }
            }

            // Fallback heuristics for value and text
            const value = item?.Id ?? item?.id ?? item?.Value ?? item?.value ?? item?.name ?? item?.Name ?? text ?? JSON.stringify(item);
            if (!text) text = item?.Description ?? item?.description ?? item?.Name ?? item?.name ?? value;

            opt.value = value;
            opt.textContent = text;

            // Preserve any raw reference if present
            if (item?.RawReference) opt.dataset.rawRef = item.RawReference;
            if (item?.rawRef) opt.dataset.rawRef = item.rawRef;

            finishSelect.appendChild(opt);
        });

        // If no filtered items, show a message
        if (filteredItems.length === 0) {
            const noOptions = document.createElement('option');
            noOptions.value = '';
            noOptions.textContent = 'No finishes available for this printing option';
            finishSelect.appendChild(noOptions);
        }

        // If a previously selected value exists, attempt to restore it
        const saved = finishSelect.getAttribute('data-selected');
        if (saved) {
            const savedOption = Array.from(finishSelect.options).find(opt => opt.value === saved);
            if (savedOption) {
                finishSelect.value = saved;
            }
        }
    } catch (err) {
        console.error('Error loading finishing types:', err);
        if (finishError) {
            finishError.textContent = 'Error loading finishes';
            finishError.style.display = 'block';
        }
        finishSelect.innerHTML = '<option value="">Error loading finishes</option>';
    } finally {
        if (finishLoading) finishLoading.style.display = 'none';
        finishSelect.style.opacity = '1';
        finishSelect.disabled = false;
    }
}