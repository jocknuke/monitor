// Minimal Plotly.js interop (expects Plotly to be loaded from CDN in _Host.cshtml)
window.opsmon = window.opsmon || {};
window.opsmon.plotly = {
  render: (el, data, layout, config) => {
    if (!window.Plotly) { console.warn("Plotly not loaded"); return; }
    // Defensive copies to avoid mutation issues from .NET interop proxies
    const d = JSON.parse(JSON.stringify(data || []));
    const l = JSON.parse(JSON.stringify(layout || {}));
    const c = JSON.parse(JSON.stringify(config || {}));
    return window.Plotly.newPlot(el, d, l, c);
  }
};
