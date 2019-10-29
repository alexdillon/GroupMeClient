using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GroupMeClient.Utilities.DirectManipulation
{
    public class PointerBasedManipulationHandler : IDirectManipulationViewportEventHandler, IDisposable
    {
        private const int ContentMatrixSize = 6;

        // Actual values don't matter
        private static tagRECT defaultViewport = new tagRECT { top = 0, left = 0, right = 1000, bottom = 1000 };

        private readonly float[] matrix = new float[ContentMatrixSize];
        private readonly IntPtr matrixContent;

        private HwndSource hwndSource;
        private IDirectManipulationManager manager;
        private IDirectManipulationViewport viewport;
        private Size viewportSize;

        private uint viewportEventHandlerRegistration;
        private float lastScale;

        private float lastTranslationX;
        private float lastTranslationY;

        public PointerBasedManipulationHandler()
        {
            this.matrixContent = Marshal.AllocCoTaskMem(sizeof(float) * ContentMatrixSize);
        }

        public event Action<float> ScaleUpdated;

        public event Action<float, float> TranslationUpdated;

        public HwndSource HwndSource
        {
            get => this.hwndSource;
            set
            {
                var first = this.hwndSource == null && value != null;
                var oldHwndSource = this.hwndSource;
                if (oldHwndSource != null)
                {
                    oldHwndSource.RemoveHook(this.WndProcHook);
                }

                if (value != null)
                {
                    value.AddHook(this.WndProcHook);
                }

                this.hwndSource = value;
                if (first && value != null)
                {
                    this.InitializeDirectManipulation();
                }
            }
        }

        private IntPtr Window => this.hwndSource.Handle;

        private void InitializeDirectManipulation()
        {
            this.manager = (IDirectManipulationManager)Activator.CreateInstance(typeof(DirectManipulationManagerClass));
            var riidViewport = typeof(IDirectManipulationViewport).GUID;

            this.viewport = this.manager.CreateViewport(null, this.Window, riidViewport) as IDirectManipulationViewport;

            var configuration = DIRECTMANIPULATION_CONFIGURATION.DIRECTMANIPULATION_CONFIGURATION_INTERACTION
                | DIRECTMANIPULATION_CONFIGURATION.DIRECTMANIPULATION_CONFIGURATION_TRANSLATION_X
                | DIRECTMANIPULATION_CONFIGURATION.DIRECTMANIPULATION_CONFIGURATION_TRANSLATION_Y
                | DIRECTMANIPULATION_CONFIGURATION.DIRECTMANIPULATION_CONFIGURATION_TRANSLATION_INERTIA
                | DIRECTMANIPULATION_CONFIGURATION.DIRECTMANIPULATION_CONFIGURATION_RAILS_X
                | DIRECTMANIPULATION_CONFIGURATION.DIRECTMANIPULATION_CONFIGURATION_RAILS_Y;

            this.viewport.ActivateConfiguration(configuration);
            this.viewportEventHandlerRegistration = this.viewport.AddEventHandler(this.Window, this);
            this.viewport.SetViewportRect(ref defaultViewport);
            this.viewport.Enable();
        }

        public void Dispose()
        {
            this.viewport.RemoveEventHandler(this.viewportEventHandlerRegistration);
            Marshal.FreeCoTaskMem(this.matrixContent);
            this.HwndSource = null;
        }

        public void SetSize(Size size)
        {
            this.viewportSize = size;
            this.viewport.Stop();
            var rect = new tagRECT
            {
                left = 0,
                top = 0,
                right = (int)size.Width,
                bottom = (int)size.Height,
            };
            this.viewport.SetViewportRect(ref rect);
        }

        // Our custom hook to process WM_POINTER event
        private IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == MouseNative.WM_POINTERDOWN || msg == MouseNative.DM_POINTERHITTEST)
            {
                var pointerID = MouseNative.GetPointerId(wParam);
                var pointerInfo = default(MouseNative.POINTER_INFO);
                if (!MouseNative.GetPointerInfo(pointerID, ref pointerInfo))
                {
                    return IntPtr.Zero;
                }

                if (pointerInfo.pointerType != MouseNative.POINTER_INPUT_TYPE.PT_TOUCHPAD &&
                    pointerInfo.pointerType != MouseNative.POINTER_INPUT_TYPE.PT_TOUCH)
                {
                    return IntPtr.Zero;
                }

                this.viewport.SetContact(pointerID);
            }
            else if (msg == MouseNative.WM_SIZE && this.manager != null)
            {
                if (wParam == MouseNative.SIZE_MAXHIDE
                    || wParam == MouseNative.SIZE_MINIMIZED)
                {
                    this.manager.Deactivate(this.Window);
                }
                else
                {
                    this.manager.Activate(this.Window);
                }
            }

            return IntPtr.Zero;
        }

        private void ResetViewport(IDirectManipulationViewport viewport)
        {
            viewport.ZoomToRect(0, 0, (float)this.viewportSize.Width, (float)this.viewportSize.Height, 0);
            this.lastScale = 1.0f;
            this.lastTranslationX = this.lastTranslationY = 0;
        }

        public void OnViewportStatusChanged([In, MarshalAs(UnmanagedType.Interface)] IDirectManipulationViewport viewport, [In] DIRECTMANIPULATION_STATUS current, [In] DIRECTMANIPULATION_STATUS previous)
        {
            if (previous == current)
            {
                return;
            }

            if (current == DIRECTMANIPULATION_STATUS.DIRECTMANIPULATION_READY)
            {
                this.ResetViewport(viewport);
            }
        }

        public void OnViewportUpdated([In, MarshalAs(UnmanagedType.Interface)] IDirectManipulationViewport viewport)
        {
        }

        public void OnContentUpdated([In, MarshalAs(UnmanagedType.Interface)] IDirectManipulationViewport viewport, [In, MarshalAs(UnmanagedType.Interface)] IDirectManipulationContent content)
        {
            content.GetContentTransform(this.matrixContent, ContentMatrixSize);
            Marshal.Copy(this.matrixContent, this.matrix, 0, ContentMatrixSize);

            float scale = this.matrix[0];
            float newX = this.matrix[4];
            float newY = this.matrix[5];

            if (scale == 0.0f)
            {
                return;
            }

            var deltaX = newX - this.lastTranslationX;
            var deltaY = newY - this.lastTranslationY;

            bool ShallowFloatEquals(float f1, float f2)
                => Math.Abs(f2 - f1) < float.Epsilon;

            if ((ShallowFloatEquals(scale, 1.0f) || ShallowFloatEquals(scale, this.lastScale))
                && (Math.Abs(deltaX) > 1.0f || Math.Abs(deltaY) > 1.0f))
            {
                this.TranslationUpdated?.Invoke(-deltaX, -deltaY);
            }
            else if (!ShallowFloatEquals(this.lastScale, scale))
            {
                this.ScaleUpdated?.Invoke(scale);
            }

            this.lastScale = scale;
            this.lastTranslationX = newX;
            this.lastTranslationY = newY;
        }
    }
}