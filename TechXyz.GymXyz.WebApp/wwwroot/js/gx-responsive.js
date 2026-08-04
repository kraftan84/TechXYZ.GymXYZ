// Decides the layout mode from the viewport only — never the user agent.
// Writes a cookie so the next server render starts on the right shell.

const listeners = new Map();
let nextId = 0;

export function watch(dotNetRef, breakpointPx) {
  const query = window.matchMedia(`(max-width: ${breakpointPx - 0.02}px)`);

  const push = () => {
    const mode = query.matches ? 'mobile' : 'desktop';
    document.cookie = `gx-device=${mode};path=/;max-age=31536000;samesite=lax`;
    dotNetRef.invokeMethodAsync('OnViewportChanged', query.matches);
  };

  query.addEventListener('change', push);
  push();

  const id = ++nextId;
  listeners.set(id, () => query.removeEventListener('change', push));

  return id;
}

export function unwatch(id) {
  const dispose = listeners.get(id);
  if (dispose) {
    dispose();
    listeners.delete(id);
  }
}
