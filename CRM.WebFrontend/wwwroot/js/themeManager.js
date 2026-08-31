// =============================================================
// NYX CRM — Theme Manager (Per-User Color Palettes)
// Applies DESIGN colors only (buttons, inputs, cards, sidebar)
// Text colors are NEVER modified by themes.
// =============================================================

window.themeManager = {
    /**
     * Set a theme for a specific user.
     * @param {string} themeName - e.g. 'theme-default', 'theme-protanopia', etc.
     * @param {string} [username] - The logged-in username for per-user localStorage key.
     */
    setTheme: function (themeName, username) {
        // Remove existing theme classes
        document.body.classList.remove('theme-default', 'theme-protanopia', 'theme-tritanopia', 'theme-high-contrast');
        
        // DO NOT apply custom themes on the login page to preserve its original design
        if (window.location.pathname.toLowerCase().includes('/login')) {
            return;
        }

        // Add new theme class if valid and not default
        if (themeName && themeName !== 'theme-default') {
            document.body.classList.add(themeName);
        }
        
        // Save to localStorage with per-user key
        try {
            var key = username ? 'nyx_theme_' + username : 'nyx_user_theme';
            localStorage.setItem(key, themeName || 'theme-default');
            // Also store the last username for init purposes
            if (username) {
                localStorage.setItem('nyx_theme_last_user', username);
            }
        } catch (e) {
            console.warn('Unable to save theme to localStorage', e);
        }
    },
    
    /**
     * Get saved theme for a specific user.
     * @param {string} [username] - The logged-in username.
     * @returns {string} The saved theme name.
     */
    getSavedTheme: function (username) {
        try {
            if (username) {
                return localStorage.getItem('nyx_theme_' + username) || 'theme-default';
            }
            // Fallback: try last known user
            var lastUser = localStorage.getItem('nyx_theme_last_user');
            if (lastUser) {
                return localStorage.getItem('nyx_theme_' + lastUser) || 'theme-default';
            }
            // Legacy fallback
            return localStorage.getItem('nyx_user_theme') || 'theme-default';
        } catch (e) {
            return 'theme-default';
        }
    },

    /**
     * Initialize theme on page load using the current user context.
     * @param {string} [username] - Optional username.
     * @returns {string} The applied theme name.
     */
    initTheme: function (username) {
        var saved = this.getSavedTheme(username);
        this.setTheme(saved, username);
        return saved;
    },

    /**
     * Sync theme from backend response (called after login or profile load).
     * @param {string} themeName - The theme from the backend.
     * @param {string} username - The logged-in username.
     */
    syncFromBackend: function (themeName, username) {
        var theme = themeName || 'theme-default';
        this.setTheme(theme, username);
    }
};

// Auto-initialize theme on page load
document.addEventListener('DOMContentLoaded', function () {
    window.themeManager.initTheme();
});
