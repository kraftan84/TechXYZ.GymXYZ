// Centres today in the mobile week strip.
//
// The strip scrolls sideways and only holds about five cells at a phone's
// width, so on a Friday or a weekend today starts off-screen — the one cell the
// dashboard exists to put in front of you. A measurement, not a layout rule:
// the cell's offset is only known once the browser has laid the strip out.

export function centerToday(strip, todayIndex) {
    if (!strip || todayIndex < 0) {
        return;
    }

    const cell = strip.children[todayIndex];
    if (!cell) {
        return;
    }

    // Left-aligned when the cell is already near the start: scrolling a Monday
    // into the middle would push the earlier days out for nothing.
    strip.scrollLeft = Math.max(0, cell.offsetLeft - (strip.clientWidth - cell.clientWidth) / 2);
}
