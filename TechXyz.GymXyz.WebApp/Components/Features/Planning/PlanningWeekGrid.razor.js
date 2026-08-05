// Places the "now" marker on the week grid and scrolls the current hour into
// view. Both are measurements: the rows grow with their content, so the pixel
// offset of an hour is only known once the browser has laid the grid out.

const MARKER_LINE = "gx-now-line";
const MARKER_DOT = "gx-now-dot";
const DOT_RADIUS = 4;

function clearMarkers(scroll) {
    scroll.querySelectorAll(`.${MARKER_LINE}, .${MARKER_DOT}`).forEach((node) => node.remove());
}

export function placeNow(scroll, todayIndex, hourOfDay, firstHour, lastHour) {
    if (!scroll) {
        return;
    }

    clearMarkers(scroll);

    const grid = scroll.querySelector(".gx-cal");
    const cells = grid && grid.querySelectorAll(".cell");
    if (!cells || !cells.length) {
        return;
    }

    const rowHeight = cells[0].offsetHeight || 52;
    const headerHeight = cells[0].offsetTop;

    // Not this week: park the view a few hours in rather than at 7:00, and draw
    // no marker at all — "now" is not on screen.
    if (todayIndex < 0) {
        scroll.scrollTop = Math.max(0, 3 * rowHeight);
        return;
    }

    const clamped = Math.min(Math.max(hourOfDay, firstHour), lastHour);
    const top = headerHeight + (clamped - firstHour) * rowHeight;

    const line = document.createElement("div");
    line.className = MARKER_LINE;
    line.style.top = `${top}px`;
    scroll.appendChild(line);

    const header = grid.querySelectorAll(".hd")[todayIndex];
    if (header) {
        const dot = document.createElement("div");
        dot.className = MARKER_DOT;
        dot.style.top = `${top - DOT_RADIUS}px`;
        dot.style.left = `${header.offsetLeft + header.offsetWidth / 2 - DOT_RADIUS}px`;
        scroll.appendChild(dot);
    }

    scroll.scrollTop = Math.max(0, top - headerHeight - 60);
}
