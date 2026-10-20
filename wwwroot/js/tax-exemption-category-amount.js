(() => {
    const select = document.querySelector('select[name="ExemptionCategory"]');
    const amount = document.querySelector('input[name="MaxAmount"]');
    if (!select || !amount) return;
    let selected = select.value;
    let request = 0;
    async function refreshAmount() {
        if (selected === select.value) return;
        selected = select.value;
        const current = ++request;
        amount.value = '';
        if (!selected) return;
        try {
            const response = await fetch('/HR/TaxExemptionCategoryAmount?category=' + encodeURIComponent(selected));
            const result = await response.json();
            if (!response.ok) throw new Error(result.message || 'Category amount could not be loaded.');
            if (current === request) { amount.value = result.amount; amount.dispatchEvent(new Event('input', { bubbles: true })); }
        } catch (error) { if (current === request) alert(error.message || 'Category amount could not be loaded.'); }
    }
    select.addEventListener('change', refreshAmount);
    document.addEventListener('click', () => setTimeout(refreshAmount, 0));
})();
