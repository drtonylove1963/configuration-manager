// SIMPLE GLOBAL THEME SYSTEM
window.currentTheme = 'light'; // Global variable: 'light' or 'dark'

window.setTheme = function(mode) {
    console.log('setTheme called with mode:', mode);

    // Simple IF statement - only two choices
    if (mode === 'dark') {
        // DARK THEME
        window.currentTheme = 'dark';

        // Apply body classes
        document.body.className = 'dark-theme';
        document.documentElement.className = 'dark-theme';

        // Apply body styles - Dark theme with better colors
        document.body.style.backgroundColor = '#121212';
        document.body.style.color = '#e0e0e0';

        // Apply to layout elements
        const elements = document.querySelectorAll('.rz-layout, .rz-body, .rz-content, .rz-sidebar, .rz-header');
        elements.forEach(el => {
            el.classList.add('dark-theme');
            el.classList.remove('light-theme');
            if (el.classList.contains('rz-sidebar')) {
                el.style.backgroundColor = '#1e1e1e';
                el.style.color = '#e0e0e0';
            } else if (el.classList.contains('rz-header')) {
                el.style.backgroundColor = '#1e40af'; // Keep header blue
                el.style.color = '#ffffff';
            } else {
                el.style.backgroundColor = '#121212';
                el.style.color = '#e0e0e0';
            }
        });

        // Apply to all Radzen components
        const radzenComponents = document.querySelectorAll('[class*="rz-"]');
        radzenComponents.forEach(el => {
            el.classList.add('dark-theme');
            el.classList.remove('light-theme');
        });

        console.log('Theme set to: DARK');
    } else {
        // LIGHT THEME - Reset everything to default
        window.currentTheme = 'light';

        // Remove all theme classes completely
        document.body.className = '';
        document.documentElement.className = '';

        // Reset body styles completely
        document.body.style.backgroundColor = '';
        document.body.style.color = '';

        // Reset all layout elements completely
        const elements = document.querySelectorAll('.rz-layout, .rz-body, .rz-content, .rz-sidebar, .rz-header');
        elements.forEach(el => {
            // Remove ALL theme classes
            el.classList.remove('light-theme');
            el.classList.remove('dark-theme');

            // Remove ALL inline styles
            el.style.backgroundColor = '';
            el.style.color = '';
            el.style.borderRight = '';
            el.style.border = '';
        });

        // Reset all Radzen components completely
        const radzenComponents = document.querySelectorAll('[class*="rz-"]');
        radzenComponents.forEach(el => {
            el.classList.remove('light-theme');
            el.classList.remove('dark-theme');
        });

        console.log('Theme set to: LIGHT (default/reset)');
    }

    // Save to localStorage
    localStorage.setItem('theme', window.currentTheme);
    console.log('Theme saved to localStorage:', window.currentTheme);
};

window.toggleTheme = function() {
    console.log('toggleTheme called. Current theme:', window.currentTheme);
    const newTheme = window.currentTheme === 'light' ? 'dark' : 'light';
    window.setTheme(newTheme);
    console.log('Theme toggled to:', newTheme);
    return newTheme;
};

// Initialize on page load
(function() {
    const saved = localStorage.getItem('theme') || 'light';
    console.log('Initializing theme from localStorage:', saved);
    window.setTheme(saved);
})();


