using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace TriCNES.Views
{
    public partial class MainWindow : Window
    {
        public Emulator EMU;
        WriteableBitmap _bitmap;
        private readonly HashSet<Key> _keysDown = new HashSet<Key>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Task? _emuTask;
        private volatile bool _frameReady;
        private int frameCount = 0;
        private bool flip = false;

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += OnKeyDown;
            this.KeyUp   += OnKeyUp;
            this.Focusable = true;
            this.Focus();
            
            EMU = new Emulator();
            EMU.Cart = new Cartridge("/home/v/Games/NES/Super Mario Bros. (World).nes");
            bool settings_ntsc = true;
            bool settings_boarder = true;
            int settings_alignment = 0;
            EMU.PPU_DecodeSignal = settings_ntsc;
            EMU.PPU_ShowScreenBoarders = settings_boarder;
            EMU.PPUClock = settings_alignment;
            
            _bitmap = new WriteableBitmap(
                new PixelSize(EMU.BoarderedNTSCScreen.Width, EMU.BoarderedNTSCScreen.Height),
                new Vector(96, 96),
                PixelFormat.Bgra8888,
                AlphaFormat.Opaque);
            Screen.Source = _bitmap;
            
            // Start emulator
            // _emuTask = Task.Run(AdvanceEmulator, _cts.Token);
            
            // Start render timer on UI thread
            var renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            int frameCount = 0;
            bool flip = false;
            renderTimer.Tick += (_, _) =>
            {
                ClockEmulator();
            };
            renderTimer.Start();
        }
        
        private void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            _keysDown.Add(e.Key);
        }

        private void OnKeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            _keysDown.Remove(e.Key);
        }
        
        private void ClockEmulator()
        {
            // LockObject = pb_Screen;
            // lock (LockObject)
            // {
            //     while (true)
            //     {
            //         if (Form.ActiveForm != null)
            //         {
            //             if (pb_Screen.InvokeRequired)
            //             {
            //                 pb_Screen.Invoke(new MethodInvoker(
            //                     delegate()
            //                     {
            //                         if (EMU.PPU_DecodeSignal)
            //                         {
            //                             if (EMU.PPU_ShowScreenBoarders)
            //                             {
            //                                 pb_Screen.Image = EMU.BoarderedNTSCScreen.Bitmap;
            //                             }
            //                             else
            //                             {
            //                                 pb_Screen.Image = EMU.NTSCScreen.Bitmap;
            //                             }
            //                         }
            //                         else
            //                         {
            //                             if (EMU.PPU_ShowScreenBoarders)
            //                             {
            //                                 pb_Screen.Image = EMU.BoarderedScreen.Bitmap;
            //                             }
            //                             else
            //                             {
            //                                 pb_Screen.Image = EMU.Screen.Bitmap;
            //                             }
            //                         }
            //
            //                         pb_Screen.Update();
            //                     }));
            //             }
            //             else
            //             {
            //                 if (EMU.PPU_DecodeSignal)
            //                 {
            //                     if (EMU.PPU_ShowScreenBoarders)
            //                     {
            //                         pb_Screen.Image = EMU.BoarderedNTSCScreen.Bitmap;
            //                     }
            //                     else
            //                     {
            //                         pb_Screen.Image = EMU.NTSCScreen.Bitmap;
            //                     }
            //                 }
            //                 else
            //                 {
            //                     if (EMU.PPU_ShowScreenBoarders)
            //                     {
            //                         pb_Screen.Image = EMU.BoarderedScreen.Bitmap;
            //                     }
            //                     else
            //                     {
            //                         pb_Screen.Image = EMU.Screen.Bitmap;
            //                     }
            //                 }
            //
            //                 pb_Screen.Update();
            //             }
            //
            //             if (TraceLogger != null)
            //             {
            //                 if (TraceLogger.Logging)
            //                 {
            //                     TraceLogger.Update();
            //                     if (TraceLogger.ClearEveryFrame())
            //                     {
            //                         EMU.DebugLog = new StringBuilder();
            //                     }
            //                 }
            //             }
            // }
            // if (TraceLogger != null)
            // {
            //     EMU.Logging = TraceLogger.Logging;
            //     if (EMU.DebugLog == null)
            //     {
            //         EMU.DebugLog = new StringBuilder();
            //     }
            //     EMU.DebugRange_Low = TraceLogger.RangeLow;
            //     EMU.DebugRange_High = TraceLogger.RangeHigh;
            //     EMU.OnlyDebugInRange = TraceLogger.OnlyDebugInRange();
            // }
            // else
            // {
            // EMU.Logging = false;
            // EMU.DebugLog = new StringBuilder();
            // }

                        // if (NametableViewer != null && !NametableViewer.IsDisposed)
                        // {
                        //     NametableViewer.Update(RenderNametable());
                        // }
            //         }
            //     }
            // }
            
            byte controller1 = 0;
            if (_keysDown.Contains(Key.X)) controller1 |= 0x80;
            if (_keysDown.Contains(Key.Z)) controller1 |= 0x40;
            if (_keysDown.Contains(Key.RightShift)) controller1 |= 0x20;
            if (_keysDown.Contains(Key.Enter)) controller1 |= 0x10;
            if (_keysDown.Contains(Key.Up)) controller1 |= 0x08;
            if (_keysDown.Contains(Key.Down)) controller1 |= 0x04;
            if (_keysDown.Contains(Key.Left)) controller1 |= 0x02;
            if (_keysDown.Contains(Key.Right)) controller1 |= 0x01;
            EMU.ControllerPort1 = controller1;
            EMU._CoreFrameAdvance();
            EMU.BoarderedNTSCScreen.CopyIntoBitmap(_bitmap);
            frameCount++;
            // if (frameCount % 4 == 0)
            // {
            //     using var fb = _bitmap.Lock();
            //
            //     unsafe
            //     {
            //         uint* p = (uint*)fb.Address;
            //
            //         for (int i = 0; i < _bitmap.Size.Width * _bitmap.Size.Height; i++)
            //             p[i] = flip ? 0xFF00FF00 : 0xFFFF0000; // green / red
            //         flip = !flip;
            //     }
            // }
            Screen.InvalidateVisual();
        }

        // DirectBitmap NametableBitmap;
        // public Bitmap RenderNametable()
        // {
        //     
        //
        //     if (NametableBitmap != null)
        //     {
        //         NametableBitmap.Dispose();
        //     }
        //     NametableBitmap = new DirectBitmap(512, 480);
        //     if (EMU.Cart == null)
        //     {
        //         return NametableBitmap.Bitmap;
        //     }
        //
        //     int tx = 0;
        //     int ty = 0;
        //     int x = 0;
        //     int y = 0;
        //     int px = 0;
        //     int py = 0;
        //
        //     int PatternTile;
        //     int pal = 0;
        //
        //     bool ForceBackdropOnIndex0 = NametableViewer.UseBackdrop();
        //
        //     while (ty < 2)
        //     {
        //         while (tx < 2)
        //         {
        //             while (y < 30)
        //             {
        //                 while (x < 32)
        //                 {
        //                     PatternTile = EMU.FetchPPU((ushort)(0x2000 + 0x400 * tx + 0x800 * ty + x + y * 32));
        //                     pal = EMU.FetchPPU((ushort)(0x2000 + 0x400 * (tx + 1) + 0x800 * ty - 0x40 + x / 4 + (y / 4) * 8));
        //                     if ((x & 3) >= 2)
        //                     {
        //                         pal = pal >> 2;
        //                     }
        //                     if ((y & 3) >= 2)
        //                     {
        //                         pal = pal >> 4;
        //                     }
        //                     pal = pal & 3;
        //                     while (py < 8)
        //                     {
        //                         while (px < 8)
        //                         {
        //
        //                             int k = ((EMU.FetchPPU((ushort)(py + PatternTile * 16 + (!EMU.PPU_PatternSelect_Background ? 0 : 0x1000))) >> (7 - px)) & 1) + 2 * ((EMU.FetchPPU((ushort)(py + 8 + PatternTile * 16 + (!EMU.PPU_PatternSelect_Background ? 0 : 0x1000))) >> (7 - px)) & 1);
        //                             if (k == 0 && ForceBackdropOnIndex0)
        //                             {
        //                                 k = EMU.FetchPPU(0x3F00);
        //                             }
        //                             else
        //                             {
        //                                 k = EMU.FetchPPU((ushort)(0x3F00 + k + pal * 4));
        //                             }
        //                             int col = unchecked((int)Emulator.NesPalInts[k & 0x3F]);
        //                             NametableBitmap.SetPixel(tx * 0x100 + x * 8 + px, ty * 0xF0 + y * 8 + py, col);
        //                             px++;
        //                         }
        //                         px = 0;
        //                         py++;
        //                     }
        //                     py = 0;
        //                     x++;
        //                 }
        //
        //                 x = 0;
        //                 y++;
        //             }
        //             y = 0;
        //             tx++;
        //         }
        //         tx = 0;
        //         ty++;
        //     }
        //
        //     bool DrawScreenBoundary = NametableViewer.DrawBoundary();
        //     if(DrawScreenBoundary)
        //     {
        //         // convert the t register into X,Y coordinates
        //         /*
        //         The v and t registers are 15 bits:
        //         yyy NN YYYYY XXXXX
        //         ||| || ||||| +++++-- coarse X scroll
        //         ||| || +++++-------- coarse Y scroll
        //         ||| ++-------------- nametable select
        //         +++----------------- fine Y scroll
        //         */
        //         int X = ((EMU.PPU_TempVRAMAddress & 0b11111) << 3) | EMU.PPU_FineXScroll | ((EMU.PPU_TempVRAMAddress & 0b10000000000) >> 2);
        //         int Y = ((EMU.PPU_TempVRAMAddress & 0b1111100000) >> 2) | ((EMU.PPU_TempVRAMAddress & 0b111000000000000) >> 12) | ((EMU.PPU_TempVRAMAddress & 0b100000000000) >> 4);
        //         int i = 0;
        //         while(i <= 257)
        //         {
        //             NametableBitmap.SetPixel((X + 511 + i) & 511, (Y + 479) % 480, Color.White);
        //             NametableBitmap.SetPixel((X + 511 + i) & 511, (Y + 240) % 480, Color.White);
        //             i++;
        //         }
        //         i = 0;
        //         while (i <= 241)
        //         {
        //             NametableBitmap.SetPixel((X + 511) & 511, (Y + 479 + i) % 480, Color.White);
        //             NametableBitmap.SetPixel((X + 256) & 511, (Y + 479 + i) % 480, Color.White);
        //             i++;
        //         }                
        //     }
        //     if (NametableViewer.OverlayScreen())
        //     {
        //         int X = ((EMU.PPU_TempVRAMAddress & 0b11111) << 3) | EMU.PPU_FineXScroll | ((EMU.PPU_TempVRAMAddress & 0b10000000000) >> 2);
        //         int Y = ((EMU.PPU_TempVRAMAddress & 0b1111100000) >> 2) | ((EMU.PPU_TempVRAMAddress & 0b111000000000000) >> 12) | ((EMU.PPU_TempVRAMAddress & 0b100000000000) >> 4);
        //         for (int xx = 0; xx < 256; xx++)
        //         {
        //             for (int yy = 0; yy < 240; yy++)
        //             {
        //                 NametableBitmap.SetPixel((X + xx) & 511, (Y + yy) % 480, EMU.Screen.GetPixel(xx, yy));
        //             }
        //         }
        //     }
        //     return NametableBitmap.Bitmap;
        // }
    }

}