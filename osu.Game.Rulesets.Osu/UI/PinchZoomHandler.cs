using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Pinch-to-zoom handler to be added to the PlayfieldAdjustmentContainer.
    /// Keeps the focal point under the fingers stable by compensating position after scaling.
    /// Consumes positional events while an active pinch is in progress to avoid accidental hits.
    /// </summary>
    public partial class PinchZoomHandler : CompositeDrawable
    {
        private object? firstSource;
        private object? secondSource;

        private Vector2 firstPos;
        private Vector2 secondPos;

        private float initialDistance;
        private float initialScale = 1f;
        private Vector2 initialFocalScreen;

        // Configurable limits / thresholds.
        private const float minScale = 0.6f;
        private const float maxScale = 2.5f;
        private const float triggerMinimumDistance = 10f;

        /// <summary>
        /// Playfield reference used to convert between screen and gamefield coordinates.
        /// </summary>
        [Resolved(CanBeNull = true)]
        private Playfield? playfield { get; set; }

        /// <summary>
        /// The container that is typically used to apply scale / position adjustments to the playfield.
        /// </summary>
        [Resolved(CanBeNull = true)]
        private PlayfieldAdjustmentContainer? playfieldAdjustmentContainer { get; set; }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            // Register first/second touches by source identity.
            if (firstSource == null)
            {
                firstSource = e.Touch.Source;
                firstPos = e.ScreenSpaceTouch.Position;
                // don't swallow the first touch (so taps still register if no pinch follows)
                return base.OnTouchDown(e);
            }

            if (secondSource == null && e.Touch.Source != firstSource)
            {
                secondSource = e.Touch.Source;
                secondPos = e.ScreenSpaceTouch.Position;
                startPinch();

                // If a pinch was started, swallow this touch to avoid it triggering gameplay tap.
                if (isPinching)
                    return true;
            }

            return base.OnTouchDown(e);
        }

        protected override void OnTouchMove(TouchMoveEvent e)  // ← Changed from bool to void
        {
            // Update positions for the tracked touches.
            if (firstSource != null && e.Touch.Source == firstSource)
                firstPos = e.ScreenSpaceTouch.Position;

            if (secondSource != null && e.Touch.Source == secondSource)
                secondPos = e.ScreenSpaceTouch.Position;

            if (isPinching)
            {
                updatePinch();
                return; // ← Changed from "return true;" to just "return;"
            }

            base.OnTouchMove(e);  // ← Changed from "return base.OnTouchMove(e);" to just call base
        }

        protected override void OnTouchUp(TouchUpEvent e)  // ← Changed from bool to void
        {
            if (e.Touch.Source == firstSource)
            {
                firstSource = null;
            }
            else if (e.Touch.Source == secondSource)
            {
                secondSource = null;
            }

            if (firstSource == null || secondSource == null)
                stopPinch();

            base.OnTouchUp(e);  // ← Changed from "return consumed || base.OnTouchUp(e);" to just call base
        }

        private bool isPinching => firstSource != null && secondSource != null && initialDistance > 0;

        private void startPinch()
        {
            initialDistance = Vector2.Distance(firstPos, secondPos);

            if (initialDistance <= triggerMinimumDistance)
            {
                initialDistance = 0;
                return;
            }

            initialFocalScreen = (firstPos + secondPos) * 0.5f;

            // Read initial scale from the container we will scale (fallback to 1).
            initialScale = playfieldAdjustmentContainer?.Scale.X ?? playfield?.Scale.X ?? 1f;
        }

        private void updatePinch()
        {
            if (!isPinching) return;

            float currentDistance = Vector2.Distance(firstPos, secondPos);
            if (currentDistance <= triggerMinimumDistance) return;

            float zoomFactor = currentDistance / initialDistance;
            float target = Math.Clamp(initialScale * zoomFactor, minScale, maxScale);

            // Convert focal screen -> gamefield before scale
            Vector2 worldBefore = playfield != null ? playfield.ScreenSpaceToGamefield(initialFocalScreen) : Vector2.Zero;

            // apply uniform scale to the adjustment container (preferred) or to playfield directly
            if (playfieldAdjustmentContainer != null)
                playfieldAdjustmentContainer.Scale = new Vector2(target);
            else if (playfield != null)
                playfield.Scale = new Vector2(target);

            // Convert focal screen -> gamefield after scale
            Vector2 worldAfter = playfield != null ? playfield.ScreenSpaceToGamefield(initialFocalScreen) : Vector2.Zero;
            Vector2 worldDelta = worldBefore - worldAfter;

            // Shift container to compensate so focal point stays visually stable.
            if (playfieldAdjustmentContainer != null)
                playfieldAdjustmentContainer.Position += worldDelta;
            else if (playfield != null)
                playfield.Position += worldDelta;
        }

        private void stopPinch()
        {
            initialDistance = 0;
        }
    }
}
