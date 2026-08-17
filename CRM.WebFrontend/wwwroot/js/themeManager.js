window.themeManager = {
    setTheme: function (themeName) {
        // Remove existing theme classes
        document.body.classList.remove('theme-default', 'theme-protanopia', 'theme-tritanopia', 'theme-high-contrast');
        
        // Add new theme class if valid
        if (themeName && themeName !== 'theme-default') {
            document.body.classList.add(themeName);
        }
        
        // Save to localStorage
        try {
            localStorage.setItem('nyx_user_theme', themeName || 'theme-default');
        } catch (e) {
            console.warn('Unable to save theme to localStorage', e);
        }
    },
    
    getSavedTheme: function () {
        try {
            return localStorage.getItem('nyx_user_theme') || 'theme-default';
        } catch (e) {
            return 'theme-default';
        }
    },

    initTheme: function () {
        var saved = this.getSavedTheme();
        this.setTheme(saved);
        return saved;
    }
};

// Auto-initialize theme on page load
document.addEventListener('DOMContentLoaded', function () {
    window.themeManager.initTheme();
});
