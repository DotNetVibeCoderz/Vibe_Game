// Bridge between JavaScript game engine and .NET Blazor

window.gameEngine.dotNetRef = null;

window.gameEngine.registerDotNet = function (dotNetRef) {
    window.gameEngine.dotNetRef = dotNetRef;

    window.addEventListener('itemBoxHit', (e) => {
        if (window.gameEngine.dotNetRef) {
            window.gameEngine.dotNetRef.invokeMethodAsync('OnItemBoxHit', e.detail.x, e.detail.z);
        }
    });

    document.addEventListener('keydown', (e) => {
        if (window.gameEngine.dotNetRef && e.key === ' ') {
            window.gameEngine.dotNetRef.invokeMethodAsync('OnKeyDown', e.key);
        }
    });
};
