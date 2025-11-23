// SignalR Notification Client
(function () {
    'use strict';

    // Initialize SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .build();

    let notificationCount = 0;
    const notifications = [];

    // Start connection
    connection.start()
        .then(() => {
            console.log("SignalR connected");
            loadExistingNotifications();
        })
        .catch(err => console.error("SignalR connection error:", err));

    // Handle reconnection
    connection.onreconnected(() => {
        console.log("SignalR reconnected");
        loadExistingNotifications();
    });

    // Receive new notifications from server
    connection.on("ReceiveNotification", function (notification) {
        console.log("Received notification:", notification);
        addNotification(notification);
        showToast(notification);
    });

    // Load existing unacknowledged notifications
    function loadExistingNotifications() {
        fetch('/Review/GetUnacknowledgedNotifications')
            .then(response => response.json())
            .then(data => {
                notifications.length = 0; // Clear array
                data.forEach(notification => addNotification(notification, false));
                updateBadge();
                renderNotifications();
            })
            .catch(err => console.error("Error loading notifications:", err));
    }

    // Add notification to list
    function addNotification(notification, shouldRender = true) {
        // Check if notification already exists
        const exists = notifications.some(n => n.notificationId === notification.notificationId);
        if (!exists) {
            notifications.unshift(notification);
            notificationCount++;
        }
        
        if (shouldRender) {
            updateBadge();
            renderNotifications();
        }
    }

    // Update badge count
    function updateBadge() {
        const badge = document.getElementById('notificationBadge');
        if (badge) {
            if (notificationCount > 0) {
                badge.textContent = notificationCount > 99 ? '99+' : notificationCount;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        }
    }

    // Render notifications in dropdown
    function renderNotifications() {
        const container = document.getElementById('notificationList');
        if (!container) return;

        if (notifications.length === 0) {
            container.innerHTML = `
                <div class="notification-empty">
                    <i class="fas fa-check-circle"></i>
                    <p>No new notifications</p>
                </div>
            `;
            return;
        }

        container.innerHTML = notifications.map(n => `
            <div class="notification-item" data-id="${n.notificationId}">
                <div class="notification-checkbox">
                    <input type="checkbox" 
                           id="notif-${n.notificationId}" 
                           onchange="acknowledgeNotification(${n.notificationId})">
                </div>
                <div class="notification-content">
                    <div class="notification-title">${escapeHtml(n.cardTitle)}</div>
                    <div class="notification-time">
                        <i class="fas fa-clock"></i> 
                        ${formatTime(n.notifiedAt)}
                    </div>
                </div>
                <a href="/Review/Session" class="notification-action">
                    <i class="fas fa-arrow-right"></i>
                </a>
            </div>
        `).join('');
    }

    // Acknowledge notification
    window.acknowledgeNotification = function (notificationId) {
        fetch('/Review/AcknowledgeNotification', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ notificationId: notificationId })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Remove from list
                    const index = notifications.findIndex(n => n.notificationId === notificationId);
                    if (index > -1) {
                        notifications.splice(index, 1);
                        notificationCount--;
                    }
                    updateBadge();
                    renderNotifications();
                }
            })
            .catch(err => console.error("Error acknowledging notification:", err));
    };

    // Show toast notification
    function showToast(notification) {
        const toast = document.createElement('div');
        toast.className = 'notification-toast';
        toast.innerHTML = `
            <div class="toast-icon"><i class="fas fa-bell"></i></div>
            <div class="toast-content">
                <div class="toast-title">Review Due!</div>
                <div class="toast-message">${escapeHtml(notification.cardTitle)}</div>
            </div>
            <button class="toast-close" onclick="this.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        `;
        
        document.body.appendChild(toast);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            toast.classList.add('fade-out');
            setTimeout(() => toast.remove(), 300);
        }, 5000);
    }

    // Toggle notification dropdown
    window.toggleNotifications = function () {
        const dropdown = document.getElementById('notificationDropdown');
        if (dropdown) {
            dropdown.classList.toggle('show');
        }
    };

    // Close dropdown when clicking outside
    document.addEventListener('click', function (event) {
        const dropdown = document.getElementById('notificationDropdown');
        const bell = document.getElementById('notificationBell');
        
        if (dropdown && bell && !dropdown.contains(event.target) && !bell.contains(event.target)) {
            dropdown.classList.remove('show');
        }
    });

    // Utility functions
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function formatTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diff = Math.floor((now - date) / 1000); // seconds

        if (diff < 60) return 'Just now';
        if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
        if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
        return `${Math.floor(diff / 86400)}d ago`;
    }
})();
