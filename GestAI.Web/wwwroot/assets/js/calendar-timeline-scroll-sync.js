// Sync vertical scroll between .resources and .grid inside a CalendarTimeline root.
// Keeps independent scrollbars but moves together so rows stay aligned.
window.CalendarTimelineScrollSync = (function () {
    const instances = new Map();

    function init(rootId) {
        const root = document.getElementById(rootId);
        if (!root) return;

        const resources = root.querySelector(".resources");
        const grid = root.querySelector(".grid");

        if (!resources || !grid) return;

        // Avoid double init
        if (instances.has(rootId)) return;

        let lock = false;

        const onResourcesScroll = () => {
            if (lock) return;
            lock = true;
            grid.scrollTop = resources.scrollTop;
            lock = false;
        };

        const onGridScroll = () => {
            if (lock) return;
            lock = true;
            resources.scrollTop = grid.scrollTop;
            lock = false;
        };

        resources.addEventListener("scroll", onResourcesScroll, { passive: true });
        grid.addEventListener("scroll", onGridScroll, { passive: true });

        instances.set(rootId, { resources, grid, onResourcesScroll, onGridScroll });
    }

    function destroy(rootId) {
        const inst = instances.get(rootId);
        if (!inst) return;

        inst.resources.removeEventListener("scroll", inst.onResourcesScroll);
        inst.grid.removeEventListener("scroll", inst.onGridScroll);

        instances.delete(rootId);
    }

    return { init, destroy };
})();