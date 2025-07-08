// SIMPLE GLOBAL THEME SYSTEM
window.currentTheme = 'light'; // Global variable: 'light' or 'dark'

window.setTheme = function(mode) {
    console.log('setTheme called with mode:', mode);

    // Check if DOM is ready
    if (!document.body) {
        console.log('DOM not ready, deferring theme setting');
        setTimeout(() => window.setTheme(mode), 100);
        return;
    }

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

    // Update the theme toggle button icon
    const themeIcon = document.getElementById('theme-icon');
    if (themeIcon) {
        themeIcon.textContent = newTheme === 'dark' ? 'light_mode' : 'dark_mode';
    }

    console.log('Theme toggled to:', newTheme);
    return newTheme;
};

// Initialize on page load - wait for DOM to be ready
(function() {
    function initializeTheme() {
        const saved = localStorage.getItem('theme') || 'light';
        console.log('Initializing theme from localStorage:', saved);
        window.setTheme(saved);
    }

    // Wait for DOM to be ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeTheme);
    } else {
        // DOM is already ready
        initializeTheme();
    }
})();


