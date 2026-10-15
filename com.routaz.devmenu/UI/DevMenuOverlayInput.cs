namespace DevMenuTool.UI
{
    /// <summary>
    /// What the runtime overlay is doing this frame, so a game can keep its own input out of the way:
    /// skip world clicks while <see cref="IsPointerOverOverlay"/>, skip typed hotkeys while
    /// <see cref="IsTextInputFocused"/>. All false in release builds, where the overlay never spawns.
    /// </summary>
    public static class DevMenuOverlayInput
    {
        public static bool IsOpen { get; private set; }
        public static bool IsPointerOverOverlay { get; private set; }
        public static bool IsTextInputFocused { get; private set; }

        internal static void Set(bool isOpen, bool isPointerOverOverlay, bool isTextInputFocused)
        {
            IsOpen = isOpen;
            IsPointerOverOverlay = isPointerOverOverlay;
            IsTextInputFocused = isTextInputFocused;
        }

        internal static void Clear() => Set(false, false, false);
    }
}
