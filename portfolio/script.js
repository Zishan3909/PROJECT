// ============================================================
//  Zishan Ali — Portfolio JavaScript
// ============================================================

document.addEventListener('DOMContentLoaded', () => {

    // ---- DOM References ----
    const navbar      = document.getElementById('navbar');
    const hamburger   = document.getElementById('hamburger');
    const navLinks    = document.getElementById('navLinks');
    const backToTop   = document.getElementById('backToTop');
    const contactForm = document.getElementById('contactForm');
    const allNavLinks = document.querySelectorAll('.nav-link');
    const sections    = document.querySelectorAll('section[id]');
    const reveals     = document.querySelectorAll('.reveal');

    // ============================================================
    //  1. NAVBAR — Scroll effects & active link highlighting
    // ============================================================
    function onScroll() {
        const y = window.scrollY;

        // Navbar background
        navbar.classList.toggle('scrolled', y > 60);

        // Back to top
        backToTop.classList.toggle('visible', y > 500);

        // Active nav link
        const offset = 200;
        sections.forEach(sec => {
            const top = sec.offsetTop;
            const h   = sec.offsetHeight;
            const id  = sec.getAttribute('id');
            const link = document.querySelector(`.nav-link[href="#${id}"]`);
            if (link) {
                if (y + offset >= top && y + offset < top + h) {
                    allNavLinks.forEach(l => l.classList.remove('active'));
                    link.classList.add('active');
                }
            }
        });
    }
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();

    // ============================================================
    //  2. HAMBURGER MENU
    // ============================================================
    hamburger.addEventListener('click', () => {
        hamburger.classList.toggle('active');
        navLinks.classList.toggle('nav-active');
    });
    // Close on link click
    document.querySelectorAll('.nav-links a').forEach(link => {
        link.addEventListener('click', () => {
            hamburger.classList.remove('active');
            navLinks.classList.remove('nav-active');
        });
    });

    // ============================================================
    //  3. SMOOTH SCROLL
    // ============================================================
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', e => {
            e.preventDefault();
            const target = document.querySelector(anchor.getAttribute('href'));
            if (target) {
                window.scrollTo({
                    top: target.getBoundingClientRect().top + window.scrollY - 80,
                    behavior: 'smooth'
                });
            }
        });
    });

    // ============================================================
    //  4. BACK TO TOP
    // ============================================================
    backToTop.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    // ============================================================
    //  5. SCROLL REVEAL (IntersectionObserver)
    // ============================================================
    const revealObserver = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('active');
                revealObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

    reveals.forEach(el => revealObserver.observe(el));

    // ============================================================
    //  6. TYPING EFFECT
    // ============================================================
    const typedEl = document.getElementById('typedText');
    if (typedEl) {
        const roles = [
            'Software Engineer Aspirant',
            'Problem Solver',
            'Web Developer',
            'CS Student',
            'DSA Enthusiast',
            'Tech Learner'
        ];
        let roleIdx   = 0;
        let charIdx   = 0;
        let deleting  = false;
        let speed     = 80;

        function typeLoop() {
            const word = roles[roleIdx];
            if (!deleting) {
                typedEl.textContent = word.substring(0, charIdx + 1);
                charIdx++;
                speed = 80;
                if (charIdx === word.length) {
                    deleting = true;
                    speed = 2200; // pause at end
                }
            } else {
                typedEl.textContent = word.substring(0, charIdx - 1);
                charIdx--;
                speed = 40;
                if (charIdx === 0) {
                    deleting = false;
                    roleIdx = (roleIdx + 1) % roles.length;
                    speed = 400;
                }
            }
            setTimeout(typeLoop, speed);
        }
        setTimeout(typeLoop, 1200);
    }

    // ============================================================
    //  7. HERO PARTICLES
    // ============================================================
    function createParticles() {
        const container = document.getElementById('heroParticles');
        if (!container) return;
        const count = 40;
        for (let i = 0; i < count; i++) {
            const p = document.createElement('div');
            p.classList.add('particle');
            const size = Math.random() * 3 + 1;
            p.style.width  = size + 'px';
            p.style.height = size + 'px';
            p.style.left   = Math.random() * 100 + '%';
            p.style.animationDuration = (Math.random() * 15 + 12) + 's';
            p.style.animationDelay    = (Math.random() * 10) + 's';
            container.appendChild(p);
        }
    }
    createParticles();

    // ============================================================
    //  8. SKILL CARD TILT
    // ============================================================
    document.querySelectorAll('.skill-card').forEach(card => {
        card.addEventListener('mousemove', e => {
            const r = card.getBoundingClientRect();
            const x = e.clientX - r.left;
            const y = e.clientY - r.top;
            const rx = (y - r.height / 2) / 18;
            const ry = (r.width / 2 - x) / 18;
            card.style.transform = `perspective(800px) rotateX(${rx}deg) rotateY(${ry}deg) translateY(-8px)`;
        });
        card.addEventListener('mouseleave', () => {
            card.style.transform = '';
        });
    });

    // ============================================================
    //  9. CONTACT FORM
    // ============================================================
    if (contactForm) {
        contactForm.addEventListener('submit', e => {
            e.preventDefault();
            const btn = document.getElementById('submitBtn');
            const orig = btn.innerHTML;
            btn.innerHTML = `
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="animation:spin 1s linear infinite"><path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83"/></svg>
                Sending...
            `;
            btn.disabled = true;
            btn.style.opacity = '0.7';

            setTimeout(() => {
                btn.innerHTML = `
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
                    Message Sent!
                `;
                btn.style.opacity = '1';
                btn.style.background = 'linear-gradient(135deg, #22c55e, #16a34a)';
                contactForm.reset();

                setTimeout(() => {
                    btn.innerHTML = orig;
                    btn.disabled = false;
                    btn.style.background = '';
                }, 3000);
            }, 1500);
        });
    }
});

// Spin animation for loading icon
const spinStyle = document.createElement('style');
spinStyle.textContent = `@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }`;
document.head.appendChild(spinStyle);
