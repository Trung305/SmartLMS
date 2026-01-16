
const notion = (function () {
    const container = document.createElement('div');
    container.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        max-width: 400px;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    `;
    document.body.appendChild(container);

    const icons = {
        success: `<svg viewBox="0 0 24 24" width="24" height="24" fill="currentColor"><path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/></svg>`,
        error: `<svg viewBox="0 0 24 24" width="24" height="24" fill="currentColor"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/></svg>`,
        warning: `<svg viewBox="0 0 24 24" width="24" height="24" fill="currentColor"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/></svg>`,
        info: `<svg viewBox="0 0 24 24" width="24" height="24" fill="currentColor"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z"/></svg>`
    };

    const colors = {
        success: '#10b981', 
        error: '#ef4444',    
        warning: '#f59e0b', 
        info: '#3b82f6'      
    };
    function show(type, message, duration = 5000) {
        const toast = document.createElement('div');
        toast.style.cssText = `
            background: white;
            color: #1f2937;
            border-left: 6px solid ${colors[type]};
            padding: 8px 15px;
            margin-bottom: 12px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.1);
            display: flex;
            align-items: center;
            gap: 12px;
            min-width: 300px;
            opacity: 0;
            transform: translateX(100%) scale(0.95);
            animation: toastSlideIn 0.5s ease forwards;

            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
        `;
        toast.addEventListener('mouseenter', () => {
            toast.style.transform = 'translateX(0) scale(1.02) translateY(-4px)';
            toast.style.boxShadow = '0 20px 40px rgba(0,0,0,0.18)';
        });
        toast.addEventListener('mouseleave', () => {
            toast.style.transform = 'translateX(0) scale(1) translateY(0)';
            toast.style.boxShadow = '0 10px 25px rgba(0,0,0,0.12)';
        });
        toast.innerHTML = `
            <div style="color: ${colors[type]}; flex-shrink: 0;">${icons[type] || icons.info}</div>
            <div style="flex: 1; font-size: 15px; font-weight: 500;">${message}</div>
            <button onclick="this.parentElement.parentElement.remove()" style="
                background: none; border: none; cursor: pointer; 
                color: #94a3b8; font-size: 20px; padding: 0; margin-left: 10px;
            ">×</button>
        `;

        container.prepend(toast);

        setTimeout(() => {
            toast.style.opacity = '1';
            toast.style.transform = 'translateX(0)';
        }, 10);

        let timeoutId;
        if (duration > 0) {
            timeoutId = setTimeout(() => {
                toast.style.opacity = '0';
                toast.style.transform = 'translateX(100%)';
                setTimeout(() => toast.remove(), 400);
            }, duration);
        }

        toast.addEventListener('mouseenter', () => clearTimeout(timeoutId));
        toast.addEventListener('mouseleave', () => {
            if (duration > 0) {
                timeoutId = setTimeout(() => {
                    toast.style.opacity = '0';
                    toast.style.transform = 'translateX(100%)';
                    setTimeout(() => toast.remove(), 400);
                }, duration);
            }
        });
    }

    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideIn {
            from { transform: translateX(100%); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }
    `;
    document.head.appendChild(style);

    return {
        success: (msg, duration) => show('success', msg, duration),
        error: (msg, duration) => show('error', msg, duration),
        warning: (msg, duration) => show('warning', msg, duration),
        info: (msg, duration) => show('info', msg, duration),

        sticky: (type, msg) => show(type, msg, 0) 
    };
})();
