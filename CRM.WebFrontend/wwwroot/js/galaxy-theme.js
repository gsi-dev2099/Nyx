// =============================================================
// NYX CRM — Galaxy Theme JS Interop
// Dark mode toggle, shooting star animation, stars, particles
// =============================================================

window.galaxyTheme = {

    // ===================== DARK MODE TOGGLE =====================
    _isTransitioning: false,

    toggleTheme: function () {
        if (this._isTransitioning) return;
        this._isTransitioning = true;

        var html = document.documentElement;
        var isDark = html.getAttribute('data-theme') === 'dark';
        var goingDark = !isDark;

        if (goingDark) {
            // Shooting star
            var starOverlay = document.createElement('div');
            starOverlay.className = 'shooting-star-overlay';
            var star = document.createElement('div');
            star.className = 'shooting-star';
            starOverlay.appendChild(star);
            document.body.appendChild(starOverlay);

            // Spark particles
            for (var i = 0; i < 12; i++) {
                (function (idx) {
                    setTimeout(function () {
                        var spark = document.createElement('div');
                        spark.className = 'spark-particle';
                        spark.style.left = (90 - (idx / 12) * 80) + '%';
                        spark.style.top = (15 + (idx / 12) * 50) + '%';
                        starOverlay.appendChild(spark);
                    }, 50 + idx * 40);
                })(i);
            }

            // Wind effect
            for (var j = 0; j < 15; j++) {
                (function () {
                    setTimeout(function () {
                        var wind = document.createElement('div');
                        wind.className = 'wind-line';
                        wind.style.top = (Math.random() * 100) + '%';
                        wind.style.width = (100 + Math.random() * 300) + 'px';
                        document.body.appendChild(wind);
                        setTimeout(function () { wind.remove(); }, 600);
                    }, Math.random() * 300);
                })();
            }

            // Curtain
            var curtain = document.createElement('div');
            curtain.className = 'transition-curtain to-dark';
            setTimeout(function () { document.body.appendChild(curtain); }, 50);

            // Apply theme
            setTimeout(function () { html.setAttribute('data-theme', 'dark'); }, 150);

            // Cleanup
            setTimeout(function () {
                curtain.style.opacity = '0';
                curtain.style.transition = 'opacity 0.15s ease';
            }, 300);

            var self = this;
            setTimeout(function () {
                starOverlay.remove();
                curtain.remove();
                self._isTransitioning = false;
            }, 500);
        } else {
            // Light mode shape transition
            var shape = document.createElement('div');
            shape.className = 'transition-curtain to-light-shape';
            document.body.appendChild(shape);

            setTimeout(function () { html.removeAttribute('data-theme'); }, 150);

            setTimeout(function () {
                shape.style.opacity = '0';
                shape.style.transition = 'opacity 0.15s ease';
            }, 300);

            var self2 = this;
            setTimeout(function () {
                shape.remove();
                self2._isTransitioning = false;
            }, 500);
        }

        return goingDark;
    },

    // ===================== STARS GENERATOR =====================
    initStars: function (elementId, count) {
        var el = document.getElementById(elementId);
        if (!el) return;
        // Clear existing stars to avoid duplication on re-render
        var existing = el.querySelectorAll('.star');
        existing.forEach(function (s) { s.remove(); });

        for (var i = 0; i < count; i++) {
            var s = document.createElement('div');
            s.className = 'star';
            var size = Math.random() * 2.5 + 0.5;
            s.style.cssText = 'width:' + size + 'px;height:' + size + 'px;left:' + (Math.random() * 100) + '%;top:' + (Math.random() * 100) + '%;opacity:' + Math.random() + ';--dur:' + (Math.random() * 3 + 2) + 's';
            el.appendChild(s);
        }
    },

    // ===================== AMBIENT PARTICLES =====================
    initAmbientParticles: function (elementId, count) {
        var el = document.getElementById(elementId);
        if (!el) return;
        // Clear existing
        var existing = el.querySelectorAll('.ambient-dot');
        existing.forEach(function (d) { d.remove(); });

        for (var i = 0; i < count; i++) {
            var dot = document.createElement('div');
            dot.className = 'ambient-dot';
            dot.style.cssText = 'left:' + (Math.random() * 100) + '%;top:' + (Math.random() * 100) + '%;--dur:' + (Math.random() * 4 + 2) + 's;opacity:' + (Math.random() * 0.4 + 0.1);
            el.appendChild(dot);
        }
    },

    // ===================== RIPPLE ON BUTTONS =====================
    initRipples: function () {
        document.querySelectorAll('.btn-ghost-galaxy, .btn-accent-galaxy, .galaxy-ficha-btn').forEach(function (b) {
            // Avoid duplicate listeners
            if (b._galaxyRipple) return;
            b._galaxyRipple = true;
            b.addEventListener('click', function (e) {
                var r = document.createElement('span');
                r.className = 'ripple';
                var rect = this.getBoundingClientRect();
                var sz = Math.max(rect.width, rect.height);
                r.style.cssText = 'width:' + sz + 'px;height:' + sz + 'px;left:' + (e.clientX - rect.left - sz / 2) + 'px;top:' + (e.clientY - rect.top - sz / 2) + 'px';
                this.style.position = 'relative';
                this.style.overflow = 'hidden';
                this.appendChild(r);
                setTimeout(function () { r.remove(); }, 500);
            });
        });
    },

    // ===================== FULL INIT =====================
    init: function () {
        this.initStars('galaxySidebar', 55);
        this.initAmbientParticles('ambientParticles', 40);
        this.initRipples();
    }
};
