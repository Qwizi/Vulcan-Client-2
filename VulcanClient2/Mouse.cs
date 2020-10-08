using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Serilog;

namespace VulcanClient2
{
    public class Win32
    {
        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }
        
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint);
        
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
    }

    public class CursorPos
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    
    public class Mouse
    {
        private static CursorPos GetMouseCursorPos()
        {
            Point p = new Point();
            Win32.GetCursorPos(ref p);
            int xPos = p.X;
            int yPos = p.Y;
            
            Log.Debug($"{xPos} | {yPos}");
            return new CursorPos {X = xPos, Y = yPos};

        }
        public static void Move(int xP, int yP)
        {
            CursorPos curPos = GetMouseCursorPos();
            Point p = new Point();
            p.X = curPos.X + xP;
            p.Y = curPos.Y + yP;
            Win32.SetCursorPos(p.X, p.Y);
        }
        
        public static void MoveX(int xP)
        {
            CursorPos curPos = GetMouseCursorPos();
            Point p = new Point();
            p.X = curPos.X + xP;
            Win32.SetCursorPos(p.X, curPos.Y);
        }
        
        public static void MoveY(int yP)
        {
            CursorPos curPos = GetMouseCursorPos();
            Point p = new Point();
            p.Y = curPos.Y + yP;
            Win32.SetCursorPos(curPos.X, p.Y);
        }

        public static void Click(Win32.MouseEventFlags value)
        {
            CursorPos curPos = GetMouseCursorPos();
            Win32.mouse_event((int)value, curPos.X, curPos.Y,0,0);
        }
    }
}