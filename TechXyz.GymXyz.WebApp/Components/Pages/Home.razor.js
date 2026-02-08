const followStates = new WeakMap();

function getMinuteHeight(container) {
    const style = getComputedStyle(container);
    const minuteHeightRaw = style.getPropertyValue("--minute-height").trim();
    let minuteHeight = parseFloat(minuteHeightRaw);

    if (!Number.isFinite(minuteHeight) || minuteHeight <= 0) {
        minuteHeight = 2;
    }

    return minuteHeight;
}

function getTargetTop(container, currentOffsetMinutes, topOffsetPx) {
    const minuteHeight = getMinuteHeight(container);
    const offset = Number.isFinite(topOffsetPx) ? topOffsetPx : 0;
    return Math.max(0, currentOffsetMinutes * minuteHeight - offset);
}

export function initFollow(container) {
    if (!container || followStates.has(container)) {
        return;
    }

    const state = {
        lastUserScroll: 0,
        programmatic: false,
        resetTimer: null
    };

    const onScroll = () => {
        if (state.programmatic) {
            return;
        }

        state.lastUserScroll = Date.now();
    };

    container.addEventListener("scroll", onScroll, { passive: true });
    state.onScroll = onScroll;
    followStates.set(container, state);
}

export function scrollToCurrentTime(container, currentOffsetMinutes, topOffsetPx) {
    if (!container) {
        return;
    }

    const top = getTargetTop(container, currentOffsetMinutes, topOffsetPx);
    container.scrollTo({ top, behavior: "auto" });
}

export function followCurrentTime(container, currentOffsetMinutes, topOffsetPx, overrideDelayMs) {
    if (!container) {
        return;
    }

    const state = followStates.get(container);
    const delay = Number.isFinite(overrideDelayMs) ? overrideDelayMs : 0;

    if (state && delay > 0 && Date.now() - state.lastUserScroll < delay) {
        return;
    }

    const top = getTargetTop(container, currentOffsetMinutes, topOffsetPx);

    if (state) {
        state.programmatic = true;
        if (state.resetTimer) {
            clearTimeout(state.resetTimer);
        }

        state.resetTimer = setTimeout(() => {
            state.programmatic = false;
            state.resetTimer = null;
        }, 250);
    }

    container.scrollTo({ top, behavior: "smooth" });
}
