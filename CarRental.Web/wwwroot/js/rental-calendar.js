/**
 * Rental Calendar Component
 * 
 * Renders a monthly calendar showing bookings AND unavailability periods for a car.
 * - Client mode: pink bars for booked, grey bars for unavailable, click-to-select.
 * - Admin mode:  multi-color bars per user, grey for unavailability, legend, click-to-select unavailability.
 */
const RentalCalendar = (function () {

    const USER_COLORS = [
        '#f48fb1', '#81d4fa', '#a5d6a7', '#ce93d8',
        '#ffcc80', '#ef9a9a', '#80cbc4', '#fff59d', '#b0bec5',
    ];

    const UNAVAILABLE_COLOR = '#9e9e9e';  // grey for unavailable periods

    const MONTH_NAMES = [
        'January', 'February', 'March', 'April', 'May', 'June',
        'July', 'August', 'September', 'October', 'November', 'December'
    ];

    const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

    let config = {};
    let currentMonth = new Date().getMonth();
    let currentYear = new Date().getFullYear();
    let entries = [];       // combined bookings + unavailability from API
    let selectionStart = null;
    let selectionEnd = null;
    let userColorMap = {};

    function init(options) {
        config = Object.assign({
            containerId: 'rental-calendar',
            carId: 0,
            mode: 'client',
            bookingsUrl: '/Car/GetBookings',
            onDateRangeSelected: null
        }, options);

        selectionStart = null;
        selectionEnd = null;
        loadData();
    }

    function loadData() {
        const url = `${config.bookingsUrl}/${config.carId}?month=${currentMonth + 1}&year=${currentYear}`;
        fetch(url)
            .then(r => r.json())
            .then(data => {
                entries = mergeUnavailability(data);
                buildUserColorMap();
                render();
            })
            .catch(err => {
                console.error('Failed to load calendar data:', err);
                entries = [];
                render();
            });
    }

    /**
     * Merges overlapping or adjacent unavailability entries into single ranges.
     * Bookings are passed through unchanged.
     */
    function mergeUnavailability(data) {
        const bookings = data.filter(e => e.type !== 'unavailable');
        const unavail = data.filter(e => e.type === 'unavailable');

        if (unavail.length <= 1) return data;

        // Sort by start date
        unavail.sort((a, b) => a.startDate.localeCompare(b.startDate));

        const merged = [unavail[0]];
        for (let i = 1; i < unavail.length; i++) {
            const prev = merged[merged.length - 1];
            const curr = unavail[i];
            const prevEnd = parseDate(prev.endDate);
            const currStart = parseDate(curr.startDate);

            // Overlap or adjacent (currStart <= prevEnd + 1 day)
            const nextDay = new Date(prevEnd);
            nextDay.setDate(nextDay.getDate() + 1);

            if (currStart <= nextDay) {
                // Extend the previous period
                const currEnd = parseDate(curr.endDate);
                if (currEnd > prevEnd) {
                    prev.endDate = curr.endDate;
                }
                // Combine reasons if different
                if (curr.username && prev.username && curr.username !== prev.username) {
                    prev.username = prev.username + ' / ' + curr.username;
                }
            } else {
                merged.push(curr);
            }
        }

        return bookings.concat(merged);
    }

    function buildUserColorMap() {
        userColorMap = {};
        let colorIndex = 0;
        entries.forEach(e => {
            if (e.type === 'booking' && e.username && !(e.username in userColorMap)) {
                userColorMap[e.username] = USER_COLORS[colorIndex % USER_COLORS.length];
                colorIndex++;
            }
        });
    }

    function render() {
        const container = document.getElementById(config.containerId);
        if (!container) return;
        container.innerHTML = '';

        // Header
        const header = document.createElement('div');
        header.className = 'rental-calendar-header';
        header.innerHTML = `
            <button id="cal-prev" title="Previous month">◀</button>
            <h5>${MONTH_NAMES[currentMonth]} ${currentYear}</h5>
            <button id="cal-next" title="Next month">▶</button>
        `;
        container.appendChild(header);

        // Grid
        const grid = document.createElement('div');
        grid.className = 'rental-calendar-grid';

        DAY_NAMES.forEach(d => {
            const dh = document.createElement('div');
            dh.className = 'day-header';
            dh.textContent = d;
            grid.appendChild(dh);
        });

        const firstDay = new Date(currentYear, currentMonth, 1).getDay();
        const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
        const daysInPrevMonth = new Date(currentYear, currentMonth, 0).getDate();
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        // Prev month padding
        for (let i = firstDay - 1; i >= 0; i--) {
            grid.appendChild(createDayCell(daysInPrevMonth - i, true, null));
        }

        // Current month
        for (let day = 1; day <= daysInMonth; day++) {
            const date = new Date(currentYear, currentMonth, day);
            grid.appendChild(createDayCell(day, false, date, date.getTime() === today.getTime()));
        }

        // Next month padding
        const totalCells = firstDay + daysInMonth;
        const remaining = totalCells % 7 === 0 ? 0 : 7 - (totalCells % 7);
        for (let i = 1; i <= remaining; i++) {
            grid.appendChild(createDayCell(i, true, null));
        }

        container.appendChild(grid);

        // Legend
        if (config.mode === 'admin') {
            const legend = document.createElement('div');
            legend.className = 'calendar-legend';
            for (const [username, color] of Object.entries(userColorMap)) {
                legend.innerHTML += `
                    <div class="legend-item">
                        <div class="legend-color" style="background:${color}"></div>
                        <span>${username}</span>
                    </div>`;
            }
            // Add unavailable legend item
            const hasUnavail = entries.some(e => e.type === 'unavailable');
            if (hasUnavail) {
                legend.innerHTML += `
                    <div class="legend-item">
                        <div class="legend-color" style="background:${UNAVAILABLE_COLOR}"></div>
                        <span>Unavailable</span>
                    </div>`;
            }
            if (legend.innerHTML) container.appendChild(legend);
        }

        // Nav events
        document.getElementById('cal-prev').addEventListener('click', () => {
            currentMonth--;
            if (currentMonth < 0) { currentMonth = 11; currentYear--; }
            loadData();
        });
        document.getElementById('cal-next').addEventListener('click', () => {
            currentMonth++;
            if (currentMonth > 11) { currentMonth = 0; currentYear++; }
            loadData();
        });
    }

    function createDayCell(day, isOtherMonth, date, isToday) {
        const cell = document.createElement('div');
        cell.className = 'day-cell';
        if (isOtherMonth) cell.classList.add('other-month');
        if (isToday) cell.classList.add('today');

        const numberSpan = document.createElement('span');
        numberSpan.className = 'day-number';
        numberSpan.textContent = day;
        cell.appendChild(numberSpan);

        if (!isOtherMonth && date) {
            const dateStr = formatDate(date);
            cell.dataset.date = dateStr;

            const dayBookings = getEntriesForDate(date, 'booking');
            const dayUnavail = getEntriesForDate(date, 'unavailable');

            if (config.mode === 'admin') {
                // Admin: stacked bars for bookings + unavailability
                dayBookings.forEach(b => {
                    const bar = document.createElement('div');
                    bar.className = 'booking-bar-stacked';
                    bar.style.background = userColorMap[b.username] || '#f48fb1';
                    bar.title = `${b.username}: ${b.startDate} → ${b.endDate}`;
                    cell.appendChild(bar);
                });
                dayUnavail.forEach(u => {
                    const bar = document.createElement('div');
                    bar.className = 'booking-bar-stacked';
                    bar.style.background = UNAVAILABLE_COLOR;
                    bar.title = u.username || 'Unavailable';
                    cell.appendChild(bar);
                });

                // Admin can also click to select dates for unavailability
                const todayDate = new Date();
                todayDate.setHours(0, 0, 0, 0);
                if (date >= todayDate) {
                    cell.classList.add('available');
                    cell.addEventListener('click', () => handleDayClick(date, dateStr));
                }
            } else {
                // Client: single color for booked, grey for unavailable
                if (dayUnavail.length > 0) {
                    const bar = document.createElement('div');
                    bar.className = 'booking-bar';
                    bar.style.background = UNAVAILABLE_COLOR;

                    const u = dayUnavail[0];
                    const uStart = parseDate(u.startDate);
                    const uEnd = parseDate(u.endDate);
                    bar.classList.add(getBarClass(date, uStart, uEnd));

                    cell.appendChild(bar);
                } else if (dayBookings.length > 0) {
                    const b = dayBookings[0];
                    const bar = document.createElement('div');
                    bar.className = 'booking-bar';
                    bar.style.background = '#f48fb1';

                    const bStart = parseDate(b.startDate);
                    const bEnd = parseDate(b.endDate);
                    bar.classList.add(getBarClass(date, bStart, bEnd));

                    cell.appendChild(bar);
                } else {
                    // Available — allow selection
                    const todayDate = new Date();
                    todayDate.setHours(0, 0, 0, 0);
                    if (date >= todayDate) {
                        cell.classList.add('available');
                        cell.addEventListener('click', () => handleDayClick(date, dateStr));
                    }
                }
            }

            // Highlight selection range
            if (selectionStart && selectionEnd) {
                const s = Math.min(selectionStart.getTime(), selectionEnd.getTime());
                const e = Math.max(selectionStart.getTime(), selectionEnd.getTime());
                const t = date.getTime();
                if (t >= s && t <= e) {
                    if (t === s || t === e) {
                        cell.classList.add(t === s ? 'selected-start' : 'selected-end');
                    } else {
                        cell.classList.add('selected-range');
                    }
                }
            }
        }

        return cell;
    }

    function getBarClass(date, rangeStart, rangeEnd) {
        if (rangeStart.getTime() === rangeEnd.getTime()) return 'bar-single';
        if (date.getTime() === rangeStart.getTime()) return 'bar-start';
        if (date.getTime() === rangeEnd.getTime()) return 'bar-end';
        return 'bar-middle';
    }

    function handleDayClick(date, dateStr) {
        if (!selectionStart || (selectionStart && selectionEnd)) {
            selectionStart = date;
            selectionEnd = null;
        } else {
            selectionEnd = date;
            if (selectionEnd < selectionStart) {
                [selectionStart, selectionEnd] = [selectionEnd, selectionStart];
            }

            // For clients: check overlap with bookings and unavailability
            if (config.mode === 'client' && hasOverlap(selectionStart, selectionEnd)) {
                alert('Selected dates overlap with an existing booking or unavailability. Please choose different dates.');
                selectionStart = null;
                selectionEnd = null;
                render();
                return;
            }

            if (config.onDateRangeSelected) {
                config.onDateRangeSelected(formatDate(selectionStart), formatDate(selectionEnd));
            }
        }
        render();
    }

    function hasOverlap(start, end) {
        return entries.some(e => {
            const eStart = parseDate(e.startDate);
            const eEnd = parseDate(e.endDate);
            return start <= eEnd && end >= eStart;
        });
    }

    function getEntriesForDate(date, type) {
        return entries.filter(e => {
            if (e.type !== type) return false;
            const eStart = parseDate(e.startDate);
            const eEnd = parseDate(e.endDate);
            return date >= eStart && date <= eEnd;
        });
    }

    function parseDate(str) {
        const parts = str.split('-');
        return new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
    }

    function formatDate(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    }

    return { init };
})();
