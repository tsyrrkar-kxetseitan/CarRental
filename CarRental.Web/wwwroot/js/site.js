// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ─── Notification Bell ────────────────────────────────────────────────────
function loadNotifications() {
    fetch('/Notification/GetNotifications')
        .then(r => r.json())
        .then(data => {
            const dropdown = document.getElementById('notifDropdown');
            const badge = document.getElementById('notifBadge');
            const empty = document.getElementById('notifEmpty');

            if (!dropdown) return;

            // Update badge
            if (data.unreadCount > 0) {
                badge.textContent = data.unreadCount > 9 ? '9+' : data.unreadCount;
                badge.style.display = 'block';
            } else {
                badge.style.display = 'none';
            }

            // Clear old items (keep header + divider)
            const items = dropdown.querySelectorAll('.notif-item');
            items.forEach(i => i.remove());

            if (data.notifications.length === 0) {
                if (empty) empty.style.display = '';
                return;
            }

            if (empty) empty.style.display = 'none';

            // Icon map
            const icons = {
                'Welcome': '🎉', 'Booking': '✅', 'Cancellation': '❌', 'Reminder': '🔔'
            };

            data.notifications.forEach(n => {
                const li = document.createElement('li');
                li.className = 'notif-item';
                const icon = icons[n.type] || '📢';
                li.innerHTML = `
                    <a class="dropdown-item py-2 ${n.isRead ? '' : 'fw-bold'}" href="#"
                       onclick="markRead(event, ${n.id})" style="${n.isRead ? 'opacity:0.7;' : ''}">
                        <div class="d-flex align-items-start">
                            <span style="font-size:1.2em;margin-right:8px;">${icon}</span>
                            <div>
                                <div class="small fw-semibold">${n.title}</div>
                                <div class="small text-muted" style="white-space:normal;">${n.message}</div>
                                <div class="text-muted" style="font-size:0.7em;">${n.timeAgo}</div>
                            </div>
                        </div>
                    </a>`;
                dropdown.appendChild(li);
            });
        })
        .catch(err => console.error('Notifications error:', err));
}

function markRead(event, id) {
    event.preventDefault();
    event.stopPropagation();
    fetch('/Notification/MarkAsRead/' + id, { method: 'POST' })
        .then(() => loadNotifications());
}

function markAllRead(event) {
    event.preventDefault();
    event.stopPropagation();
    fetch('/Notification/MarkAllAsRead', { method: 'POST' })
        .then(() => loadNotifications());
}

// Auto-check for unread notifications on page load
document.addEventListener('DOMContentLoaded', function () {
    const badge = document.getElementById('notifBadge');
    if (badge) {
        fetch('/Notification/GetNotifications')
            .then(r => r.json())
            .then(data => {
                if (data.unreadCount > 0) {
                    badge.textContent = data.unreadCount > 9 ? '9+' : data.unreadCount;
                    badge.style.display = 'block';
                }
            })
            .catch(() => { });
    }
});
